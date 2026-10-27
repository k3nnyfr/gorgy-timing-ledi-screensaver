@echo off
rem Installe GorgyLedi.scr dans System32 puis ouvre les parametres d'ecran de veille.
rem A lancer en tant qu'administrateur (clic droit > Executer en tant qu'administrateur).
cd /d "%~dp0"
if not exist GorgyLedi.scr (
  echo GorgyLedi.scr introuvable : lancez d'abord build.bat.
  pause
  exit /b 1
)
copy /y GorgyLedi.scr "%WINDIR%\System32\GorgyLedi.scr" >nul
if errorlevel 1 (
  echo Copie impossible : relancez ce script en tant qu'administrateur.
  pause
  exit /b 1
)
reg add "HKCU\Control Panel\Desktop" /v SCRNSAVE.EXE /t REG_SZ /d "%WINDIR%\System32\GorgyLedi.scr" /f >nul
start "" control desk.cpl,,@screensaver
echo Ecran de veille installe et selectionne.
