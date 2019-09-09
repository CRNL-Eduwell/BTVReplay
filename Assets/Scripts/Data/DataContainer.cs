using System;
using System.Collections.Generic;
using System.Linq;
using Tools.CSharp.EEG;

namespace BTV.Data
{
    public class DataContainer
    {
        public Dictionary<string, float[]> ValuesByChannel { get; set; } = new Dictionary<string, float[]>();
        public Dictionary<string, string> UnitByChannel { get; set; } = new Dictionary<string, string>();
        public Frequency Frequency { get; set; } = new Frequency();
        public string FilePath
        {
            get;
            private set;
        }

        public DataContainer(string path, File.FileType Type)
        {
            FilePath = path;
            File file = new File(Type, true, FilePath);
            List<Electrode> channels = file.Electrodes;
            foreach (var channel in channels)
            {
                ValuesByChannel.Add(channel.Label, channel.Data);
                UnitByChannel.Add(channel.Label, channel.Unit);
            }
            Frequency = file.SamplingFrequency;

            //Recuperer les notes et les triggers plus tard aussi

            file.Dispose();
        }
    }
}
