using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools.CSharp.EEG;
using UnityEngine;

namespace BTV.Data
{
    public class BtvProgram
    {
        public int NumberOfElectrodes
        {
            get
            {
                return Channels.Count;
            }
        }
        public int TotalDurationInSeconds
        {
            get
            {
                return (int)Math.Round((float)NumberOfSample / Frequency.Value);
            }
        }
        public int NumberOfSample
        {
            get
            {
                return Channels.Count > 0 ? Channels[0].NumberOfSample : 0;
            }
        }
        public List<BtvChannel> Channels { get; set; } = new List<BtvChannel>();
        public Frequency Frequency { get; set; } = new Frequency();
        public string Directory
        {
            get
            {
                return System.IO.Path.GetDirectoryName(m_FilePath);
            }
        }
        private readonly string m_FilePath = "";

        public BtvProgram(DataContainer container)
        {
            int count = 0;
            foreach (KeyValuePair<string, float[]> pair in container.ValuesByChannel)
            {
                Channels.Add(new BtvChannel(pair.Key, count, container.Frequency.RawValue, pair.Value));
                count++;
            }
            Frequency = container.Frequency;
            m_FilePath = container.FilePath;
        }

        public int GetElectrodeIDFromElectrodeName(string Name, bool IsLowerCaseName = false)
        {
            if (IsLowerCaseName)
                return Channels.FindIndex(x => x.Label.ToLower().Equals(Name));
            else
                return Channels.FindIndex(x => x.Label.Equals(Name));
        }

        public string GetElectrodeNameFromElectrodeID(int ID)
        {
            return ((ID >= 0) && (ID < Channels.Count)) ? Channels[ID].Label : "";
        }
    }
}
