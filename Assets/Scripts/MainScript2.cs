using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class MainScript2 : MonoBehaviour
{
    public GameObject colorPicker = null;
    public GameObject colorPicker2 = null;
    public GameObject telecommandeCurve = null;
    public GameObject telecommandeCurve2 = null;

    public PhyObject3D brain3DScript = null;
    public VLCSharp.VLCSharp vlcScript = null;
    public TVCurve curveScript = null;
    public TVCurve curveScript2 = null;
    public courbeClick curveClick1 = null;
    public courbeClick curveClick2 = null;
    public clickableObject brainClick = null;

    public TVCurvePerf curvePerfScript = null;
    public eventDisplay eventDisp = null;
    public BTVMedia_New btvMedia = null;

    public bool init = false;
    public List<string> elecList = null;

    void Start ()
    {
        colorPicker.SetActive(false);
        telecommandeCurve.SetActive(false);
        colorPicker2.SetActive(false);
        telecommandeCurve2.SetActive(false);
    }

    void Update ()
    {
        if (init == false)
        {
            if (btvMedia.loaded == true)
            {
                curveScript.init();
                curveScript2.init();
                curveClick1.init();
                curveClick2.init();
                brainClick.init();
                elecList = new List<string>(curveScript.eHandle.electList); //Create Clone, not ref

                if (btvMedia.perfOk == true)
                {
                    curvePerfScript.init(); //Implement destroy to kill spawned object and prevent leak /!\
                    eventDisp.init();
                }
                load3DObject();
                vlcScript.loadVideoInit(btvMedia.pm.currentPatients[btvMedia.pm.idCurrentPatientLoaded].video);
                init = true;
            }
        }
        else
        {
            if (vlcScript.player.IsPlaying)
            {
                int sampleToLook = ((int)(vlcScript.time) - 640);
                curveScript.updateDraw(sampleToLook);
                curveScript2.updateDraw(sampleToLook);
            }
        }
    }

    void load3DObject()
    {
        string LHemi = btvMedia.pm.currentPatients[btvMedia.pm.idCurrentPatientLoaded].lhemi_MNI;
        string RHemi = btvMedia.pm.currentPatients[btvMedia.pm.idCurrentPatientLoaded].rhemi_MNI;
        string PTS = btvMedia.pm.currentPatients[btvMedia.pm.idCurrentPatientLoaded].pts_MNI;

        brain3DScript.loadBrainAndElectrodes(LHemi, RHemi, PTS);
    }
}
