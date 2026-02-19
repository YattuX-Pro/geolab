using NetTopologySuite.Geometries;

namespace Domain.Entities;

public class Terrain : BaseEntity
{
    public string Titre { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Quartier { get; set; } = string.Empty;
    public string Commune { get; set; } = string.Empty;
    public decimal Surface { get; set; }
    public decimal Prix { get; set; }
    public decimal? PrixParM2 { get; set; }
    public string Statut { get; set; } = "Disponible";
    public string? TypeTerrain { get; set; }
    public Polygon Geometrie { get; set; } = null!;
    public string? ContactNom { get; set; }
    public string? ContactTelephone { get; set; }
    public DateTime DateAjout { get; set; } = DateTime.UtcNow;
    public DateTime DateModification { get; set; } = DateTime.UtcNow;
}
