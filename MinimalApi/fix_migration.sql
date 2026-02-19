-- Marquer la migration InitialCreate comme appliquée
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") 
VALUES ('20260118184716_InitialCreate', '10.0.2') 
ON CONFLICT DO NOTHING;

-- Créer la table dxf_features
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

-- Créer les index
CREATE INDEX IF NOT EXISTS "IX_dxf_features_LayerName" ON dxf_features ("LayerName");
CREATE INDEX IF NOT EXISTS "IX_dxf_features_Geometry" ON dxf_features USING GIST ("Geometry");

-- Créer la table DXFLayers si elle n'existe pas
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

CREATE INDEX IF NOT EXISTS "IX_DXFLayers_LayerName" ON "DXFLayers" ("LayerName");

-- Marquer la nouvelle migration comme appliquée
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") 
VALUES ('20260209074003_AddDXFFeatureTable', '10.0.2') 
ON CONFLICT DO NOTHING;
