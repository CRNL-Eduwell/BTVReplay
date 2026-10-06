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
        public int TotalDurationInMilliseconds
        {
            get
            {
                return (int)Math.Round((float)NumberOfSample / Frequency.Value * 1000);
            }
        }
        public int NumberOfSample
        {
            get
            {
                return Channels.Count > 0 ? Channels[0].NumberOfSample : 0;
            }
        }
        public string Description { get; private set; } = "";
        public List<BtvChannel> Channels { get; private set; } = new List<BtvChannel>();
        public List<BtvEvent> Events { get; private set; } = new List<BtvEvent>();
        public Frequency Frequency { get; set; } = new Frequency();
        public string Directory
        {
            get
            {
                return System.IO.Path.GetDirectoryName(m_FilePath);
            }
        }
        private readonly string m_FilePath = "";

        public BtvProgram(DataContainer container, string description = "")
        {
            int count = 0;
            foreach (KeyValuePair<string, float[]> pair in container.ValuesByChannel)
            {
                Channels.Add(new BtvChannel(pair.Key, count, container.Frequency.RawValue, pair.Value));
                count++;
            }
            Events = new List<BtvEvent>(container.Events);

            Frequency = container.Frequency;
            m_FilePath = container.FilePath;
            Description = description;

            //Little hack due to micromed seemingly finishing a recording but keep events after said end
            //See at some point if it's not better to expand end of file time and add zero's to data (or not)
            int removedCount = Events.RemoveAll(x => x.TimeInMilliSeconds > TotalDurationInMilliseconds);
        }

        /// <summary>
        /// Same file with other channels (a montage). The channels are taken as given, so they
        /// can be shared with the base file - channels are never written to - while the events
        /// are copied, each montage editing its own.
        /// </summary>
        public BtvProgram(BtvProgram copy, List<BtvChannel> channels)
        {
            Channels = new List<BtvChannel>(channels);
            foreach (var ev in copy.Events)
                Events.Add(new BtvEvent(ev));
            Frequency = new Frequency(copy.Frequency.RawValue);
            m_FilePath = copy.m_FilePath;
            Description = copy.Description;
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

        public void AddData(float[] Data, string Name)
        {
            Channels.Add(new BtvChannel(Name, Channels.Count, Frequency.Value, Data));
        }
    }
}
