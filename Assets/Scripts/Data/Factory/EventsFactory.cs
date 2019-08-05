using Assets.Scripts.Data.Files;
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
        public static IEventsContext GetEventsContext(string FilePath)
        {
            FileInfo fileInfo = new FileInfo(FilePath);
            switch (fileInfo.Extension)
            {
                case ".pos":
                    return new PosFile(FilePath);
                case ".btv":
                    return new BtvFile(FilePath);
                default:
                    throw new ArgumentException("EventsFactory.GetEventsContext : file extension not supported => " + fileInfo.Extension); ;
            }
        }

        public static void SaveEvents(string FilePath, List<TraceEvent> Events)
        {
            FileInfo fileInfo = new FileInfo(FilePath);
            switch (fileInfo.Extension)
            {
                case ".pos":
                    PosFile.Save(FilePath, Events);
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
