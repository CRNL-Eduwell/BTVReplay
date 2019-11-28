using Assets.Scripts.Data.Factory;
using System;
using System.Collections.Generic;
using System.IO;
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

        private int Load(string FilePath)
        {
            try
            {
                using (StreamReader sr = new StreamReader(FilePath))
                {
                    Events = new List<BtvEvent>();

                    string r;
                    while ((r = sr.ReadLine()) != null)
                    {
                        //the regex mean you split by everything but a single white space
                        string[] resultSplit = System.Text.RegularExpressions.Regex.Split(r, @"\s{2,}");
                        if (resultSplit.Length == 7)
                        {
                            int Time = TimeStringToMilliSeconds(resultSplit[0]);
                            string Comment = resultSplit[1];
                            int Code = int.Parse(resultSplit[2]);
                            int Sample = int.Parse(resultSplit[3]);
                            int Duration = int.Parse(resultSplit[4]);
                            string FirstElectrodeOfInterest = resultSplit[5];
                            string SecondElectrodeOfInterest = resultSplit[6];

                            Events.Add(new BtvEvent(Code, Time, Duration, FirstElectrodeOfInterest, SecondElectrodeOfInterest, Comment));
                        }
                    }
                    sr.Close();
                    return 0;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The btv file could not be read:");
                Console.WriteLine(e.Message);
                Events = new List<BtvEvent>();
                return -1;
            }
        }

        private int TimeStringToMilliSeconds(string str)
        {
            string[] timeSplit = str.Split(new string[] { ":" }, StringSplitOptions.None);
            if (timeSplit.Length == 3)
            {
                int HourInSeconds = Convert.ToInt32(timeSplit[0]) * 3600;
                int MinInSeconds = Convert.ToInt32(timeSplit[1]) * 60;
                int Seconds = Convert.ToInt32(timeSplit[2]);
                return ((HourInSeconds + MinInSeconds + Seconds) * 1000);
            }
            else
            {
                Console.WriteLine("BtvFile => Error when spliting time string, we should only have 3 elements");
                Console.WriteLine("Make sure the format is hh:mm:ss");
                Console.WriteLine("Returning 0 as value");
                return 0;
            }
        }

        public static void Save(string FilePath, List<BtvEvent> Events)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(FilePath))
                {
                    foreach (BtvEvent eegEvent in Events)
                    {
                        string timeString = MilliSecondsToTimeString((int)eegEvent.TimeInMilliSeconds);

                        sw.Write(timeString.PadRight(10));
                        sw.Write(eegEvent.Comment.PadRight(40));
                        sw.Write(eegEvent.Code.ToString().PadRight(10));
                        sw.Write(eegEvent.sample.ToString().PadRight(10));
                        sw.Write(eegEvent.Duration.ToString().PadRight(10));
                        sw.Write(eegEvent.SiteOfInterest.PadRight(10));
                        sw.WriteLine(eegEvent.SecondSiteOfInterest);
                    }

                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not write btv file");
                Console.WriteLine(e.Message);
            }
        }

        private static string MilliSecondsToTimeString(int timeInMilliSec)
        {
            string TimeString = "";
            int TimeInSeconds = timeInMilliSec / 1000;
            int h = TimeInSeconds / 3600;
            int m = (TimeInSeconds / 60) % 60;
            int s = TimeInSeconds % 60;

            if (h > 0)
                TimeString = h.FormatToTimeString() + ":" + m.FormatToTimeString() + ":" + s.FormatToTimeString();
            else
                TimeString = "00:" + m.FormatToTimeString() + ":" + s.FormatToTimeString();

            return TimeString;
        }

    }
}
