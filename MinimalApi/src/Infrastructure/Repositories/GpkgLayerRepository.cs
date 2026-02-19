using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class GpkgLayerRepository : Repository<GpkgLayer>, IGpkgLayerRepository
{
    public GpkgLayerRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<GpkgLayer>> GetByFileIdAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(l => l.GpkgFileId == fileId)
            .ToListAsync(cancellationToken);
    }

    public async Task<GpkgLayer?> GetByIdWithFeaturesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(l => l.Features)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<GpkgLayer> layers, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(layers, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
