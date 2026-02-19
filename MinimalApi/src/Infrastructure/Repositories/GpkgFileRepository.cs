using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class GpkgFileRepository : Repository<GpkgFile>, IGpkgFileRepository
{
    public GpkgFileRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<GpkgFile?> GetByIdWithLayersAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(f => f.Layers)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<GpkgFile?> GetByIdWithAllDataAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(f => f.Layers)
                .ThenInclude(l => l.Features)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<GpkgFile>> GetAllWithLayersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(f => f.Layers)
            .OrderByDescending(f => f.UploadDate)
            .ToListAsync(cancellationToken);
    }
}
