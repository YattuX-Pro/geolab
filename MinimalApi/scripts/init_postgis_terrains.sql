-- Enable PostGIS extension
CREATE EXTENSION IF NOT EXISTS postgis;

-- Create Terrains table
CREATE TABLE IF NOT EXISTS "Terrains" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "Titre" VARCHAR(200) NOT NULL,
    "Description" TEXT,
    "Quartier" VARCHAR(100) NOT NULL,
    "Commune" VARCHAR(100) NOT NULL,
    "Surface" DECIMAL(18,2) NOT NULL,
    "Prix" DECIMAL(18,2) NOT NULL,
    "PrixParM2" DECIMAL(18,2),
    "Statut" VARCHAR(50) DEFAULT 'Disponible',
    "TypeTerrain" VARCHAR(50),
    "Geometrie" geometry(Polygon, 4326) NOT NULL,
    "ContactNom" VARCHAR(100),
    "ContactTelephone" VARCHAR(20),
    "DateAjout" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "DateModification" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create spatial index
CREATE INDEX IF NOT EXISTS idx_terrains_geometrie ON "Terrains" USING GIST ("Geometrie");

-- Create text search index
CREATE INDEX IF NOT EXISTS idx_terrains_quartier ON "Terrains" ("Quartier");
CREATE INDEX IF NOT EXISTS idx_terrains_commune ON "Terrains" ("Commune");

-- Insert test data for Conakry land parcels

-- Kaloum (Centre-ville)
INSERT INTO "Terrains" ("Titre", "Description", "Quartier", "Commune", "Surface", "Prix", "PrixParM2", "Statut", "TypeTerrain", "Geometrie", "ContactNom", "ContactTelephone") VALUES
('Terrain commercial Kaloum Centre', 'Terrain idéal pour commerce ou bureau, proche du port', 'Tombo', 'Kaloum', 450.00, 150000000, 333333, 'Disponible', 'Commercial',
ST_GeomFromText('POLYGON((-13.7120 9.5090, -13.7115 9.5090, -13.7115 9.5085, -13.7120 9.5085, -13.7120 9.5090))', 4326),
'Mamadou Diallo', '+224 622 11 22 33'),
('Parcelle résidentielle Boulbinet', 'Belle parcelle avec vue sur mer, quartier calme', 'Boulbinet', 'Kaloum', 320.00, 120000000, 375000, 'Disponible', 'Résidentiel',
ST_GeomFromText('POLYGON((-13.7050 9.5120, -13.7045 9.5120, -13.7045 9.5115, -13.7050 9.5115, -13.7050 9.5120))', 4326),
'Fatoumata Camara', '+224 628 44 55 66');

-- Ratoma
INSERT INTO "Terrains" ("Titre", "Description", "Quartier", "Commune", "Surface", "Prix", "PrixParM2", "Statut", "TypeTerrain", "Geometrie", "ContactNom", "ContactTelephone") VALUES
('Grand terrain Kipé', 'Terrain spacieux dans quartier résidentiel en développement', 'Kipé', 'Ratoma', 800.00, 200000000, 250000, 'Disponible', 'Résidentiel',
ST_GeomFromText('POLYGON((-13.6280 9.5650, -13.6270 9.5650, -13.6270 9.5640, -13.6280 9.5640, -13.6280 9.5650))', 4326),
'Ibrahima Sow', '+224 621 77 88 99'),
('Terrain Nongo', 'Parcelle plate, accès facile, électricité disponible', 'Nongo', 'Ratoma', 500.00, 125000000, 250000, 'Disponible', 'Résidentiel',
ST_GeomFromText('POLYGON((-13.6150 9.5720, -13.6145 9.5720, -13.6145 9.5715, -13.6150 9.5715, -13.6150 9.5720))', 4326),
'Aissatou Barry', '+224 625 00 11 22'),
('Parcelle Kaporo Rails', 'Terrain bien situé près des rails, idéal pour investissement', 'Kaporo', 'Ratoma', 600.00, 180000000, 300000, 'Réservé', 'Mixte',
ST_GeomFromText('POLYGON((-13.6320 9.5580, -13.6310 9.5580, -13.6310 9.5570, -13.6320 9.5570, -13.6320 9.5580))', 4326),
'Ousmane Bah', '+224 622 33 44 55');

