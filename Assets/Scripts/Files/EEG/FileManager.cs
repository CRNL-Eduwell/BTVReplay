using System;
using System.Collections;
using Tools.CSharp.EEG;

namespace Assets.Scripts.Files.EEG
{
    public class FileManager
    {
        public static File changeHandle(File currentFile, File[] elanFiles, int newID)
        {
            if (elanFiles[newID] != null)
            {
                //elanFiles[newID].idFileHandle = newID;
                return elanFiles[newID];
            }
            else
            {
                return currentFile;
            }
        }

        public static bool checkHandle(File[] elanFiles, int newID)
        {
            if (elanFiles[newID] != null)
                return true;
            else
                return false;
        }

        public static File returnFirstValidHandle(File[] elanFiles)
        {
            for (int i = 0; i < elanFiles.Length; i++)
            {
                if (elanFiles[i] != null)
                    return elanFiles[i];
            }

            return null;
        }

        public static int returnFirstValidHandleId(File[] elanFiles)
        {
            for (int i = 0; i < elanFiles.Length; i++)
            {
                if (elanFiles[i] != null)
                    return i;
            }

            return -1;
        }

        public static IEnumerator c_loadIfExist(string filePath, Action<File> resultCB)
        {
            if (filePath != "") //System.IO.File.Exists(filePath))
            {
                UnityEngine.Debug.Log("Loading " + filePath);
                File elan = new File(File.FileType.ELAN, true, new string[] { filePath });
                while (elan.NumberOfSamples != elan.Electrodes[0].Data.Length)
                    yield return null;
                resultCB(elan);
            }
            else
            {
                resultCB(null);
            }
        }

        public static float getSamplingFreq(File[] elanFiles)
        {
            for (int i = 0; i < elanFiles.Length; i++)
            {
                if (elanFiles[i] != null)
                    return elanFiles[i].SamplingFrequency.RawValue;
            }

            return -1;
        }

        public static long getTotalFileDuration(File file)
        {
            return (long)(file.NumberOfSamples / file.SamplingFrequency.RawValue);
        }
    }
}
