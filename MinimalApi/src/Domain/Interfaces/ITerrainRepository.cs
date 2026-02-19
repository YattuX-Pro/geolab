using Domain.Entities;

namespace Domain.Interfaces;

public interface ITerrainRepository : IRepository<Terrain>
{
    Task<IEnumerable<Terrain>> SearchAsync(string? query, string? commune, string? quartier);
    Task<IEnumerable<Terrain>> GetByBoundsAsync(double minLng, double minLat, double maxLng, double maxLat);
    Task<IEnumerable<string>> GetSuggestionsAsync(string query);
}
