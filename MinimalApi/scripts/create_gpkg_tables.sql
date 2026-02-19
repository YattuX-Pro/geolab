-- Script de création des tables pour GeoPackage
-- Exécuter ce script sur votre base PostgreSQL avec PostGIS

-- Activer PostGIS si ce n'est pas déjà fait
CREATE EXTENSION IF NOT EXISTS postgis;

-- Table des fichiers GeoPackage uploadés
CREATE TABLE IF NOT EXISTS gpkg_files (
    "Id" UUID PRIMARY KEY,
    "FileName" VARCHAR(500) NOT NULL,
    "UploadDate" TIMESTAMP NOT NULL,
    "Description" VARCHAR(2000),
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP
);

-- Table des couches (layers) extraites des fichiers GeoPackage
CREATE TABLE IF NOT EXISTS gpkg_layers (
    "Id" UUID PRIMARY KEY,
    "GpkgFileId" UUID NOT NULL REFERENCES gpkg_files("Id") ON DELETE CASCADE,
    "LayerName" VARCHAR(200) NOT NULL,
    "GeometryType" VARCHAR(50) NOT NULL,
    "FeatureCount" INTEGER NOT NULL DEFAULT 0,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP
);

-- Index sur la clé étrangère
CREATE INDEX IF NOT EXISTS idx_gpkg_layers_file_id ON gpkg_layers("GpkgFileId");

-- Table des features (entités géographiques)
CREATE TABLE IF NOT EXISTS gpkg_features (
    "Id" UUID PRIMARY KEY,
    "LayerId" UUID NOT NULL REFERENCES gpkg_layers("Id") ON DELETE CASCADE,
    "Geometry" geometry NOT NULL,
    "Properties" JSONB NOT NULL DEFAULT '{}',
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP
);

-- Index sur la clé étrangère
CREATE INDEX IF NOT EXISTS idx_gpkg_features_layer_id ON gpkg_features("LayerId");

-- Index spatial sur la géométrie
CREATE INDEX IF NOT EXISTS idx_gpkg_features_geometry ON gpkg_features USING GIST ("Geometry");

-- Commentaires sur les tables
COMMENT ON TABLE gpkg_files IS 'Fichiers GeoPackage uploadés';
COMMENT ON TABLE gpkg_layers IS 'Couches vectorielles extraites des fichiers GeoPackage';
COMMENT ON TABLE gpkg_features IS 'Features géographiques avec géométrie et propriétés';
