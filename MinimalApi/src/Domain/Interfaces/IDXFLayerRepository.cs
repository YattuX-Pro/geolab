using Domain.Entities;

namespace Domain.Interfaces;

public interface IDXFLayerRepository : IRepository<DXFLayer>
{
    Task<IEnumerable<DXFLayer>> GetBySourceFileAsync(string sourceFile);
    Task<DXFLayer?> GetByLayerNameAsync(string layerName);
}
