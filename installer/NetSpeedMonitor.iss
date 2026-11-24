; NetSpeed Monitor - Inno Setup 6 installer script
; Build after publishing:  dotnet publish ... -o publish\win-x64
; Then:  ISCC.exe installer\NetSpeedMonitor.iss
; Output: installer\Output\NetSpeedMonitor-Setup-<version>.exe

#define AppName "NetSpeed Monitor"
#define AppVersion "1.0.0"
#define AppPublisher "NetSpeed Monitor Contributors"
#define AppExeName "NetSpeedMonitor.exe"

[Setup]
AppId={{8F3A2B1C-4D5E-4F6A-9B8C-7D6E5F4A3B2C}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL=https://github.com/thebinsultan/NetSpeed-Monitor
AppSupportURL=https://github.com/thebinsultan/NetSpeed-Monitor/issues
AppUpdatesURL=https://github.com/thebinsultan/NetSpeed-Monitor/releases
VersionInfoVersion={#AppVersion}
VersionInfoProductName={#AppName}
VersionInfoCompany={#AppPublisher}
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
LicenseFile=..\LICENSE
OutputDir=Output
OutputBaseFilename=NetSpeedMonitor-Setup-{#AppVersion}
SetupIconFile=..\src\NetSpeedMonitor\Resources\icon.ico
UninstallDisplayIcon={app}\{#AppExeName}
UninstallDisplayName={#AppName}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
; Per-user install: no administrator rights required.
PrivilegesRequired=lowest
CloseApplications=yes
RestartApplications=no
MinVersion=10.0

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "startupicon"; Description: "Start {#AppName} when Windows starts"; GroupDescription: "Startup:"

[Files]
Source: "..\publish\win-x64\{#AppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\README.md"; DestDir: "{app}"; Flags: ignoreversion isreadme
Source: "..\LICENSE"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{group}\{cm:UninstallProgram,{#AppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon
Name: "{userstartup}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: startupicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
; Stop the app before removing files (ignore errors if it is not running).
Filename: "{cmd}"; Parameters: "/c taskkill /im {#AppExeName} /f"; Flags: runhidden; RunOnceId: "StopApp"

; Settings and usage history live in %LocalAppData%\NetSpeedMonitor and are
; intentionally kept on uninstall so statistics survive a reinstall.
