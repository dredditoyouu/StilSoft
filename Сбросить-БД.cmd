@echo off
title StilsoftIRS - Reset Database
echo.
echo  WARNING: all database data will be deleted!
echo  After reset: logins admin/operator/analyst, password 1
echo.
pause
powershell -ExecutionPolicy Bypass -NoProfile -File "%~dp0scripts\reset-database.ps1"
if %ERRORLEVEL% neq 0 (
    echo  Error. Code: %ERRORLEVEL%
    pause
) else (
    pause
)
