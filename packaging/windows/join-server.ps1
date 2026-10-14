# Player-side setup: point rec.net at the server.
# Run on the PLAYER's PC (not the server). Asks before changing anything,
# and can be fully undone. Run again with -Remove to undo.
param([switch]$Remove)

$hostsFile = 'C:\Windows\System32\drivers\etc\hosts'
$marker = '# DeluxeNET'

# The hosts file needs admin rights; relaunch elevated if we aren't.
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()
  ).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    $arg = '-NoProfile -ExecutionPolicy Bypass -File "' + $PSCommandPath + '"'
    if ($Remove) { $arg += ' -Remove' }
    Start-Process powershell -Verb RunAs -ArgumentList $arg -Wait
    exit
}

if ($Remove) {
    $before = Get-Content $hostsFile
    $after = @($before | Where-Object { $_ -notmatch [regex]::Escape($marker) })
    if ($after.Count -eq $before.Count) {
        Write-Host 'No DeluxeNET entry found - nothing to remove.'
    } else {
        $after | Set-Content $hostsFile -Encoding ASCII
        Write-Host 'Removed. rec.net is back to normal.'
    }
    pause
    exit
}

$ip = Read-Host 'Server IP (press Enter for 192.168.137.1)'
if (-not $ip) { $ip = '192.168.137.1' }
if ($ip -notmatch '^\d{1,3}(\.\d{1,3}){3}$') {
    Write-Host 'That does not look like an IP address.'
    pause
    exit 1
}

$content = Get-Content $hostsFile
if ($content -match [regex]::Escape($marker)) {
    Write-Host 'Already set up - rec.net already points at the server.'
} else {
    Add-Content $hostsFile ''
    Add-Content $hostsFile "$ip rec.net  $marker"
    Write-Host ''
    Write-Host "Done! http://rec.net/ now opens the server at $ip."
    Write-Host 'To undo this later, run this script again with -Remove'
    Write-Host '(or: join-server.bat remove).'
}
pause
