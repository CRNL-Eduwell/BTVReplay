using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PatientGUIManager : MonoBehaviour
{
    [SerializeField] BrainAnatGUIManager _MniGUIManager = null;
    [SerializeField] BrainAnatGUIManager _PatGUIManager = null;
    [SerializeField] EegInfoGUIManager[] _EegFiles = new EegInfoGUIManager[6] { null, null, null, null, null, null, };
    [SerializeField] browseButton _Video = null;

    public Subject GetSubjectsFromGUI()
    {
        Subject myPat = new Subject();

        myPat.AnatomicalSpaces.Add("MNI", _MniGUIManager.GetDataContainer());
        myPat.AnatomicalSpaces.Add("PAT", _PatGUIManager.GetDataContainer());
        int fileCount = _EegFiles.Length;
        for (int i = 0; i < fileCount; i++)
        {
            KeyValuePair<string, IEegFileInfo> kvp = _EegFiles[i].GetEegFileInfoFromGUI();
            myPat.Files.Add(kvp.Key, kvp.Value);
        }
        myPat.Video = _Video.inputfield.text;

        return myPat;
    }

    public void SetSubjectToGUI(Subject subject)
    {
        bool mniFound = subject.AnatomicalSpaces.TryGetValue("MNI", out BrainDataContainer mniContainer);
        if(mniFound) _MniGUIManager.SetDataConainerInUI(mniContainer);

        bool patFound = subject.AnatomicalSpaces.TryGetValue("PAT", out BrainDataContainer patContainer);
        if (patFound) _PatGUIManager.SetDataConainerInUI(patContainer);

        int fileCount = _EegFiles.Length;
        for (int i = 0; i < fileCount; i++)
            _EegFiles[i].SetEegFileInfoToGUI(subject.Files.ElementAtOrDefault(i));

        _Video.inputfield.text = subject.Video;
    }
}
