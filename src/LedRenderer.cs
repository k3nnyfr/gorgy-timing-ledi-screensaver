using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace GorgyLedi
{
    /// <summary>
    /// Dessine une horloge LEDI : couronne de 60 LED pour les secondes, 12 repères horaires
    /// et chiffres formés de points LED. Toute la géométrie est exprimée en « pas »
    /// (distance entre deux LED voisines d'un chiffre).
    /// </summary>
    static class LedRenderer
    {
        // Bits des segments : a=haut, b=haut droite, c=bas droite, d=bas, e=bas gauche, f=haut gauche, g=milieu
        static readonly int[] DigitMasks = { 0x3F, 0x06, 0x5B, 0x4F, 0x66, 0x6D, 0x7D, 0x07, 0x7F, 0x6F };
        const int DashMask = 0x40;

        // Matrice de points d'un chiffre (5 × 9, coins vides comme sur l'horloge)
        const int Cols = 5;
        const int Rows = 9;
        const float DigitGap = 2.2f;     // écart entre deux chiffres
        const float ColonGap = 2.6f;     // écart entre un chiffre et les deux-points
        const float Slant = 0.17f;       // inclinaison des chiffres vers la droite
        const float DigitDot = 0.72f;    // diamètre d'une LED de chiffre

        // Couronne
        const float RingRadius = 16f;
        const float RingDot = 0.72f;
        const float MarkerRadius = 17.3f; // LED supplémentaire à chaque repère des 5 minutes
        const float Extent = MarkerRadius + RingDot; // rayon total de l'horloge

        // Lignes secondaires (date au-dessus, secondes en dessous)
        const float SmallScale = 0.5f;
        const float LineOffset = 8f;      // distance entre le centre de l'heure et celui d'une ligne secondaire

        // Logo GORGY TIMING (blanc, vectoriel), sous l'heure
        const float LogoWidth = 7.2f;
        const float LogoOffset = 10.8f;            // centre du logo sous le centre de l'horloge
        const float LogoOffsetWithSeconds = 12.6f; // plus bas quand les secondes en chiffres sont affichées

        const float Fill = 0.92f;        // part de la hauteur (ou largeur) d'écran occupée

        public static void Render(Graphics g, Rectangle area, DateTime now, Settings settings, bool colonOn)
        {
            g.Clear(Color.Black);
            if (area.Width <= 0 || area.Height <= 0)
                return;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float pitch = Math.Min(area.Width, area.Height) * Fill / (2 * Extent);
            float cx = area.X + area.Width / 2f;
            float cy = area.Y + area.Height / 2f;

            Color lit = settings.LedColor;
            using (var leds = new Leds(lit, settings.ShowUnlit))
            {
                // Couronne des secondes : les LED s'allument une à une depuis midi
                int second = now.Second;
                for (int i = 0; i < 60; i++)
                    leds.Dot(PointOnCircle(cx, cy, RingRadius * pitch, i * 6), RingDot * pitch, i <= second);

                // Repères des 5 minutes, toujours allumés
                for (int i = 0; i < 12; i++)
                    leds.Dot(PointOnCircle(cx, cy, MarkerRadius * pitch, i * 30), RingDot * pitch, true);

                DrawText(leds, now.ToString("HH:mm", CultureInfo.InvariantCulture), cx, cy, pitch, colonOn);
                if (settings.ShowDate)
                    DrawText(leds, now.ToString("dd.MM.yy", CultureInfo.InvariantCulture),
                             cx, cy - LineOffset * pitch, pitch * SmallScale, true);
                if (settings.ShowSeconds)
                    DrawText(leds, now.ToString("ss", CultureInfo.InvariantCulture),
                             cx, cy + LineOffset * pitch, pitch * SmallScale, true);

                leds.Flush(g);
            }

            if (settings.ShowLogo)
            {
                float y = cy + (settings.ShowSeconds ? LogoOffsetWithSeconds : LogoOffset) * pitch;
                Logo.Draw(g, cx, y, LogoWidth * pitch, Color.White, Color.Black);
            }
        }

        static PointF PointOnCircle(float cx, float cy, float radius, float degrees)
        {
            double rad = degrees * Math.PI / 180;
            return new PointF(cx + (float)(radius * Math.Sin(rad)), cy - (float)(radius * Math.Cos(rad)));
        }

        static bool IsSeparator(char c)
        {
            return c == ':' || c == '.';
        }

        // Ligne de texte centrée sur (cx, cy), inclinée comme sur l'horloge
        static void DrawText(Leds leds, string text, float cx, float cy, float pitch, bool colonOn)
        {
            float width = 0;
            for (int i = 0; i < text.Length; i++)
            {
                width += IsSeparator(text[i]) ? 0 : Cols - 1;
                if (i > 0)
                    width += IsSeparator(text[i]) || IsSeparator(text[i - 1]) ? ColonGap : DigitGap;
            }

            float x = cx - width * pitch / 2;
            float top = cy - (Rows - 1) * pitch / 2;
            float dot = DigitDot * pitch;
            Func<float, float, PointF> led = (px, py) => new PointF(px + (cy - py) * Slant, py);

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (i > 0)
                    x += (IsSeparator(c) || IsSeparator(text[i - 1]) ? ColonGap : DigitGap) * pitch;

                if (c == ':')
                {
                    leds.Dot(led(x, top + 2.5f * pitch), dot, colonOn);
                    leds.Dot(led(x, top + 5.5f * pitch), dot, colonOn);
                }
                else if (c == '.')
                {
                    leds.Dot(led(x, top + (Rows - 1) * pitch), dot, true);
                }
                else
                {
                    int mask = c >= '0' && c <= '9' ? DigitMasks[c - '0'] : c == '-' ? DashMask : 0;
                    for (int row = 0; row < Rows; row++)
                        for (int col = 0; col < Cols; col++)
                        {
                            int segments = SegmentsAt(row, col);
                            if (segments != 0)
                                leds.Dot(led(x + col * pitch, top + row * pitch), dot, (mask & segments) != 0);
                        }
                    x += (Cols - 1) * pitch;
                }
            }
        }

        // Segment auquel appartient une LED de la matrice (0 pour les coins et l'intérieur)
        static int SegmentsAt(int row, int col)
        {
            const int middle = Rows / 2, last = Rows - 1, right = Cols - 1;
            bool innerCol = col > 0 && col < right;
            bool upper = row > 0 && row < middle;
            bool lower = row > middle && row < last;
            if (innerCol)
                return row == 0 ? 0x01 : row == middle ? 0x40 : row == last ? 0x08 : 0;   // a, g, d
            if (col == right)
                return upper ? 0x02 : lower ? 0x04 : 0;                                   // b, c
            if (col == 0)
                return upper ? 0x20 : lower ? 0x10 : 0;                                   // f, e
            return 0;
        }

        /// <summary>
        /// Accumule les LED puis les dessine en trois passes (éteintes, halo, allumées)
        /// pour que les halos ne recouvrent pas les LED voisines.
        /// </summary>
        sealed class Leds : IDisposable
        {
            readonly GraphicsPath unlit = new GraphicsPath(FillMode.Winding);
            readonly GraphicsPath glow = new GraphicsPath(FillMode.Winding);
            readonly GraphicsPath lit = new GraphicsPath(FillMode.Winding);
            readonly Color color;
            readonly bool showUnlit;

            public Leds(Color color, bool showUnlit)
            {
                this.color = color;
                this.showUnlit = showUnlit;
            }

            public void Dot(PointF center, float diameter, bool on)
            {
                if (on)
                {
                    float halo = diameter * 1.7f;
                    glow.AddEllipse(center.X - halo / 2, center.Y - halo / 2, halo, halo);
                    lit.AddEllipse(center.X - diameter / 2, center.Y - diameter / 2, diameter, diameter);
                }
                else if (showUnlit)
                {
                    unlit.AddEllipse(center.X - diameter / 2, center.Y - diameter / 2, diameter, diameter);
                }
            }

            public void Flush(Graphics g)
            {
                using (var unlitBrush = new SolidBrush(Color.FromArgb(color.R / 7, color.G / 7, color.B / 7)))
                using (var glowBrush = new SolidBrush(Color.FromArgb(45, color)))
                using (var litBrush = new SolidBrush(color))
                {
                    g.FillPath(unlitBrush, unlit);
                    g.FillPath(glowBrush, glow);
                    g.FillPath(litBrush, lit);
                }
            }

            public void Dispose()
            {
                unlit.Dispose();
                glow.Dispose();
                lit.Dispose();
            }
        }
    }
}
