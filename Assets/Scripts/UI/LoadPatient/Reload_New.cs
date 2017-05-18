using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Reload_New : MonoBehaviour
{
    public string lhemi_MNI = "";
    public string rhemi_MNI = "";
    public string pts_MNI = "";

    public string lhemi_PAT = "";
    public string rhemi_PAT = "";
    public string pts_PAT = "";

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

    GameObject g = null;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (lhemi_MNI != "")
        {
            g = GameObject.Find("Canvas").transform.GetChild(8).gameObject;
            g.SetActive(true);
            g.SetActive(false);
            BTVMedia_New m = g.GetComponent<BTVMedia_New>();
            if (m != null)
            {
                m.gameObject.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(1).GetComponent<browseButton>().inputfield.text = lhemi_MNI;
                m.gameObject.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(2).GetComponent<browseButton>().inputfield.text = rhemi_MNI;
                m.gameObject.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(3).GetComponent<browseButton>().inputfield.text = pts_MNI;

                m.gameObject.transform.GetChild(1).GetChild(0).GetChild(1).GetChild(1).GetComponent<browseButton>().inputfield.text = lhemi_PAT;
                m.gameObject.transform.GetChild(1).GetChild(0).GetChild(1).GetChild(2).GetComponent<browseButton>().inputfield.text = rhemi_PAT;
                m.gameObject.transform.GetChild(1).GetChild(0).GetChild(1).GetChild(3).GetComponent<browseButton>().inputfield.text = pts_PAT;

                m.gameObject.transform.GetChild(1).GetChild(1).GetChild(1).GetComponent<browseButton>().inputfield.text = sm0;
                m.gameObject.transform.GetChild(1).GetChild(1).GetChild(2).GetComponent<browseButton>().inputfield.text = sm250;
                m.gameObject.transform.GetChild(1).GetChild(1).GetChild(3).GetComponent<browseButton>().inputfield.text = sm500;
                m.gameObject.transform.GetChild(1).GetChild(1).GetChild(4).GetComponent<browseButton>().inputfield.text = sm1000;
                m.gameObject.transform.GetChild(1).GetChild(1).GetChild(5).GetComponent<browseButton>().inputfield.text = sm2500;
                m.gameObject.transform.GetChild(1).GetChild(1).GetChild(6).GetComponent<browseButton>().inputfield.text = sm5000;

                m.gameObject.transform.GetChild(1).GetChild(2).GetChild(1).GetComponent<browseButton>().inputfield.text = pos;
                m.gameObject.transform.GetChild(1).GetChild(2).GetChild(2).GetComponent<browseButton>().inputfield.text = prov;
                m.gameObject.transform.GetChild(1).GetChild(2).GetChild(4).GetComponent<browseButton>().inputfield.text = video;

                
                m.pm.LoadList(false);
                //m.InstantiateDB();
                m.pm.idCurrentPatientLoaded = id;
                m.loadMedia(m.pm.currentPatients[m.pm.idCurrentPatientLoaded]);
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
