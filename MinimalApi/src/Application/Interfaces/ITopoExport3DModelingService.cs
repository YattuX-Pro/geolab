using Domain.Entities;

namespace Application.Interfaces;

public interface ITopoExport3DModelingService
{
    Task<IEnumerable<TopoExport3DModeling>> GetAllAsync();
    Task<TopoExport3DModeling?> GetByIdAsync(int id);
    Task<IEnumerable<TopoExport3DModeling>> GetByLayersAsync(IEnumerable<string> layers);
    Task<IEnumerable<string>> GetDistinctLayersAsync();
}
