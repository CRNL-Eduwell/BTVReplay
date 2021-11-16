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
        private float m_Min = -666;
        private float m_Max = -666;

        public BtvChannel(string Name, int Position, float DataFrequency, float[] DataArray)
        {
            Label = Name;
            ID = Position;
            Frequency = new Frequency(DataFrequency);
            Data = DataArray;
            //Get some values usefull to manipulate data 
            //Median (possibly later mean, max and min)
            m_Median = CalculationService.Median(Data, Data.Length);
            m_Min = Data.Min();
            m_Max = Data.Max();
            MaxValue = Math.Max(Math.Abs(m_Min), Math.Abs(m_Max));
        }

        public float GetSample(int index, bool centered = false)
        {
            if (index >= Data.Length)
            {
                return 0;
            }

            return centered ? Data[index] - m_Median : Data[index];
        }

        /// <summary>
        /// Send back Data normalized beetween -1 and 1 , centered or not
        /// </summary>
        /// <param name="index"></param>
        /// <param name="centered"></param>
        /// <returns></returns>
        public float GetNormalizedSample(int index, bool centered = false)
        {
            if (index >= Data.Length)
            {
                return 0;
            }

            return centered ? (Data[index] - m_Median) / (m_Max - m_Min) : Data[index] / (m_Max - m_Min);
        }

        /// <summary>
        /// Send back Data normalized beetween 0 and 1 with max and min taken either at begin and end
        /// or beetween begin sample and end sample
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public float[] GetBaselineNormalizedValues(int begin = -1, int end = -1)
        {
            float[] dataResult = new float[NumberOfSample];

            if (begin == -1 && end == -1)
            {
                begin = (10 * Frequency.Value);
                end = NumberOfSample - (10 * Frequency.Value);
            }

            float min = float.PositiveInfinity;
            float max = float.NegativeInfinity;
            for (int i = begin; i < end; i++)
            {
                if (Data[i] < min) min = Data[i];
                if (Data[i] > max) max = Data[i];
            }

            for (int i = 0; i < NumberOfSample; i++)
            {
                dataResult[i] = /*100 */ ((Data[i] - min) / (max - min));
            }

            return dataResult;
        }
    }
}
