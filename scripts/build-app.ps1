$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src\StilsoftIRS.WinForms\StilsoftIRS.WinForms.csproj'

function Resolve-DotNetExe {
    $candidates = @(
        (Join-Path $root '.tools\dotnet\dotnet.exe'),
        (Join-Path (Split-Path -Parent $root) 'wkd\.tools\dotnet\dotnet.exe')
    )

    foreach ($candidate in $candidates) {
        if (Test-Path $candidate) {
            return $candidate
        }
    }

    $command = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($command) {
        return $command.Source
    }

    throw 'dotnet.exe was not found. Install .NET SDK 8.0 or place it in .tools\dotnet.'
}

$dotnet = Resolve-DotNetExe
$env:DOTNET_ROOT = Split-Path $dotnet -Parent
$env:NUGET_PACKAGES = Join-Path $root '.local\nuget'

& $dotnet restore $project
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

& $dotnet build $project -c Debug
exit $LASTEXITCODE
