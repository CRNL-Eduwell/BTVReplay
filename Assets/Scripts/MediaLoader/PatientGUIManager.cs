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
    [SerializeField] GameObject EEG = null;
    [SerializeField] GameObject EventAndVideo = null;

    browseButton[] eegFile = new browseButton[6];
    browseButton video = null;

    void Awake()
    {
        //======
        for (int i = 0; i < 3; i++)
        {
            eegFile[i] = EEG.transform.GetChild(1).GetChild(0).GetChild(i).GetComponent<browseButton>();
            eegFile[i + 3] = EEG.transform.GetChild(1).GetChild(1).GetChild(i).GetComponent<browseButton>();
        }
        //==
        video = EventAndVideo.transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<browseButton>();
    }

    void OnDestroy()
    {

    }

    public Subject GetSubjectsFromGUI()
    {
        Subject myPat = new Subject();

        myPat.AnatomicalSpaces.Add("MNI", _MniGUIManager.GetDataContainer());
        myPat.AnatomicalSpaces.Add("PAT", _PatGUIManager.GetDataContainer());

        for (int i = 0; i < 6; i++)
        {
            string path = eegFile[i].inputfield.text;
            if (!string.IsNullOrEmpty(path))
            {
                FileInfo fileInfo = new FileInfo(path);
                if (fileInfo.Extension == ".TRC")
                {
                    myPat.Files.Add( "", new MicromedFileInfo(fileInfo.FullName));
                }
                else if (fileInfo.Extension == ".eeg")
                {
                    myPat.Files.Add("", new ElanFileInfo(fileInfo.FullName));
                }
                else if (fileInfo.Extension == ".vhdr")
                {
                    myPat.Files.Add("", new BrainvisionFileInfo(fileInfo.FullName));
                }
                else if (fileInfo.Extension == ".edf")
                {
                    myPat.Files.Add("", new EdfFileInfo(fileInfo.FullName));
                }
            }
        }
        myPat.Video = video.inputfield.text;

        return myPat;
    }

    public void SetSubjectToGUI(Subject subject)
    {
        bool mniFound = subject.AnatomicalSpaces.TryGetValue("MNI", out BrainDataContainer mniContainer);
        if(mniFound) _MniGUIManager.SetDataConainerInUI(mniContainer);

        bool patFound = subject.AnatomicalSpaces.TryGetValue("PAT", out BrainDataContainer patContainer);
        if (patFound) _PatGUIManager.SetDataConainerInUI(patContainer);

        //TODO : Do the whole modification of the ui to present different ui element 
        //in case of the elan file for instance
        for (int i = 0; i < 6; i++)
        {
            KeyValuePair<string, IEegFileInfo> kvp = subject.Files.ElementAtOrDefault(i);
            if (!kvp.Equals(default(KeyValuePair<string, IEegFileInfo>)))
            {
                eegFile[i].inputfield.text = kvp.Value.Files[0];
            }
            else
            {
                eegFile[i].inputfield.text = "";
            }
        }

        video.inputfield.text = subject.Video;
    }
}
