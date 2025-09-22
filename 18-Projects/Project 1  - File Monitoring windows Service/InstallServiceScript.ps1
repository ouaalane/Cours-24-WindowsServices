# InstallService.ps1
param (
    [string]$ServiceExePath = "C:\Programming Advice\Cours-24-Windows Services\18-Projects\Project 1  - File Monitoring windows Service\LogMonitoringServiceProject\bin\Release\LogMonitoringServiceProject.exe"
)

# Path to InstallUtil (from .NET Framework)
$installUtil = "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\InstallUtil.exe"

if (-Not (Test-Path $ServiceExePath)) {
    Write-Host "❌ Service executable not found: $ServiceExePath"
    exit 1
}

if (-Not (Test-Path $installUtil)) {
    Write-Host "❌ InstallUtil.exe not found at: $installUtil"
    exit 1
}

Write-Host "➡ Installing service from $ServiceExePath ..."
& $installUtil $ServiceExePath

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Service installed successfully."
} else {
    Write-Host "⚠ Service installation failed. Exit code: $LASTEXITCODE"
}