namespace Domain.Entities;

public class GpkgLayer : BaseEntity
{
    public Guid GpkgFileId { get; set; }
    public string LayerName { get; set; } = string.Empty;
    public string GeometryType { get; set; } = string.Empty;
    public int FeatureCount { get; set; }
    
    public virtual GpkgFile GpkgFile { get; set; } = null!;
    public virtual ICollection<GpkgFeature> Features { get; set; } = new List<GpkgFeature>();
}
