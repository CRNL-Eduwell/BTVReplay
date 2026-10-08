using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Tools.CSharp.EEG
{
    /// <summary>
    /// A native EEGFormat file opened header-only, for range reading (EEGF_ReadRange). Unlike
    /// File's HandleRef, a SafeHandle is reference-counted by the marshaller for the duration of
    /// every call it is passed to: Dispose while a worker thread is reading defers the native
    /// delete until that read returns, instead of freeing the file under it.
    /// </summary>
    public sealed class EegFileHandle : SafeHandle
    {
        public const int StatusOk = 0;
        public const int StatusArgument = -1;
        public const int StatusUnsupported = -2;
        public const int StatusIO = -3;
        public const int StatusFormat = -4;

        private EegFileHandle() : base(IntPtr.Zero, true) { }

        public override bool IsInvalid => handle == IntPtr.Zero;

        protected override bool ReleaseHandle()
        {
            DeleteGenericFile(handle);
            return true;
        }

        /// <summary>Opens the file without loading its samples; throws FileLoadException as File does.</summary>
        public static EegFileHandle OpenHeaderOnly(File.FileType type, params string[] paths)
        {
            string dataPath = paths.Length > 0 ? paths[0] : "";
            IntPtr file;
            switch (type)
            {
                case File.FileType.ELAN:
                    file = CreateElanFile(dataPath, paths.Length > 1 ? paths[1] : "", paths.Length > 2 ? paths[2] : "", false);
                    break;
                case File.FileType.EDF:
                    file = CreateEDFFile(dataPath, false);
                    break;
                case File.FileType.Micromed:
                    file = CreateMicromedFile(dataPath, false);
                    break;
                case File.FileType.BrainVision:
                    file = CreateBrainVisionFile(dataPath, false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown EEG file type");
            }
            EegFileHandle result = new EegFileHandle();
            result.SetHandle(file);
            if (result.IsInvalid)
                throw new System.IO.FileLoadException("The " + type + " reader could not open this file (corrupt, truncated, or a companion file is missing).", dataPath);
            return result;
        }

        /// <summary>The message of the last EEGF_ failure on the calling thread.</summary>
        public static string LastError()
        {
            byte[] buffer = new byte[1024];
            int length = GetLastError(buffer, buffer.Length);
            return Encoding.UTF8.GetString(buffer, 0, Math.Max(0, Math.Min(length, buffer.Length - 1)));
        }

        #region DLLImport
        [DllImport("EEGFormat", EntryPoint = "CreateMicromedFile", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr CreateMicromedFile(string filePath, bool loadData);
        [DllImport("EEGFormat", EntryPoint = "CreateElanFile", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr CreateElanFile(string dataPath, string eventsPath, string notesPath, bool loadData);
        [DllImport("EEGFormat", EntryPoint = "CreateEDFFile", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr CreateEDFFile(string filePath, bool loadData);
        [DllImport("EEGFormat", EntryPoint = "CreateBrainVisionFile", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr CreateBrainVisionFile(string filePath, bool loadData);
        [DllImport("EEGFormat", EntryPoint = "DeleteGenericFile", CallingConvention = CallingConvention.Cdecl)]
        private static extern void DeleteGenericFile(IntPtr file);

        [DllImport("EEGFormat", EntryPoint = "GetElectrodeCount", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GetElectrodeCount(EegFileHandle file);
        [DllImport("EEGFormat", EntryPoint = "GetSamplingFrequency", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GetSamplingFrequency(EegFileHandle file);
        [DllImport("EEGFormat", EntryPoint = "EEGF_GetSampleCount64", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GetSampleCount64(EegFileHandle file, out long sampleCount);
        [DllImport("EEGFormat", EntryPoint = "EEGF_ReadRange", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int ReadRange(EegFileHandle file, long firstSample, int count, int[] channels, int channelCount, float[] output, long outputCapacity, out int samplesRead);
        [DllImport("EEGFormat", EntryPoint = "EEGF_GetLastError", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetLastError(byte[] buffer, int capacity);
        #endregion
    }
}
