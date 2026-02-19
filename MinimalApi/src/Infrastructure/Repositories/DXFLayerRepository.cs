using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class DXFLayerRepository : Repository<DXFLayer>, IDXFLayerRepository
{
    public DXFLayerRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<DXFLayer>> GetBySourceFileAsync(string sourceFile)
    {
        return await _dbSet
            .Where(l => l.SourceFile == sourceFile)
            .ToListAsync();
    }

    public async Task<DXFLayer?> GetByLayerNameAsync(string layerName)
    {
        return await _dbSet
            .FirstOrDefaultAsync(l => l.LayerName == layerName);
    }
}
