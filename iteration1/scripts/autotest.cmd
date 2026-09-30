@echo off
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0manage-ui-tests.ps1" %*
exit /b %errorlevel%
