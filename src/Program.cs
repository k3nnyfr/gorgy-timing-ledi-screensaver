using System;
using System.Windows.Forms;

namespace GorgyLedi
{
    /// <summary>
    /// Point d'entrée. Windows lance un écran de veille avec :
    ///   /s           plein écran
    ///   /p &lt;hwnd&gt;    aperçu dans la fenêtre des paramètres
    ///   /c[:hwnd]    réglages (ou aucun argument)
    /// </summary>
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            string mode = "";
            string handle = null;
            if (args.Length > 0)
            {
                string first = args[0].Trim().ToLowerInvariant();
                if (first.Length >= 2 && (first[0] == '/' || first[0] == '-'))
                {
                    mode = first.Substring(1, 1);
                    if (first.Length > 2)
                        handle = first.Substring(2).TrimStart(':');
                }
                if (string.IsNullOrEmpty(handle) && args.Length > 1)
                    handle = args[1];
            }

            EnableDpiAwareness(mode == "s" || mode == "p");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Settings settings = Settings.Load();

            switch (mode)
            {
                case "s":
                    RunFullScreen(settings);
                    break;
                case "p":
                    long hwnd;
                    if (long.TryParse(handle, out hwnd))
                        Application.Run(new SaverForm(new IntPtr(hwnd), settings));
                    break;
                default:
                    Application.Run(new ConfigForm(settings));
                    break;
            }
        }

        static void RunFullScreen(Settings settings)
        {
            Screen[] screens = Screen.AllScreens;
            var forms = new SaverForm[screens.Length];
            for (int i = 0; i < screens.Length; i++)
                forms[i] = new SaverForm(screens[i].Bounds, settings);
            for (int i = 1; i < forms.Length; i++)
                forms[i].Show();
            Application.Run(forms[0]);
        }

        // Plein écran : pixels physiques sur chaque moniteur (évite le flou des écrans à 125 %, 150 %…).
        // Réglages : mise à l'échelle système, gérée par la mise en page automatique de WinForms.
        static void EnableDpiAwareness(bool perMonitor)
        {
            try
            {
                if (perMonitor && NativeMethods.SetProcessDpiAwarenessContext(NativeMethods.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2))
                    return;
                NativeMethods.SetProcessDPIAware();
            }
            catch (EntryPointNotFoundException)
            {
                NativeMethods.SetProcessDPIAware();
            }
        }
    }
}
