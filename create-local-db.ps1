#!/usr/bin/env pwsh
<#
.SYNOPSIS
  Starts a SQL Server 2022 Docker container in background using DB_SA_PASSWORD from .env file.
  Interactive if a container with the same name already exists.
#>

$ErrorActionPreference = "Stop"
$containerName = "sqlserver"
$envFile = Join-Path (Get-Location) ".env"

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

# --- Check if container exists ---
$existing = docker ps -a --filter "name=^/${containerName}$" --format "{{.Status}}"

if ($existing) {
    Write-Host "⚠️  A container named '$containerName' already exists. Current status: $existing"

    if ($existing -match "Up") {
        Write-Host "Options:"
        Write-Host "  [R] Recreate (remove + start new)"
        Write-Host "  [K] Keep running (do nothing)"
        $choice = Read-Host "Choose (R/K)"
        switch ($choice.ToUpper()) {
            "R" {
                Write-Host "♻️ Removing running container '$containerName'..."
                docker rm -f $containerName | Out-Null
            }
            Default {
                Write-Host "✅ Keeping container running."
                exit 0
            }
        }
    }
    else {
        Write-Host "Options:"
        Write-Host "  [S] Start the existing container"
        Write-Host "  [R] Recreate (remove + start new)"
        $choice = Read-Host "Choose (S/R)"
        switch ($choice.ToUpper()) {
            "S" {
                Write-Host "▶️ Starting existing container '$containerName'..."
                docker start $containerName | Out-Null
                Write-Host "✅ Container started."
                exit 0
            }
            "R" {
                Write-Host "♻️ Removing old container '$containerName'..."
                docker rm $containerName | Out-Null
            }
            Default {
                Write-Host "❌ No valid choice. Exiting."
                exit 1
            }
        }
    }
}

# --- Start container fresh ---
Write-Host "🚀 Starting SQL Server container '$containerName' in background..."
docker run -d `
  --name $containerName `
  -e "ACCEPT_EULA=Y" `
  -e "SA_PASSWORD=$saPwd" `
  -p 1433:1433 `
  mcr.microsoft.com/mssql/server:2022-latest
