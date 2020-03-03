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
    public string[] Files { get { return new string[] { Edf }; } }
    public string Edf { get; private set; } = "";
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
}
