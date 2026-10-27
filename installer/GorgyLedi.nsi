; Installeur de l'écran de veille GORGY TIMING LEDI
; Compilation : makensis installer/GorgyLedi.nsi   (après build.bat ; voir README)
; Version : makensis -DVERSION=1.2.3 installer/GorgyLedi.nsi

Unicode true
SetCompressor /SOLID lzma

!ifndef VERSION
  !define VERSION "1.0.2"
!endif
!define PRODUCT     "GORGY TIMING LEDI"
!define SCR_FILE    "GorgyLedi.scr"
!define UNINST_KEY  "Software\Microsoft\Windows\CurrentVersion\Uninstall\GorgyLediScreenSaver"
!define SETTINGS_KEY "Software\GorgyTiming\LediScreenSaver"

!include "MUI2.nsh"
!include "LogicLib.nsh"
!include "x64.nsh"

Name "${PRODUCT}"
OutFile "../dist/GorgyLedi-Setup-${VERSION}.exe"
InstallDir "$PROGRAMFILES64\GORGY TIMING\LEDI Screen Saver"
RequestExecutionLevel admin
ManifestDPIAware true

VIProductVersion "${VERSION}.0"
VIAddVersionKey /LANG=0 "ProductName" "${PRODUCT}"
VIAddVersionKey /LANG=0 "FileDescription" "${PRODUCT} screen saver setup"
VIAddVersionKey /LANG=0 "FileVersion" "${VERSION}"
VIAddVersionKey /LANG=0 "ProductVersion" "${VERSION}"

!define MUI_ICON   "../res/icon.ico"
!define MUI_UNICON "../res/icon.ico"
!define MUI_ABORTWARNING
!define MUI_COMPONENTSPAGE_NODESC
!define MUI_FINISHPAGE_RUN
!define MUI_FINISHPAGE_RUN_TEXT "$(OpenSettings)"
!define MUI_FINISHPAGE_RUN_FUNCTION OpenScreenSaverSettings

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

!insertmacro MUI_LANGUAGE "French"
!insertmacro MUI_LANGUAGE "English"

LangString SecMain      ${LANG_FRENCH}  "Écran de veille ${PRODUCT}"
LangString SecMain      ${LANG_ENGLISH} "${PRODUCT} screen saver"
LangString SecActivate  ${LANG_FRENCH}  "Définir comme écran de veille actif"
LangString SecActivate  ${LANG_ENGLISH} "Set as the active screen saver"
LangString OpenSettings ${LANG_FRENCH}  "Ouvrir les paramètres de l'écran de veille"
LangString OpenSettings ${LANG_ENGLISH} "Open the screen saver settings"

Function .onInit
  !insertmacro MUI_LANGDLL_DISPLAY
  ${If} ${RunningX64}
    SetRegView 64
  ${EndIf}
FunctionEnd

Function un.onInit
  ${If} ${RunningX64}
    SetRegView 64
  ${EndIf}
FunctionEnd

; Le .scr doit être dans le vrai System32 pour apparaître dans la liste de Windows :
; on désactive la redirection vers SysWOW64 (l'installeur NSIS est un programme 32 bits).
!macro SystemDirAccess
  ${If} ${RunningX64}
    ${DisableX64FSRedirection}
  ${EndIf}
!macroend
!macro SystemDirAccessEnd
  ${If} ${RunningX64}
    ${EnableX64FSRedirection}
  ${EndIf}
!macroend

Section "$(SecMain)" SEC_MAIN
  SectionIn RO
  !insertmacro SystemDirAccess
  SetOutPath "$WINDIR\System32"
  File "../${SCR_FILE}"
  !insertmacro SystemDirAccessEnd

  SetOutPath "$INSTDIR"
  WriteUninstaller "$INSTDIR\Uninstall.exe"

  WriteRegStr   HKLM "${UNINST_KEY}" "DisplayName"     "${PRODUCT}"
  WriteRegStr   HKLM "${UNINST_KEY}" "DisplayVersion"  "${VERSION}"
  WriteRegStr   HKLM "${UNINST_KEY}" "Publisher"       "GORGY TIMING"
  WriteRegStr   HKLM "${UNINST_KEY}" "DisplayIcon"     "$WINDIR\System32\${SCR_FILE}"
  WriteRegStr   HKLM "${UNINST_KEY}" "UninstallString" '"$INSTDIR\Uninstall.exe"'
  WriteRegDWORD HKLM "${UNINST_KEY}" "NoModify" 1
  WriteRegDWORD HKLM "${UNINST_KEY}" "NoRepair" 1
SectionEnd

Section "$(SecActivate)" SEC_ACTIVATE
  ; Pour l'utilisateur qui lance l'installeur
  WriteRegStr HKCU "Control Panel\Desktop" "SCRNSAVE.EXE" "$WINDIR\System32\${SCR_FILE}"
  WriteRegStr HKCU "Control Panel\Desktop" "ScreenSaveActive" "1"
  ; SPI_SETSCREENSAVEACTIVE, avec SPIF_UPDATEINIFILE | SPIF_SENDCHANGE
  System::Call 'user32::SystemParametersInfoW(i 17, i 1, p 0, i 3)'
SectionEnd

Function OpenScreenSaverSettings
  Exec '"$WINDIR\System32\control.exe" desk.cpl,,@screensaver'
FunctionEnd

Section "Uninstall"
  !insertmacro SystemDirAccess
  Delete "$WINDIR\System32\${SCR_FILE}"
  !insertmacro SystemDirAccessEnd

  ; Ne désactive l'écran de veille que s'il s'agit du nôtre
  ReadRegStr $0 HKCU "Control Panel\Desktop" "SCRNSAVE.EXE"
  ${If} $0 == "$WINDIR\System32\${SCR_FILE}"
    DeleteRegValue HKCU "Control Panel\Desktop" "SCRNSAVE.EXE"
  ${EndIf}
  DeleteRegKey HKCU "${SETTINGS_KEY}"
  DeleteRegKey /ifempty HKCU "Software\GorgyTiming"

  DeleteRegKey HKLM "${UNINST_KEY}"
  Delete "$INSTDIR\Uninstall.exe"
  RMDir "$INSTDIR"
  RMDir "$INSTDIR\.."
SectionEnd
