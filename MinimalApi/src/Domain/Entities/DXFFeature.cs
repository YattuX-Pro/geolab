using NetTopologySuite.Geometries;

namespace Domain.Entities;

public class DXFFeature : BaseEntity
{
    public string LayerName { get; set; } = string.Empty;
    public Geometry Geometry { get; set; } = null!;
    public string? SourceFile { get; set; }
    public int? Color { get; set; }
    public string? LineType { get; set; }
    public string? Handle { get; set; }
    public string? Text { get; set; }
    public string? EntityType { get; set; }
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
}
