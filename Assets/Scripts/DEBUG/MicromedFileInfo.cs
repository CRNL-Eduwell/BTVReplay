using System;
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
    public string[] Files { get { return new string[] { Trc }; } }
    public string Trc { get; private set; } = "";
    #endregion

    public MicromedFileInfo(string trc)
    {
        if (string.IsNullOrEmpty(trc)) throw new ArgumentException("Trc file path should not be a null string");

        FileInfo fileInfo = new FileInfo(trc);
        if(fileInfo.Extension != ".TRC")
            throw new ArgumentException("File extension should be .TRC, i am seeing " + fileInfo.Extension);
        if (!fileInfo.Exists)
            throw new ArgumentException("It seems the given file does not exist, please check if the file is present at this path : " + fileInfo.FullName);

        Trc = fileInfo.FullName;
    }
}