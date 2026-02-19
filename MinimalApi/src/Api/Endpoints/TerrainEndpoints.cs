using Application.Interfaces;
using Domain.Entities;
using NetTopologySuite.Geometries;

namespace Api.Endpoints;

public static class TerrainEndpoints
{
    public static void MapTerrainEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/terrains").WithTags("Terrains");

        group.MapGet("/", async (ITerrainService terrainService) =>
        {
            var terrains = await terrainService.GetAllAsync();
            return Results.Ok(terrains.Select(ToDto));
        });

        group.MapGet("/{id:guid}", async (Guid id, ITerrainService terrainService) =>
        {
            var terrain = await terrainService.GetByIdAsync(id);
            return terrain is null ? Results.NotFound() : Results.Ok(ToDto(terrain));
        });

        group.MapGet("/suggestions", async (
            string q,
            ITerrainService terrainService) =>
        {
            var suggestions = await terrainService.GetSuggestionsAsync(q);
            return Results.Ok(suggestions);
        });

        group.MapGet("/search", async (
            string? q,
            string? commune,
            string? quartier,
            ITerrainService terrainService) =>
        {
            var terrains = await terrainService.SearchAsync(q, commune, quartier);
            return Results.Ok(terrains.Select(ToDto));
        });

        group.MapGet("/bounds", async (
            double minLng,
            double minLat,
            double maxLng,
            double maxLat,
            ITerrainService terrainService) =>
        {
            var terrains = await terrainService.GetByBoundsAsync(minLng, minLat, maxLng, maxLat);
            return Results.Ok(terrains.Select(ToDto));
        });

        group.MapGet("/geojson", async (ITerrainService terrainService) =>
        {
            var terrains = await terrainService.GetAllAsync();
            var features = terrains.Select(t => new
            {
                type = "Feature",
                properties = new
                {
                    id = t.Id,
                    titre = t.Titre,
                    description = t.Description,
                    quartier = t.Quartier,
                    commune = t.Commune,
                    surface = t.Surface,
                    prix = t.Prix,
                    prixParM2 = t.PrixParM2,
                    statut = t.Statut,
                    typeTerrain = t.TypeTerrain,
                    contactNom = t.ContactNom,
                    contactTelephone = t.ContactTelephone
                },
                geometry = new
                {
                    type = "Polygon",
                    coordinates = GetCoordinates(t.Geometrie)
                }
            });

            return Results.Ok(new
            {
                type = "FeatureCollection",
                features
            });
        });

        group.MapGet("/geojson/search", async (
            string? q,
            string? commune,
            string? quartier,
            ITerrainService terrainService) =>
        {
            var terrains = await terrainService.SearchAsync(q, commune, quartier);
            var features = terrains.Select(t => new
            {
                type = "Feature",
                properties = new
                {
                    id = t.Id,
                    titre = t.Titre,
                    description = t.Description,
                    quartier = t.Quartier,
                    commune = t.Commune,
                    surface = t.Surface,
                    prix = t.Prix,
                    prixParM2 = t.PrixParM2,
                    statut = t.Statut,
                    typeTerrain = t.TypeTerrain,
                    contactNom = t.ContactNom,
                    contactTelephone = t.ContactTelephone
                },
                geometry = new
                {
                    type = "Polygon",
                    coordinates = GetCoordinates(t.Geometrie)
                }
            });

            return Results.Ok(new
            {
                type = "FeatureCollection",
                features
            });
        });

        group.MapPost("/", async (CreateTerrainRequest request, ITerrainService terrainService) =>
        {
            var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
            var coordinates = request.Coordinates
                .Select(coord => new Coordinate(coord[0], coord[1]))
                .ToArray();
            
            var polygon = geometryFactory.CreatePolygon(coordinates);

            var terrain = new Terrain
            {
                Id = Guid.NewGuid(),
                Titre = request.Titre,
                Description = request.Description,
                Quartier = request.Quartier,
                Commune = request.Commune,
                Surface = request.Surface,
                Prix = request.Prix,
                PrixParM2 = request.PrixParM2,
                Statut = request.Statut ?? "Disponible",
                TypeTerrain = request.TypeTerrain,
                Geometrie = polygon,
                ContactNom = request.ContactNom,
                ContactTelephone = request.ContactTelephone,
                DateAjout = DateTime.UtcNow,
                DateModification = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await terrainService.CreateAsync(terrain);
            return Results.Created($"/api/terrains/{terrain.Id}", ToDto(terrain));
        });
    }

    public record CreateTerrainRequest(
        string Titre,
        string? Description,
        string Quartier,
        string Commune,
        decimal Surface,
        decimal Prix,
        decimal? PrixParM2,
        string? Statut,
        string? TypeTerrain,
        double[][] Coordinates,
        string? ContactNom,
        string? ContactTelephone
    );

    private static object ToDto(Terrain t) => new
    {
        t.Id,
        t.Titre,
        t.Description,
        t.Quartier,
        t.Commune,
        t.Surface,
        t.Prix,
        t.PrixParM2,
        t.Statut,
        t.TypeTerrain,
        t.ContactNom,
        t.ContactTelephone,
        t.DateAjout,
        t.DateModification,
        Coordinates = GetCoordinates(t.Geometrie)
    };

    private static double[][][] GetCoordinates(Polygon polygon)
    {
        var rings = new List<double[][]>();
        
        var exteriorRing = polygon.ExteriorRing.Coordinates
            .Select(c => new[] { c.X, c.Y })
            .ToArray();
        rings.Add(exteriorRing);

        for (int i = 0; i < polygon.NumInteriorRings; i++)
        {
            var interiorRing = polygon.GetInteriorRingN(i).Coordinates
                .Select(c => new[] { c.X, c.Y })
                .ToArray();
            rings.Add(interiorRing);
        }

        return rings.ToArray();
    }
}
