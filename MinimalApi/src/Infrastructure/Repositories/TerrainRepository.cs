using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Infrastructure.Repositories;

public class TerrainRepository : Repository<Terrain>, ITerrainRepository
{
    public TerrainRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Terrain>> SearchAsync(string? query, string? commune, string? quartier)
    {
        var queryable = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var lowerQuery = query.ToLower();
            queryable = queryable.Where(t => 
                t.Titre.ToLower().Contains(lowerQuery) ||
                t.Quartier.ToLower().Contains(lowerQuery) ||
                t.Commune.ToLower().Contains(lowerQuery) ||
                (t.Description != null && t.Description.ToLower().Contains(lowerQuery)));
        }

        if (!string.IsNullOrWhiteSpace(commune))
        {
            queryable = queryable.Where(t => t.Commune.ToLower() == commune.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(quartier))
        {
            queryable = queryable.Where(t => t.Quartier.ToLower() == quartier.ToLower());
        }

        return await queryable.ToListAsync();
    }

    public async Task<IEnumerable<Terrain>> GetByBoundsAsync(double minLng, double minLat, double maxLng, double maxLat)
    {
        var envelope = new Envelope(minLng, maxLng, minLat, maxLat);
        var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
        var boundingBox = geometryFactory.ToGeometry(envelope);

        return await _dbSet
            .Where(t => t.Geometrie.Intersects(boundingBox))
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetSuggestionsAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Enumerable.Empty<string>();

        var lowerQuery = query.ToLower();
        var suggestions = new List<string>();

        var titres = await _dbSet
            .Where(t => t.Titre.ToLower().Contains(lowerQuery))
            .Select(t => t.Titre)
            .Distinct()
            .Take(5)
            .ToListAsync();
        suggestions.AddRange(titres);

        var quartiers = await _dbSet
            .Where(t => t.Quartier.ToLower().Contains(lowerQuery))
            .Select(t => t.Quartier)
            .Distinct()
            .Take(5)
            .ToListAsync();
        suggestions.AddRange(quartiers);

        var communes = await _dbSet
            .Where(t => t.Commune.ToLower().Contains(lowerQuery))
            .Select(t => t.Commune)
            .Distinct()
            .Take(5)
            .ToListAsync();
        suggestions.AddRange(communes);

        return suggestions.Distinct().Take(10);
    }
}
