@echo off
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0tail-log.ps1" %*
exit /b %errorlevel%
