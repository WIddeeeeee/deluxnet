@echo off
REM DeluxeNET one-shot setup for Windows.
REM Installs MariaDB if missing, then tells you the two manual steps.
where mysql >nul 2>nul
if %errorlevel%==0 (
  echo MariaDB/MySQL is already installed.
) else (
  echo Installing MariaDB (this opens its installer - set a root password when asked)...
  winget install -e --id MariaDB.Server --accept-source-agreements --accept-package-agreements
  if %errorlevel% neq 0 (
    echo.
    echo winget failed. Install MariaDB manually from https://mariadb.org/download/
    pause
    exit /b 1
  )
)
echo.
echo === Almost done - two quick manual steps: ===
echo 1. Open HeidiSQL (comes with MariaDB) or any MySQL client,
echo    open setup-db.sql, set your password in it, and run it.
echo 2. Double-click run.bat - it asks for that password ONCE and stores it
echo    in Windows Credential Manager. No passwords in any text file.
echo.
echo Then the server starts. Leave the window open.
echo Website: http://localhost:5118/web/index.html
pause
