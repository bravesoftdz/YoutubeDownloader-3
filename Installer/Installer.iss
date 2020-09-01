#define AppName "YoutubeDownloader"
#define AppVersion "1.3.1"

[Setup]
AppId={{5A9BE5B6-CE10-45E2-AC95-68524D1109E5}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher="Thomas 'derech1e' Nürk"
AppPublisherURL="https://github.com/derech1e/YoutubeDownloader"
AppSupportURL="https://github.com/derech1e/YoutubeDownloader/issues"
AppUpdatesURL="https://github.com/derech1e/YoutubeDownloader/releases"
AppMutex=YoutubeDownloader_Identity
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
AllowNoIcons=yes
DisableWelcomePage=yes
DisableProgramGroupPage=no
DisableReadyPage=yes
SetupIconFile=..\favicon.ico
UninstallDisplayIcon=..\favicon.ico
LicenseFile=..\License.txt
OutputDir=bin\
OutputBaseFilename=YoutubeDownloader-Installer

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: ".installed"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\License.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "Source\*"; DestDir: "{app}"; Flags: ignoreversion
Source: "Install-Dotnet.ps1"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\YoutubeDownloader.exe"
Name: "{group}\{cm:UninstallProgram,{#AppName}}"; Filename: "{uninstallexe}"
Name: "{group}\{#AppName} on Github"; Filename: "https://github.com/derech1e/YoutubeDownloader"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\YoutubeDownloader.exe"; Tasks: desktopicon

[Registry]
Root: HKLM; Subkey: "Software\Microsoft\Windows NT\CurrentVersion\ICM"; ValueType: dword; ValueName: "GdiICMGammaRange"; ValueData: "256"

[Run]
Filename: "{app}\YoutubeDownloader.exe"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Name: "{userappdata}\YoutubeDownloader"; Type: filesandordirs

[Code]
procedure InstallDotnet();
var
  ErrorCode: Integer;
begin
  ShellExec('', 'powershell',
    '-NoProfile -ExecutionPolicy Unrestricted -File "' + ExpandConstant('{app}') + '\Install-Dotnet.ps1"',
    '', SW_SHOW, ewWaitUntilTerminated, ErrorCode);  
end;
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then InstallDotnet();
end;