using NetTopologySuite.Geometries;

namespace Domain.Entities;

public class TopoExport3DModeling
{
    public int Id { get; set; }
    public Geometry Geom { get; set; } = null!;
    public string? Layer { get; set; }
    public int? PaperSpace { get; set; }
    public string? SubClasses { get; set; }
    public string? Linetype { get; set; }
    public string? EntityHandle { get; set; }
    public string? Text { get; set; }
}
