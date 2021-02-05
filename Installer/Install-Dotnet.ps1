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
    $installerDownloadUrl = "https://download.visualstudio.microsoft.com/download/pr/c5cf65f5-85ca-4ae0-9c36-a0e0a852c218/07b9418c61804efb0fb079c28b1b1c90/dotnet-sdk-3.1.405-win-x64.exe"
} else {
    $installerDownloadUrl = "https://download.visualstudio.microsoft.com/download/pr/cf521b5e-c9f2-4f28-aac5-0404f2f4183c/65c9d9038013e4efdb772fa5ba6127f9/dotnet-sdk-3.1.405-win-x86.exe"
}

# Download .NET runtime to temp directory
Write-Host "Downloading installer from $installerDownloadUrl"
Write-Host "Please wait, this can take some time..."

$installerFilePath = [IO.Path]::ChangeExtension([IO.Path]::GetTempFileName(), "exe")

Import-Module BitsTransfer
Start-BitsTransfer $installerDownloadUrl $installerFilePath -DisplayName "Downloading Microsoft .NET Runtime installer..." -Description "Installer will run as soon as it's downloaded"

# Run the installer
Start-Process $installerFilePath -Wait