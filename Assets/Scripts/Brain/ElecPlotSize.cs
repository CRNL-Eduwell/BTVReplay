using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VLCSharp;

public class ElecPlotSize : MonoBehaviour
{
    public int ID
    {
        get
        {
            return bipID;
        }
    }
    public bool isFrozen
    {
        get;
        set;
    }

    GameObject plotObject = null;
    TraceCurve curve = null;
    BTVMedia media = null;
    optionsHub hub = null;
    VideoPlayer video = null;
    BrainWarden warden = null;

    ELAN eHandle = null;
    int bipID = 0;
    int mostRecentSample = 0;
    float gain = 1;

    public float scale = 0;

    MeshRenderer mySphereRenderer = null;

    public void init(string goName)
    {
        hub = GameObject.Find("Canvas").transform.GetChild(0).GetChild(0).GetComponent<optionsHub>();
        warden = GameObject.Find("Canvas").transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<BrainWarden>();
        video = GameObject.Find("Canvas").transform.GetChild(1).GetChild(1).GetComponent<VideoPlayer>();
        media = GameObject.Find("Canvas").transform.GetChild(3).GetComponent<BTVMedia>();
        curve = GameObject.Find("Canvas").transform.GetChild(1).GetChild(0).GetChild(1).GetComponent<TraceCurve>();

        video.sendTime += new timeVideo(updateSize);
        hub.brainRemote.gainHasChanged += new gainChangedEventHandler((newGain) => 
        {
            gain = newGain;
        });
        warden.changeColorEvent += new changeColorPlotEvent(updateColorForEvent);

        plotObject = GameObject.Find(goName.ToLower());
        mySphereRenderer = plotObject.GetComponent<MeshRenderer>();

        if (media != null)
            eHandle = ELAN.returnFirstValidHandle(media.elanFiles);

        int plotID = eHandle.electrodes.ToList().FindIndex(x => x.name.ToLower().Equals(plotObject.name));

        if (plotID != -1)
            bipID = plotID;
        else
            plotObject.SetActive(false);

        isFrozen = false;
    }

    void OnDestroy()
    {
        if(video != null)
            video.sendTime -= new timeVideo(updateSize);

        if (hub != null)
        {
            hub.brainRemote.gainHasChanged -= new gainChangedEventHandler((newGain) =>
            {
                gain = newGain;
            });
        }

        if(warden != null)
            warden.changeColorEvent -= new changeColorPlotEvent(updateColorForEvent);
    }

    void updateSize(int milliSecToLook)
    {
        if (!isFrozen)
        {
            mostRecentSample = (int)(milliSecToLook * (curve.fileHandle.sampFreq / 1000));
            int posInArray = (bipID * curve.fileHandle.nbSam) + mostRecentSample;
            float currentValue = curve.fileHandle.eegData[posInArray] / 100;
            scale = 2 + (gain * currentValue);

            if (scale >= 7)
                scale = 7;
            else if (scale <= 0)
                scale = 0.1f;

            plotObject.transform.localScale = new Vector3(scale, scale, scale);
        }
    }

    void updateColorForEvent(string namePlot, Color color)
    {
        if (namePlot.ToLower() == plotObject.name || namePlot == "")
        {
            mySphereRenderer.materials[0].color = color;
        }
    }

    public void fixSize()
    {
        plotObject.transform.localScale = new Vector3(1, 1, 1);
    }
}
