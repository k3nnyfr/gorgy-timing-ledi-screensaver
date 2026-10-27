@echo off
rem Compile GorgyLedi.scr avec le compilateur C# fourni avec Windows (.NET Framework 4.8).
rem Aucun SDK ni Visual Studio necessaire.
setlocal
set CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe
if not exist "%CSC%" set CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe
if not exist "%CSC%" (
  echo Compilateur C# introuvable : activez .NET Framework 4.8 dans les fonctionnalites Windows.
  exit /b 1
)
cd /d "%~dp0"
"%CSC%" /nologo /target:winexe /platform:anycpu /optimize+ /codepage:65001 ^
  /out:GorgyLedi.scr ^
  /r:System.Windows.Forms.dll /r:System.Drawing.dll ^
  /win32icon:res\icon.ico ^
  src\*.cs
if errorlevel 1 (
  echo ECHEC de la compilation.
  exit /b 1
)
echo GorgyLedi.scr compile avec succes.
