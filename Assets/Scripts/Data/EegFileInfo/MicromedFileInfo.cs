using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

public class MicromedFileInfo : IEegFileInfo
{
    #region public properties
    public Tools.CSharp.EEG.File.FileType FileType
    {
        get
        {
            return Tools.CSharp.EEG.File.FileType.Micromed;
        }
    }
    public string Trc { get; private set; } = "";
    [JsonIgnore]
    public string[] Files { get { return new string[] { Trc }; } }
    #endregion

    public MicromedFileInfo(string trc)
    {
        Trc = trc;
    }

    public List<ArgumentException> ChecKForErrors()
    {
        List<ArgumentException> Errors = new List<ArgumentException>();
        if (string.IsNullOrEmpty(Trc)) Errors.Add(new ArgumentException("Trc file path should not be a null string"));
        FileInfo fileInfo = new FileInfo(Trc);
        if (fileInfo.Extension != ".TRC") Errors.Add(new ArgumentException("File extension should be .TRC, i am seeing " + fileInfo.Extension));
        if (!fileInfo.Exists) Errors.Add(new ArgumentException("It seems the given file does not exist, please check if the file is present at this path : " + fileInfo.FullName));
        return Errors;
    }

    #region operators
    public override bool Equals(object obj)
    {
        if (obj is MicromedFileInfo baseData)
        {
            return string.Equals(Trc, baseData.Trc);
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

    public object Clone()
    {
        return new MicromedFileInfo(Trc);
    }

    public static bool operator ==(MicromedFileInfo a, MicromedFileInfo b)
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
    public static bool operator !=(MicromedFileInfo a, MicromedFileInfo b)
    {
        return !(a == b);
    }
    #endregion
}