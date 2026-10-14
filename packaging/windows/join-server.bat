@echo off
REM Run on the PLAYER's PC to point rec.net at the server.
REM Usage: join-server.bat  (or: join-server.bat remove)
cd /d "%~dp0"
if "%1"=="remove" (
  powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0join-server.ps1" -Remove
) else (
  powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0join-server.ps1"
)
