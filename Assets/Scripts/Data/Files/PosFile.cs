using Assets.Scripts.Data.Factory;
using BTV.Data;
using System;
using System.Collections.Generic;
using System.IO;
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

        private int Load(string FilePath, float samplingFrequency)
        {
            try
            {
                using (StreamReader sr = new StreamReader(FilePath))
                {
                    Events = new List<BtvEvent>();

                    string r;
                    while ((r = sr.ReadLine()) != null)
                    {
                        string[] resultSplit = r.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (resultSplit.Length == 3)
                        {
                            int Code = int.Parse(resultSplit[1]);
                            int Sample = int.Parse(resultSplit[0]);
                            float TimeInMilliSec = (Sample / samplingFrequency) * 1000;

                            Events.Add(new BtvEvent(Code, TimeInMilliSec));
                        }
                    }
                    sr.Close();
                    return 0;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The pos file could not be read:");
                Console.WriteLine(e.Message);
                Events = new List<BtvEvent>();
                return -1;
            }
        }

        public static void Save(string FilePath, List<BtvEvent> Events, float samplingFrequency = 0)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(FilePath))
                {
                    foreach (BtvEvent Event in Events)
                    {
                        int sample = Convert.ToInt32(Event.TimeInSeconds * samplingFrequency);
                        sw.Write(sample.ToString().PadRight(10));
                        sw.Write(Event.Code.ToString().PadRight(10));
                        sw.Write("0\n");
                    }

                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not write pos file");
                Console.WriteLine(e.Message);
            }
        }
    }
}
