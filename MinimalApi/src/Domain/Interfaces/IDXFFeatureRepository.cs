using Domain.Entities;
using NetTopologySuite.Geometries;

namespace Domain.Interfaces;

public interface IDXFFeatureRepository : IRepository<DXFFeature>
{
    Task<IEnumerable<DXFFeature>> GetByLayerNameAsync(string layerName);
    Task<IEnumerable<DXFFeature>> GetBySourceFileAsync(string sourceFile);
    Task<IEnumerable<DXFFeature>> GetByBoundsAsync(double minX, double minY, double maxX, double maxY);
    Task<IEnumerable<string>> GetDistinctLayerNamesAsync();
    Task<int> BulkInsertAsync(IEnumerable<DXFFeature> features);
}
