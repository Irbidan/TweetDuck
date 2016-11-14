; Script generated by the Inno Script Studio Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "TweetDuck"
#define MyAppPublisher "chylex"
#define MyAppURL "https://tweetduck.chylex.com"
#define MyAppExeName "TweetDuck.exe"

#define MyAppVersion GetFileVersion("..\bin\x86\Release\TweetDuck.exe")

[Setup]
AppId={{8C25A716-7E11-4AAD-9992-8B5D0C78AE06}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputBaseFilename={#MyAppName}.Portable
VersionInfoVersion={#MyAppVersion}
LicenseFile=.\Resources\LICENSE
SetupIconFile=.\Resources\icon.ico
Uninstallable=no
UsePreviousAppDir=no
Compression=lzma
SolidCompression=yes
InternalCompressLevel=max
MinVersion=0,6.1

#include <idp.iss>

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "..\bin\x86\Release\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall shellexec

[Code]
function TDGetNetFrameworkVersion: Cardinal; forward;
function TDGetAppVersionClean: String; forward;
procedure TDExecuteFullDownload; forward;

{ Check .NET Framework version on startup, ask user if they want to proceed if older than 4.5.2, and prepare full download package. }
function InitializeSetup: Boolean;
begin
  idpAddFile('https://github.com/{#MyAppPublisher}/{#MyAppName}/releases/download/'+TDGetAppVersionClean()+'/{#MyAppName}.exe', ExpandConstant('{tmp}\{#MyAppName}.Full.exe'));
  
  if TDGetNetFrameworkVersion() >= 379893 then
  begin
    Result := True;
    Exit;
  end;
  
  if (MsgBox('{#MyAppName} requires .NET Framework 4.5.2 or newer,'+#13+#10+'please download it from {#MyAppURL}'+#13+#10+#13+#10'Do you want to proceed with the setup anyway?', mbCriticalError, MB_YESNO or MB_DEFBUTTON2) = IDNO) then
  begin
    Result := False;
    Exit;
  end;
  
  Result := True;
end;

{ Prepare download plugin if there are any files to download, and set the installation path. }
procedure InitializeWizard();
begin
  idpDownloadAfter(wpReady);
end;

{ Remove uninstallation data and application to force them to be replaced with updated ones. }
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssInstall then
  begin
    TDExecuteFullDownload();
  end;
end;

{ Return DWORD value containing the build version of .NET Framework. }
function TDGetNetFrameworkVersion: Cardinal;
var FrameworkVersion: Cardinal;

begin
  if RegQueryDWordValue(HKEY_LOCAL_MACHINE, 'Software\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', FrameworkVersion) then
  begin
    Result := FrameworkVersion;
    Exit;
  end;
  
  Result := 0;
end;

{ Return a cleaned up form of the app version string (removes all .0 suffixes). }
function TDGetAppVersionClean: String;
var Substr: String;
var CleanVersion: String;

begin
  CleanVersion := '{#MyAppVersion}'
  
  while True do
  begin
    Substr := Copy(CleanVersion, Length(CleanVersion)-1, 2);
    
    if (CompareStr(Substr, '.0') <> 0) then
    begin
      break;
    end;
    
    CleanVersion := Copy(CleanVersion, 1, Length(CleanVersion)-2);
  end;
  
  Result := CleanVersion;
end;

{ Run the full package installer if downloaded. }
procedure TDExecuteFullDownload;
var InstallFile: String;
var ResultCode: Integer;

begin
  InstallFile := ExpandConstant('{tmp}\{#MyAppName}.Full.exe')
  WizardForm.ProgressGauge.Style := npbstMarquee;
  
  try
    if Exec(InstallFile, '/SP- /SILENT /MERGETASKS="!desktopicon" /UPDATEPATH="'+ExpandConstant('{app}\')+'" /PORTABLEINSTALL=1', '', SW_SHOW, ewWaitUntilTerminated, ResultCode) then begin
      if ResultCode <> 0 then
      begin
        DeleteFile(InstallFile);
        Abort();
        Exit;
      end;
    end else
    begin
      MsgBox('Could not run the full installer in portable mode. Error: '+SysErrorMessage(ResultCode), mbCriticalError, MB_OK);
      
      DeleteFile(InstallFile);
      Abort();
      Exit;
    end;
  finally
    WizardForm.ProgressGauge.Style := npbstNormal;
    DeleteFile(InstallFile);
  end;
end;