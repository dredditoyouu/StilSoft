#Requires -Version 5.1
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

function Find-SqlLocalDb {
    $cmd = Get-Command sqllocaldb -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }
    foreach ($p in @(
        'C:\Program Files\Microsoft SQL Server\160\Tools\Binn\sqllocaldb.exe',
        'C:\Program Files\Microsoft SQL Server\150\Tools\Binn\sqllocaldb.exe',
        'C:\Program Files\Microsoft SQL Server\130\Tools\Binn\sqllocaldb.exe'
    )) { if (Test-Path $p) { return $p } }
    return $null
}

$sqlLocalDb = Find-SqlLocalDb
if (-not $sqlLocalDb) {
    Write-Host "sqllocaldb.exe не найден. Запустите сначала Установка.cmd." -ForegroundColor Red
    Read-Host "Нажмите Enter для выхода"; exit 1
}

# Запустить экземпляр
$instanceName = $null
foreach ($name in @('MSSQLLocalDB','v11.0')) {
    & $sqlLocalDb info $name 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0) {
        & $sqlLocalDb start $name 2>&1 | Out-Null
        $instanceName = $name; break
    }
}
if (-not $instanceName) {
    Write-Host "Экземпляр LocalDB не найден." -ForegroundColor Red
    Read-Host "Нажмите Enter для выхода"; exit 1
}

$server = "(localdb)\$instanceName"
Write-Host "Сервер: $server" -ForegroundColor Cyan

# Удалить БД через sqlcmd
$sqlcmd = Get-Command sqlcmd -ErrorAction SilentlyContinue
if (-not $sqlcmd) {
    # sqlcmd может быть рядом с sqllocaldb
    $sqlcmdPath = Join-Path (Split-Path $sqlLocalDb) 'sqlcmd.exe'
    if (Test-Path $sqlcmdPath) { $sqlcmd = $sqlcmdPath } else { $sqlcmd = $null }
}

if ($sqlcmd) {
    Write-Host "Удаляем базу данных StilsoftIRS..." -ForegroundColor Yellow
    $dropSql = "IF DB_ID('StilsoftIRS') IS NOT NULL BEGIN ALTER DATABASE StilsoftIRS SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE StilsoftIRS; END"
    & $sqlcmd -S $server -Q $dropSql 2>&1 | Out-Null
    Write-Host "OK: база данных удалена" -ForegroundColor Green
} else {
    # Fallback: через Microsoft.Data.SqlClient, если sqlcmd недоступен
    Write-Host "sqlcmd не найден, удаляем через .NET SqlClient..." -ForegroundColor Yellow
    $exePath = Join-Path $root 'src\StilsoftIRS.WinForms\bin\Debug\net472\StilsoftIRS.exe'
    if (-not (Test-Path $exePath)) {
        Write-Host "Приложение не собрано. Запустите сначала Установка.cmd." -ForegroundColor Red
        Read-Host "Нажмите Enter для выхода"; exit 1
    }
    Add-Type -Path (Join-Path (Split-Path $exePath) 'Microsoft.Data.SqlClient.dll') -ErrorAction SilentlyContinue
    try {
        $connStr = "Server=$server;Database=master;Integrated Security=True;TrustServerCertificate=True;"
        $conn = New-Object Microsoft.Data.SqlClient.SqlConnection($connStr)
        $conn.Open()
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = "IF DB_ID('StilsoftIRS') IS NOT NULL BEGIN ALTER DATABASE StilsoftIRS SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE StilsoftIRS; END"
        $cmd.ExecuteNonQuery() | Out-Null
        $conn.Close()
        Write-Host "OK: база данных удалена" -ForegroundColor Green
    } catch {
        Write-Host "Не удалось удалить БД: $_" -ForegroundColor Red
        Read-Host "Нажмите Enter для выхода"; exit 1
    }
}

# Переинициализировать
Write-Host ""
Write-Host "Инициализируем базу данных заново..." -ForegroundColor Cyan
$env:STILSOFT_IRS_CONNECTION_STRING = "Server=$server;Database=StilsoftIRS;Integrated Security=True;TrustServerCertificate=True;"
$env:STILSOFT_IRS_PROVIDER_NAME     = 'Microsoft.Data.SqlClient'
& (Join-Path $PSScriptRoot 'init-database.ps1')
if ($LASTEXITCODE -ne 0) {
    Write-Host "Ошибка при инициализации БД. Код: $LASTEXITCODE" -ForegroundColor Red
    Read-Host "Нажмите Enter для выхода"; exit $LASTEXITCODE
}

Write-Host ""
Write-Host "=== База данных пересоздана ===" -ForegroundColor Green
Write-Host "    Логины: admin / operator / analyst    Пароль: 1" -ForegroundColor Cyan
Write-Host ""
