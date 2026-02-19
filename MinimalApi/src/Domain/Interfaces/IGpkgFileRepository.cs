using Domain.Entities;

namespace Domain.Interfaces;

public interface IGpkgFileRepository : IRepository<GpkgFile>
{
    Task<GpkgFile?> GetByIdWithLayersAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GpkgFile?> GetByIdWithAllDataAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<GpkgFile>> GetAllWithLayersAsync(CancellationToken cancellationToken = default);
}
