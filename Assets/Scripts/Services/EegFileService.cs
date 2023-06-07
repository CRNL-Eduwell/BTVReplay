using System;
using System.Collections;
using Tools.CSharp.EEG;
using BTV.Data;
using CielaSpike;
using System.Collections.Generic;

//If need destructor https://stackoverflow.com/questions/4364665/static-destructor

namespace BTV.Services.EegFileService
{
    public static class EegFileService
    {
        private static List<BtvMontage> m_Montages = new List<BtvMontage>();
        public static int SelectedMontageID { get; set; } = 0;
        private static BtvMontage m_CurrentMontage { get { return m_Montages[SelectedMontageID]; } }

        public static void Reset()
        {
            m_Montages = new List<BtvMontage>() { new BtvMontage("Default", new BtvProgram[6] { null, null, null, null, null, null }) };
        }

        public static IEnumerator c_Load(IEegFileInfo fileInfo, int FileID, string description)
        {
            if (FileID >= 6)
                throw new ArgumentException("There is only 6 possible file to load, fileID argument is wrong => " + FileID);

            BtvProgram[] eegFiles = new BtvProgram[6];
            if (fileInfo.Files.Length > 0 && System.IO.File.Exists(fileInfo.Files[0]))
            {
                IEegDataContainer container = new IEegDataContainer(fileInfo);
                eegFiles[FileID] = new BtvProgram(container, description);
            }
            m_Montages[0] = new BtvMontage("Default", eegFiles);

            yield return null;
        }

        public static void Load(IEegFileInfo fileInfo, int FileID, string description)
        {
            if (FileID >= 6)
                throw new ArgumentException("There is only 6 possible file to load, fileID argument is wrong => " + FileID);

            BtvProgram[] eegFiles = new BtvProgram[6];
            if (fileInfo.Files.Length > 0 && System.IO.File.Exists(fileInfo.Files[0]))
            {
                IEegDataContainer container = new IEegDataContainer(fileInfo);
                eegFiles[FileID] = new BtvProgram(container, description);
            }
            m_Montages[0] = new BtvMontage("Default", eegFiles);
        }

        public static int GetContainerSuffix(BtvProgram currentFile)
        {
            for (int i = 0; i < m_CurrentMontage.EegFiles.Length; i++)
            {
                if (m_CurrentMontage.EegFiles[i] == currentFile) 
                    return i;
            }
            return -1;
        }

        public static BtvProgram ChangeContainerHandle(BtvProgram currentFile, int newID)
        {
            return m_CurrentMontage.EegFiles[newID] != null ? m_CurrentMontage.EegFiles[newID] : currentFile;
        }

        public static BtvProgram ReturnFirstValidContainer()
        {
            for (int i = 0; i < m_CurrentMontage.EegFiles.Length; i++)
            {
                if (m_CurrentMontage.EegFiles[i] != null)
                    return m_CurrentMontage.EegFiles[i];
            }

            return null;
        }

        public static bool IsFileIdValid(int FileID)
        {
            if (FileID < 0) return false;
            if (FileID >= 6) return false;

            return m_CurrentMontage.EegFiles[FileID] != null;
        }

        public static void AddNewChannel(float[] Data, string Name, int SamplingFrequency, int ProgramID)
        {
            if (m_CurrentMontage.EegFiles[ProgramID] == null)
                throw new Exception("Error : Attempting to add data to an empty program");

            if (m_CurrentMontage.EegFiles[ProgramID].Frequency.Value != SamplingFrequency)
                throw new Exception("Error : Sampling Frequency from new data is different from the program");

            m_CurrentMontage.EegFiles[ProgramID].AddData(Data, Name);
        }
    }
}
