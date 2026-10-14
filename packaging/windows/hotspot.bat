@echo off
REM hotspot.bat start ^| hotspot.bat stop
REM Turns this PC into a Wi-Fi hotspot for your server.
REM Right-click and "Run as administrator".
cd /d "%~dp0"
if "%1"=="stop" goto stop

echo Starting hotspot "Nextnox"...
netsh wlan set hostednetwork mode=allow ssid=Nextnox key=nextnox123 keyUsage=persistent >nul
netsh wlan start hostednetwork
if %errorlevel% neq 0 (
  echo.
  echo This Wi-Fi adapter can't do the hosted network.
  echo Do it manually: Settings -^> Network -^> Mobile hotspot -^> turn it ON.
  pause
  exit /b 1
)
REM Let http://rec.net/ reach the server: forward port 80 to 5118.
netsh interface portproxy add v4tov4 listenport=80 listenaddress=0.0.0.0 connectport=5118 connectaddress=127.0.0.1 >nul 2>nul
echo.
echo === Hotspot "Nextnox" is UP  (password: nextnox123) ===
echo.
echo For players:
echo   1. Connect to the "Nextnox" Wi-Fi
echo   2. Run join-server.bat on their PC (it asks before changing anything)
echo   3. Open http://rec.net/ in a browser - it opens YOUR server
echo.
echo Then start the server with run.bat.
pause
exit /b 0

:stop
netsh wlan stop hostednetwork >nul 2>nul
netsh interface portproxy delete v4tov4 listenport=80 listenaddress=0.0.0.0 >nul 2>nul
echo Hotspot stopped.
pause
