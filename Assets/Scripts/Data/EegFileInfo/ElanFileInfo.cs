using Newtonsoft.Json;
using System;
using System.IO;

public class ElanFileInfo : IEegFileInfo
{
    #region public properties
    public Tools.CSharp.EEG.File.FileType FileType
    {
        get
        {
            return Tools.CSharp.EEG.File.FileType.ELAN;
        }
    }
    public string Eeg { get; private set; } = "";
    public string Ent { get { return Eeg + ".ent"; } }
    public string Pos { get; set; } = "";
    public string Notes { get; set; } = "";
    [JsonIgnore]
    public string[] Files { get { return new string[] { Eeg, Pos, Notes }; } }
    #endregion

    public ElanFileInfo(string eeg, string pos = "", string notes = "")
    {
        if (string.IsNullOrEmpty(eeg)) throw new ArgumentException("Elan file path should not be a null string");

        FileInfo eegFileInfo = new FileInfo(eeg);
        if (eegFileInfo.Extension != ".eeg")
            throw new ArgumentException("File extension should be .eeg, i am seeing " + eegFileInfo.Extension);
        if (!eegFileInfo.Exists)
            throw new ArgumentException("It seems the given file does not exist, please check if the file is present at this path : " + eegFileInfo.FullName);

        Eeg = eeg;

        if (!string.IsNullOrEmpty(pos))
        {
            FileInfo posFileInfo = new FileInfo(pos);
            if (posFileInfo.Extension != ".pos")
                throw new ArgumentException("File extension should be .pos, i am seeing " + posFileInfo.Extension);
            if (!posFileInfo.Exists)
                throw new ArgumentException("It seems the given file does not exist, please check if the file is present at this path : " + posFileInfo.FullName);
        }
        Pos = pos;

        if (!string.IsNullOrEmpty(notes))
        {
            FileInfo notesFileInfo = new FileInfo(notes);
            if (notesFileInfo.Extension != ".txt")
                throw new ArgumentException("File extension should be .txt, i am seeing " + notesFileInfo.Extension);
            if (!notesFileInfo.Exists)
                throw new ArgumentException("It seems the given file does not exist, please check if the file is present at this path : " + notesFileInfo.FullName);
        }
        Notes = notes;
    }
}