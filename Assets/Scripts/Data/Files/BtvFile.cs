using Assets.Scripts.Data.Factory;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using BrainTV.Tools.NumberExtensions;
using BTV.Data;

namespace Assets.Scripts.Data.Files
{
    class BtvFile : IEventsContext
    {
        public List<BtvEvent> Events
        {
            get;
            private set;
        }
        public string FilePath
        {
            get;
            private set;
        }

        public BtvFile(string filePath)
        {
            FilePath = filePath;

            if (File.Exists(FilePath))
                Load(FilePath);
            else
                Debug.LogError("PosFile => Filepath : " + FilePath + " does not exist ");
        }

        // Throws when the file cannot be read, and when it has content but not a single event
        // line parses (wrong format or corrupt). The old version substituted an empty list and
        // reported only through Console.WriteLine, so a failed load looked like "no events".
        private void Load(string FilePath)
        {
            bool hasContent = false;
            using (StreamReader sr = new StreamReader(FilePath))
            {
                Events = new List<BtvEvent>();

                string r;
                while ((r = sr.ReadLine()) != null)
                {
                    if (r.Trim().Length > 0) hasContent = true;
                    //the regex mean you split by everything but a single white space
                    string[] resultSplit = System.Text.RegularExpressions.Regex.Split(r, @"\s{2,}");
                    if (resultSplit.Length == 7)
                    {
                        int Time = TimeStringToMilliSeconds(resultSplit[0]);
                        string Comment = resultSplit[1] == "EMPTY_COMMENT" ? "" : resultSplit[1];
                        int Code = int.Parse(resultSplit[2], CultureInfo.InvariantCulture);
                        int Sample = int.Parse(resultSplit[3], CultureInfo.InvariantCulture);
                        resultSplit[4].TryParseFloat(out float floatValue);
                        int Duration = (int)floatValue;
                        string FirstElectrodeOfInterest = resultSplit[5] == "E_F_SITE" ? "" : resultSplit[5];
                        string SecondElectrodeOfInterest = resultSplit[6] == "E_S_SITE" ? "" : resultSplit[6];

                        Events.Add(new BtvEvent(Code, Time, Duration, FirstElectrodeOfInterest, SecondElectrodeOfInterest, Comment));
                    }
                }
            }
            if (hasContent && Events.Count == 0)
                throw new InvalidDataException("No event line could be read; is this a .btv file?");
        }

        private int TimeStringToMilliSeconds(string str)
        {
            string[] timeSplit = str.Split(new string[] { ":" , "." }, StringSplitOptions.None);
            if (timeSplit.Length == 4)
            {
                int HourInSeconds = Convert.ToInt32(timeSplit[0]) * 3600;
                int MinInSeconds = Convert.ToInt32(timeSplit[1]) * 60;
                int Seconds = Convert.ToInt32(timeSplit[2]);
                int Milliseconds = Convert.ToInt32(timeSplit[3]);
                return ((HourInSeconds + MinInSeconds + Seconds) * 1000) + Milliseconds;
            }
            else if (timeSplit.Length == 3)
            {
                int HourInSeconds = Convert.ToInt32(timeSplit[0]) * 3600;
                int MinInSeconds = Convert.ToInt32(timeSplit[1]) * 60;
                int Seconds = Convert.ToInt32(timeSplit[2]);
                return ((HourInSeconds + MinInSeconds + Seconds) * 1000);
            }
            else
            {
                Debug.LogWarning("BtvFile => cannot read time \"" + str + "\" (expected hh:mm:ss.ms or hh:mm:ss), using 0.");
                return 0;
            }
        }

        // Throws on failure (it used to swallow the error into Console.WriteLine, so a failed
        // save looked like success). Built in memory and swapped in atomically.
        public static void Save(string FilePath, List<BtvEvent> Events)
        {
            StringBuilder sb = new StringBuilder();
            foreach (BtvEvent eegEvent in Events)
            {
                // Use local strings for the file sentinels instead of writing them back
                // into the live event - the old code mutated Comment/SiteOfInterest in
                // memory to "EMPTY_COMMENT"/"E_F_SITE", which then showed up in the UI.
                string comment = string.IsNullOrWhiteSpace(eegEvent.Comment) ? "EMPTY_COMMENT" : eegEvent.Comment;
                string firstSite = string.IsNullOrWhiteSpace(eegEvent.SiteOfInterest) ? "E_F_SITE" : eegEvent.SiteOfInterest;
                string secondSite = string.IsNullOrWhiteSpace(eegEvent.SecondSiteOfInterest) ? "E_S_SITE" : eegEvent.SecondSiteOfInterest;

                string timeString = MilliSecondsToTimeString(eegEvent.TimeInMilliSeconds);
                sb.Append(timeString.PadRight(18));
                sb.Append(comment.PadRight(40));
                sb.Append(eegEvent.Code.ToString(CultureInfo.InvariantCulture).PadRight(10));
                // Sample column: BtvEvent has no sample field and the value is discarded
                // on load, so a placeholder is written. (Proper fix needs the sampling
                // frequency to derive it from the timestamp - out of scope here.)
                sb.Append("00000".PadRight(10));
                sb.Append(eegEvent.Duration.ToString(CultureInfo.InvariantCulture).PadRight(10));
                sb.Append(firstSite.PadRight(10));
                sb.AppendLine(secondSite);
            }
            BrainTV.Tools.AtomicFile.WriteAllText(FilePath, sb.ToString());
        }

        private static string MilliSecondsToTimeString(float timeInMilliSec)
        {
            float TimeInSeconds = timeInMilliSec / 1000;

            int h = (int)(TimeInSeconds / 3600);
            int m = (int)((TimeInSeconds / 60) % 60);
            int s = (int)(TimeInSeconds % 60);
            int ms = (int)(timeInMilliSec % 1000);

            return h.ToString("00") + ":" + m.ToString("00") + ":" + s.ToString("00") + "." + ms.ToString("000");
        }

    }
}
