using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDXFFeatureTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dxf_features",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LayerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Geometry = table.Column<Geometry>(type: "geometry", nullable: false),
                    SourceFile = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Color = table.Column<int>(type: "integer", nullable: true),
                    LineType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Handle = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Text = table.Column<string>(type: "text", nullable: true),
                    EntityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ImportedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dxf_features", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DXFLayers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LayerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    GeoJsonData = table.Column<string>(type: "text", nullable: false),
                    SourceFile = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ImportedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DXFLayers", x => x.Id);
                });

            // Table topoexport_3D_modeling already exists in database - skipping creation

            migrationBuilder.CreateIndex(
                name: "IX_dxf_features_Geometry",
                table: "dxf_features",
                column: "Geometry")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "IX_dxf_features_LayerName",
                table: "dxf_features",
                column: "LayerName");

            migrationBuilder.CreateIndex(
                name: "IX_DXFLayers_LayerName",
                table: "DXFLayers",
                column: "LayerName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dxf_features");

            migrationBuilder.DropTable(
                name: "DXFLayers");

            // Table topoexport_3D_modeling managed externally - not dropping
        }
    }
}
