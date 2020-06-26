using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        Eeg = eeg;
        Pos = pos;
        Notes = notes;
    }

    public List<ArgumentException> ChecKForErrors()
    {
        List<ArgumentException> Errors = new List<ArgumentException>();

        if (string.IsNullOrEmpty(Eeg))
            Errors.Add(new ArgumentException("Elan eeg file path should not be a null string"));

        List<ArgumentException> eegErrors = CheckFileForError(Eeg, ".eeg");
        if (eegErrors != null)
            Errors = Errors.Concat(eegErrors).ToList();

        if (!string.IsNullOrEmpty(Pos))
        {
            List<ArgumentException> posError = CheckFileForError(Pos, ".pos");
            if (posError != null)
                Errors = Errors.Concat(posError).ToList();
        }
        if (!string.IsNullOrEmpty(Notes))
        {
            List<ArgumentException> notesError = CheckFileForError(Notes, ".txt");
            if (notesError != null)
                Errors.Concat(notesError).ToList();
        }

        return Errors;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="extention">file extention, ie : .txt </param>
    private List<ArgumentException> CheckFileForError(string path, string extention)
    {
        List<ArgumentException> Errors = new List<ArgumentException>();
        FileInfo fileInfo = new FileInfo(path);
        if (fileInfo.Extension != extention) Errors.Add(new ArgumentException("File extension should be " + extention + ", i am seeing " + fileInfo.Extension));
        if (!fileInfo.Exists) Errors.Add(new ArgumentException("It seems the given file does not exist, please check if the file is present at this path : " + fileInfo.FullName));
        return Errors;
    }

    #region operators
    public override bool Equals(object obj)
    {
        if (obj is ElanFileInfo baseData)
        {
            return string.Equals(Eeg, baseData.Eeg) && string.Equals(Ent, baseData.Ent) && string.Equals(Pos, baseData.Pos) && string.Equals(Notes, baseData.Notes);
        }
        else
        {
            return false;
        }
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public static bool operator ==(ElanFileInfo a, ElanFileInfo b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        if (((object)a == null) || ((object)b == null))
        {
            return false;
        }

        return a.Equals(b);
    }
    public static bool operator !=(ElanFileInfo a, ElanFileInfo b)
    {
        return !(a == b);
    }
    #endregion
}