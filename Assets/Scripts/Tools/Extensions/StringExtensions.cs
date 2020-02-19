namespace BrainTV.Tools.NumberExtensions
{
    public static class StringExtensions
    {
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
