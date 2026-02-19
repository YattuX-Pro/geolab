using Domain.Entities;

namespace Domain.Interfaces;

public interface ITopoExport3DModelingRepository
{
    Task<IEnumerable<TopoExport3DModeling>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TopoExport3DModeling?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TopoExport3DModeling>> GetByLayersAsync(IEnumerable<string> layers, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetDistinctLayersAsync(CancellationToken cancellationToken = default);
}
