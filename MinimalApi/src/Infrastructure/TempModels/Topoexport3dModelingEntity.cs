using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Infrastructure.TempModels;

[Table("topoexport_3D_modeling — entities")]
public partial class Topoexport3dModelingEntity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("geom", TypeName = "geometry(LineStringZ,4326)")]
    public LineString? Geom { get; set; }

    [Column(TypeName = "character varying")]
    public string? Layer { get; set; }

    public bool? PaperSpace { get; set; }

    [Column(TypeName = "character varying")]
    public string? SubClasses { get; set; }

    [Column(TypeName = "character varying")]
    public string? Linetype { get; set; }

    [Column(TypeName = "character varying")]
    public string? EntityHandle { get; set; }

    [Column(TypeName = "character varying")]
    public string? Text { get; set; }
}
