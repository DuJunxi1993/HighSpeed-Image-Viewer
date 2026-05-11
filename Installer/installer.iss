; HighSpeedImageViewer Inno Setup Script
; Requires Inno Setup 6.0 or later

#define MyAppName "HighSpeed Image Viewer"
#define MyAppVersion "1.0"
#define MyAppPublisher "ImageViewerNeo"
#define MyAppExeName "HighSpeedImageViewer.exe"
#define SupportedExtensions ".jpg|.jpeg|.png|.bmp|.gif|.tiff|.tif|.webp|.heic|.heif|.avif|.ico"

[Setup]
AppId={{8A3D4E2F-5B1C-4D8E-9F3A-2C7B6E1D4A0F}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\HighSpeedImageViewer
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
OutputDir=.
OutputBaseFilename=HighSpeedImageViewer-Setup
SetupIconFile=Assets\hsiv.ico
UninstallDisplayIcon={app}\HighSpeedImageViewer.exe
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Shortcuts:"
Name: "fileassoc"; Description: "Set as default image viewer"; GroupDescription: "File associations:"

[Files]
Source: "bin\Release\net10.0-windows\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "Assets\LICENSE.txt"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Run {#MyAppName} now"; Flags: nowait postinstall skipifsilent

[Registry]
; ProgID for file associations
Root: HKCR; Subkey: "ImageViewerNeo.Image"; ValueType: string; ValueName: ""; ValueData: "Image Viewer Document"; Flags: uninsdeletekey; Tasks: fileassoc
Root: HKCR; Subkey: "ImageViewerNeo.Image\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"; Tasks: fileassoc
Root: HKCR; Subkey: "ImageViewerNeo.Image\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: fileassoc
Root: HKCR; Subkey: "ImageViewerNeo.Image\shell\print\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: fileassoc

; Register all supported extensions
Root: HKCR; Subkey: ".jpg\OpenWithProgids"; ValueType: string; ValueName: "ImageViewerNeo.Image"; ValueData: ""; Flags: uninsdeletevalue; Tasks: fileassoc
Root: HKCR; Subkey: ".jpeg\OpenWithProgids"; ValueType: string; ValueName: "ImageViewerNeo.Image"; ValueData: ""; Flags: uninsdeletevalue; Tasks: fileassoc
Root: HKCR; Subkey: ".png\OpenWithProgids"; ValueType: string; ValueName: "ImageViewerNeo.Image"; ValueData: ""; Flags: uninsdeletevalue; Tasks: fileassoc
Root: HKCR; Subkey: ".bmp\OpenWithProgids"; ValueType: string; ValueName: "ImageViewerNeo.Image"; ValueData: ""; Flags: uninsdeletevalue; Tasks: fileassoc
Root: HKCR; Subkey: ".gif\OpenWithProgids"; ValueType: string; ValueName: "ImageViewerNeo.Image"; ValueData: ""; Flags: uninsdeletevalue; Tasks: fileassoc
Root: HKCR; Subkey: ".tiff\OpenWithProgids"; ValueType: string; ValueName: "ImageViewerNeo.Image"; ValueData: ""; Flags: uninsdeletevalue; Tasks: fileassoc
Root: HKCR; Subkey: ".tif\OpenWithProgids"; ValueType: string; ValueName: "ImageViewerNeo.Image"; ValueData: ""; Flags: uninsdeletevalue; Tasks: fileassoc
Root: HKCR; Subkey: ".webp\OpenWithProgids"; ValueType: string; ValueName: "ImageViewerNeo.Image"; ValueData: ""; Flags: uninsdeletevalue; Tasks: fileassoc
Root: HKCR; Subkey: ".heic\OpenWithProgids"; ValueType: string; ValueName: "ImageViewerNeo.Image"; ValueData: ""; Flags: uninsdeletevalue; Tasks: fileassoc
Root: HKCR; Subkey: ".heif\OpenWithProgids"; ValueType: string; ValueName: "ImageViewerNeo.Image"; ValueData: ""; Flags: uninsdeletevalue; Tasks: fileassoc
Root: HKCR; Subkey: ".avif\OpenWithProgids"; ValueType: string; ValueName: "ImageViewerNeo.Image"; ValueData: ""; Flags: uninsdeletevalue; Tasks: fileassoc
Root: HKCR; Subkey: ".ico\OpenWithProgids"; ValueType: string; ValueName: "ImageViewerNeo.Image"; ValueData: ""; Flags: uninsdeletevalue; Tasks: fileassoc

[Code]
procedure CurStepChanged(CurStep: TSetupStep);
var
  ResultCode: Integer;
begin
  if CurStep = ssPostInstall then
  begin
    if WizardIsTaskSelected('fileassoc') then
    begin
      Exec('cmd.exe', '/c assoc .=jpg', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    end;
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  I: Integer;
  Ext: String;
  Extensions: Array of String;
begin
  if CurUninstallStep = usPostUninstall then
  begin
    Extensions := ['.jpg', '.jpeg', '.png', '.bmp', '.gif', '.tiff', '.tif', '.webp', '.heic', '.heif', '.avif', '.ico'];
    for I := 0 to GetArrayLength(Extensions) - 1 do
    begin
      Ext := Extensions[I];
      RegDeleteKeyIncludingSubkeys(HKCR, Ext + '\OpenWithProgids\ImageViewerNeo.Image');
    end;
  end;
end;
