using Domain.Entities;

namespace Application.Interfaces;

public interface ITerrainService
{
    Task<IEnumerable<Terrain>> GetAllAsync();
    Task<Terrain?> GetByIdAsync(Guid id);
    Task<IEnumerable<Terrain>> SearchAsync(string? query, string? commune, string? quartier);
    Task<IEnumerable<Terrain>> GetByBoundsAsync(double minLng, double minLat, double maxLng, double maxLat);
    Task<Terrain> CreateAsync(Terrain terrain);
    Task<Terrain?> UpdateAsync(Terrain terrain);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<string>> GetSuggestionsAsync(string query);
}
