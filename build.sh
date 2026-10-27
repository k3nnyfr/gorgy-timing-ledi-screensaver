#!/bin/sh
# Compile GorgyLedi.scr sous Linux avec Mono (utilisé par la CI).
# Équivalent de build.bat ; le résultat tourne sur le .NET Framework 4.8 de Windows.
#
# On compile contre les assemblys de référence .NET Framework 4.8 fournies par Mono
# (/usr/lib/mono/4.8-api, même surface d'API que Windows) et non contre les
# bibliothèques d'exécution de Mono : celles-ci exposent des méthodes absentes du
# .NET Framework (ex. string.TrimStart(char)), qui provoqueraient une
# MissingMethodException sous Windows.
#
# Dépendance : paquet mono-devel.
set -eu
cd "$(dirname "$0")"

REFS=${NET48_REFS:-/usr/lib/mono/4.8-api}
if [ ! -f "$REFS/mscorlib.dll" ]; then
    echo "Assemblys de référence .NET 4.8 introuvables dans $REFS (installer mono-devel)." >&2
    exit 1
fi

mcs -nologo -noconfig -nostdlib -target:winexe -platform:anycpu -optimize+ -codepage:utf8 \
    -langversion:5 \
    -out:GorgyLedi.scr \
    -r:"$REFS/mscorlib.dll" -r:"$REFS/System.dll" \
    -r:"$REFS/System.Drawing.dll" -r:"$REFS/System.Windows.Forms.dll" \
    -win32icon:res/icon.ico \
    src/*.cs
echo "GorgyLedi.scr compilé avec succès."
