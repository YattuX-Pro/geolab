using NetTopologySuite.Geometries;

namespace Domain.Entities;

public class GpkgFile : BaseEntity
{
    public string FileName { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; }
    public string? Description { get; set; }
    
    public virtual ICollection<GpkgLayer> Layers { get; set; } = new List<GpkgLayer>();
}
