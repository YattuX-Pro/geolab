using Domain.Entities;

namespace Domain.Interfaces;

public interface IGpkgFeatureRepository : IRepository<GpkgFeature>
{
    Task<IEnumerable<GpkgFeature>> GetByLayerIdAsync(Guid layerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<GpkgFeature>> GetByFileIdAsync(Guid fileId, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<GpkgFeature> features, CancellationToken cancellationToken = default);
    Task DeleteByLayerIdAsync(Guid layerId, CancellationToken cancellationToken = default);
    
    Task<(IEnumerable<GpkgFeature> Features, int TotalCount)> GetByFileIdWithBboxAsync(
        Guid fileId, 
        double? minLng, double? minLat, double? maxLng, double? maxLat,
        int page, int pageSize,
        CancellationToken cancellationToken = default);
}
