using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using BrainTV.Tools.NumberExtensions;

namespace Assets.Scripts.Data
{
    class PtsFile
    {
        public List<Tuple<string, Vector3>> Electrodes
        {
            get;
            private set;
        }
        private string m_FilePath = "";

        public PtsFile(string FilePath)
        {
            m_FilePath = FilePath;
            
            if (File.Exists(m_FilePath))
                Load(m_FilePath);
            else
                Debug.LogError("PtsFile => Filepath : " + m_FilePath + " does not exist ");
        }

        private int Load(string FilePath)
        {
            Electrodes = new List<Tuple<string, Vector3>>();

            using (StreamReader sr = new StreamReader(FilePath))
            {
                string line = sr.ReadLine();
                if (!line.StartsWith("ptsfile"))
                {
                    Debug.LogError("Error PtsFile.Load -> format file incorrect : " + FilePath);
                    return -1;
                }

                line = sr.ReadLine();
                line = sr.ReadLine();
                int numberPlot = int.Parse(line);

                if (numberPlot <= 0)
                {
                    Debug.LogError("Error PtsFile.Load -> format file incorrect : " + FilePath);
                    return -1;
                }

                for (int i = 0; i < numberPlot; i++)
                {
                    line = sr.ReadLine();
                    string[] split = line.Split(new string[] { "\t" }, System.StringSplitOptions.RemoveEmptyEntries);

                    if (split.Length >= 4)
                    {
                        string plot = split.GetValue(0).ToString().ToLower();

                        string x_coord = split.GetValue(1).ToString();
                        x_coord.TryParseFloat(out float x);
                        string y_coord = split.GetValue(2).ToString();
                        y_coord.TryParseFloat(out float y);
                        string z_coord = split.GetValue(3).ToString();
                        z_coord.TryParseFloat(out float z);

                        Electrodes.Add(new Tuple<string, Vector3>(plot, new Vector3(x, y, z)));
                    }
                    else
                    {
                        Debug.LogError("Error Reading Pts : Each Line must have at least 4 elements (label + xyz coordinates) ");
                    }
                }
            }

            return 0;
        }
    }
}
