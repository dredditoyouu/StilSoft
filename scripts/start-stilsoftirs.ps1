[CmdletBinding()]
param(
    [string]$ExecutablePath,
    [string]$EnvironmentFile,
    [string]$ProviderName,
    [string]$ConnectionString
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$environmentFileName = 'StilsoftIRS.env'

function Get-EnvironmentFileCandidates {
    param(
        [string]$ExplicitPath,
        [string]$TargetExecutablePath
    )

    $candidates = @()

    if ($ExplicitPath) {
        $resolved = Resolve-Path $ExplicitPath -ErrorAction Stop
        $candidates += $resolved.Path
    }

    if ($TargetExecutablePath) {
        $candidates += (Join-Path (Split-Path -Parent $TargetExecutablePath) $environmentFileName)
    }

    $candidates += (Join-Path $root $environmentFileName)
    return $candidates | Select-Object -Unique
}

function Read-EnvironmentFile {
    param([string[]]$CandidatePaths)

    foreach ($path in $CandidatePaths) {
        if (-not (Test-Path $path)) {
            continue
        }

        $values = @{}
        foreach ($rawLine in Get-Content $path) {
            $line = $rawLine.Trim()
            if (-not $line -or $line.StartsWith('#')) {
                continue
            }

            $separatorIndex = $line.IndexOf('=')
            if ($separatorIndex -le 0) {
                continue
            }

            $key = $line.Substring(0, $separatorIndex).Trim()
            $value = $line.Substring($separatorIndex + 1).Trim().Trim('"').Trim("'")
            $values[$key] = $value
        }

        return [pscustomobject]@{
            Path = $path
            Values = $values
        }
    }

    return $null
}

function Resolve-ExecutablePath {
    $preferred = Join-Path $root 'src\StilsoftIRS.WinForms\bin\Debug\net472\StilsoftIRS.exe'
    if (Test-Path $preferred) {
        return $preferred
    }

    $candidate = Get-ChildItem -Path $root -Filter StilsoftIRS.exe -Recurse -ErrorAction SilentlyContinue |
        Select-Object -First 1

    if ($candidate) {
        return $candidate.FullName
    }

    return $null
}

function Get-SqlexpressService {
    return Get-Service -Name 'MSSQL$SQLEXPRESS' -ErrorAction SilentlyContinue
}

function Get-PreferredLocalDbInstance {
    $command = Get-Command sqllocaldb -ErrorAction SilentlyContinue
    if (-not $command) {
        return $null
    }

    foreach ($name in @('MSSQLLocalDB', 'v11.0')) {
        & $command.Source info $name | Out-Null
        if ($LASTEXITCODE -eq 0) {
            & $command.Source start $name | Out-Null
            return $name
        }
    }

    return $null
}

if (-not $ExecutablePath) {
    $ExecutablePath = Resolve-ExecutablePath
}

if (-not $ExecutablePath -or -not (Test-Path $ExecutablePath)) {
    throw 'StilsoftIRS.exe was not found. Run scripts\build-app.ps1 first.'
}

$workingDirectory = Split-Path -Parent $ExecutablePath
$environmentData = Read-EnvironmentFile -CandidatePaths (
    Get-EnvironmentFileCandidates -ExplicitPath $EnvironmentFile -TargetExecutablePath $ExecutablePath
)

if (-not $ConnectionString) {
    $ConnectionString = $env:STILSOFT_IRS_CONNECTION_STRING
}
if (-not $ProviderName) {
    $ProviderName = $env:STILSOFT_IRS_PROVIDER_NAME
}

if ($environmentData) {
    if (-not $ConnectionString -and $environmentData.Values.ContainsKey('STILSOFT_IRS_CONNECTION_STRING')) {
        $ConnectionString = [string]$environmentData.Values['STILSOFT_IRS_CONNECTION_STRING']
    }
    if (-not $ProviderName -and $environmentData.Values.ContainsKey('STILSOFT_IRS_PROVIDER_NAME')) {
        $ProviderName = [string]$environmentData.Values['STILSOFT_IRS_PROVIDER_NAME']
    }
}

if (-not $ConnectionString) {
    $instance = Get-PreferredLocalDbInstance
    if ($instance) {
        $ProviderName = 'Microsoft.Data.SqlClient'
        $ConnectionString = "Server=(localdb)\$instance;Database=StilsoftIRS;Integrated Security=True;TrustServerCertificate=True;"
    }
}
elseif ($ConnectionString -match 'SQLEXPRESS' -and -not (Get-SqlexpressService)) {
    $instance = Get-PreferredLocalDbInstance
    if ($instance) {
        $ProviderName = 'Microsoft.Data.SqlClient'
        $ConnectionString = "Server=(localdb)\$instance;Database=StilsoftIRS;Integrated Security=True;TrustServerCertificate=True;"
    }
}
elseif ($ConnectionString -match '\(localdb\)') {
    [void](Get-PreferredLocalDbInstance)
}

if ($ProviderName) {
    $env:STILSOFT_IRS_PROVIDER_NAME = $ProviderName
}
if ($ConnectionString) {
    $env:STILSOFT_IRS_CONNECTION_STRING = $ConnectionString
}

Start-Process -FilePath $ExecutablePath -WorkingDirectory $workingDirectory