-- Dixinn
INSERT INTO "Terrains" ("Titre", "Description", "Quartier", "Commune", "Surface", "Prix", "PrixParM2", "Statut", "TypeTerrain", "Geometrie", "ContactNom", "ContactTelephone") VALUES
('Terrain Dixinn Centre', 'Emplacement stratégique près de l''université', 'Dixinn Centre', 'Dixinn', 400.00, 160000000, 400000, 'Disponible', 'Commercial',
ST_GeomFromText('POLYGON((-13.6680 9.5350, -13.6675 9.5350, -13.6675 9.5345, -13.6680 9.5345, -13.6680 9.5350))', 4326),
'Mariama Sylla', '+224 628 66 77 88'),
('Parcelle Belle Vue', 'Vue panoramique sur la ville, quartier sécurisé', 'Belle Vue', 'Dixinn', 550.00, 220000000, 400000, 'Disponible', 'Résidentiel',
ST_GeomFromText('POLYGON((-13.6620 9.5400, -13.6610 9.5400, -13.6610 9.5390, -13.6620 9.5390, -13.6620 9.5400))', 4326),
'Sékou Touré', '+224 621 99 00 11');

-- Matam
INSERT INTO "Terrains" ("Titre", "Description", "Quartier", "Commune", "Surface", "Prix", "PrixParM2", "Statut", "TypeTerrain", "Geometrie", "ContactNom", "ContactTelephone") VALUES
('Terrain Matam Marché', 'Proche du grand marché, fort potentiel commercial', 'Madina', 'Matam', 350.00, 140000000, 400000, 'Disponible', 'Commercial',
ST_GeomFromText('POLYGON((-13.6850 9.5250, -13.6845 9.5250, -13.6845 9.5245, -13.6850 9.5245, -13.6850 9.5250))', 4326),
'Abdoulaye Diallo', '+224 625 22 33 44'),
('Parcelle Bonfi', 'Terrain plat dans quartier populaire, bon prix', 'Bonfi', 'Matam', 280.00, 70000000, 250000, 'Disponible', 'Résidentiel',
ST_GeomFromText('POLYGON((-13.6780 9.5180, -13.6775 9.5180, -13.6775 9.5175, -13.6780 9.5175, -13.6780 9.5180))', 4326),
'Kadiatou Balde', '+224 622 55 66 77');

-- Matoto
INSERT INTO "Terrains" ("Titre", "Description", "Quartier", "Commune", "Surface", "Prix", "PrixParM2", "Statut", "TypeTerrain", "Geometrie", "ContactNom", "ContactTelephone") VALUES
('Grand terrain Matoto Centre', 'Terrain commercial avec grande façade sur route principale', 'Matoto Centre', 'Matoto', 1000.00, 300000000, 300000, 'Disponible', 'Commercial',
ST_GeomFromText('POLYGON((-13.6050 9.5050, -13.6040 9.5050, -13.6040 9.5040, -13.6050 9.5040, -13.6050 9.5050))', 4326),
'Mohamed Camara', '+224 628 88 99 00'),
('Parcelle Cosa', 'Quartier résidentiel calme, proche école', 'Cosa', 'Matoto', 420.00, 105000000, 250000, 'Disponible', 'Résidentiel',
ST_GeomFromText('POLYGON((-13.5980 9.5100, -13.5975 9.5100, -13.5975 9.5095, -13.5980 9.5095, -13.5980 9.5100))', 4326),
'Aminata Keita', '+224 621 11 22 33'),
('Terrain Sonfonia', 'Grande parcelle idéale pour projet immobilier', 'Sonfonia', 'Matoto', 1500.00, 375000000, 250000, 'Disponible', 'Résidentiel',
ST_GeomFromText('POLYGON((-13.5850 9.5200, -13.5830 9.5200, -13.5830 9.5180, -13.5850 9.5180, -13.5850 9.5200))', 4326),
'Boubacar Bah', '+224 625 44 55 66');

-- Coyah (périphérie)
INSERT INTO "Terrains" ("Titre", "Description", "Quartier", "Commune", "Surface", "Prix", "PrixParM2", "Statut", "TypeTerrain", "Geometrie", "ContactNom", "ContactTelephone") VALUES
('Terrain agricole Coyah', 'Grand terrain pour agriculture ou projet résidentiel', 'Manéah', 'Coyah', 5000.00, 500000000, 100000, 'Disponible', 'Agricole',
ST_GeomFromText('POLYGON((-13.3800 9.7100, -13.3750 9.7100, -13.3750 9.7050, -13.3800 9.7050, -13.3800 9.7100))', 4326),
'Mamady Condé', '+224 622 77 88 99'),
('Parcelle Dubréka', 'Terrain en bordure de route nationale', 'Dubréka Centre', 'Dubréka', 750.00, 112500000, 150000, 'Disponible', 'Mixte',
ST_GeomFromText('POLYGON((-13.5200 9.7900, -13.5190 9.7900, -13.5190 9.7890, -13.5200 9.7890, -13.5200 9.7900))', 4326),
'Fanta Soumah', '+224 628 00 11 22');

-- Verify data
SELECT "Id", "Titre", "Quartier", "Commune", "Surface", "Prix", ST_AsText("Geometrie") as "GeometrieWKT" FROM "Terrains";
