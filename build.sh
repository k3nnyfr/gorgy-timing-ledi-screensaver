#!/bin/sh
# Compile GorgyLedi.scr sous Linux avec Mono (utilisé par la CI).
# Équivalent de build.bat ; le résultat tourne sur le .NET Framework 4.8 de Windows.
# Dépendance : paquet mono-devel (ou mono-mcs + bibliothèques WinForms/Drawing).
set -eu
cd "$(dirname "$0")"
mcs -nologo -target:winexe -platform:anycpu -optimize+ -codepage:utf8 \
    -langversion:5 \
    -out:GorgyLedi.scr \
    -r:System.Windows.Forms.dll -r:System.Drawing.dll \
    -win32icon:res/icon.ico \
    src/*.cs
echo "GorgyLedi.scr compilé avec succès."
