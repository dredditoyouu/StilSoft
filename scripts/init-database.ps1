$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot

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

if (-not $env:STILSOFT_IRS_CONNECTION_STRING) {
    $instance = Get-PreferredLocalDbInstance
    if ($instance) {
        $env:STILSOFT_IRS_PROVIDER_NAME = 'Microsoft.Data.SqlClient'
        $env:STILSOFT_IRS_CONNECTION_STRING = "Server=(localdb)\$instance;Database=StilsoftIRS;Integrated Security=True;TrustServerCertificate=True;"
    }
}

& (Join-Path $PSScriptRoot 'build-app.ps1')
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$executable = Join-Path $root 'src\StilsoftIRS.WinForms\bin\Debug\net472\StilsoftIRS.exe'
if (-not (Test-Path $executable)) {
    throw 'StilsoftIRS.exe was not found after build.'
}

Push-Location (Split-Path -Parent $executable)
try {
    & $executable --init-db
    exit $LASTEXITCODE
}
finally {
    Pop-Location
}
