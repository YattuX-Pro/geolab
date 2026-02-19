using Domain.Entities;

namespace Domain.Interfaces;

public interface IGpkgLayerRepository : IRepository<GpkgLayer>
{
    Task<IEnumerable<GpkgLayer>> GetByFileIdAsync(Guid fileId, CancellationToken cancellationToken = default);
    Task<GpkgLayer?> GetByIdWithFeaturesAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<GpkgLayer> layers, CancellationToken cancellationToken = default);
}
