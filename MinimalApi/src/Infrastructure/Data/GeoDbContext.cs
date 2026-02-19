using System;
using System.Collections.Generic;
using Infrastructure.TempModels;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public partial class GeoDbContext : DbContext
{
    public GeoDbContext()
    {
    }

    public GeoDbContext(DbContextOptions<GeoDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<QgisProject> QgisProjects { get; set; }

    public virtual DbSet<Terrain> Terrains { get; set; }

    public virtual DbSet<Topoexport3dModelingEntity> Topoexport3dModelingEntities { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=geo_db;Username=postgres;Password=postgres", x => x.UseNetTopologySuite());

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("postgis");

        modelBuilder.Entity<QgisProject>(entity =>
        {
            entity.HasKey(e => e.Name).HasName("qgis_projects_pkey");
        });

        modelBuilder.Entity<Terrain>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Terrains_pkey");

            entity.HasIndex(e => e.Geometrie, "idx_terrains_geometrie").HasMethod("gist");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.DateAjout).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.DateModification).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Statut).HasDefaultValueSql("'Disponible'::character varying");
        });

        modelBuilder.Entity<Topoexport3dModelingEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("topoexport_3D_modeling — entities_pkey");

            entity.HasIndex(e => e.Geom, "sidx_topoexport_3D_modeling — entities_geom").HasMethod("gist");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
