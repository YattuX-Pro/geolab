using NetTopologySuite.Geometries;

namespace Domain.Entities;

public class DXFLayer : BaseEntity
{
    public string LayerName { get; set; } = string.Empty;
    public string GeoJsonData { get; set; } = string.Empty;
    public string? SourceFile { get; set; }
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
}
