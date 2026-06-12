using System;
using System.Globalization;

namespace BrainTV.Tools.NumberExtensions
{
    public static class NumberExtensions
    {
        // Invariant first, so the same file parses identically on every machine (everything
        // the app writes is invariant). The fr-FR fallback covers legacy files and
        // comma-decimal user input ("1,5"). It must stay AFTER invariant: when CurrentCulture
        // came first, the result depended on the machine's locale.
        private static readonly CultureInfo[] s_ParseCultures = new CultureInfo[]
        {
            CultureInfo.InvariantCulture,
            CultureInfo.CreateSpecificCulture("fr-FR"),
        };

        public static bool TryParseFloat(this string text, out float result)
        {
            foreach (CultureInfo culture in s_ParseCultures)
            {
                if (float.TryParse(text, NumberStyles.Float, culture, out result))
                    return true;
            }
            result = 0;
            return false;
        }

        public static bool TryParseInt(this string value, out int result)
        {
            return Int32.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
        }
    }
}
