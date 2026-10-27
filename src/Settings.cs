using System;
using System.Drawing;
using System.Globalization;
using Microsoft.Win32;

namespace GorgyLedi
{
    /// <summary>Réglages de l'écran de veille, stockés dans HKCU\Software\GorgyTiming\LediScreenSaver.</summary>
    sealed class Settings
    {
        const string KeyPath = @"Software\GorgyTiming\LediScreenSaver";

        public Color LedColor = Color.FromArgb(255, 32, 32);
        public bool ShowSeconds = false; // secondes en chiffres (la couronne les montre déjà)
        public bool ShowDate = false;
        public bool BlinkColon = true;
        public bool ShowUnlit = false;
        public bool ShowLogo = true;
        public string Language = DefaultLanguage();

        public static Settings Load()
        {
            var s = new Settings();
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(KeyPath))
                {
                    if (key == null)
                        return s;
                    object color = key.GetValue("Color");
                    if (color is int)
                        s.LedColor = Color.FromArgb(255, Color.FromArgb((int)color));
                    s.ShowSeconds = ReadBool(key, "ShowSeconds", s.ShowSeconds);
                    s.ShowDate = ReadBool(key, "ShowDate", s.ShowDate);
                    s.BlinkColon = ReadBool(key, "BlinkColon", s.BlinkColon);
                    s.ShowUnlit = ReadBool(key, "ShowUnlit", s.ShowUnlit);
                    s.ShowLogo = ReadBool(key, "ShowLogo", s.ShowLogo);
                    string lang = key.GetValue("Language") as string;
                    if (Array.IndexOf(Strings.Languages, lang) >= 0)
                        s.Language = lang;
                }
            }
            catch (Exception)
            {
                // Réglages illisibles : on garde les valeurs par défaut
            }
            return s;
        }

        public void Save()
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(KeyPath))
            {
                key.SetValue("Color", LedColor.ToArgb(), RegistryValueKind.DWord);
                key.SetValue("ShowSeconds", ShowSeconds ? 1 : 0, RegistryValueKind.DWord);
                key.SetValue("ShowDate", ShowDate ? 1 : 0, RegistryValueKind.DWord);
                key.SetValue("BlinkColon", BlinkColon ? 1 : 0, RegistryValueKind.DWord);
                key.SetValue("ShowUnlit", ShowUnlit ? 1 : 0, RegistryValueKind.DWord);
                key.SetValue("ShowLogo", ShowLogo ? 1 : 0, RegistryValueKind.DWord);
                key.SetValue("Language", Language, RegistryValueKind.String);
            }
        }

        static bool ReadBool(RegistryKey key, string name, bool defaultValue)
        {
            object value = key.GetValue(name);
            return value is int ? (int)value != 0 : defaultValue;
        }

        static string DefaultLanguage()
        {
            string lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            return Array.IndexOf(Strings.Languages, lang) >= 0 ? lang : "en";
        }
    }
}
