[Setup]
AppName=Envisionary Marketing POS
AppVersion=1.0
DefaultDirName={autopf}\Envisionary Marketing POS
DefaultGroupName=Envisionary Marketing POS
UninstallDisplayIcon={app}\EnvisionaryMarketing.exe
Compression=lzma2
SolidCompression=yes
OutputDir=.\Installer
OutputBaseFilename=EnvisionaryMarketing_Setup_v1.0
ArchitecturesInstallIn64BitMode=x64
DisableProgramGroupPage=yes

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\Envisionary Marketing POS"; Filename: "{app}\EnvisionaryMarketing.exe"
Name: "{autodesktop}\Envisionary Marketing POS"; Filename: "{app}\EnvisionaryMarketing.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\EnvisionaryMarketing.exe"; Description: "{cm:LaunchProgram,Envisionary Marketing POS}"; Flags: nowait postinstall skipifsilent

[Code]
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usPostUninstall then
  begin
    if MsgBox('Kayıtlı ürünlerinizi, satışlarınızı ve kullanıcı bilgilerinizi (veritabanını) kalıcı olarak silmek istiyor musunuz?' + #13#10 + #13#10 + 'Eğer uygulamayı tekrar kurmayı düşünüyorsanız HAYIR seçeneğini tıklayın.', mbConfirmation, MB_YESNO or MB_DEFBUTTON2) = idYes then
    begin
      DelTree(ExpandConstant('{userappdata}\PosApp'), True, True, True);
    end;
  end;
end;
