using System.Text.Json;
using Domain.Entities;
using Microsoft.Data.Sqlite;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace Api.Services;

public class GpkgParserService
{
    private readonly WKBReader _wkbReader;

    public GpkgParserService()
    {
        _wkbReader = new WKBReader();
    }

    public async Task<GpkgParseResult> ParseGpkgFileAsync(Stream fileStream, string fileName)
    {
        var tempFilePath = Path.GetTempFileName();
        GpkgParseResult result;
        
        try
        {
            using (var tempFile = File.Create(tempFilePath))
            {
                await fileStream.CopyToAsync(tempFile);
            }

            result = await ParseGpkgFromPathAsync(tempFilePath, fileName);
        }
        finally
        {
            // Libérer tous les pools de connexion SQLite pour débloquer le fichier
            SqliteConnection.ClearAllPools();
            
            // Attendre un peu pour que le fichier soit libéré
            await Task.Delay(100);
            
            // Supprimer le fichier temporaire
            try
            {
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
            catch (IOException)
            {
                // Si le fichier ne peut pas être supprimé, on l'ignore
                // Il sera nettoyé par le système plus tard
                Console.WriteLine($"Impossible de supprimer le fichier temporaire: {tempFilePath}");
            }
        }
        
        return result;
    }

    private async Task<GpkgParseResult> ParseGpkgFromPathAsync(string filePath, string fileName)
    {
        var result = new GpkgParseResult
        {
            GpkgFile = new GpkgFile
            {
                Id = Guid.NewGuid(),
                FileName = fileName,
                UploadDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            }
        };

        var connectionString = $"Data Source={filePath};Mode=ReadOnly;Pooling=False";
        using (var connection = new SqliteConnection(connectionString))
        {
            await connection.OpenAsync();

            if (!await IsValidGeoPackageAsync(connection))
            {
                throw new InvalidOperationException("Le fichier n'est pas un GeoPackage valide.");
            }

            var layers = await GetLayersAsync(connection);

            foreach (var layerInfo in layers)
            {
                var layer = new GpkgLayer
                {
                    Id = Guid.NewGuid(),
                    GpkgFileId = result.GpkgFile.Id,
                    LayerName = layerInfo.TableName,
                    GeometryType = layerInfo.GeometryType,
                    CreatedAt = DateTime.UtcNow
                };

                var features = await GetFeaturesAsync(connection, layerInfo, layer.Id);
                layer.FeatureCount = features.Count;

                result.Layers.Add(layer);
                result.Features.AddRange(features);
            }
            
            connection.Close();
        }

        return result;
    }

    private async Task<bool> IsValidGeoPackageAsync(SqliteConnection connection)
    {
        try
        {
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT COUNT(*) FROM sqlite_master 
                WHERE type='table' AND name='gpkg_contents'";
            var count = await command.ExecuteScalarAsync();
            return Convert.ToInt32(count) > 0;
        }
        catch
        {
            return false;
        }
    }

    private async Task<List<GpkgLayerInfo>> GetLayersAsync(SqliteConnection connection)
    {
        var layers = new List<GpkgLayerInfo>();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT 
                gc.table_name,
                gc.data_type,
                COALESCE(ggc.geometry_type_name, 'GEOMETRY') as geometry_type,
                COALESCE(ggc.column_name, 'geom') as geometry_column,
                COALESCE(ggc.srs_id, 4326) as srs_id
            FROM gpkg_contents gc
            LEFT JOIN gpkg_geometry_columns ggc ON gc.table_name = ggc.table_name
            WHERE gc.data_type IN ('features', 'tiles')";

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var dataType = reader.GetString(1);
            if (dataType == "features")
            {
                layers.Add(new GpkgLayerInfo
                {
                    TableName = reader.GetString(0),
                    GeometryType = reader.GetString(2),
                    GeometryColumn = reader.GetString(3),
                    SrsId = reader.GetInt32(4)
                });
            }
        }

        return layers;
    }

    private async Task<List<GpkgFeature>> GetFeaturesAsync(
        SqliteConnection connection, 
        GpkgLayerInfo layerInfo, 
        Guid layerId)
    {
        var features = new List<GpkgFeature>();

        var columnsCommand = connection.CreateCommand();
        columnsCommand.CommandText = $"PRAGMA table_info(\"{layerInfo.TableName}\")";
        
        var columns = new List<string>();
        using (var columnsReader = await columnsCommand.ExecuteReaderAsync())
        {
            while (await columnsReader.ReadAsync())
            {
                var columnName = columnsReader.GetString(1);
                if (!columnName.Equals(layerInfo.GeometryColumn, StringComparison.OrdinalIgnoreCase) &&
                    !columnName.Equals("fid", StringComparison.OrdinalIgnoreCase))
                {
                    columns.Add(columnName);
                }
            }
        }

        var selectColumns = columns.Count > 0 
            ? string.Join(", ", columns.Select(c => $"\"{c}\"")) + ", "
            : "";

        var command = connection.CreateCommand();
        command.CommandText = $@"
            SELECT {selectColumns}""{layerInfo.GeometryColumn}""
            FROM ""{layerInfo.TableName}""";

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            try
            {
                var properties = new Dictionary<string, object?>();
                for (int i = 0; i < columns.Count; i++)
                {
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    properties[columns[i]] = value;
                }

                var geometryIndex = columns.Count;
                if (!reader.IsDBNull(geometryIndex))
                {
                    var geometryBytes = (byte[])reader.GetValue(geometryIndex);
                    var geometry = ParseGpkgGeometry(geometryBytes);

                    if (geometry != null)
                    {
                        geometry.SRID = layerInfo.SrsId;

                        features.Add(new GpkgFeature
                        {
                            Id = Guid.NewGuid(),
                            LayerId = layerId,
                            Geometry = geometry,
                            Properties = JsonSerializer.Serialize(properties),
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du parsing d'une feature: {ex.Message}");
            }
        }

        return features;
    }

    private Geometry? ParseGpkgGeometry(byte[] gpkgGeometry)
    {
        if (gpkgGeometry == null || gpkgGeometry.Length < 8)
            return null;

        try
        {
            if (gpkgGeometry[0] != 0x47 || gpkgGeometry[1] != 0x50)
            {
                return _wkbReader.Read(gpkgGeometry);
            }

            var flags = gpkgGeometry[3];
            var envelopeType = (flags >> 1) & 0x07;

            int headerSize = 8;
            switch (envelopeType)
            {
                case 1: headerSize += 32; break;
                case 2: headerSize += 48; break;
                case 3: headerSize += 48; break;
                case 4: headerSize += 64; break;
            }

            if (gpkgGeometry.Length <= headerSize)
                return null;

            var wkbBytes = new byte[gpkgGeometry.Length - headerSize];
            Array.Copy(gpkgGeometry, headerSize, wkbBytes, 0, wkbBytes.Length);

            return _wkbReader.Read(wkbBytes);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur parsing géométrie: {ex.Message}");
            return null;
        }
    }
}

public class GpkgParseResult
{
    public GpkgFile GpkgFile { get; set; } = null!;
    public List<GpkgLayer> Layers { get; set; } = new();
    public List<GpkgFeature> Features { get; set; } = new();
}

public class GpkgLayerInfo
{
    public string TableName { get; set; } = string.Empty;
    public string GeometryType { get; set; } = string.Empty;
    public string GeometryColumn { get; set; } = "geom";
    public int SrsId { get; set; } = 4326;
}
