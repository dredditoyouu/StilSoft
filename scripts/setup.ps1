#Requires -Version 5.1
[CmdletBinding()]
param(
    [switch]$NoLaunch
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

function Write-Step   { param([string]$M) Write-Host "`n==> $M" -ForegroundColor Cyan }
function Write-Ok     { param([string]$M) Write-Host "    OK: $M"   -ForegroundColor Green }
function Write-Fail   { param([string]$M) Write-Host "    ОШИБКА: $M" -ForegroundColor Red }
function Write-Warn   { param([string]$M) Write-Host "    ВНИМАНИЕ: $M" -ForegroundColor Yellow }

function Exit-WithPause {
    param([int]$Code = 1)
    Write-Host ""
    Read-Host "Нажмите Enter для выхода"
    exit $Code
}

# ── 1. .NET Framework 4.7.2 ─────────────────────────────────────────────────
Write-Step ".NET Framework 4.7.2..."
$netRelease = (Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full' -ErrorAction SilentlyContinue).Release
if ($netRelease -ge 461808) {
    Write-Ok ".NET Framework 4.7.2+ присутствует (release=$netRelease)"
} else {
    Write-Fail ".NET Framework 4.7.2 не найден."
    Write-Warn "Установите через Windows Update или скачайте с microsoft.com/net."
    Exit-WithPause
}

# ── 2. SQL Server LocalDB ────────────────────────────────────────────────────
Write-Step "SQL Server LocalDB..."

function Find-SqlLocalDb {
    $cmd = Get-Command sqllocaldb -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }
    $knownPaths = @(
        'C:\Program Files\Microsoft SQL Server\160\Tools\Binn\sqllocaldb.exe',
        'C:\Program Files\Microsoft SQL Server\150\Tools\Binn\sqllocaldb.exe',
        'C:\Program Files\Microsoft SQL Server\130\Tools\Binn\sqllocaldb.exe',
        'C:\Program Files\Microsoft SQL Server\110\Tools\Binn\sqllocaldb.exe'
    )
    foreach ($p in $knownPaths) {
        if (Test-Path $p) { return $p }
    }
    return $null
}

function Refresh-Path {
    $env:PATH = [System.Environment]::GetEnvironmentVariable('PATH','Machine') + ';' +
                [System.Environment]::GetEnvironmentVariable('PATH','User')
}

$sqlLocalDb = Find-SqlLocalDb
if ($sqlLocalDb) {
    Write-Ok "SQL Server LocalDB найден: $sqlLocalDb"
} else {
    Write-Warn "SQL Server LocalDB не найден. Устанавливаем..."
    $winget = Get-Command winget -ErrorAction SilentlyContinue
    if (-not $winget) {
        Write-Fail "winget не найден. Установите 'App Installer' из Microsoft Store."
        Write-Warn "Или вручную скачайте SQL Server LocalDB с microsoft.com/sql-server."
        Exit-WithPause
    }

    $installed = $false
    foreach ($pkg in @('Microsoft.SQLServer.2022.Express.LocalDB','Microsoft.SQLServer.2019.Express.LocalDB')) {
        Write-Host "    winget install $pkg ..." -ForegroundColor Gray
        & winget install --id $pkg --silent --accept-package-agreements --accept-source-agreements 2>&1 | Out-Null
        Refresh-Path
        $sqlLocalDb = Find-SqlLocalDb
        if ($sqlLocalDb) { $installed = $true; break }
    }

    if (-not $installed) {
        Write-Fail "Не удалось установить SQL Server LocalDB автоматически."
        Write-Warn "Скачайте вручную: microsoft.com/sql-server — выберите Express -> LocalDB."
        Exit-WithPause
    }
    Write-Ok "SQL Server LocalDB установлен: $sqlLocalDb"
}

# ── 3. Запуск экземпляра LocalDB ─────────────────────────────────────────────
Write-Step "Запуск экземпляра LocalDB..."
$instanceName = $null
foreach ($name in @('MSSQLLocalDB','v11.0')) {
    & $sqlLocalDb info $name 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0) {
        & $sqlLocalDb start $name 2>&1 | Out-Null
        $instanceName = $name
        Write-Ok "Экземпляр '$name' запущен"
        break
    }
}
if (-not $instanceName) {
    Write-Host "    Создаём новый экземпляр MSSQLLocalDB..." -ForegroundColor Gray
    & $sqlLocalDb create MSSQLLocalDB 2>&1 | Out-Null
    & $sqlLocalDb start  MSSQLLocalDB 2>&1 | Out-Null
    $instanceName = 'MSSQLLocalDB'
    Write-Ok "Создан и запущен MSSQLLocalDB"
}
$connStr = "Server=(localdb)\$instanceName;Database=StilsoftIRS;Integrated Security=True;TrustServerCertificate=True;"
$env:STILSOFT_IRS_CONNECTION_STRING = $connStr
$env:STILSOFT_IRS_PROVIDER_NAME     = 'Microsoft.Data.SqlClient'

