# DeluxeNET launcher — run.bat starts the exe directly; this is only needed
# for the DNS mode (dns.bat calls it with -WithDns). The exe handles first-run
# setup itself; secrets live in Windows Credential Manager.
param([switch]$WithDns)

$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

if ($WithDns) {
    # The DNS server needs UDP port 53 -> requires admin. Relaunch elevated if asked.
    $isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()
      ).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
    if (-not $isAdmin) {
        Write-Host 'DNS needs admin rights for port 53 - relaunching elevated...'
        Start-Process powershell -Verb RunAs -ArgumentList '-NoProfile','-ExecutionPolicy','Bypass','-File',"`"$PSCommandPath`"",'-WithDns' -Wait
        exit
    }
    # Let player devices reach our DNS through the firewall.
    netsh advfirewall firewall delete rule name="DeluxeNET DNS" >$null 2>&1
    netsh advfirewall firewall add rule name="DeluxeNET DNS" dir=in action=allow protocol=UDP localport=53 >$null 2>&1
    netsh advfirewall firewall add rule name="DeluxeNET DNS" dir=in action=allow protocol=TCP localport=53 >$null 2>&1
    $env:Dns__Enabled = 'true'
}

$port = '5118'
$env:Public__BaseUrl = "http://localhost:$port"
& "$PSScriptRoot\DeluxeNET.exe" --urls "http://0.0.0.0:$port"
