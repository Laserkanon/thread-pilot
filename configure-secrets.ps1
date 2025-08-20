<#
.SYNOPSIS
    Configures secrets for both local and Docker Compose development.
.DESCRIPTION
    This script reads a single password from 'secrets.local.json', and uses it to configure both
    the .env file for Docker Compose and the .NET User Secrets for local development.
.NOTES
    - Requires .NET SDK to be installed.
    - Before running, copy 'secrets.local.json.template' to 'secrets.local.json' and populate the password.
#>

$ErrorActionPreference = "Stop"
# In environments where $PSScriptRoot is not available, fall back to the current directory.
if ($PSScriptRoot) {
    $executionRoot = $PSScriptRoot
} else {
    $executionRoot = Get-Location
}

$secretsFile = Join-Path $executionRoot "secrets.local.json"

if (-not (Test-Path $secretsFile)) {
    Write-Error "The secrets file '$secretsFile' was not found."
    Write-Host "Please copy 'secrets.local.json.template' to 'secrets.local.json' and fill in your password."
    exit 1
}

# --- Read the shared secret ---
try {
    $secretsConfig = Get-Content $secretsFile -Raw | ConvertFrom-Json
    $password = $secretsConfig.DB_SA_PASSWORD
    if ([string]::IsNullOrEmpty($password)) {
        throw "DB_SA_PASSWORD is empty or not found in $secretsFile"
    }
}
catch {
    Write-Error "Failed to parse '$secretsFile' or find DB_SA_PASSWORD. Please ensure it is valid JSON with a 'DB_SA_PASSWORD' key."
    exit 1
}

# --- Configure Docker Compose .env file ---
$envFile = Join-Path $executionRoot ".env"
Write-Host "Generating .env file for Docker Compose..."
Set-Content -Path $envFile -Value "DB_SA_PASSWORD=$password"
Write-Host ".env file created successfully."

# --- Configure .NET User Secrets ---
Write-Host "`nConfiguring .NET User Secrets for local development..."

$projects = @{
    "Insurance.Service" = "Server=localhost,1433;Database=InsuranceDb;User ID=sa;Password=$password;Encrypt=True;TrustServerCertificate=True;"
    "Vehicle.Service"   = "Server=localhost,1433;Database=VehicleDb;User ID=sa;Password=$password;Encrypt=True;TrustServerCertificate=True;"
}

foreach ($entry in $projects.GetEnumerator()) {
    $projectName = $entry.Name
    $connectionString = $entry.Value

    $serviceName = $projectName.Split('.')[0]
    $projectPath = Join-Path $executionRoot "src/$serviceName/$projectName"

    if (-not (Test-Path $projectPath)) {
        Write-Warning "Project path not found for '$projectName' at '$projectPath'. Skipping."
        continue
    }

    Write-Host "Configuring secrets for project: $projectName"

    # Initialize user secrets for the project.
    dotnet user-secrets init --project $projectPath

    # Set the connection string secret.
    Write-Host "  - Setting secret: 'ConnectionStrings:Default'"
    dotnet user-secrets set "ConnectionStrings:Default" "$connectionString" --project $projectPath
}

Write-Host "`nSecrets configured successfully for all environments."
