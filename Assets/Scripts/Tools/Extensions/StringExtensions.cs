using System.Text.RegularExpressions;

namespace BrainTV.Tools.NumberExtensions
{
    public static class StringExtensions
    {
        public static string StandardizeToPath(this string path)
        {
            path = new Regex("/+").Replace(path, "/");
            path = new Regex("\\\\+").Replace(path, "\\");
            path = path.Replace('/', System.IO.Path.DirectorySeparatorChar);
            path = path.Replace('\\', System.IO.Path.DirectorySeparatorChar);
            return path;
        }

        public static string FormatToTimeString(this int time)
        {
            if (time < 10)
            {
                return "0" + time;
            }
            else
            {
                return time.ToString();
            }
        }

        public static string FormatToTimeString(this long time)
        {
            if (time < 10)
            {
                return "0" + time;
            }
            else
            {
                return time.ToString();
            }
        }
    }
}
