using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadMedia : MonoBehaviour
{
    public string lhemi_MNI = "";
    public string rhemi_MNI = "";
    public string pts_MNI = "";

    public string lhemi_PAT = "";
    public string rhemi_PAT = "";
    public string pts_PAT = "";
    public string atlas_PAT = "";

    public string sm0 = "";
    public string sm250 = "";
    public string sm500 = "";
    public string sm1000 = "";
    public string sm2500 = "";
    public string sm5000 = "";

    public string pos = "";
    public string prov = "";
    public string video = "";

    public int id = -2;
    public string path = "";

    GameObject mediaGameObject = null;
    Transform addPatientPanel = null;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (lhemi_MNI != "")
        {
            mediaGameObject = GameObject.Find("Canvas").transform.GetChild(1).gameObject;
            mediaGameObject.SetActive(true);
            mediaGameObject.SetActive(false);
            BTVMedia media = mediaGameObject.GetComponent<BTVMedia>();
            if (media != null)
            {
                ApplicationState.ResetAllServices();

                addPatientPanel = media.transform.gameObject.transform.GetChild(0).GetChild(1).GetChild(0);

                addPatientPanel.GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetChild(0).GetComponent<browseButton>().inputfield.text = lhemi_MNI;
                addPatientPanel.GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetChild(1).GetComponent<browseButton>().inputfield.text = rhemi_MNI;
                addPatientPanel.GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetChild(2).GetComponent<browseButton>().inputfield.text = pts_MNI;

                addPatientPanel.GetChild(0).GetChild(1).GetChild(1).GetChild(0).GetChild(0).GetComponent<browseButton>().inputfield.text = lhemi_PAT;
                addPatientPanel.GetChild(0).GetChild(1).GetChild(1).GetChild(0).GetChild(1).GetComponent<browseButton>().inputfield.text = rhemi_PAT;
                addPatientPanel.GetChild(0).GetChild(1).GetChild(1).GetChild(0).GetChild(2).GetComponent<browseButton>().inputfield.text = pts_PAT;
                addPatientPanel.GetChild(0).GetChild(1).GetChild(1).GetChild(0).GetChild(3).GetComponent<browseButton>().inputfield.text = atlas_PAT;

                addPatientPanel.GetChild(1).GetChild(1).GetChild(0).GetChild(0).GetComponent<browseButton>().inputfield.text = sm0;
                addPatientPanel.GetChild(1).GetChild(1).GetChild(0).GetChild(1).GetComponent<browseButton>().inputfield.text = sm250;
                addPatientPanel.GetChild(1).GetChild(1).GetChild(0).GetChild(2).GetComponent<browseButton>().inputfield.text = sm500;
                addPatientPanel.GetChild(1).GetChild(1).GetChild(1).GetChild(0).GetComponent<browseButton>().inputfield.text = sm1000;
                addPatientPanel.GetChild(1).GetChild(1).GetChild(1).GetChild(1).GetComponent<browseButton>().inputfield.text = sm2500;
                addPatientPanel.GetChild(1).GetChild(1).GetChild(1).GetChild(2).GetComponent<browseButton>().inputfield.text = sm5000;

                addPatientPanel.GetChild(2).GetChild(0).GetChild(1).GetChild(0).GetComponent<browseButton>().inputfield.text = pos;
                addPatientPanel.GetChild(2).GetChild(0).GetChild(1).GetChild(1).GetComponent<browseButton>().inputfield.text = prov;
                addPatientPanel.GetChild(2).GetChild(1).GetChild(1).GetChild(0).GetComponent<browseButton>().inputfield.text = video;

                media.pm.LoadList(false, path);
                //m.InstantiateDB();
                media.pm.idCurrentPatientLoaded = id;
                media.gameObject.SetActive(true);
                media.loadMedia(media.pm.currentPatients[media.pm.idCurrentPatientLoaded]);
                Destroy(gameObject);
            }
        }
    }

    //void OnDestroy()
    //{
    //    Resources.UnloadUnusedAssets();
    //    Debug.Log("OK mem");
    //}
}
