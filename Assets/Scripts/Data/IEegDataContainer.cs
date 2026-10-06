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
                int channelCount = file.ElectrodeCount;
                int numberOfSamples = file.NumberOfSamples;
                List<string> labels = new List<string>(channelCount);
                List<string> units = new List<string>(channelCount);
                for (int i = 0; i < channelCount; i++)
                {
                    Electrode electrode = file.GetElectrodeWithoutData(i);
                    labels.Add(electrode.Label);
                    units.Add(electrode.Unit);
                }
                Validate(labels, numberOfSamples, fileInfo.Files[0]);
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

                // The samples used to be copied all at once (File.Electrodes), so the native and
                // managed copies coexisted: twice the file's float size at the peak. Moving one
                // electrode at a time - copy it, then free it natively - keeps the peak at one
                // copy plus one channel. The last electrode goes first so the indices of the
                // remaining ones never shift.
                float[][] data = new float[channelCount][];
                for (int i = channelCount - 1; i >= 0; i--)
                {
                    data[i] = file.ReadElectrodeData(i, numberOfSamples);
                    file.DeleteElectrodesAndData(new[] { i });
                    if (file.ElectrodeCount != i)
                        throw new System.InvalidOperationException("The EEG reader did not release channel " + labels[i] + " of " + fileInfo.Files[0]);
                }
                for (int i = 0; i < channelCount; i++)
                {
                    ValuesByChannel.Add(labels[i], data[i]);
                    UnitByChannel.Add(labels[i], units[i]);
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
