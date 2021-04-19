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
            List<Electrode> channels = file.Electrodes;
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

            file.Dispose();
        }
    }
}
