using BTV.Services.CalculationService;
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
                return m_Data != null ? m_Data.Length : 0;
            }
        }
        public string Label { get; private set; } = "";
        public int ID { get; private set; } = -1;
        public Frequency Frequency { get; private set; } = new Frequency();
        public float MaxValue { get; private set; }
        //==
        private float[] m_Data = null;
        private float m_Median = 0;

        public BtvChannel(string Name, int Position, float DataFrequency, float[] DataArray)
        {
            Label = Name;
            ID = Position;
            Frequency = new Frequency(DataFrequency);
            m_Data = DataArray;
            //Get some values usefull to manipulate data 
            //Median (possibly later mean, max and min)
            m_Median = CalculationService.Median(m_Data, m_Data.Length);
            MaxValue = Math.Max(Math.Abs(m_Data.Min()), Math.Abs(m_Data.Max()));
        }

        public float GetSample(int index, bool centered = false)
        {
            if (index >= m_Data.Length)
            {
                throw new ArgumentException("Index value : " + index + " is greater or equal to the size of the data array : " + m_Data.Length);
            }

            return centered ? m_Data[index] - m_Median : m_Data[index];
        }
    }
}
