using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Infrastructure.TempModels;

[Index("Commune", Name = "idx_terrains_commune")]
[Index("Quartier", Name = "idx_terrains_quartier")]
public partial class Terrain
{
    [Key]
    public Guid Id { get; set; }

    [StringLength(200)]
    public string Titre { get; set; } = null!;

    public string? Description { get; set; }

    [StringLength(100)]
    public string Quartier { get; set; } = null!;

    [StringLength(100)]
    public string Commune { get; set; } = null!;

    [Precision(18, 2)]
    public decimal Surface { get; set; }

    [Precision(18, 2)]
    public decimal Prix { get; set; }

    [Precision(18, 2)]
    public decimal? PrixParM2 { get; set; }

    [StringLength(50)]
    public string? Statut { get; set; }

    [StringLength(50)]
    public string? TypeTerrain { get; set; }

    [Column(TypeName = "geometry(Polygon,4326)")]
    public Polygon Geometrie { get; set; } = null!;

    [StringLength(100)]
    public string? ContactNom { get; set; }

    [StringLength(20)]
    public string? ContactTelephone { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? DateAjout { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? DateModification { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
