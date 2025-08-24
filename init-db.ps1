#!/usr/bin/env pwsh
<#
.SYNOPSIS
  Starts a SQL Server 2022 Docker container and runs database migrations.
  - Reads DB_SA_PASSWORD from the .env file in the project root.
  - Handles cases where a container with the same name already exists.
  - The C# migration apps are responsible for waiting for the DB to be ready.
#>

$ErrorActionPreference = "Stop"
$containerName = "sqlserver"
$envFile = Join-Path (Get-Location) ".env"

#============================================================================
# HELPER FUNCTIONS
#============================================================================

function Invoke-Migration {
    param(
        [string]$Path
    )

    if (-not (Test-Path $Path)) {
        Write-Error "❌ Migration project path not found: $Path"
        exit 1
    }

    Write-Host "  - Running migrations for '$Path'..."
    try {
        Push-Location $Path
        dotnet run
        Pop-Location
    }
    catch {
        Write-Error "❌ Failed to run migrations in '$Path'."
        Pop-Location # Ensure we pop location even on failure
        exit 1
    }
}


#============================================================================
# SCRIPT LOGIC
#============================================================================

# --- Read DB_SA_PASSWORD from .env ---
if (-not (Test-Path $envFile)) {
    Write-Error "❌ .env file not found at $envFile"
    exit 1
}

$saPwd = Get-Content $envFile |
    ForEach-Object {
        if ($_ -match '^DB_SA_PASSWORD=(.+)$') { $Matches[1] }
    }

if ([string]::IsNullOrWhiteSpace($saPwd)) {
    Write-Error "❌ DB_SA_PASSWORD not found in .env"
    exit 1
}

# --- Check if container exists and get it running ---
$existing = docker ps -a --filter "name=^/${containerName}$" --format "{{.Status}}"
$startNew = $false

if ($existing) {
    Write-Host "⚠️  A container named '$containerName' already exists. Current status: $existing"

    if ($existing -match "Up") {
        Write-Host "Options:"
        Write-Host "  [R] Recreate (remove + start new, will re-run migrations)"
        Write-Host "  [K] Keep running (will re-run migrations on existing DB)"
        $choice = Read-Host "Choose (R/K)"
        if ($choice.ToUpper() -eq "R") {
            Write-Host "♻️ Removing running container '$containerName'..."
            docker rm -f $containerName | Out-Null
            $startNew = $true
        }
    }
    else { # Exited
        Write-Host "Options:"
        Write-Host "  [S] Start the existing container (will run migrations)"
        Write-Host "  [R] Recreate (remove + start new, will run migrations)"
        $choice = Read-Host "Choose (S/R)"
        switch ($choice.ToUpper()) {
            "S" {
                Write-Host "▶️ Starting existing container '$containerName'..."
                docker start $containerName | Out-Null
            }
            "R" {
                Write-Host "♻️ Removing old container '$containerName'..."
                docker rm $containerName | Out-Null
                $startNew = $true
            }
            Default {
                Write-Host "❌ No valid choice. Exiting."
                exit 1
            }
        }
    }
}
else {
    $startNew = $true
}


# --- Start container if needed ---
if ($startNew) {
    Write-Host "🚀 Starting new SQL Server container '$containerName' in background..."
    docker run -d `
      --name $containerName `
      -e "ACCEPT_EULA=Y" `
      -e "SA_PASSWORD=$saPwd" `
      -p 1433:1433 `
      mcr.microsoft.com/mssql/server:2022-latest | Out-Null
}

# --- Run migrations (the C# app will handle the waiting) ---
Write-Host "🚀 Applying database migrations..."
Write-Host "   (The .NET app will now wait for the database to become available before migrating)"
Invoke-Migration -Path "src/Vehicle/Vehicle.Db"
Invoke-Migration -Path "src/Insurance/Insurance.Db"

Write-Host "✅ Migration process finished. Check the console output above for status."