using Newtonsoft.Json;
using System;
using System.IO;

public class EdfFileInfo : IEegFileInfo
{
    #region public properties
    public Tools.CSharp.EEG.File.FileType FileType
    {
        get
        {
            return Tools.CSharp.EEG.File.FileType.EDF;
        }
    }
    public string Edf { get; private set; } = "";
    [JsonIgnore]
    public string[] Files { get { return new string[] { Edf }; } }
    #endregion

    public EdfFileInfo(string edf)
    {
        if (string.IsNullOrEmpty(edf)) throw new ArgumentException("Edf file path should not be a null string");

        FileInfo fileInfo = new FileInfo(edf);
        if (fileInfo.Extension != ".edf")
            throw new ArgumentException("File extension should be .edf, i am seeing " + fileInfo.Extension);
        if (!fileInfo.Exists)
            throw new ArgumentException("It seems the given file does not exist, please check if the file is present at this path : " + fileInfo.FullName);

        Edf = fileInfo.FullName;
    }

    #region operators
    public override bool Equals(object obj)
    {
        if (obj is EdfFileInfo baseData)
        {
            return string.Equals(Edf, baseData.Edf);
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

    public static bool operator ==(EdfFileInfo a, EdfFileInfo b)
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
    public static bool operator !=(EdfFileInfo a, EdfFileInfo b)
    {
        return !(a == b);
    }
    #endregion
}
