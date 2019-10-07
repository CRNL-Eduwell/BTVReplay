using System;
using System.IO;

namespace Assets.Scripts.Data.Files
{
    public static class CsvFile
    {
        public static void SaveFloatDataHorizontally(string filePath, float[][] Data)
        {
            using (StreamWriter writter = new StreamWriter(filePath))
            {
                for (int i = 0; i < Data.Length; i++)
                {
                    for (int j = 0; j < Data[i].Length; j++)
                    {
                        writter.Write(Data[i][j] + ";");
                    }
                    writter.Write("\n");
                }
            }
            //writter.Close();
        }

        public static float[][] LoadFloatDataHorizontally(string filePath)
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string[] RawLines = reader.ReadToEnd().Split(new char[] { '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                //reader.Close();

                float[][] Data = new float[RawLines.Length][];
                for (int i = 0; i < Data.Length; i++)
                {
                    string[] RawElement = RawLines[i].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    Data[i] = new float[RawElement.Length];
                    for (int j = 0; j < Data[i].Length; j++)
                    {
                        Data[i][j] = float.Parse(RawElement[j]);
                    }
                }
                return Data;
            }
        }
    }
}