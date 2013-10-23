!include "MUI2.nsh"
!include "version.nsh"

Name "Universal File Editor"

OutFile "setup.exe"

InstallDir "$PROGRAMFILES\Universal File Editor"

RequestExecutionLevel admin

!define MUI_ABORTWARNING

!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES

!define MUI_FINISHPAGE_NOAUTOCLOSE
!define MUI_FINISHPAGE_RUN_NOTCHECKED
!define MUI_FINISHPAGE_RUN "$INSTDIR\UniversalEditor.exe"
!define MUI_FINISHPAGE_RUN_TEXT "Launch the program after exit"

!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

!insertmacro MUI_LANGUAGE "English"

VIProductVersion "${VERSION}"
VIAddVersionKey "ProductName" "Universal File Editor"
VIAddVersionKey "FileDescription" "https://universaleditor.codeplex.com/"
VIAddVersionKey "FileVersion" "${VERSION}"

Section

  SetOutPath $INSTDIR
  
  File ..\compilation.output\UniversalEditor.exe
  File ..\compilation.output\UniversalEditor.exe.config
  File ..\compilation.output\UniversalEditor.Base.dll
  File ..\compilation.output\UniversalEditor.Csv.dll
  File ..\compilation.output\UniversalEditor.Bat.dll
  File ..\compilation.output\UniversalEditor.Actionscript.dll
  File ..\compilation.output\UniversalEditor.CSharp.dll
  File ..\compilation.output\UniversalEditor.Java.dll
  File ..\compilation.output\UniversalEditor.Nsis.dll
  File ..\compilation.output\UniversalEditor.Audio.dll
  File ..\compilation.output\UniversalEditor.Video.dll
  File ..\compilation.output\UniversalEditor.PlainImage.dll
  File ..\compilation.output\UniversalEditor.PlainText.dll
  File ..\compilation.output\UniversalEditor.Pdf.dll
  File ..\compilation.output\UniversalEditor.Icon.dll
  File ..\compilation.output\UniversalEditor.Xml.dll
  File ..\compilation.output\TextEditor.dll
  File ..\compilation.output\Raccoom.Xml.dll
  File ..\compilation.output\TextEditor.ScriptHighlight.dll
  File ..\compilation.output\UniversalEditor.OpenXml.dll
  File ..\compilation.output\DocumentFormat.OpenXml.dll
  File ..\compilation.output\WPFPdfViewer.dll
  File ..\compilation.output\AxInterop.AcroPDFLib.dll
  File ..\compilation.output\Interop.AcroPDFLib.dll
  File ..\compilation.output\Microsoft.DirectX.AudioVideoPlayback.dll
  File ..\compilation.output\Microsoft.DirectX.dll
  File ..\compilation.output\SevenZipSharp.dll
  File ..\compilation.output\WinForms.Utils.dll
  File ..\compilation.output\UniversalEditor.Archive.dll
  File ..\compilation.output\7z.dll
  File ..\compilation.output\7z64.dll
  
  SetShellVarContext all  
  
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\UniversalFileEditor" "DisplayName" "Universal File Editor"  
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\UniversalFileEditor" "UninstallString" "$INSTDIR\uninstall.exe"  
  
  WriteUninstaller "uninstall.exe"
  
SectionEnd

Section "Start Menu Shortcuts"

  CreateDirectory "$SMPROGRAMS\Universal File Editor"
  CreateShortCut "$SMPROGRAMS\Universal File Editor\Universal File Editor.lnk" "$INSTDIR\UniversalEditor.exe"
  CreateShortCut "$SMPROGRAMS\Universal File Editor\Uninstall.lnk" "$INSTDIR\uninstall.exe"
  
  CreateShortCut "$DESKTOP\Universal File Editor.lnk" "$INSTDIR\UniversalEditor.exe"
  
SectionEnd

Section "Context Menu Integration"

  SetShellVarContext all  
  
  WriteRegStr HKCR "*\shell\UniversalFileEditor" "" "Universal File Editor"
  WriteRegStr HKCR "*\shell\UniversalFileEditor" "Icon" "$INSTDIR\UniversalEditor.exe"
  WriteRegStr HKCR "*\shell\UniversalFileEditor\command" "" '"$INSTDIR\UniversalEditor.exe" "%1"'
  
  
SectionEnd

Section "Uninstall"
  
  Delete $INSTDIR\*.*
  RMDir "$INSTDIR"

  Delete "$SMPROGRAMS\Universal File Editor\*.*"
  RMDir "$SMPROGRAMS\Universal File Editor\"
  
  Delete "$DESKTOP\Universal File Editor.lnk"

  SetShellVarContext all  
  DeleteRegKey HKLM Software\Microsoft\Windows\CurrentVersion\Uninstall\UniversalFileEditor
  DeleteRegKey HKCR *\shell\UniversalFileEditor
  
SectionEnd




