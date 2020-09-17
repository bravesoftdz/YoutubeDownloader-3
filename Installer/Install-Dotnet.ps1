Write-Host "========================================================================"
Write-Host ""
Write-Host "This script will install dependencies required by YoutubeDownloader"
Write-Host ""
Write-Host "========================================================================"
Write-Host ""

# Ensure it's not already installed
try {
    Write-Host "Checking installed .NET runtimes..."

    $runtimes = & dotnet --list-runtimes | Out-String -Stream
    foreach ($runtime in $runtimes) {
        if ($runtime -like "Microsoft.WindowsDesktop.App 3.1.*") {
            Write-Host "Already installed: $runtime"
            Write-Host "Exiting..."
            exit
        }
    }
} catch {
    # If .NET is not installed, this will throw.
    # That's fine, just continue.
}

# Get .NET runtime installer URL
$installerDownloadUrl = ""
if ([Environment]::Is64BitOperatingSystem) {
    $installerDownloadUrl = "https://download.visualstudio.microsoft.com/download/pr/add2ffbe-a288-4d47-8b09-a39c8645f505/8516700dd5bd85fe07e8010e55d8f653/windowsdesktop-runtime-3.1.8-win-x64.exe"
} else {
    $installerDownloadUrl = "https://download.visualstudio.microsoft.com/download/pr/712f4ec2-79a4-4897-af5b-6c814dd49741/2025ef17bfc218cce1699787352d84b8/windowsdesktop-runtime-3.1.8-win-x86.exe"
}

# Download .NET runtime to temp directory
Write-Host "Downloading installer from $installerDownloadUrl"
Write-Host "Please wait, this can take some time..."

$installerFilePath = [IO.Path]::ChangeExtension([IO.Path]::GetTempFileName(), "exe")

Import-Module BitsTransfer
Start-BitsTransfer $installerDownloadUrl $installerFilePath -DisplayName "Downloading Microsoft .NET Runtime installer..." -Description "Installer will run as soon as it's downloaded"

# Run the installer
Start-Process $installerFilePath -Wait