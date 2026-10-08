using System;
using System.IO;
using Tools.CSharp.EEG;
using File = Tools.CSharp.EEG.File;

namespace BTV.Data
{
    /// <summary>
    /// An EEG file read by range from disk through EEGFormat (EEGF_ReadRange): nothing of the
    /// recording stays in memory but what is being read. Every EEG file goes through it,
    /// whatever its size; it used to be loaded whole into managed arrays (3.9 GiB for a 6 h,
    /// 183-channel TRC). Worker-safe: EEGFormat serializes the reads of one file.
    /// </summary>
    public sealed class NativeRangeSampleSource : ISampleSource
    {
        private readonly EegFileHandle m_Handle;
        // Planar output of a multi-channel read, scattered into the caller's arrays.
        [ThreadStatic] private static float[] t_Planar;

        public NativeRangeSampleSource(File.FileType type, params string[] paths)
        {
            m_Handle = EegFileHandle.OpenHeaderOnly(type, paths);
            try
            {
                // Read now, so a file EEGFormat cannot read by range fails at load time.
                Check(EegFileHandle.GetSampleCount64(m_Handle, out long sampleCount));
                SampleCount = sampleCount;
                ChannelCount = EegFileHandle.GetElectrodeCount(m_Handle);
                Frequency = new Frequency(EegFileHandle.GetSamplingFrequency(m_Handle));
            }
            catch
            {
                m_Handle.Dispose();
                throw;
            }
        }

        public int ChannelCount { get; }
        public long SampleCount { get; }
        public Frequency Frequency { get; }

        public int ReadRange(long first, int count, int[] channels, float[][] dst)
        {
            if (first < 0)
                throw new ArgumentOutOfRangeException(nameof(first), first, "The first sample cannot be negative.");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), count, "The sample count cannot be negative.");
            if (count == 0 || channels.Length == 0 || first >= SampleCount)
                return (int)Math.Max(0, Math.Min(count, SampleCount - first));

            int read;
            if (channels.Length == 1)
            {
                // One channel (traces' single-channel readers, STFT, correlation): straight into dst.
                Check(EegFileHandle.ReadRange(m_Handle, first, count, channels, 1, dst[0], dst[0].Length, out read));
                return read;
            }

            long planarLength = (long)channels.Length * count;
            if (t_Planar == null || t_Planar.Length < planarLength)
                t_Planar = new float[planarLength];
            Check(EegFileHandle.ReadRange(m_Handle, first, count, channels, channels.Length, t_Planar, t_Planar.Length, out read));
            for (int k = 0; k < channels.Length; k++)
                Array.Copy(t_Planar, (long)k * count, dst[k], 0, read);
            return read;
        }

        public void Dispose()
        {
            m_Handle.Dispose();
        }

        private static void Check(int status)
        {
            if (status == EegFileHandle.StatusOk)
                return;
            string message = EegFileHandle.LastError();
            switch (status)
            {
                case EegFileHandle.StatusIO: throw new IOException(message);
                case EegFileHandle.StatusArgument: throw new ArgumentException(message);
                default: throw new InvalidDataException(message);
            }
        }
    }
}
