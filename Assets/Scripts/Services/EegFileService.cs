using System;
using System.Collections;
using Tools.CSharp.EEG;
using BTV.Data.DataContainer;

//If need destructor https://stackoverflow.com/questions/4364665/static-destructor

namespace BTV.Services.EegFileService
{
    public static class EegFileService
    {
        private static DataContainer[] m_EegFiles = new DataContainer[6];

        public static IEnumerator c_Load(string FilePath, File.FileType Type, int FileID)
        {
            if (FileID >= 6)
                throw new ArgumentException("There is only 6 possible file to load, fileID argument is wrong => " + FileID);

            if (System.IO.File.Exists(FilePath))
            {
                m_EegFiles[FileID] = new DataContainer(FilePath, Type);
            }

            yield return null;
        }

        public static void Load(string FilePath, File.FileType Type, int FileID)
        {
            if (FileID >= 6)
                throw new ArgumentException("There is only 6 possible file to load, fileID argument is wrong => " + FileID);

            if (System.IO.File.Exists(FilePath))
            {
                m_EegFiles[FileID] = new DataContainer(FilePath, Type);
            }
        }

        public static DataContainer ChangeContainerHandle(DataContainer currentFile, int newID)
        {
            return m_EegFiles[newID] != null ? m_EegFiles[newID] : currentFile;
        }

        public static DataContainer ReturnFirstValidContainer()
        {
            for (int i = 0; i < m_EegFiles.Length; i++)
            {
                if (m_EegFiles[i] != null)
                    return m_EegFiles[i];
            }

            return null;
        }
    }
}
