@echo off
dotnet publish ../YoutubeDownloader/ -o Source/ --configuration Release
"c:\Program Files (x86)\Inno Setup 6\ISCC.exe" Installer.iss"
pause