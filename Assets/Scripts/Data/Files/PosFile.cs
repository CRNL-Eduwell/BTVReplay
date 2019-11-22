using Assets.Scripts.Data.Factory;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.Data.Files
{
    class PosFile : IEventsContext
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

        public PosFile(string filePath)
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
                        string[] resultSplit = r.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (resultSplit.Length == 3)
                        {
                            int Code = int.Parse(resultSplit[1]);
                            int Sample = int.Parse(resultSplit[0]);
                            EegEvent currentEvent = new EegEvent(Code, Sample);
                            Events.Add(new TraceEvent(currentEvent));
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
                    foreach (TraceEvent Event in Events)
                    {
                        sw.Write(Event.sample.ToString().PadRight(10));
                        sw.Write(Event.code.ToString().PadRight(10));
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
