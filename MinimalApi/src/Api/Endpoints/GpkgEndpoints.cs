using System.Text.Json;
using Api.Dtos;
using Api.Services;
using Domain.Entities;
using Domain.Interfaces;
using NetTopologySuite.IO;

namespace Api.Endpoints;

public static class GpkgEndpoints
{
    public static void MapGpkgEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/gpkg")
            .WithTags("GeoPackage");

        group.MapPost("/upload", UploadGpkgFile)
            .DisableAntiforgery()
            .WithName("UploadGpkgFile")
            .WithDescription("Upload et traitement d'un fichier GeoPackage");

        group.MapGet("/files", GetAllFiles)
            .WithName("GetAllGpkgFiles")
            .WithDescription("Liste de tous les fichiers GeoPackage uploadés");

        group.MapGet("/files/{id:guid}", GetFileById)
            .WithName("GetGpkgFileById")
            .WithDescription("Détails d'un fichier GeoPackage spécifique");

        group.MapGet("/files/{id:guid}/layers", GetFileLayers)
            .WithName("GetGpkgFileLayers")
            .WithDescription("Liste des couches d'un fichier GeoPackage");

        group.MapGet("/files/{id:guid}/features", GetFileFeatures)
            .WithName("GetGpkgFileFeatures")
            .WithDescription("Toutes les features d'un fichier en GeoJSON");

