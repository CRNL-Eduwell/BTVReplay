using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tools.CSharp.EEG;
using UnityEngine;

namespace BTV.Data
{
    public class IEegDataContainer : DataContainer
    {
        public IEegDataContainer(IEegFileInfo fileInfo) : base(fileInfo.Files[0])
        {
            File file = new File(fileInfo.FileType, true, fileInfo.Files);
            // The native file is released even when the copy below throws; it used to leak on
            // any managed failure (duplicate labels, for instance).
            try
            {
                List<Electrode> channels = file.Electrodes;
                Validate(channels.Select(c => c.Label).ToList(), file.NumberOfSamples, fileInfo.Files[0]);
                foreach (var channel in channels)
                {
                    ValuesByChannel.Add(channel.Label, channel.Data);
                    UnitByChannel.Add(channel.Label, channel.Unit);
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
        }

        /// <summary>
        /// Rejects files the rest of the app cannot display, with a message naming the problem.
        /// A zero-sample file left every channel's data null (BtvChannel then dereferenced it),
        /// and duplicate labels failed on Dictionary.Add with "An item with the same key has
        /// already been added".
        /// </summary>
        public static void Validate(IList<string> channelLabels, int numberOfSamples, string path)
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
