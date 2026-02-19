using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Infrastructure.Repositories;

public class DXFFeatureRepository : Repository<DXFFeature>, IDXFFeatureRepository
{
    public DXFFeatureRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<DXFFeature>> GetByLayerNameAsync(string layerName)
    {
        return await _dbSet
            .Where(f => f.LayerName == layerName)
            .ToListAsync();
    }

    public async Task<IEnumerable<DXFFeature>> GetBySourceFileAsync(string sourceFile)
    {
        return await _dbSet
            .Where(f => f.SourceFile == sourceFile)
            .ToListAsync();
    }

    public async Task<IEnumerable<DXFFeature>> GetByBoundsAsync(double minX, double minY, double maxX, double maxY)
    {
        var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
        var envelope = geometryFactory.CreatePolygon(new[]
        {
            new Coordinate(minX, minY),
            new Coordinate(maxX, minY),
            new Coordinate(maxX, maxY),
            new Coordinate(minX, maxY),
            new Coordinate(minX, minY)
        });

        return await _dbSet
            .Where(f => f.Geometry.Intersects(envelope))
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetDistinctLayerNamesAsync()
    {
        return await _dbSet
            .Select(f => f.LayerName)
            .Distinct()
            .OrderBy(name => name)
            .ToListAsync();
    }

    public async Task<int> BulkInsertAsync(IEnumerable<DXFFeature> features)
    {
        await _dbSet.AddRangeAsync(features);
        return await _context.SaveChangesAsync();
    }
}
