"""Génère res/icon.ico : le disque du logo GORGY TIMING (rouge, horloge verte),
couleurs reprises de la fenêtre de réglages de l'écran de veille d'origine.

Usage : python3 tools/gen_icon.py res/icon.ico      (dépendance : Pillow)
"""
import sys

from PIL import Image, ImageDraw

RED = (255, 0, 0, 255)
GREEN = (173, 231, 41, 255)
CLOCK_RATIO = 0.38  # comme src/Logo.cs
SIZES = [16, 24, 32, 48, 64, 128, 256]
SUPERSAMPLE = 8


def render(size):
    big = size * SUPERSAMPLE
    img = Image.new("RGBA", (big, big), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    draw.ellipse((0, 0, big - 1, big - 1), fill=RED)
    clock = big * CLOCK_RATIO
    o = (big - clock) / 2
    # Trois quarts de disque, du haut (−90°) dans le sens horaire jusqu'à 9 h
    draw.pieslice((o, o, o + clock, o + clock), -90, 180, fill=GREEN)
    return img.resize((size, size), Image.LANCZOS)


def main():
    images = [render(s) for s in SIZES]
    images[-1].save(sys.argv[1], format="ICO", sizes=[(s, s) for s in SIZES], append_images=images[:-1])


if __name__ == "__main__":
    main()
