using System;
using System.Collections.Generic;

namespace GorgyLedi
{
    /// <summary>Textes de la fenêtre de réglages (mêmes langues que l'écran de veille d'origine).</summary>
    static class Strings
    {
        public static readonly string[] Languages = { "fr", "en", "de", "es" };
        public static readonly string[] LanguageNames = { "Français", "English", "Deutsch", "Español" };

        static readonly Dictionary<string, string[]> Table = new Dictionary<string, string[]>
        {
            { "title",   new[] { "Écran de veille GORGY TIMING LEDI", "GORGY TIMING LEDI screen saver", "Bildschirmschoner GORGY TIMING LEDI", "Protector de pantalla GORGY TIMING LEDI" } },
            { "header",  new[] { "Options pour les horloges GORGY TIMING type LEDI", "Available options for the LEDI clocks range", "Optionen für die GORGY TIMING LEDI-Uhren", "Opciones para los relojes GORGY TIMING tipo LEDI" } },
            { "language", new[] { "Langue", "Language", "Sprache", "Idioma" } },
            { "color",   new[] { "Couleur des LED", "LED colour", "LED-Farbe", "Color de los LED" } },
            { "seconds", new[] { "Afficher aussi les secondes en chiffres", "Also show seconds as digits", "Sekunden zusätzlich als Ziffern anzeigen", "Mostrar también los segundos en cifras" } },
            { "date",    new[] { "Afficher la date", "Show date", "Datum anzeigen", "Mostrar la fecha" } },
            { "blink",   new[] { "Faire clignoter les deux-points", "Blink the colon", "Doppelpunkt blinken lassen", "Hacer parpadear los dos puntos" } },
            { "unlit",   new[] { "Afficher les LED éteintes", "Show unlit LEDs", "Ausgeschaltete LEDs anzeigen", "Mostrar los LED apagados" } },
            { "logo",    new[] { "Afficher le logo GORGY TIMING", "Show the GORGY TIMING logo", "GORGY TIMING-Logo anzeigen", "Mostrar el logotipo GORGY TIMING" } },
            { "ok",      new[] { "OK", "OK", "OK", "Aceptar" } },
            { "cancel",  new[] { "Annuler", "Cancel", "Abbrechen", "Cancelar" } },
            { "red",     new[] { "Rouge", "Red", "Rot", "Rojo" } },
            { "green",   new[] { "Vert", "Green", "Grün", "Verde" } },
            { "amber",   new[] { "Ambre", "Amber", "Bernstein", "Ámbar" } },
            { "blue",    new[] { "Bleu", "Blue", "Blau", "Azul" } },
            { "white",   new[] { "Blanc", "White", "Weiß", "Blanco" } },
            { "custom",  new[] { "Personnalisée…", "Custom…", "Benutzerdefiniert…", "Personalizado…" } },
            { "saveError", new[] { "Impossible d'enregistrer les réglages :", "Unable to save the settings:", "Einstellungen konnten nicht gespeichert werden:", "No se pudieron guardar los ajustes:" } },
        };

        public static string Get(string key, string language)
        {
            int index = Array.IndexOf(Languages, language);
            return Table[key][index < 0 ? 0 : index];
        }
    }
}
