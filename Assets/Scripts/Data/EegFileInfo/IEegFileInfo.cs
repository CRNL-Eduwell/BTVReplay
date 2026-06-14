using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tools.CSharp.EEG;
using System;

public interface IEegFileInfo : ICloneable
{
    File.FileType FileType { get; }
    string[] Files { get; }

    List<ArgumentException> CheckForErrors();

    /// <summary>
    /// Applies the transformation to every stored file path (used by the persistence layer to
    /// expand/tokenize portable paths - see PathTokens).
    /// </summary>
    void TransformPaths(Func<string, string> transform);
}
