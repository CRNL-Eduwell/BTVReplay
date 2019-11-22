using System;
using System.IO;
using System.Collections.Generic;

public class ChangeCodeFile
{
    public List<List<KeyValuePair<int, int>>> OldCodes { get; private set; } = new List<List<KeyValuePair<int, int>>>();
    public List<KeyValuePair<int, int>> NewCodes { get; private set; } = new List<KeyValuePair<int, int>>();
    private string m_FilePath = "";

    public ChangeCodeFile(string FilePath)
    {
        m_FilePath = FilePath;

        if (File.Exists(m_FilePath))
            Load(m_FilePath);
        else
            UnityEngine.Debug.LogError("ChangeCodeFile => Filepath : " + m_FilePath + " does not exist ");
    }

    private int Load(string FilePath)
    {
        using (StreamReader sr = new StreamReader(FilePath))
        {
            string[] changeFileSplit = sr.ReadToEnd().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < changeFileSplit.Length; i++)
            {
                string[] lineSplit = changeFileSplit[i].Split(new char[] { '+', '=' });
                KeyValuePair<int, int> oldCode = new KeyValuePair<int, int>(Convert.ToInt32(lineSplit[0]), Convert.ToInt32(lineSplit[1]));
                KeyValuePair<int, int> newCode = new KeyValuePair<int, int>(Convert.ToInt32(lineSplit[2]), Convert.ToInt32(lineSplit[3]));

                if (NewCodes.Contains(newCode)) //if the pair already exists
                {
                    int indexOfPair = NewCodes.IndexOf(newCode);
                    OldCodes[indexOfPair].Add(oldCode);
                }
                else
                {
                    NewCodes.Add(newCode);
                    //==
                    List<KeyValuePair<int, int>> newVector = new List<KeyValuePair<int, int>>();
                    newVector.Add(oldCode);
                    OldCodes.Add(newVector);
                }
            }
        }

        return 0;
    }
}