        group.MapDelete("/files/{id:guid}", DeleteFile)
            .WithName("DeleteGpkgFile")
            .WithDescription("Supprimer un fichier GeoPackage et ses données");
    }

    private static async Task<IResult> UploadGpkgFile(
        HttpRequest request,
        IGpkgFileRepository fileRepository,
        IGpkgLayerRepository layerRepository,
        IGpkgFeatureRepository featureRepository)
    {
        try
        {
            if (!request.HasFormContentType)
            {
                return Results.BadRequest(new { error = "Le contenu doit être de type multipart/form-data" });
            }

            var form = await request.ReadFormAsync();
            var file = form.Files.GetFile("file");

            if (file == null || file.Length == 0)
            {
                return Results.BadRequest(new { error = "Aucun fichier fourni" });
            }

            if (!file.FileName.EndsWith(".gpkg", StringComparison.OrdinalIgnoreCase))
            {
                return Results.BadRequest(new { error = "Le fichier doit être au format .gpkg" });
            }

            var description = form["description"].FirstOrDefault();

            var parserService = new GpkgParserService();
            GpkgParseResult parseResult;

            using (var stream = file.OpenReadStream())
            {
                parseResult = await parserService.ParseGpkgFileAsync(stream, file.FileName);
            }

            parseResult.GpkgFile.Description = description;

            await fileRepository.AddAsync(parseResult.GpkgFile);

            foreach (var layer in parseResult.Layers)
            {
                await layerRepository.AddAsync(layer);
            }

            const int batchSize = 1000;
            for (int i = 0; i < parseResult.Features.Count; i += batchSize)
            {
                var batch = parseResult.Features.Skip(i).Take(batchSize);
                await featureRepository.AddRangeAsync(batch);
            }

            var response = new GpkgUploadResponseDto(
                parseResult.GpkgFile.Id,
                parseResult.GpkgFile.FileName,
                parseResult.Layers.Count,
                parseResult.Features.Count,
                "Fichier GeoPackage importé avec succès"
            );

            return Results.Created($"/api/gpkg/files/{parseResult.GpkgFile.Id}", response);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de l'upload: {ex}");
            return Results.Problem(
                detail: "Une erreur est survenue lors du traitement du fichier",
                statusCode: 500
            );
        }
    }

    private static async Task<IResult> GetAllFiles(IGpkgFileRepository fileRepository)
    {
        var files = await fileRepository.GetAllWithLayersAsync();

        var dtos = files.Select(f => new GpkgFileDto(
            f.Id,
            f.FileName,
            f.UploadDate,
            f.Description,
            f.Layers.Count,
            f.Layers.Sum(l => l.FeatureCount)
        )).ToList();

        return Results.Ok(dtos);
    }

    private static async Task<IResult> GetFileById(
        Guid id,
        IGpkgFileRepository fileRepository)
    {
        var file = await fileRepository.GetByIdWithLayersAsync(id);

        if (file == null)
        {
            return Results.NotFound(new { error = "Fichier non trouvé" });
        }

        var dto = new GpkgFileDetailDto(
            file.Id,
            file.FileName,
            file.UploadDate,
            file.Description,
            file.Layers.Select(l => new GpkgLayerDto(
                l.Id,
                l.LayerName,
                l.GeometryType,
                l.FeatureCount
            )).ToList()
        );

        return Results.Ok(dto);
    }

    private static async Task<IResult> GetFileLayers(
        Guid id,
        IGpkgLayerRepository layerRepository,
        IGpkgFileRepository fileRepository)
    {
        var file = await fileRepository.GetByIdAsync(id);
        if (file == null)
        {
            return Results.NotFound(new { error = "Fichier non trouvé" });
        }

        var layers = await layerRepository.GetByFileIdAsync(id);

        var dtos = layers.Select(l => new GpkgLayerDto(
            l.Id,
            l.LayerName,
            l.GeometryType,
            l.FeatureCount
        )).ToList();

        return Results.Ok(dtos);
    }

    private static async Task<IResult> GetFileFeatures(
        Guid id,
        IGpkgFeatureRepository featureRepository,
        IGpkgFileRepository fileRepository,
        IGpkgLayerRepository layerRepository,
        HttpContext context)
    {
        var file = await fileRepository.GetByIdAsync(id);
        if (file == null)
        {
            return Results.NotFound(new { error = "Fichier non trouvé" });
        }

        // Paramètres de pagination
        var pageSize = int.TryParse(context.Request.Query["pageSize"], out var ps) ? ps : 1000;
        var page = int.TryParse(context.Request.Query["page"], out var p) ? p : 1;
        
        // Paramètres de bbox (minLng, minLat, maxLng, maxLat)
        double minLng = 0, minLat = 0, maxLng = 0, maxLat = 0;
        var hasBbox = double.TryParse(context.Request.Query["minLng"], out minLng) &&
                      double.TryParse(context.Request.Query["minLat"], out minLat) &&
                      double.TryParse(context.Request.Query["maxLng"], out maxLng) &&
                      double.TryParse(context.Request.Query["maxLat"], out maxLat);

        var layers = await layerRepository.GetByFileIdAsync(id);
        var layerDict = layers.ToDictionary(l => l.Id, l => l.LayerName);

        // Filtrage spatial et pagination directement en SQL avec PostGIS
        var (paginatedFeatures, totalCount) = await featureRepository.GetByFileIdWithBboxAsync(
            id,
            hasBbox ? minLng : null,
            hasBbox ? minLat : null,
            hasBbox ? maxLng : null,
            hasBbox ? maxLat : null,
            page,
            pageSize
        );

        var geoJsonWriter = new GeoJsonWriter();
        
        var geoJsonFeatures = paginatedFeatures.Select(f =>
        {
            var geometryJson = geoJsonWriter.Write(f.Geometry);
            var geometryObj = JsonSerializer.Deserialize<object>(geometryJson);
            var properties = JsonSerializer.Deserialize<Dictionary<string, object?>>(f.Properties) 
                ?? new Dictionary<string, object?>();

            return new
            {
                type = "Feature",
                id = f.Id,
                properties = new Dictionary<string, object?>(properties)
                {
                    ["layerName"] = layerDict.GetValueOrDefault(f.LayerId, "Unknown"),
                    ["layerId"] = f.LayerId
                },
                geometry = geometryObj
            };
        }).ToList();

        var geoJson = new
        {
            type = "FeatureCollection",
            features = geoJsonFeatures,
            metadata = new
            {
                totalCount,
                page,
                pageSize,
                totalPages = Math.Min(500, (int)Math.Ceiling((double)totalCount / pageSize)),
                hasBbox
            }
        };

        return Results.Ok(geoJson);
    }

    private static async Task<IResult> DeleteFile(
        Guid id,
        IGpkgFileRepository fileRepository)
    {
        var file = await fileRepository.GetByIdAsync(id);
        if (file == null)
        {
            return Results.NotFound(new { error = "Fichier non trouvé" });
        }

        await fileRepository.DeleteAsync(id);

        return Results.Ok(new { message = "Fichier supprimé avec succès" });
    }
}
