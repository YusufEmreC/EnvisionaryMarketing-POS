[Setup]
AppId={{2A9E58A3-DB0F-4C8F-B9AC-DA78DF1B308C}
AppName=ENVISIONARY MARKETING POS
AppVersion=1.0
AppPublisher=Envisionary Marketing
DefaultDirName={autopf}\ENVISIONARY MARKETING POS
DefaultGroupName=ENVISIONARY MARKETING POS
OutputDir=.\Installer
OutputBaseFilename=EnvisionaryMarketing_POS_Setup
SetupIconFile=Assets\app.ico
WizardImageFile=Assets\WizardImage.bmp
WizardSmallImageFile=Assets\WizardSmallImage.bmp
Compression=lzma
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\EnvisionaryMarketing.exe

[Files]
Source: "bin\Release\net8.0-windows\EnvisionaryMarketing.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\net8.0-windows\*.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\net8.0-windows\*.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\net8.0-windows\runtimes\*"; DestDir: "{app}\runtimes"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\ENVISIONARY MARKETING POS"; Filename: "{app}\EnvisionaryMarketing.exe"
Name: "{autodesktop}\ENVISIONARY MARKETING POS"; Filename: "{app}\EnvisionaryMarketing.exe"

[Run]
Filename: "{app}\EnvisionaryMarketing.exe"; Description: "{cm:LaunchProgram,ENVISIONARY MARKETING POS}"; Flags: nowait postinstall skipifsilent

[Code]
function IsDotNet8DesktopInstalled(): Boolean;
var
  FindRec: TFindRec;
begin
  Result := False;
  // 64-bit veya 32-bit Program Files içinde dotnet klasörünü kontrol et
  if FindFirst(ExpandConstant('{pf64}\dotnet\shared\Microsoft.WindowsDesktop.App\8.0.*'), FindRec) then
  begin
    Result := True;
    FindClose(FindRec);
  end
  else if FindFirst(ExpandConstant('{pf32}\dotnet\shared\Microsoft.WindowsDesktop.App\8.0.*'), FindRec) then
  begin
    Result := True;
    FindClose(FindRec);
  end;
end;

function InitializeSetup(): Boolean;
var
  ErrorCode: Integer;
begin
  Result := True;
  
  if not IsDotNet8DesktopInstalled() then
  begin
    if MsgBox('Bu programın çalışması için .NET 8.0 Masaüstü Çalışma Zamanı (Desktop Runtime) gereklidir. İndirme sayfasına yönlendirilmek ister misiniz?', mbConfirmation, MB_YESNO) = idYes then
    begin
      ShellExec('open', 'https://aka.ms/dotnet/8.0/windowsdesktop-runtime-win-x64.exe', '', '', SW_SHOW, ewNoWait, ErrorCode);
      MsgBox('Lütfen açılan pencereden .NET kurulumunu tamamladıktan sonra PosApp kurulumuna tekrar başlayın.', mbInformation, MB_OK);
    end;
    Result := False;
  end;
end;
