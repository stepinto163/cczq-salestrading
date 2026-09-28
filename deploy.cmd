@echo off
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0iteration1\scripts\product.ps1" -Action deploy %*
exit /b %errorlevel%
