-- Script SQL pour créer les tables DXF si elles n'existent pas
-- Base de données: geo_db (PostgreSQL avec PostGIS)

-- Activer l'extension PostGIS si elle n'existe pas
CREATE EXTENSION IF NOT EXISTS postgis;

-- Table DXFLayers: Stocke les layers DXF avec leurs données GeoJSON
CREATE TABLE IF NOT EXISTS "DXFLayers" (
    "Id" uuid NOT NULL,
    "LayerName" character varying(200) NOT NULL,
    "GeoJsonData" text NOT NULL,
    "SourceFile" character varying(500),
    "ImportedAt" timestamp with time zone NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_DXFLayers" PRIMARY KEY ("Id")
);

-- Index sur LayerName pour améliorer les performances de recherche
CREATE INDEX IF NOT EXISTS "IX_DXFLayers_LayerName" ON "DXFLayers" ("LayerName");

-- Table dxf_features: Stocke les features DXF individuelles avec géométrie PostGIS
CREATE TABLE IF NOT EXISTS dxf_features (
    "Id" uuid NOT NULL,
    "LayerName" character varying(200) NOT NULL,
    "Geometry" geometry NOT NULL,
    "SourceFile" character varying(500),
    "Color" integer,
    "LineType" character varying(100),
    "Handle" character varying(50),
    "Text" text,
    "EntityType" character varying(50),
    "ImportedAt" timestamp with time zone NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_dxf_features" PRIMARY KEY ("Id")
);

-- Index spatial GIST sur la géométrie pour les requêtes spatiales
CREATE INDEX IF NOT EXISTS "IX_dxf_features_Geometry" ON dxf_features USING gist ("Geometry");

-- Index sur LayerName pour filtrer par layer
CREATE INDEX IF NOT EXISTS "IX_dxf_features_LayerName" ON dxf_features ("LayerName");

-- Commentaires sur les tables
COMMENT ON TABLE "DXFLayers" IS 'Stocke les layers DXF complets au format GeoJSON';
COMMENT ON TABLE dxf_features IS 'Stocke les features DXF individuelles avec géométrie PostGIS native';

COMMENT ON COLUMN "DXFLayers"."LayerName" IS 'Nom du layer DXF (ex: 0, Contours, Batiments)';
COMMENT ON COLUMN "DXFLayers"."GeoJsonData" IS 'Données GeoJSON complètes du layer (FeatureCollection)';
COMMENT ON COLUMN "DXFLayers"."SourceFile" IS 'Chemin du fichier DXF source';

COMMENT ON COLUMN dxf_features."Geometry" IS 'Géométrie PostGIS (Point, LineString, Polygon)';
COMMENT ON COLUMN dxf_features."EntityType" IS 'Type d''entité DXF (POINT, LINE, LWPOLYLINE, etc.)';
COMMENT ON COLUMN dxf_features."Handle" IS 'Handle unique de l''entité dans le fichier DXF';

-- Afficher un message de confirmation
DO $$
BEGIN
    RAISE NOTICE 'Tables DXF créées avec succès!';
    RAISE NOTICE '- DXFLayers: Pour stocker les GeoJSON par layer';
    RAISE NOTICE '- dxf_features: Pour stocker les features avec géométrie PostGIS';
END $$;
