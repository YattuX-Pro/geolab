using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services;

public class TerrainService : ITerrainService
{
    private readonly ITerrainRepository _terrainRepository;

    public TerrainService(ITerrainRepository terrainRepository)
    {
        _terrainRepository = terrainRepository;
    }

    public async Task<IEnumerable<Terrain>> GetAllAsync()
    {
        return await _terrainRepository.GetAllAsync();
    }

    public async Task<Terrain?> GetByIdAsync(Guid id)
    {
        return await _terrainRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Terrain>> SearchAsync(string? query, string? commune, string? quartier)
    {
        return await _terrainRepository.SearchAsync(query, commune, quartier);
    }

    public async Task<IEnumerable<Terrain>> GetByBoundsAsync(double minLng, double minLat, double maxLng, double maxLat)
    {
        return await _terrainRepository.GetByBoundsAsync(minLng, minLat, maxLng, maxLat);
    }

    public async Task<Terrain> CreateAsync(Terrain terrain)
    {
        return await _terrainRepository.AddAsync(terrain);
    }

    public async Task<Terrain?> UpdateAsync(Terrain terrain)
    {
        terrain.DateModification = DateTime.UtcNow;
        return await _terrainRepository.UpdateAsync(terrain);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _terrainRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<string>> GetSuggestionsAsync(string query)
    {
        return await _terrainRepository.GetSuggestionsAsync(query);
    }
}
