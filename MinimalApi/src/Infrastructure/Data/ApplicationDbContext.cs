using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Terrain> Terrains => Set<Terrain>();
    public DbSet<TopoExport3DModeling> TopoExport3DModelings => Set<TopoExport3DModeling>();
    public DbSet<DXFLayer> DXFLayers => Set<DXFLayer>();
    public DbSet<DXFFeature> DXFFeatures => Set<DXFFeature>();
    public DbSet<GpkgFile> GpkgFiles => Set<GpkgFile>();
    public DbSet<GpkgLayer> GpkgLayers => Set<GpkgLayer>();
    public DbSet<GpkgFeature> GpkgFeatures => Set<GpkgFeature>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Price).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Terrain>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Titre).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Quartier).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Commune).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Surface).HasPrecision(18, 2);
            entity.Property(e => e.Prix).HasPrecision(18, 2);
            entity.Property(e => e.PrixParM2).HasPrecision(18, 2);
            entity.Property(e => e.Geometrie).HasColumnType("geometry(Polygon, 4326)");
        });

        modelBuilder.Entity<TopoExport3DModeling>(entity =>
        {
            entity.ToTable("topoexport_3D_modeling — entities");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Geom).HasColumnName("geom");
            entity.Property(e => e.Layer).HasColumnName("Layer");
            entity.Property(e => e.PaperSpace).HasColumnName("PaperSpace");
            entity.Property(e => e.SubClasses).HasColumnName("SubClasses");
            entity.Property(e => e.Linetype).HasColumnName("Linetype");
            entity.Property(e => e.EntityHandle).HasColumnName("EntityHandle");
            entity.Property(e => e.Text).HasColumnName("Text");
        });

        modelBuilder.Entity<DXFLayer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LayerName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.GeoJsonData).IsRequired();
            entity.Property(e => e.SourceFile).HasMaxLength(500);
            entity.Property(e => e.ImportedAt).IsRequired();
            entity.HasIndex(e => e.LayerName);
        });

        modelBuilder.Entity<DXFFeature>(entity =>
        {
            entity.ToTable("dxf_features");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LayerName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Geometry).HasColumnType("geometry").IsRequired();
            entity.Property(e => e.SourceFile).HasMaxLength(500);
            entity.Property(e => e.Color);
            entity.Property(e => e.LineType).HasMaxLength(100);
            entity.Property(e => e.Handle).HasMaxLength(50);
            entity.Property(e => e.Text);
            entity.Property(e => e.EntityType).HasMaxLength(50);
            entity.Property(e => e.ImportedAt).IsRequired();
            
            entity.HasIndex(e => e.LayerName);
            entity.HasIndex(e => e.Geometry).HasMethod("gist");
        });

        modelBuilder.Entity<GpkgFile>(entity =>
        {
            entity.ToTable("gpkg_files");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.UploadDate).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.HasMany(e => e.Layers)
                  .WithOne(l => l.GpkgFile)
                  .HasForeignKey(l => l.GpkgFileId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GpkgLayer>(entity =>
        {
            entity.ToTable("gpkg_layers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LayerName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.GeometryType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.FeatureCount).IsRequired();
            entity.HasMany(e => e.Features)
                  .WithOne(f => f.Layer)
                  .HasForeignKey(f => f.LayerId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.GpkgFileId);
        });

        modelBuilder.Entity<GpkgFeature>(entity =>
        {
            entity.ToTable("gpkg_features");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Geometry).HasColumnType("geometry").IsRequired();
            entity.Property(e => e.Properties).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(e => e.LayerId);
            entity.HasIndex(e => e.Geometry).HasMethod("gist");
        });
    }
}
