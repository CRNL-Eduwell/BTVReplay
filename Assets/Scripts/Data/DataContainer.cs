using System;
using System.Collections.Generic;
using System.Linq;
using Tools.CSharp.EEG;

namespace BTV.Data.DataContainer
{
    public class DataContainer
    {
        public Dictionary<string, float[]> ValuesByChannel { get; set; } = new Dictionary<string, float[]>();
        public Dictionary<string, string> UnitByChannel { get; set; } = new Dictionary<string, string>();
        public Frequency Frequency { get; set; } = new Frequency();
        public int NumberOfSample
        {
            get
            {
                KeyValuePair<string, float[]> firstValue = ValuesByChannel.FirstOrDefault();
                return firstValue.Value.Length;
            }
        }
        public int NumberOfElectrode
        {
            get
            {
                return ValuesByChannel.Count();
            }
        }
        public string FilePath
        {
            get;
            private set;
        }
        public string Directory
        {
            get
            {
                return System.IO.Path.GetDirectoryName(FilePath);
            }
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

        public int GetElectrodeIDFromElectrodeName(string Name, bool IsLowerCaseName = false)
        {
            if (IsLowerCaseName)
                return ValuesByChannel.ToList().FindIndex(x => x.Key.ToLower().Equals(Name));
            else
                return ValuesByChannel.ToList().FindIndex(x => x.Key == Name);
        }

        public string GetElectrodeNameFromElectrodeID(int ID)
        {
            return ValuesByChannel.ElementAt(ID).Key;
        }

        public float[] GetEegDataFromElectrodeID(int ID)
        {
            return ValuesByChannel.ElementAt(ID).Value;
        }

    }
}
