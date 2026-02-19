using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class GpkgFeatureRepository : Repository<GpkgFeature>, IGpkgFeatureRepository
{
    public GpkgFeatureRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<GpkgFeature>> GetByLayerIdAsync(Guid layerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(f => f.LayerId == layerId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<GpkgFeature>> GetByFileIdAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(f => f.Layer)
            .Where(f => f.Layer.GpkgFileId == fileId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<GpkgFeature> features, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(features, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteByLayerIdAsync(Guid layerId, CancellationToken cancellationToken = default)
    {
        var features = await _dbSet.Where(f => f.LayerId == layerId).ToListAsync(cancellationToken);
        _dbSet.RemoveRange(features);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<(IEnumerable<GpkgFeature> Features, int TotalCount)> GetByFileIdWithBboxAsync(
        Guid fileId, 
        double? minLng, double? minLat, double? maxLng, double? maxLat,
        int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(f => f.Layer)
            .Where(f => f.Layer.GpkgFileId == fileId);

        // Filtrage spatial avec PostGIS si bbox fourni
        if (minLng.HasValue && minLat.HasValue && maxLng.HasValue && maxLat.HasValue)
        {
            var envelope = new NetTopologySuite.Geometries.Envelope(
                minLng.Value, maxLng.Value, minLat.Value, maxLat.Value);
            var geometryFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            var bboxPolygon = geometryFactory.ToGeometry(envelope);
            
            query = query.Where(f => f.Geometry.Intersects(bboxPolygon));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        
        var features = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (features, totalCount);
    }
}
