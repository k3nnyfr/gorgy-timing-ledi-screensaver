"""Génère src/LogoGeometry.cs : le texte du logo GORGY TIMING en tracés vectoriels.

Les contours des lettres viennent de TeX Gyre Adventor (clone libre d'Avant Garde Gothic,
GUST Font License), proportionnés pour correspondre au logo de l'écran de veille d'origine.

Usage :
    curl -LO https://mirrors.ctan.org/fonts/tex-gyre/opentype/texgyreadventor-regular.otf
    python3 tools/gen_logo.py texgyreadventor-regular.otf > src/LogoGeometry.cs

Dépendance : fontTools (pip install fonttools).
"""
import sys

from fontTools.pens.boundsPen import BoundsPen
from fontTools.pens.recordingPen import RecordingPen
from fontTools.pens.transformPen import TransformPen
from fontTools.ttLib import TTFont

TEXT = "GORGY TIMING"
CAP_HEIGHT = 100.0     # unités du logo : hauteur des capitales = 100
TARGET_WIDTH = 914.0   # largeur du texte dans le logo d'origine (201 × 22 px → 914 × 100)
X_SCALE = 0.94         # lettres légèrement plus étroites que la police, comme dans le logo
SPACE_SCALE = 0.6      # espace entre « GORGY » et « TIMING » plus courte que celle de la police


def glyph_outlines(font_path):
    font = TTFont(font_path)
    glyph_set = font.getGlyphSet()
    cmap = font.getBestCmap()
    cap = font["OS/2"].sCapHeight
    scale = CAP_HEIGHT / cap

    names = [cmap[ord(c)] for c in TEXT]
    sx = scale * X_SCALE

    def advance(name):
        return glyph_set[name].width * sx * (SPACE_SCALE if name == "space" else 1)

    # Largeur réelle des contours sans approche, puis approche pour retrouver celle du logo
    first, last = BoundsPen(glyph_set), BoundsPen(glyph_set)
    glyph_set[names[0]].draw(first)
    glyph_set[names[-1]].draw(last)
    left = first.bounds[0] * sx
    natural = sum(advance(n) for n in names[:-1]) + last.bounds[2] * sx - left
    tracking = (TARGET_WIDTH - natural) / (len(TEXT) - 1)

    commands = []
    x = -left  # le texte commence à x = 0
    for name in names:
        rec = RecordingPen()
        # Police : y vers le haut ; écran : y vers le bas, ligne de base à y = 0
        glyph_set[name].draw(TransformPen(rec, (sx, 0, 0, -scale, x, 0)))
        for op, pts in rec.value:
            if op == "moveTo":
                commands.append("M " + fmt(pts))
            elif op == "lineTo":
                commands.append("L " + fmt(pts))
            elif op == "curveTo":
                commands.append("C " + fmt(pts))
            elif op == "qCurveTo":
                raise SystemExit("Courbes quadratiques non gérées : utilisez la version OpenType/CFF")
            elif op in ("closePath", "endPath"):
                commands.append("Z")
        x += advance(name) + tracking
    print("approche : %.1f unités" % tracking, file=sys.stderr)
    return commands


def fmt(points):
    return " ".join("%g %g" % (round(px, 2), round(py, 2)) for px, py in points)


def main():
    commands = glyph_outlines(sys.argv[1])
    lines = []
    line = ""
    for cmd in commands:
        if len(line) + len(cmd) > 100:
            lines.append(line)
            line = ""
        line += cmd + " "
    lines.append(line)
    body = " +\n".join('            "%s"' % l for l in lines) + ";"
    print('''// Fichier généré par tools/gen_logo.py : ne pas modifier à la main.
namespace GorgyLedi
{
    static class LogoGeometry
    {
        public const float TextWidth = %g;
        public const float CapHeight = %g;

        // Texte « %s » : M x y | L x y | C x1 y1 x2 y2 x y | Z ; ligne de base à y = 0
        public const string TextPath =
%s
    }
}''' % (TARGET_WIDTH, CAP_HEIGHT, TEXT, body))


if __name__ == "__main__":
    main()
