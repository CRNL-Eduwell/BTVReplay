using System;
using System.Linq;
using Tools.CSharp.EEG;
using BTV.Services.CalculationService;

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
        public Frequency Frequency { get; private set; } = new Frequency();
        public float MaxValue { get; private set; }
        public float[] Data { get; private set; } = null;
        //==
        private float m_Median = 0;

        public BtvChannel(string Name, int Position, float DataFrequency, float[] DataArray)
        {
            Label = Name;
            ID = Position;
            Frequency = new Frequency(DataFrequency);
            Data = DataArray;
            //Get some values usefull to manipulate data 
            //Median (possibly later mean, max and min)
            m_Median = CalculationService.Median(Data, Data.Length);
            MaxValue = Math.Max(Math.Abs(Data.Min()), Math.Abs(Data.Max()));
        }

        public float GetSample(int index, bool centered = false)
        {
            if (index >= Data.Length)
            {
                throw new ArgumentException("Index value : " + index + " is greater or equal to the size of the data array : " + Data.Length);
            }

            return centered ? Data[index] - m_Median : Data[index];
        }
    }
}
