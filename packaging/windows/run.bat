@echo off
REM Start DeluxeNET. First run: the exe sets up the database itself
REM (installs MariaDB if needed, saves secrets to Credential Manager).
cd /d "%~dp0"
DeluxeNET.exe --urls "http://0.0.0.0:5118"
pause
