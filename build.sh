#!/bin/sh
# Compile GorgyLedi.scr sous Linux avec Mono (utilisé par la CI).
# Équivalent de build.bat ; le résultat tourne sur le .NET Framework 4.8 de Windows.
#
# On compile contre les assemblys de référence officielles du .NET Framework 4.8
# (paquet NuGet Microsoft) et non contre les bibliothèques de Mono : celles-ci exposent
# des méthodes absentes du .NET Framework (ex. string.TrimStart(char)), qui
# provoqueraient une MissingMethodException sous Windows.
#
# Dépendances : mono-mcs (ou mono-devel), curl, unzip, sha256sum.
set -eu
cd "$(dirname "$0")"

REFS_VERSION=1.0.3
REFS_SHA256=8a7e348538e7eb91351696911689f49e3d4f63f8bab517432bbe159b8b1104a2
REFS_DIR=.cache/net48-refs-$REFS_VERSION
REFS=$REFS_DIR/build/.NETFramework/v4.8

if [ ! -f "$REFS/mscorlib.dll" ]; then
    mkdir -p .cache
    PKG=.cache/net48-refs-$REFS_VERSION.nupkg
    curl -fsSL -o "$PKG" \
        "https://www.nuget.org/api/v2/package/Microsoft.NETFramework.ReferenceAssemblies.net48/$REFS_VERSION"
    echo "$REFS_SHA256  $PKG" | sha256sum -c --quiet
    rm -rf "$REFS_DIR"
    unzip -q "$PKG" -d "$REFS_DIR"
    rm "$PKG"
fi

mcs -nologo -noconfig -nostdlib -target:winexe -platform:anycpu -optimize+ -codepage:utf8 \
    -langversion:5 \
    -out:GorgyLedi.scr \
    -r:"$REFS/mscorlib.dll" -r:"$REFS/System.dll" \
    -r:"$REFS/System.Drawing.dll" -r:"$REFS/System.Windows.Forms.dll" \
    -win32icon:res/icon.ico \
    src/*.cs
echo "GorgyLedi.scr compilé avec succès."
