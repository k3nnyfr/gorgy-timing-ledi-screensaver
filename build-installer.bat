@echo off
rem Compile GorgyLedi.scr puis l'installeur NSIS dans dist\.
rem Necessite NSIS 3 (https://nsis.sourceforge.io) ou "winget install NSIS.NSIS".
setlocal
cd /d "%~dp0"
call build.bat || exit /b 1
set MAKENSIS=%ProgramFiles(x86)%\NSIS\makensis.exe
if not exist "%MAKENSIS%" set MAKENSIS=%ProgramFiles%\NSIS\makensis.exe
if not exist "%MAKENSIS%" (
  echo makensis introuvable : installez NSIS 3.
  exit /b 1
)
if not exist dist mkdir dist
"%MAKENSIS%" /V2 installer\GorgyLedi.nsi || exit /b 1
echo Installeur genere dans dist\.
