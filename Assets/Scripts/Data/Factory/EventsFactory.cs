using Assets.Scripts.Data.Files;
using BTV.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Data.Factory
{
    public class EventsFactory
    {
        public static IEventsContext GetEventsContext(string FilePath, float samplingFrequency = 0)
        {
            FileInfo fileInfo = new FileInfo(FilePath);
            switch (fileInfo.Extension)
            {
                case ".pos":
                    if (samplingFrequency.Equals(0))
                    {
                        throw new ArgumentException("EventsFactory.GetEventsContext : SamplingFrequency should not be 0");
                    }
                    return new PosFile(FilePath, samplingFrequency);
                case ".btv":
                    return new BtvFile(FilePath);
                default:
                    throw new ArgumentException("EventsFactory.GetEventsContext : file extension not supported => " + fileInfo.Extension); ;
            }
        }

        public static void SaveEvents(string FilePath, List<BtvEvent> Events, float samplingFrequency = 0)
        {
            FileInfo fileInfo = new FileInfo(FilePath);
            switch (fileInfo.Extension)
            {
                case ".pos":
                    if (samplingFrequency.Equals(0))
                    {
                        throw new ArgumentException("EventsFactory.SaveEvents : SamplingFrequency should not be 0");
                    }
                    PosFile.Save(FilePath, Events, samplingFrequency);
                    break;
                case ".btv":
                    BtvFile.Save(FilePath, Events);
                    break;
                default:
                    throw new ArgumentException("EventsFactory.SaveEvents : file extension not supported => " + fileInfo.Extension); ;
            }
        }
    }
}
