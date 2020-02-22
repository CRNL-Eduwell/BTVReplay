using System;
using UnityEngine.UI;
using BrainTV.Tools.NumberExtensions;

namespace BrainTV.Tools.TextExtensions
{
    public static class TextExtensions
    {
        /// <summary>
        /// Set a time as a properly formated string
        /// </summary>
        /// <param name="text"></param>
        /// <param name="time">Time in seconds</param>
        public static void DisplayToTimeFormat(this Text text, long time)
        {
            long h = time / 3600;
            long m = (time / 60) % 60;
            long s = time % 60;

            text.SetTime(h, m, s);
        }

        /// <summary>
        /// Set a time as a properly formated string
        /// </summary>
        /// <param name="text"></param>
        /// <param name="h">Hour</param>
        /// <param name="m">Minute</param>
        /// <param name="s">Second</param>
        public static void SetTime(this Text text, long h, long m, long s)
        {
            if (h > 0)
                text.text = h.FormatToTimeString() + ":" + m.FormatToTimeString() + ":" + s.FormatToTimeString();
            else
                text.text = m.FormatToTimeString() + ":" + s.FormatToTimeString();
        }
    }
}
