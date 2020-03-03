using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PatientGUIManager : MonoBehaviour
{
    [SerializeField] GameObject BrainData = null;
    [SerializeField] GameObject EEG = null;
    [SerializeField] GameObject EventAndVideo = null;

    browseButton mni_LHemi = null, mni_RHemi = null, mni_PTS = null;
    Dropdown mni_nbMesh = null, mni_eegTech = null;
    browseButton pat_LHemi = null, pat_RHemi = null, pat_PTS = null, pat_Atlas = null;
    Dropdown pat_nbMesh = null, pat_eegTech = null;
    browseButton[] eegFile = new browseButton[6];
    browseButton pos = null, prov = null, video = null;

    void Awake()
    {
        mni_LHemi = BrainData.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(0).GetComponent<browseButton>();
        mni_RHemi = BrainData.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(1).GetComponent<browseButton>();
        mni_PTS = BrainData.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(2).GetComponent<browseButton>();
        mni_nbMesh = BrainData.transform.GetChild(0).GetChild(1).GetChild(1).GetChild(0).GetComponent<Dropdown>();
        mni_eegTech = BrainData.transform.GetChild(0).GetChild(1).GetChild(1).GetChild(1).GetComponent<Dropdown>();
        //==
        pat_LHemi = BrainData.transform.GetChild(1).GetChild(1).GetChild(0).GetChild(0).GetComponent<browseButton>();
        pat_RHemi = BrainData.transform.GetChild(1).GetChild(1).GetChild(0).GetChild(1).GetComponent<browseButton>();
        pat_PTS = BrainData.transform.GetChild(1).GetChild(1).GetChild(0).GetChild(2).GetComponent<browseButton>();
        pat_Atlas = BrainData.transform.GetChild(1).GetChild(1).GetChild(0).GetChild(3).GetComponent<browseButton>();
        pat_nbMesh = BrainData.transform.GetChild(1).GetChild(1).GetChild(1).GetChild(0).GetComponent<Dropdown>();
        pat_eegTech = BrainData.transform.GetChild(1).GetChild(1).GetChild(1).GetChild(1).GetComponent<Dropdown>();
        //======
        for (int i = 0; i < 3; i++)
        {
            eegFile[i] = EEG.transform.GetChild(1).GetChild(0).GetChild(i).GetComponent<browseButton>();
            eegFile[i + 3] = EEG.transform.GetChild(1).GetChild(1).GetChild(i).GetComponent<browseButton>();
        }
        //==
        pos = EventAndVideo.transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<browseButton>();
        prov = EventAndVideo.transform.GetChild(0).GetChild(1).GetChild(1).GetComponent<browseButton>();
        video = EventAndVideo.transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<browseButton>();

        mni_LHemi.inputfield.text = Application.dataPath + "/Config/Data/MNI/MNI_single_hight_Lhemi.tri";
        mni_RHemi.inputfield.text = Application.dataPath + "/Config/Data/MNI/MNI_single_hight_Rhemi.tri";

        mni_nbMesh.onValueChanged.AddListener((int value) => 
        {
            if (value == 0)
            {
                mni_LHemi.gameObject.SetActive(true);
                mni_LHemi.inputfield.placeholder.GetComponent<Text>().text = "LHemi File";
                mni_LHemi.inputfield.text = Application.dataPath + "/Config/Data/MNI/MNI_single_hight_Lhemi.tri";
                mni_RHemi.gameObject.SetActive(true);
                mni_RHemi.inputfield.placeholder.GetComponent<Text>().text = "RHemi File";
                mni_RHemi.inputfield.text = Application.dataPath + "/Config/Data/MNI/MNI_single_hight_Rhemi.tri";
            }
            else
            {
                mni_LHemi.gameObject.SetActive(true);
                mni_LHemi.inputfield.placeholder.GetComponent<Text>().text = "Single File";
                mni_LHemi.inputfield.text = "";
                mni_RHemi.gameObject.SetActive(false);
                mni_RHemi.inputfield.text = "";
            }
        });
        //mni_eegTech.onValueChanged.AddListener((int value) => { });
        pat_nbMesh.onValueChanged.AddListener((int value) => 
        {
            if (value == 0)
            {
                pat_LHemi.gameObject.SetActive(true);
                pat_LHemi.inputfield.placeholder.GetComponent<Text>().text = "LHemi File";
                pat_RHemi.gameObject.SetActive(true);
                pat_RHemi.inputfield.placeholder.GetComponent<Text>().text = "RHemi File";
            }
            else
            {
                pat_LHemi.gameObject.SetActive(true);
                pat_LHemi.inputfield.placeholder.GetComponent<Text>().text = "Single File";
                pat_RHemi.gameObject.SetActive(false);
            }
        });
        //pat_eegTech.onValueChanged.AddListener((int value) => { });
    }

    void OnDestroy()
    {
        mni_nbMesh.onValueChanged.RemoveAllListeners();
        //mni_eegTech.onValueChanged.RemoveAllListeners();
        pat_nbMesh.onValueChanged.RemoveAllListeners();
        //pat_eegTech.onValueChanged.RemoveAllListeners();
    }

    public Patient getPatientGUI()
    {
        Patient myPat = new Patient();

        myPat.mni = new BrainDataContainer(mni_LHemi.inputfield.text, mni_RHemi.inputfield.text, "", mni_PTS.inputfield.text, mni_nbMesh.options[mni_nbMesh.value].text, mni_eegTech.options[mni_eegTech.value].text);
        myPat.pat = new BrainDataContainer(pat_LHemi.inputfield.text, pat_RHemi.inputfield.text, "", pat_PTS.inputfield.text, pat_Atlas.inputfield.text, pat_nbMesh.options[pat_nbMesh.value].text, pat_eegTech.options[pat_eegTech.value].text);

        for (int i = 0; i < 6; i++)
            myPat.smFiles[i] = eegFile[i].inputfield.text;

        myPat.pos = pos.inputfield.text;
        myPat.prov = prov.inputfield.text;
        myPat.video = video.inputfield.text;

        return myPat;
    }

    public void setPatientGUI(Patient myPat)
    {
        mni_LHemi.inputfield.text = myPat.mni.LeftHemisphere;
        mni_RHemi.inputfield.text = myPat.mni.RightHemisphere;
        mni_PTS.inputfield.text = myPat.mni.Pts;
        mni_nbMesh.value = (int)myPat.mni.MeshConfiguration;
        mni_eegTech.value = (int)myPat.mni.EegTechnology;

        if (myPat.mni.MeshConfiguration == MeshConfiguration.LeftRight)
        {
            mni_LHemi.gameObject.SetActive(true);
            mni_LHemi.inputfield.placeholder.GetComponent<Text>().text = "LHemi File";
            mni_RHemi.gameObject.SetActive(true);
            mni_RHemi.inputfield.placeholder.GetComponent<Text>().text = "RHemi File";
        }
        else if (myPat.mni.MeshConfiguration == MeshConfiguration.Single)
        {
            mni_LHemi.gameObject.SetActive(true);
            mni_LHemi.inputfield.placeholder.GetComponent<Text>().text = "Single File";
            mni_RHemi.gameObject.SetActive(false);
        }

        pat_LHemi.inputfield.text = myPat.pat.LeftHemisphere;
        pat_RHemi.inputfield.text = myPat.pat.RightHemisphere;
        pat_PTS.inputfield.text = myPat.pat.Pts;
        pat_Atlas.inputfield.text = myPat.pat.Atlas;
        pat_nbMesh.value = (int)myPat.pat.MeshConfiguration;
        pat_eegTech.value = (int)myPat.pat.EegTechnology;

        if (myPat.pat.MeshConfiguration == MeshConfiguration.LeftRight)
        {
            pat_LHemi.gameObject.SetActive(true);
            pat_LHemi.inputfield.placeholder.GetComponent<Text>().text = "LHemi File";
            pat_RHemi.gameObject.SetActive(true);
            pat_RHemi.inputfield.placeholder.GetComponent<Text>().text = "RHemi File";
        }
        else if (myPat.pat.MeshConfiguration == MeshConfiguration.Single)
        {
            pat_LHemi.gameObject.SetActive(true);
            pat_LHemi.inputfield.placeholder.GetComponent<Text>().text = "Single File";
            pat_RHemi.gameObject.SetActive(false);
        }

        for (int i = 0; i < 6; i++)
            eegFile[i].inputfield.text = myPat.smFiles[i];

        pos.inputfield.text = myPat.pos;
        prov.inputfield.text = myPat.prov;
        video.inputfield.text = myPat.video;
    }
}
