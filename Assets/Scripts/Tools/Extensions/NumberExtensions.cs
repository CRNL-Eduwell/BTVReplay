using System;

namespace BrainTV.Tools.NumberExtensions
{
    public static class NumberExtensions
    {
        public static bool TryParseFloat(this string text, out float result)
        {
            System.Globalization.CultureInfo[] cultures = new System.Globalization.CultureInfo[]
            {
                System.Globalization.CultureInfo.CurrentCulture,
                System.Globalization.CultureInfo.CreateSpecificCulture("fr-FR"),
                System.Globalization.CultureInfo.CreateSpecificCulture("en-GB"),
                System.Globalization.CultureInfo.CreateSpecificCulture("en-US"),
                System.Globalization.CultureInfo.InvariantCulture
            };
            foreach (var culture in cultures)
            {
                try
                {
                    if (float.TryParse(text, System.Globalization.NumberStyles.Float, culture, out result))
                    {
                        return true;
                    }
                }
                catch
                {
                    continue;
                }
            }
            result = 0;
            return false;
        }

        public static bool TryParseInt(this string value, out int result)
        {
            try
            {
                if (Int32.TryParse(value, out result))
                {
                    return true;
                }
            }
            catch (FormatException)
            {
                UnityEngine.Debug.Log("Unable to convert " + value);
            }
            catch (OverflowException)
            {
                Console.WriteLine(value + "is out of range of the Int32 type.");
            }
            result = 0;
            return false;
        }
    }
}
