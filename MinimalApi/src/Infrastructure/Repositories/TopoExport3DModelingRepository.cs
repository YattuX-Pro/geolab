using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class TopoExport3DModelingRepository : ITopoExport3DModelingRepository
{
    private readonly ApplicationDbContext _context;

    public TopoExport3DModelingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TopoExport3DModeling>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.TopoExport3DModelings.ToListAsync(cancellationToken);
    }

    public async Task<TopoExport3DModeling?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.TopoExport3DModelings.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<TopoExport3DModeling>> GetByLayersAsync(IEnumerable<string> layers, CancellationToken cancellationToken = default)
    {
        var layerList = layers.ToList();
        if (!layerList.Any())
        {
            return await GetAllAsync(cancellationToken);
        }

        return await _context.TopoExport3DModelings
            .Where(t => t.Layer != null && layerList.Contains(t.Layer))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<string>> GetDistinctLayersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.TopoExport3DModelings
            .Where(t => t.Layer != null)
            .Select(t => t.Layer!)
            .Distinct()
            .OrderBy(l => l)
            .ToListAsync(cancellationToken);
    }
}
