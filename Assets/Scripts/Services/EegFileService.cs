using System;
using System.Collections;
using Tools.CSharp.EEG;
using BTV.Data;

//If need destructor https://stackoverflow.com/questions/4364665/static-destructor

namespace BTV.Services.EegFileService
{
    public static class EegFileService
    {
        private static BtvProgram[] m_EegFiles = new BtvProgram[6];

        public static IEnumerator c_Load(string FilePath, File.FileType Type, int FileID)
        {
            if (FileID >= 6)
                throw new ArgumentException("There is only 6 possible file to load, fileID argument is wrong => " + FileID);

            if (System.IO.File.Exists(FilePath))
            {
                IEegDataContainer container = new IEegDataContainer(FilePath, Type);
                m_EegFiles[FileID] = new BtvProgram(container);
            }

            yield return null;
        }

        public static void Load(string FilePath, File.FileType Type, int FileID)
        {
            if (FileID >= 6)
                throw new ArgumentException("There is only 6 possible file to load, fileID argument is wrong => " + FileID);

            if (System.IO.File.Exists(FilePath))
            {
                IEegDataContainer container = new IEegDataContainer(FilePath, Type);
                m_EegFiles[FileID] = new BtvProgram(container);
            }
        }

        public static BtvProgram ChangeContainerHandle(BtvProgram currentFile, int newID)
        {
            return m_EegFiles[newID] != null ? m_EegFiles[newID] : currentFile;
        }

        public static BtvProgram ReturnFirstValidContainer()
        {
            for (int i = 0; i < m_EegFiles.Length; i++)
            {
                if (m_EegFiles[i] != null)
                    return m_EegFiles[i];
            }

            return null;
        }

        public static void AddNewChannel(float[] Data, string Name, int SamplingFrequency, int ProgramID)
        {
            if (m_EegFiles[ProgramID] == null)
                throw new Exception("Error : Attempting to add data to an empty program");

            if (m_EegFiles[ProgramID].Frequency.Value != SamplingFrequency)
                throw new Exception("Error : Sampling Frequency from new data is different from the program");

            m_EegFiles[ProgramID].AddData(Data, Name);
        }
    }
}
