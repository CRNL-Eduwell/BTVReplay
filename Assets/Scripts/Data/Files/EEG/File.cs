using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;

namespace Tools.CSharp.EEG
{
    public class File : DLL.CppDLLImportBase
    {
        #region Properties
        public enum FileType { ELAN, EDF, Micromed, BrainVision }

        /// <summary>
        /// Size of the data
        /// </summary>
        public int NumberOfSamples
        {
            get
            {
                return GetNumberOfSamples(_handle);
            }
        }
        /// <summary>
        /// Number of electrodes in this file
        /// </summary>
        public int ElectrodeCount
        {
            get
            {
                return GetElectrodeCount(_handle);
            }
        }
        /// <summary>
        /// List of electrodes of this file. WARNING: every access re-reads and re-marshals ALL
        /// samples from the native file - read it once into a local, never inside a loop. It
        /// also doubles the memory of the samples while both copies exist: IEegDataContainer
        /// moves them one electrode at a time instead.
        /// </summary>
        public List<Electrode> Electrodes
        {
            get
            {
                int electrodeCount = ElectrodeCount;
                int numberOfSamples = NumberOfSamples;
                List<Electrode> electrodes = new List<Electrode>(electrodeCount);
                for (int i = 0; i < electrodeCount; i++)
                {
                    float[] data = null;
                    if (numberOfSamples != 0)
                    {
                        data = new float[numberOfSamples];
                        GetElectrodeData(_handle, i, 0, data, data.Length);
                    }
                    electrodes.Add(new Electrode(GetElectrode(_handle, i), data));
                }
                return electrodes;
            }
        }
        /// <summary>
        /// Number of triggers in this file
        /// </summary>
        public int TriggerCount
        {
            get
            {
                return GetTriggerCount(_handle);
            }
        }
        /// <summary>
        /// List of triggers of this file
        /// </summary>
        public List<Trigger> Triggers
        {
            get
            {
                int triggerCount = TriggerCount;
                List<Trigger> triggers = new List<Trigger>(triggerCount);
                for (int i = 0; i < triggerCount; i++)
                {
                    triggers.Add(new Trigger(GetTrigger(_handle, i)));
                }
                return triggers;
            }
        }
        /// <summary>
        /// Number of notes in this file
        /// </summary>
        public int NoteCount
        {
            get
            {
                return GetNoteCount(_handle);
            }
        }
        /// <summary>
        /// List of notes of this file
        /// </summary>
        public List<Note> Notes
        {
            get
            {
                int noteCount = NoteCount;
                List<Note> notes = new List<Note>(noteCount);
                for (int i = 0; i < noteCount; i++)
                {
                    notes.Add(new Note(GetNote(_handle, i)));
                }
                return notes;
            }
        }
        /// <summary>
        /// Sampling frequency of this file
        /// </summary>
        public Frequency SamplingFrequency
        {
            get
            {
                return new Frequency(GetSamplingFrequency(_handle));
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Fix the name of the electrodes using the same pattern as Site Name Correction
        /// </summary>
        public void FixElectrodeName()
        {
            FixElectrodeName(_handle);
        }
        /// <summary>
        /// Electrode at this index, without copying its samples (Data is null). Its Label and Unit
        /// are read live from native memory: read them before deleting this electrode.
        /// </summary>
        public Electrode GetElectrodeWithoutData(int index)
        {
            return new Electrode(GetElectrode(_handle, index), null);
        }
        /// <summary>
        /// Copy of the samples of one electrode (numberOfSamples must be NumberOfSamples)
        /// </summary>
        public float[] ReadElectrodeData(int index, int numberOfSamples)
        {
            float[] data = new float[numberOfSamples];
            GetElectrodeData(_handle, index, 0, data, data.Length);
            return data;
        }
        /// <summary>
        /// Delete some electrodes and their data
        /// </summary>
        /// <param name="electrodes"></param>
        public void DeleteElectrodesAndData(int[] electrodes)
        {
            DeleteElectrodesAndData(_handle, electrodes, electrodes.Length);
        }
        /// <summary>
        /// Load the data of the file
        /// </summary>
        public void Load()
        {
            Load(_handle);
        }
        /// <summary>
        /// Save the file
        /// </summary>
        public void Save()
        {
            Save(_handle);
        }
        /// <summary>
        /// Save the file in a specific directory with a specific base name (without extension)
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="baseFileName"></param>
        public void SaveAs(string directoryPath, string baseFileName)
        {
            SaveAs(_handle, directoryPath, baseFileName);
        }
        #endregion

        #region Memory Management
        /// <summary>
        /// File constructor with an already allocated dll File
        /// </summary>
        /// <param name="filePtr"></param>
        public File(IntPtr filePtr) : base(filePtr) { }
        /// <summary>
        /// File constructor
        /// </summary>
        public File(FileType type, bool loadData, params string[] paths)
        {
            string dataPath = paths.Length > 0 ? paths[0] : "";
            string eventsPath, notesPath;
            switch (type)
            {
                case FileType.ELAN:
                    eventsPath = paths.Length > 1 ? paths[1] : "";
                    notesPath = paths.Length > 2 ? paths[2] : "";
                    _handle = new HandleRef(this, CreateElanFile(dataPath, eventsPath, notesPath, loadData));
                    break;
                case FileType.EDF:
                    _handle = new HandleRef(this, CreateEDFFile(dataPath, loadData));
                    break;
                case FileType.Micromed:
                    _handle = new HandleRef(this, CreateMicromedFile(dataPath, loadData));
                    break;
                case FileType.BrainVision:
                    _handle = new HandleRef(this, CreateBrainVisionFile(dataPath, loadData));
                    break;
            }

            // The native readers catch their own errors and return null (a corrupt or truncated
            // file, a missing Elan .ent sidecar, a BrainVision header pointing at missing data).
            // Every getter would then pass that null to native code, which dereferences it: a
            // hard crash of the whole app instead of an error the loader can report.
            if (_handle.Handle == IntPtr.Zero)
            {
                GC.SuppressFinalize(this);
                throw new System.IO.FileLoadException("The " + type + " reader could not open this file (corrupt, truncated, or a companion file is missing).", dataPath);
            }
        }
        /// <summary>
        /// Allocate DLL memory
        /// </summary>
        protected override void create_DLL_class()
        {
        }
        /// <summary>
        /// Clean DLL memory
        /// </summary>
        protected override void delete_DLL_class()
        {
            // Zero when the native open failed, or after Dispose: nothing to free.
            if (_handle.Handle != IntPtr.Zero)
                DeleteFile(_handle);
        }
        #endregion

        #region DLLImport
        [DllImport("EEGFormat", EntryPoint = "CreateMicromedFile", CallingConvention = CallingConvention.Cdecl)]
        static private extern IntPtr CreateMicromedFile(string filePath, bool loadData);
        [DllImport("EEGFormat", EntryPoint = "CreateElanFile", CallingConvention = CallingConvention.Cdecl)]
        static private extern IntPtr CreateElanFile(string dataPath, string eventsPath, string notesPath, bool loadData);
        [DllImport("EEGFormat", EntryPoint = "CreateEDFFile", CallingConvention = CallingConvention.Cdecl)]
        static private extern IntPtr CreateEDFFile(string filePath, bool loadData);
        [DllImport("EEGFormat", EntryPoint = "CreateBrainVisionFile", CallingConvention = CallingConvention.Cdecl)]
        static private extern IntPtr CreateBrainVisionFile(string filePath, bool loadData);
        [DllImport("EEGFormat", EntryPoint = "DeleteGenericFile", CallingConvention = CallingConvention.Cdecl)]
        static private extern void DeleteFile(HandleRef fileToDelete);

        [DllImport("EEGFormat", EntryPoint = "GetNumberOfSamples", CallingConvention = CallingConvention.Cdecl)]
        static private extern int GetNumberOfSamples(HandleRef file);
        [DllImport("EEGFormat", EntryPoint = "GetElectrodeCount", CallingConvention = CallingConvention.Cdecl)]
        static private extern int GetElectrodeCount(HandleRef file);
        [DllImport("EEGFormat", EntryPoint = "GetTriggerCount", CallingConvention = CallingConvention.Cdecl)]
        static private extern int GetTriggerCount(HandleRef file);
        [DllImport("EEGFormat", EntryPoint = "GetNoteCount", CallingConvention = CallingConvention.Cdecl)]
        static private extern int GetNoteCount(HandleRef file);
        [DllImport("EEGFormat", EntryPoint = "GetSamplingFrequency", CallingConvention = CallingConvention.Cdecl)]
        static private extern int GetSamplingFrequency(HandleRef file);

        [DllImport("EEGFormat", EntryPoint = "GetData", CallingConvention = CallingConvention.Cdecl)]
        static private extern float GetData(HandleRef file, int electrodeID, int sample, int dataConverterType);
        [DllImport("EEGFormat", EntryPoint = "GetElectrodeData", CallingConvention = CallingConvention.Cdecl)]
        static private extern void GetElectrodeData(HandleRef file, int electrodeID, int dataConverterType, float[] values, int size);
        [DllImport("EEGFormat", EntryPoint = "GetElectrode", CallingConvention = CallingConvention.Cdecl)]
        static private extern IntPtr GetElectrode(HandleRef file, int electrodeID);
        [DllImport("EEGFormat", EntryPoint = "GetTrigger", CallingConvention = CallingConvention.Cdecl)]
        static private extern IntPtr GetTrigger(HandleRef file, int triggerID);
        [DllImport("EEGFormat", EntryPoint = "GetNote", CallingConvention = CallingConvention.Cdecl)]
        static private extern IntPtr GetNote(HandleRef file, int noteID);

        [DllImport("EEGFormat", EntryPoint = "FixElectrodeName", CallingConvention = CallingConvention.Cdecl)]
        static private extern void FixElectrodeName(HandleRef file);
        [DllImport("EEGFormat", EntryPoint = "DeleteElectrodesAndData", CallingConvention = CallingConvention.Cdecl)]
        static private extern void DeleteElectrodesAndData(HandleRef file, int[] electrodesToDelete, int numberOfElectrodesToDelete);
        [DllImport("EEGFormat", EntryPoint = "Load", CallingConvention = CallingConvention.Cdecl)]
        static private extern void Load(HandleRef file);
        [DllImport("EEGFormat", EntryPoint = "Save", CallingConvention = CallingConvention.Cdecl)]
        static private extern void Save(HandleRef file);
        [DllImport("EEGFormat", EntryPoint = "SaveAs", CallingConvention = CallingConvention.Cdecl)]
        static private extern void SaveAs(HandleRef file, string directoryPath, string baseFileName);
        #endregion
    }
}