using System;
using System.Drawing;
using System.Windows.Forms;

namespace GorgyLedi
{
    /// <summary>
    /// Fenêtre de l'écran de veille : plein écran (une par moniteur) ou enfant
    /// de la petite fenêtre d'aperçu des paramètres Windows.
    /// </summary>
    sealed class SaverForm : Form
    {
        const int MouseTolerance = 8; // pixels de mouvement tolérés avant de quitter

        readonly Settings settings;
        readonly IntPtr previewParent;
        readonly bool isPreview;
        readonly Timer timer = new Timer();

        Point? mouseOrigin;
        string lastState;

        // Plein écran sur un moniteur
        public SaverForm(Rectangle bounds, Settings settings) : this(settings)
        {
            StartPosition = FormStartPosition.Manual;
            Bounds = bounds;
            TopMost = true;
        }

        // Aperçu dans la fenêtre des paramètres (argument /p <hwnd>)
        public SaverForm(IntPtr previewParent, Settings settings) : this(settings)
        {
            this.previewParent = previewParent;
            isPreview = true;
        }

        SaverForm(Settings settings)
        {
            this.settings = settings;
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            BackColor = Color.Black;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            timer.Interval = 100;
            timer.Tick += OnTick;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (isPreview)
            {
                NativeMethods.SetParent(Handle, previewParent);
                NativeMethods.SetWindowLong(Handle, NativeMethods.GWL_STYLE,
                    NativeMethods.GetWindowLong(Handle, NativeMethods.GWL_STYLE) | NativeMethods.WS_CHILD);
                NativeMethods.RECT rect;
                NativeMethods.GetClientRect(previewParent, out rect);
                Location = Point.Empty;
                Size = new Size(rect.Right, rect.Bottom);
            }
            else
            {
                Cursor.Hide();
            }
            timer.Start();
        }

        void OnTick(object sender, EventArgs e)
        {
            if (isPreview && !NativeMethods.IsWindow(previewParent))
            {
                Close();
                return;
            }

            DateTime now = DateTime.Now;
            string state = now.ToString("HHmmss") + ColonOn(now);
            if (state != lastState)
            {
                lastState = state;
                Invalidate();
            }

            // Réarme le timer juste après la prochaine demi-seconde : les secondes et le
            // clignotement des deux-points changent ainsi ensemble, sans la dérive d'un
            // intervalle fixe (le Timer WinForms n'a qu'une résolution d'environ 15 ms).
            timer.Interval = 500 - now.Millisecond % 500 + 10;
        }

        bool ColonOn(DateTime now)
        {
            return !settings.BlinkColon || now.Millisecond < 500;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            DateTime now = DateTime.Now;
            LedRenderer.Render(e.Graphics, ClientRectangle, now, settings, ColonOn(now));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (isPreview)
                return;
            if (!mouseOrigin.HasValue)
            {
                mouseOrigin = e.Location;
                return;
            }
            if (Math.Abs(e.X - mouseOrigin.Value.X) > MouseTolerance || Math.Abs(e.Y - mouseOrigin.Value.Y) > MouseTolerance)
                Application.Exit();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (!isPreview)
                Application.Exit();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (!isPreview)
                Application.Exit();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                timer.Dispose();
            base.Dispose(disposing);
        }
    }
}
