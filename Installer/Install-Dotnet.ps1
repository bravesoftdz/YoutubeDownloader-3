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
    $installerDownloadUrl = "https://download.visualstudio.microsoft.com/download/pr/5e4695fb-da51-4fa8-a090-07a64480888c/65aa842670d2280b5d05b8a070a9f495/windowsdesktop-runtime-3.1.7-win-x64.exe"
} else {
    $installerDownloadUrl = "https://download.visualstudio.microsoft.com/download/pr/3e6c8a13-9d89-4991-b683-b6bb279bc096/d1c44ba0c34f2be8878c36d27287e1a5/windowsdesktop-runtime-3.1.7-win-x86.exe"
}

# Download .NET runtime to temp directory
Write-Host "Downloading installer from $installerDownloadUrl"
Write-Host "Please wait, this can take some time..."

$installerFilePath = [IO.Path]::ChangeExtension([IO.Path]::GetTempFileName(), "exe")

Import-Module BitsTransfer
Start-BitsTransfer $installerDownloadUrl $installerFilePath -DisplayName "Downloading Microsoft .NET Runtime installer..." -Description "Installer will run as soon as it's downloaded"

# Run the installer
Start-Process $installerFilePath -Wait