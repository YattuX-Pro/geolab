using NetTopologySuite.Geometries;

namespace Domain.Entities;

public class GpkgFeature : BaseEntity
{
    public Guid LayerId { get; set; }
    public Geometry Geometry { get; set; } = null!;
    public string Properties { get; set; } = "{}";
    
    public virtual GpkgLayer Layer { get; set; } = null!;
}
