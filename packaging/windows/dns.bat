@echo off
REM Start DeluxeNET WITH the DNS server (for consoles / VR / phones).
REM Asks for admin rights (needed for DNS port 53), then asks for your DB
REM password once and stores it in Windows Credential Manager.
cd /d "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run.ps1" -WithDns
pause
