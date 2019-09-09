using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools.CSharp.EEG;

namespace BTV.Data
{
    public class BtvChannel
    {
        public int NumberOfSample
        {
            get
            {
                return Data != null ? Data.Length : 0;
            }
        }
        public string Label { get; private set; } = "";
        public int ID { get; private set; } = -1;
        public float[] Data { get; private set; } = null;
        public Frequency Frequency { get; private set; } = new Frequency();

        public BtvChannel(string Name, int Position, float DataFrequency, float[] DataArray)
        {
            Label = Name;
            ID = Position;
            Frequency = new Frequency(DataFrequency);
            Data = DataArray;
        }
    }
}
