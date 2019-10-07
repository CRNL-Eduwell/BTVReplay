using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;

namespace Tools.CSharp.Audio
{
    /// <summary>
    /// Class for reading raw Audio Files
    ///
    /// Compatible format :
    ///     - Wav
    /// </summary>
    ///
    public class AudioFile : DLL.CppDLLImportBase
    {
        public enum AudioFileType { Wav, Processed }

        /// <summary>
        /// Size of the data
        /// </summary>
        public int NumberOfSamples
        {
            get { return NbSample(_handle); }
        }

        /// <summary>
        /// Sampling frequency of this file
        /// </summary>
        public EEG.Frequency SamplingFrequency
        {
            get
            {
                return new EEG.Frequency(SampleRate(_handle));
            }
        }

        /// <summary>
        /// Data of this Audio File
        /// </summary>
        public float[] Data { get; private set; }

        #region Memory Management
        public AudioFile(AudioFileType type, string path)
        {
            switch (type)
            {
                case AudioFileType.Wav:
                    _handle = new HandleRef(this, CreateWavFile(path));
                    if (NumberOfSamples != 0)
                    {
                        Data = new float[NumberOfSamples];
                        GetData(_handle, Data, NumberOfSamples);
                    }
                    break;
                default:
                    throw new Exception("FileType " + type.ToString() + " unknown");
            }
        }

        protected override void create_DLL_class()
        {

        }

        protected override void delete_DLL_class()
        {
            DeleteWavFile(_handle);
        }
        #endregion

        #region DLLImport
        [DllImport("AudioFormat", EntryPoint = "CreateWavFile", CallingConvention = CallingConvention.Cdecl)]
        static private extern IntPtr CreateWavFile(string filePath);
        [DllImport("AudioFormat", EntryPoint = "DeleteWavFile", CallingConvention = CallingConvention.Cdecl)]
        static private extern void DeleteWavFile(HandleRef fileToDelete);

        [DllImport("AudioFormat", EntryPoint = "SampleRate", CallingConvention = CallingConvention.Cdecl)]
        static private extern int SampleRate(HandleRef file);
        [DllImport("AudioFormat", EntryPoint = "NbSample", CallingConvention = CallingConvention.Cdecl)]
        static private extern int NbSample(HandleRef file);

        [DllImport("AudioFormat", EntryPoint = "NearestFrequency", CallingConvention = CallingConvention.Cdecl)]
        static private extern int NearestFrequency(HandleRef file, int NewSampleRate);
        [DllImport("AudioFormat", EntryPoint = "GetData", CallingConvention = CallingConvention.Cdecl)]
        static private extern void GetData(HandleRef file, float[] values, int size);
        #endregion
    }
}