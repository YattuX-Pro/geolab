using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.TempModels;

[Table("qgis_projects")]
public partial class QgisProject
{
    [Key]
    [Column("name")]
    public string Name { get; set; } = null!;

    [Column("metadata", TypeName = "jsonb")]
    public string? Metadata { get; set; }

    [Column("content")]
    public byte[]? Content { get; set; }
}
