using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tools.CSharp.EEG;

public interface IEegFileInfo
{
    File.FileType FileType { get; }
    string[] Files { get; }
}
