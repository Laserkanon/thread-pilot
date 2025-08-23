#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Initializes local development environment:
      - Trusts .NET dev cert
      - Applies secrets from secrets.local.json to user-secrets,
        scoped per project based on top-level section name.
      - Generates merged .env file for Docker Compose.
.DESCRIPTION
    - secrets.local.json is structured by project name:
        {
          "MyService.ProjectA": { "Key1": "Value1", "Section": { "Key2": "Value2" } },
          "MyService.ProjectB": { "Key3": "Value3" }
        }
    - Each section only applies to its corresponding project.
    - No project names are hardcoded in the script.
.NOTES
    - Requires .NET SDK
    - Project name in JSON must match .csproj file name
#>

param(
    [string] $RootPath  # optional explicit repo root
)

$ErrorActionPreference = "Stop"

# --- Recursive function to handle nested secrets for .NET User Secrets ---
function Set-SecretsRecursively {
    param(
        $ProjectObject,
        $ProjectPath,
        [string]$KeyPrefix = ""
    )

    foreach ($prop in $ProjectObject.PSObject.Properties) {
        $key = if ($KeyPrefix) { "$KeyPrefix`:$($prop.Name)" } else { $prop.Name }
        $value = $prop.Value

        if ($value -is [System.Management.Automation.PSCustomObject]) {
            Set-SecretsRecursively -ProjectObject $value -ProjectPath $ProjectPath -KeyPrefix $key
        }
        else {
            Write-Host "   - Setting $key"
            dotnet user-secrets set $key "$value" --project $ProjectPath | Out-Null
        }
    }
}

# --- Configure Development Certificate ---
Write-Host "🔐 Configuring .NET development certificate..."
try {
    dotnet dev-certs https --trust | Out-Null
    Write-Host "✅ Development certificate configured successfully."
}
catch {
    Write-Warning "⚠️ Failed to configure the development certificate automatically. You may need to run 'dotnet dev-certs https --trust' manually with administrator privileges."
}

# Determine repo root
if ($RootPath) {
    $executionRoot = $RootPath
} elseif ($PSScriptRoot) {
    $executionRoot = Get-Location
} else {
    $executionRoot = Get-Location
}

$secretsFile = Join-Path $executionRoot "secrets.local.json"
if (-not (Test-Path $secretsFile)) {
    Write-Error "❌ The secrets file '$secretsFile' was not found."
    Write-Host "👉 Please copy 'secrets.local.json.template' to 'secrets.local.json' and fill it in."
    exit 1
}

Write-Host "🔑 Reading secrets from $secretsFile ..."
$secretsConfig = Get-Content $secretsFile -Raw | ConvertFrom-Json

# --- Create .env file (merged and flattened from all secrets) ---
$envFile = Join-Path $executionRoot ".env"
Write-Host "📝 Generating merged and flattened .env file for Docker Compose..."

$envVars = @{} # Use a hashtable to store unique, flattened keys.

# Recursive helper function to flatten the JSON properties
function Add-ToEnvHashtable {
    param(
        $ConfigObject,
        [string]$Prefix = ""
    )
    foreach ($prop in $ConfigObject.PSObject.Properties) {
        # Docker Compose .env files don't handle dots or colons well in keys.
        # Replace them with double underscores, which is also the .NET convention.
        $key = if ([string]::IsNullOrEmpty($Prefix)) { $prop.Name } else { "$Prefix`__$($prop.Name)" }
        $key = $key -replace "[:.]", "__"
        $value = $prop.Value

        if ($value -is [System.Management.Automation.PSCustomObject]) {
            Add-ToEnvHashtable -ConfigObject $value -Prefix $key
        }
        else {
            $envVars[$key] = $value
        }
    }
}

# Process all top-level sections from secrets.local.json
foreach ($proj in $secretsConfig.PSObject.Properties) {
    Add-ToEnvHashtable -ConfigObject $proj.Value
}

# Convert the hashtable to an array of "KEY=VALUE" strings and save to file
$lines = $envVars.GetEnumerator() | ForEach-Object { "$($_.Name)=$($_.Value)" }
Set-Content -Path $envFile -Value ($lines | Sort-Object) -Encoding UTF8
Write-Host "✅ .env file created at $envFile"


# --- Apply to .NET User Secrets ---
Write-Host "`n📦 Configuring .NET User Secrets..."

$updatedProjects = @()

foreach ($proj in $secretsConfig.PSObject.Properties) {
    $projectName = $proj.Name
    $projectPathObject = Get-ChildItem -Path (Join-Path $executionRoot "src") -Recurse -Filter "$projectName.csproj" | Select-Object -First 1

    if (-not $projectPathObject) {
        Write-Warning "⚠️ Could not find .csproj for $projectName. Skipping."
        continue
    }

    $projectPath = $projectPathObject.FullName
    Write-Host "`n→ Project: $projectPath"
    $updatedProjects += $projectName

    # Init secrets if missing
    dotnet user-secrets init --project $projectPath | Out-Null

    # Call the recursive function for .NET secrets
    Set-SecretsRecursively -ProjectObject $proj.Value -ProjectPath $projectPath
}

Write-Host "`n🎉 Secrets applied successfully."
if ($updatedProjects.Count -gt 0) {
    Write-Host "📂 Updated projects:"
    $updatedProjects | ForEach-Object { Write-Host "   - $_" }
} else {
    Write-Host "ℹ️ No projects were updated (no matching sections)."
}