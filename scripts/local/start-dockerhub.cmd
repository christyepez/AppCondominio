@echo off
setlocal
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0start-dockerhub.ps1" %*
exit /b %ERRORLEVEL%
