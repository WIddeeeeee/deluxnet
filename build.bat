@echo off
REM ============================================================
REM  build.bat -- builds the DeluxeNET Windows package from source.
REM
REM  Needs the .NET 9 SDK (one time):
REM      winget install Microsoft.DotNet.SDK.9
REM
REM  Output:  dist\deluxenet-windows.zip
REM ============================================================
setlocal
cd /d "%~dp0"

echo --- Checking for .NET 9 SDK ---
where dotnet >nul 2>nul || goto :nosdk
dotnet --list-sdks 2>nul | findstr /R "^9\." >nul || goto :nosdk
echo Found:
dotnet --list-sdks 2>nul | findstr /R "^9\."
goto :build

:nosdk
echo.
echo .NET 9 SDK not found. Install it first, then run build.bat again:
echo    winget install Microsoft.DotNet.SDK.9
echo.
exit /b 1

:build
echo.
echo --- Publishing DeluxeNET (win-x64, self-contained) ---
set DIST=%~dp0dist
if exist "%DIST%" rmdir /s /q "%DIST%"
mkdir "%DIST%\deluxenet-windows" || goto :fail
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "%DIST%\deluxenet-windows" || goto :fail

echo.
echo --- Adding launchers and docs ---
xcopy "%~dp0packaging\windows\*" "%DIST%\deluxenet-windows\" /Y /Q >nul || goto :fail
del "%DIST%\deluxenet-windows\DeluxeNET.pdb" 2>nul

echo.
echo --- Zipping ---
powershell -NoProfile -Command "Compress-Archive -Path '%DIST%\deluxenet-windows' -DestinationPath '%DIST%\deluxenet-windows.zip' -Force" || goto :fail

echo.
echo === BUILD OK ===
echo %DIST%\deluxenet-windows.zip
echo.
echo Unzip it anywhere, then:  install.bat  -^>  run.bat  (or dns.bat for DNS)
exit /b 0

:fail
echo.
echo BUILD FAILED - see the error above.
exit /b 1
