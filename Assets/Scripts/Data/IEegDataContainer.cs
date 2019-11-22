using System.Collections;
using System.Collections.Generic;
using Tools.CSharp.EEG;
using UnityEngine;

namespace BTV.Data
{
    public class IEegDataContainer : DataContainer
    {
        public IEegDataContainer(string path, File.FileType Type) : base(path)
        {
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
