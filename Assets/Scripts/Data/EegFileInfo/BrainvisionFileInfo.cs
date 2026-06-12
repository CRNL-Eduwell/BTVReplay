using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

public class BrainvisionFileInfo : IEegFileInfo
{
    #region public properties
    public Tools.CSharp.EEG.File.FileType FileType
    {
        get
        {
            return Tools.CSharp.EEG.File.FileType.BrainVision;
        }
    }
    public string Header { get; private set; } = "";
    [JsonIgnore]
    public string[] Files { get { return new string[] { Header }; } }
    #endregion

    public BrainvisionFileInfo(string bvheader)
    {
        Header = bvheader;
    }

    public List<ArgumentException> CheckForErrors()
    {
        List<ArgumentException> Errors = new List<ArgumentException>();
        if (string.IsNullOrEmpty(Header)) Errors.Add(new ArgumentException("Brainvision file path should not be a null string"));
        FileInfo fileInfo = new FileInfo(Header);
        if (fileInfo.Extension != ".vhdr") Errors.Add(new ArgumentException("File extension should be .vhdr, i am seeing " + fileInfo.Extension));
        if (!fileInfo.Exists) Errors.Add(new ArgumentException("It seems the given file does not exist, please check if the file is present at this path : " + fileInfo.FullName));
        return Errors;
    }

    #region operators
    public override bool Equals(object obj)
    {
        if (obj is BrainvisionFileInfo baseData)
        {
            return string.Equals(Header, baseData.Header);
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
        return new BrainvisionFileInfo(Header);
    }

    public static bool operator ==(BrainvisionFileInfo a, BrainvisionFileInfo b)
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
    public static bool operator !=(BrainvisionFileInfo a, BrainvisionFileInfo b)
    {
        return !(a == b);
    }
    #endregion
}