# ── 4. .NET SDK (нужен для сборки) ───────────────────────────────────────────
$exePath = Join-Path $root 'src\StilsoftIRS.WinForms\bin\Debug\net472\StilsoftIRS.exe'
if (-not (Test-Path $exePath)) {
    Write-Step ".NET SDK 8.0 (для сборки)..."

    function Find-DotNet {
        foreach ($c in @((Join-Path $root '.tools\dotnet\dotnet.exe'))) {
            if (Test-Path $c) { return $c }
        }
        $cmd = Get-Command dotnet -ErrorAction SilentlyContinue
        if ($cmd) { return $cmd.Source }
        return $null
    }

    $dotnet = Find-DotNet
    if (-not $dotnet) {
        Write-Warn ".NET SDK не найден. Устанавливаем через winget..."
        $winget = Get-Command winget -ErrorAction SilentlyContinue
        if ($winget) {
            & winget install --id Microsoft.DotNet.SDK.8 --silent --accept-package-agreements --accept-source-agreements
            Refresh-Path
            $dotnet = Find-DotNet
        }
    }
    if (-not $dotnet) {
        Write-Fail ".NET SDK не найден после установки."
        Write-Warn "Скачайте вручную: dot.net/download"
        Exit-WithPause
    }
    Write-Ok ".NET SDK: $dotnet"

    # ── 5. Сборка ────────────────────────────────────────────────────────────
    Write-Step "Сборка приложения..."
    $project = Join-Path $root 'src\StilsoftIRS.WinForms\StilsoftIRS.WinForms.csproj'
    $env:DOTNET_ROOT    = Split-Path $dotnet -Parent
    $env:NUGET_PACKAGES = Join-Path $root '.local\nuget'

    & $dotnet restore $project
    if ($LASTEXITCODE -ne 0) { Write-Fail "restore завершился с ошибкой"; Exit-WithPause }

    & $dotnet build $project -c Debug --no-restore
    if ($LASTEXITCODE -ne 0) { Write-Fail "build завершился с ошибкой"; Exit-WithPause }

    Write-Ok "Сборка завершена"
}

if (-not (Test-Path $exePath)) {
    Write-Fail "StilsoftIRS.exe не найден: $exePath"
    Exit-WithPause
}

# ── 6. Инициализация БД ───────────────────────────────────────────────────────
Write-Step "Инициализация базы данных..."
$exeDir = Split-Path -Parent $exePath
Push-Location $exeDir
try {
    & $exePath --init-db
    if ($LASTEXITCODE -ne 0) {
        Write-Fail "Инициализация БД завершилась с кодом $LASTEXITCODE"
        Exit-WithPause
    }
} finally {
    Pop-Location
}
Write-Ok "База данных готова"

# ── 7. Ярлык на рабочем столе ─────────────────────────────────────────────────
Write-Step "Ярлык на рабочем столе..."
try {
    $desktop      = [Environment]::GetFolderPath('Desktop')
    $shortcutPath = Join-Path $desktop 'StilsoftIRS.lnk'
    $cmdPath      = Join-Path $root 'Start-StilsoftIRS.cmd'
    $wsh          = New-Object -ComObject WScript.Shell
    $sc           = $wsh.CreateShortcut($shortcutPath)
    $sc.TargetPath       = $cmdPath
    $sc.WorkingDirectory = $root
    $sc.Description      = 'Система регистрации инцидентов StilsoftIRS'
    $sc.Save()
    Write-Ok "Ярлык: $shortcutPath"
} catch {
    Write-Warn "Не удалось создать ярлык: $_"
}

# ── 8. Запуск ─────────────────────────────────────────────────────────────────
if (-not $NoLaunch) {
    Write-Step "Запуск приложения..."
    Start-Process -FilePath $exePath -WorkingDirectory $exeDir
}

Write-Host ""
Write-Host "=== Установка завершена успешно! ===" -ForegroundColor Green
Write-Host "    Логины: admin / operator / analyst    Пароль: 1" -ForegroundColor Cyan
Write-Host ""
