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
        /// <summary>
        /// The source this program opened and must close (an EEG file read from disk); null for
        /// a montage, which shares its base file's source. Disposed by Session.Dispose, or when
        /// another file replaces this one in its slot.
        /// </summary>
        public ISampleSource OwnedSource { get; private set; }

        /// <summary>Channels held in the container's arrays (audio, in-memory test data).</summary>
        public BtvProgram(DataContainer container, string description = "")
            : this(new InMemorySampleSource(container.ValuesByChannel.Values.ToArray(), new Frequency(container.Frequency.RawValue)),
                  container.ValuesByChannel.Keys.ToList(), container, description)
        {
            OwnedSource = null;
        }

        /// <summary>
        /// Channels read from <paramref name="source"/>, labelled in its channel order, with the
        /// container's events, rate and path. The program owns the source. Statistics come from one
        /// pass over the source, block by block: run it off the main thread.
        /// </summary>
        public BtvProgram(ISampleSource source, IList<string> labels, DataContainer metadata, string description = "")
        {
            ChannelStats[] stats = ChannelStats.Compute(source, Enumerable.Range(0, labels.Count).ToArray());
            BlockCache cache = new BlockCache(source);
            for (int i = 0; i < labels.Count; i++)
            {
                Channels.Add(new BtvChannel(labels[i], i, source, i, stats[i], cache));
            }
            Events = new List<BtvEvent>(metadata.Events);

            Frequency = metadata.Frequency;
            m_FilePath = metadata.FilePath;
            Description = description;
            OwnedSource = source;

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
    }
}
