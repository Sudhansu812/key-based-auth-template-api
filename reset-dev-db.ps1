<#
.SYNOPSIS
    Drops the Development AuthDb (SQL Server) and AppDb (PostgreSQL) databases so they can be
    recreated and re-seeded with the dummy data.

.DESCRIPTION
    After this script, start the API in Development (dotnet run). On startup DbInitializer:
      1. applies the EF Core migrations (recreating both databases),
      2. creates the stg.ProductRiskSummary staging table,
      3. seeds the system roles, the four dev API keys and the sample products.

    Connection strings come from env/.env (or the environment / launchSettings.json), exactly as
    the API reads them. This deletes ALL data in both databases - Development only.

.EXAMPLE
    ./reset-dev-db.ps1          # asks for confirmation
    ./reset-dev-db.ps1 -Force   # no prompt
#>
param(
    [switch]$Force
)

$ErrorActionPreference = 'Stop'
$projectDir = Join-Path $PSScriptRoot 'KeyBasedAuthenticationTemplate_API'

if (-not $Force) {
    $answer = Read-Host 'This permanently deletes the AuthDb and AppDb databases. Type "yes" to continue'
    if ($answer -ne 'yes') {
        Write-Host 'Aborted.'
        exit 1
    }
}

# The API resolves env/.env relative to its working directory, so run from the project folder.
Push-Location $projectDir
try {
    foreach ($context in 'AuthDbContext', 'AppDbContext') {
        Write-Host "Dropping database for $context..."
        dotnet ef database drop --context $context --force
        if ($LASTEXITCODE -ne 0) { throw "dotnet ef database drop failed for $context." }
    }
}
finally {
    Pop-Location
}

Write-Host ''
Write-Host 'Done. Start the API in Development to recreate and re-seed both databases:'
Write-Host "  dotnet run --project `"$projectDir`""
