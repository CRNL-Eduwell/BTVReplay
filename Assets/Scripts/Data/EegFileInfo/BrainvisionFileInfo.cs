using Newtonsoft.Json;
using System;
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
        if (string.IsNullOrEmpty(bvheader)) throw new ArgumentException("Brainvision file path should not be a null string");

        FileInfo fileInfo = new FileInfo(bvheader);
        if (fileInfo.Extension != ".vhdr")
            throw new ArgumentException("File extension should be .vhdr, i am seeing " + fileInfo.Extension);
        if (!fileInfo.Exists)
            throw new ArgumentException("It seems the given file does not exist, please check if the file is present at this path : " + fileInfo.FullName);

        Header = fileInfo.FullName;
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
