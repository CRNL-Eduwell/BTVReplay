using Assets.Scripts.Data.Factory;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using BrainTV.Tools.NumberExtensions;

namespace Assets.Scripts.Data.Files
{
    class BtvFile : IEventsContext
    {
        public List<TraceEvent> Events
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
                    Events = new List<TraceEvent>();

                    string r;
                    while ((r = sr.ReadLine()) != null)
                    {
                        //the regex mean you split by everything but a single white space
                        string[] resultSplit = System.Text.RegularExpressions.Regex.Split(r, @"\s{2,}");
                        if (resultSplit.Length == 7)
                        {
                            int Code = int.Parse(resultSplit[2]);
                            int Sample = int.Parse(resultSplit[3]);
                            EegEvent currentEvent = new EegEvent(Code, Sample);
                            int Duration = int.Parse(resultSplit[4]);
                            string FirstElectrodeOfInterest = resultSplit[5];
                            string SecondElectrodeOfInterest = resultSplit[6];
                            string Comment = resultSplit[1];
                            Events.Add(new TraceEvent(currentEvent, Duration, FirstElectrodeOfInterest, SecondElectrodeOfInterest, Comment));
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
                Events = new List<TraceEvent>();
                return -1;
            }
        }

        public static void Save(string FilePath, List<TraceEvent> Events)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(FilePath))
                {
                    foreach (TraceEvent eegEvent in Events)
                    {
                        //if eegEvent.samplingFrequency does not work , see to use ApplicationState.CurrentSelectedFile.sampFreq
                        int timeInSec = eegEvent.sample / eegEvent.samplingFrequency;
                        int h = timeInSec / 3600;
                        int m = (timeInSec / 60) % 60;
                        int s = timeInSec % 60;
                        
                        string timeString = "";
                        if (h > 0)
                            timeString = h.FormatToTimeString() + ":" + m.FormatToTimeString() + ":" + s.FormatToTimeString();
                        else
                            timeString = "00:" + m.FormatToTimeString() + ":" + s.FormatToTimeString();

                        sw.Write(timeString.PadRight(10));
                        sw.Write(eegEvent.comment.PadRight(40));
                        sw.Write(eegEvent.code.ToString().PadRight(10));
                        sw.Write(eegEvent.sample.ToString().PadRight(10));
                        sw.Write(eegEvent.duration.ToString().PadRight(10));
                        sw.Write(eegEvent.elecOfInterest.PadRight(10));
                        sw.WriteLine(eegEvent.secondElecOfInterest);
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
    }
}
