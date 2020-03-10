using System.Collections;
using System.Collections.Generic;
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

            //Recuperer les notes et les triggers plus tard aussi

            file.Dispose();
        }
    }
}
