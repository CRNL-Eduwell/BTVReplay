using Assets.Scripts.Data.Factory;
using BTV.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Data.Files
{
    class PosFile : IEventsContext
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

        public PosFile(string filePath, float samplingFrequency)
        {
            FilePath = filePath;

            if (File.Exists(FilePath))
                Load(FilePath, samplingFrequency);
            else
                Debug.LogError("PosFile => Filepath : " + FilePath + " does not exist ");
        }

        // Throws when the file cannot be read, and when it has content but not a single event
        // line parses (wrong format or corrupt). The old version substituted an empty list and
        // reported only through Console.WriteLine, so a failed load looked like "no events".
        private void Load(string FilePath, float samplingFrequency)
        {
            bool hasContent = false;
            using (StreamReader sr = new StreamReader(FilePath))
            {
                Events = new List<BtvEvent>();

                string r;
                while ((r = sr.ReadLine()) != null)
                {
                    if (r.Trim().Length > 0) hasContent = true;
                    string[] resultSplit = r.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (resultSplit.Length == 3)
                    {
                        int Code = int.Parse(resultSplit[1], CultureInfo.InvariantCulture);
                        int Sample = int.Parse(resultSplit[0], CultureInfo.InvariantCulture);
                        float TimeInMilliSec = (Sample / samplingFrequency) * 1000;

                        Events.Add(new BtvEvent(Code, TimeInMilliSec));
                    }
                }
            }
            if (hasContent && Events.Count == 0)
                throw new InvalidDataException("No event line could be read; is this a .pos file?");
        }

        // Throws on failure (it used to swallow the error into Console.WriteLine, so a failed
        // save looked like success). Built in memory and swapped in atomically.
        public static void Save(string FilePath, List<BtvEvent> Events, float samplingFrequency = 0)
        {
            StringBuilder sb = new StringBuilder();
            foreach (BtvEvent Event in Events)
            {
                int sample = Convert.ToInt32(Event.TimeInSeconds * samplingFrequency);
                sb.Append(sample.ToString(CultureInfo.InvariantCulture).PadRight(10));
                sb.Append(Event.Code.ToString(CultureInfo.InvariantCulture).PadRight(10));
                sb.Append("0\n");
            }
            BrainTV.Tools.AtomicFile.WriteAllText(FilePath, sb.ToString());
        }
    }
}
