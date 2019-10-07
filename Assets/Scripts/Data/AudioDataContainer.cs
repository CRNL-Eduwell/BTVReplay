using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tools.CSharp.Audio;
using Assets.Scripts.Data.Files;
using Tools.CSharp.EEG;

namespace BTV.Data
{
    public class AudioDataContainer : DataContainer
    {
        public AudioDataContainer(string path, AudioFile.AudioFileType Type) : base(path)
        {
            switch (Type)
            {
                case AudioFile.AudioFileType.Wav:
                    {
                        AudioFile file = new AudioFile(Type, FilePath);

                        ValuesByChannel.Add("RawAudio", file.Data);
                        UnitByChannel.Add("RawAudio", "mV");
                        Frequency = file.SamplingFrequency;

                        file.Dispose();
                    }
                    break;
                case AudioFile.AudioFileType.Processed:
                    {
                        float[][] Data = CsvFile.LoadFloatDataHorizontally(FilePath);
                        float[] Smoothing = { 0, 250, 500, 1000, 2500, 5000 };
                        for (int i = 0; i < 6; i++)
                        {
                            ValuesByChannel.Add("AUD-" + Smoothing, Data[i]);
                            Frequency = new Frequency(64);
                        }
                    }
                    break;
            }
        }
    }
}
