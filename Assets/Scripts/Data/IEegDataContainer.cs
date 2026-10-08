using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tools.CSharp.EEG;
using UnityEngine;

namespace BTV.Data
{
    /// <summary>
    /// An EEG file's metadata (labels, units, rate, triggers, notes) and its samples, which stay
    /// on disk behind <see cref="Source"/> and are read by range. The samples used to be copied
    /// into ValuesByChannel channel by channel; for an EEG file that dictionary now stays empty.
    /// </summary>
    public class IEegDataContainer : DataContainer
    {
        /// <summary>The file's samples, read by range. Owned by the BtvProgram built from it.</summary>
        public ISampleSource Source { get; }
        /// <summary>Channel labels, in the file's order (the source's channel order).</summary>
        public List<string> Labels { get; } = new List<string>();

        public IEegDataContainer(IEegFileInfo fileInfo) : base(fileInfo.Files[0])
        {
            // Metadata from a header-only open, released at once; the source keeps its own handle.
            File file = new File(fileInfo.FileType, false, fileInfo.Files);
            try
            {
                int channelCount = file.ElectrodeCount;
                for (int i = 0; i < channelCount; i++)
                {
                    Electrode electrode = file.GetElectrodeWithoutData(i);
                    Labels.Add(electrode.Label);
                    UnitByChannel[electrode.Label] = electrode.Unit;
                }
                Frequency = file.SamplingFrequency;

                List<Trigger> events = file.Triggers;
                foreach (var _event in events)
                {
                    int code = _event.Code;
                    int time = (int)((float)_event.Sample / Frequency.Value * 1000);
                    Events.Add(new BtvEvent(code, time));
                }

                List<Note> notes = file.Notes;
                foreach (var _note in notes)
                {
                    string description = _note.Description;
                    int time = (int)((float)_note.Sample / Frequency.Value * 1000);
                    Events.Add(new BtvEvent(-1, time, 0, "", "", description));
                }
                Events = Events.OrderBy(x => x.TimeInMilliSeconds).ToList();
            }
            finally
            {
                file.Dispose();
            }

            Source = new NativeRangeSampleSource(fileInfo.FileType, fileInfo.Files);
            try
            {
                Validate(Labels, Source.SampleCount, fileInfo.Files[0]);
                if (Source.ChannelCount != Labels.Count)
                    throw new System.IO.InvalidDataException("The EEG reader reports " + Source.ChannelCount + " channels for " + Labels.Count + " labels: " + fileInfo.Files[0]);
            }
            catch
            {
                Source.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Rejects files the rest of the app cannot display, with a message naming the problem.
        /// A zero-sample file left every channel's data null (BtvChannel then dereferenced it),
        /// and duplicate labels failed on Dictionary.Add with "An item with the same key has
        /// already been added".
        /// </summary>
        public static void Validate(IList<string> channelLabels, long numberOfSamples, string path)
        {
            if (channelLabels.Count == 0)
                throw new System.IO.InvalidDataException("The EEG file has no channels: " + path);
            if (numberOfSamples <= 0)
                throw new System.IO.InvalidDataException("The EEG file has no samples: " + path);

            List<string> duplicates = channelLabels.GroupBy(label => label).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (duplicates.Count > 0)
                throw new System.IO.InvalidDataException("The EEG file has several channels with the same label (" + string.Join(", ", duplicates) + "): " + path);
        }
    }
}
