@echo off
title StilsoftIRS Setup
echo.
echo  StilsoftIRS - Installation
echo  Requires Internet if SQL Server LocalDB is missing.
echo.
pause

:: Re-save setup.ps1 with UTF-8 BOM so PowerShell 5.1 reads Cyrillic correctly
set "SETUP_PS1=%~dp0scripts\setup.ps1"
powershell -NoProfile -Command "$f=$env:SETUP_PS1; [IO.File]::WriteAllText($f, [IO.File]::ReadAllText($f, [Text.Encoding]::UTF8), (New-Object Text.UTF8Encoding $true))"

powershell -ExecutionPolicy Bypass -NoProfile -File "%~dp0scripts\setup.ps1" %*
if %ERRORLEVEL% neq 0 (
    echo.
    echo  Setup failed. Code: %ERRORLEVEL%
    pause
)
