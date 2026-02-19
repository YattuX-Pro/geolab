# Script PowerShell pour exécuter le fix de migration
# Nécessite le package Npgsql

$connectionString = "Host=localhost;Port=5432;Database=geo_db;Username=postgres;Password=postgres"
$sqlScript = Get-Content -Path "fix_migration.sql" -Raw

try {
    # Charger l'assembly Npgsql depuis le projet
    Add-Type -Path "src\Api\bin\Debug\net10.0\Npgsql.dll"
    
    $connection = New-Object Npgsql.NpgsqlConnection($connectionString)
    $connection.Open()
    
    $command = $connection.CreateCommand()
    $command.CommandText = $sqlScript
    $command.ExecuteNonQuery()
    
    Write-Host "Migration fix appliquee avec succes!" -ForegroundColor Green
    
    $connection.Close()
}
catch {
    Write-Host "Erreur: $_" -ForegroundColor Red
}
