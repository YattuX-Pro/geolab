using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services;

public class TopoExport3DModelingService : ITopoExport3DModelingService
{
    private readonly ITopoExport3DModelingRepository _repository;

    public TopoExport3DModelingService(ITopoExport3DModelingRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<TopoExport3DModeling>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<TopoExport3DModeling?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<TopoExport3DModeling>> GetByLayersAsync(IEnumerable<string> layers)
    {
        return await _repository.GetByLayersAsync(layers);
    }

    public async Task<IEnumerable<string>> GetDistinctLayersAsync()
    {
        return await _repository.GetDistinctLayersAsync();
    }
}
