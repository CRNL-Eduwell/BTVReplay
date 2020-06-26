using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tools.CSharp.EEG;
using System;

public interface IEegFileInfo
{
    File.FileType FileType { get; }
    string[] Files { get; }

    List<ArgumentException> ChecKForErrors();
}
