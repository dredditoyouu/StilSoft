@echo off
title StilsoftIRS - Update Passwords
echo.
echo  Sets password "1" for all users (admin, operator, analyst).
echo.
pause
powershell -ExecutionPolicy Bypass -NoProfile -File "%~dp0scripts\update-passwords.ps1"
if %ERRORLEVEL% neq 0 (
    echo  Error. Code: %ERRORLEVEL%
)
pause
