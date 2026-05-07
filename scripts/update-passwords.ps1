#Requires -Version 5.1
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

# SHA-256("1") — пароль для всех учётных записей
$hash1 = '6b86b273ff34fce19d6b804eff5a3f5747ada4eaa22f1d49c01e52ddb7875b4b'

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

Add-Type -AssemblyName System.Data

$connStr = "Server=$server;Database=StilsoftIRS;Integrated Security=True;"
try {
    $conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
    $conn.Open()

    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "UPDATE dbo.Users SET PasswordHash = @h"
    $cmd.Parameters.AddWithValue('@h', $hash1) | Out-Null
    $rows = $cmd.ExecuteNonQuery()

    $conn.Close()
    Write-Host ""
    Write-Host "Updated $rows user(s)." -ForegroundColor Green
    Write-Host "Password for admin / operator / analyst is now: 1" -ForegroundColor Cyan
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
    Read-Host "Press Enter to exit"; exit 1
}
Write-Host ""
