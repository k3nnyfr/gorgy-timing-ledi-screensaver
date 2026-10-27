using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace GorgyLedi
{
    /// <summary>
    /// Logo GORGY TIMING entièrement vectoriel (net à toutes les résolutions) :
    /// disque avec horloge stylisée, texte en dessous. Unités : hauteur des capitales = 100.
    /// </summary>
    static class Logo
    {
        // Proportions relevées sur le logo de l'écran de veille d'origine
        const float DiscDiameter = 135f;
        const float ClockRatio = 0.38f;   // diamètre de l'horloge / diamètre du disque
        const float Gap = 55f;            // entre le disque et le haut du texte

        public const float Width = LogoGeometry.TextWidth;
        public const float Height = DiscDiameter + Gap + LogoGeometry.CapHeight;

        static readonly GraphicsPath Text = ParsePath(LogoGeometry.TextPath);

        /// <summary>Dessine le logo, de largeur <paramref name="width"/>, centré sur (cx, cy).</summary>
        public static void Draw(Graphics g, float cx, float cy, float width, Color color, Color background)
        {
            float scale = width / Width;
            GraphicsState state = g.Save();
            g.TranslateTransform(cx - width / 2, cy - Height * scale / 2);
            g.ScaleTransform(scale, scale);
            using (var brush = new SolidBrush(color))
            using (var hole = new SolidBrush(background))
            {
                float discX = (Width - DiscDiameter) / 2;
                g.FillEllipse(brush, discX, 0, DiscDiameter, DiscDiameter);

                // Horloge : trois quarts de disque, le quart haut-gauche restant plein
                float clock = DiscDiameter * ClockRatio;
                float offset = (DiscDiameter - clock) / 2;
                g.FillPie(hole, discX + offset, offset, clock, clock, -90f, 270f);

                g.TranslateTransform(0, Height); // ligne de base du texte
                g.FillPath(brush, Text);
            }
            g.Restore(state);
        }

        // Lit un tracé « M x y | L x y | C x1 y1 x2 y2 x y | Z »
        static GraphicsPath ParsePath(string data)
        {
            var path = new GraphicsPath(FillMode.Winding);
            string[] tokens = data.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            PointF current = PointF.Empty;
            int i = 0;
            Func<PointF> next = () =>
            {
                float x = float.Parse(tokens[i++], CultureInfo.InvariantCulture);
                float y = float.Parse(tokens[i++], CultureInfo.InvariantCulture);
                return new PointF(x, y);
            };

            while (i < tokens.Length)
            {
                switch (tokens[i++])
                {
                    case "M":
                        path.StartFigure();
                        current = next();
                        break;
                    case "L":
                        PointF to = next();
                        path.AddLine(current, to);
                        current = to;
                        break;
                    case "C":
                        PointF c1 = next(), c2 = next(), end = next();
                        path.AddBezier(current, c1, c2, end);
                        current = end;
                        break;
                    case "Z":
                        path.CloseFigure();
                        break;
                    default:
                        throw new FormatException("Commande de tracé inconnue : " + tokens[i - 1]);
                }
            }
            return path;
        }
    }
}
