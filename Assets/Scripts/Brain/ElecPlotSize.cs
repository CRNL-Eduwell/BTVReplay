using UnityEngine;
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
        media = GameObject.Find("Canvas").transform.GetChild(2).GetComponent<BTVMedia>();
        video = GameObject.Find("Canvas").transform.GetChild(0).GetChild(1).GetComponent<VideoPlayer>();
        hub = GameObject.Find("Canvas").transform.GetChild(0).GetChild(3).GetComponent<optionsHub>();
        warden = GameObject.Find("Canvas").transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<BrainWarden>();

        video.sendTime += new timeVideo(updateSize);
        hub.brainRemote.gainHasChanged += new gainChangedEventHandler((newGain) => 
        {
            gain = newGain;
        });
        warden.changeColorEvent += new changeColorPlotEvent(updateColorForEvent);

        if (media != null)
            eHandle = ELAN.returnFirstValidHandle(media.elanFiles);

        plotObject = GameObject.Find(goName.ToLower());
        mySphereRenderer = plotObject.GetComponent<MeshRenderer>();

        int plotID = eHandle.electList.FindIndex(x => x.ToLower().Equals(plotObject.name));

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

    void updateSize(int sampleToLook)
    {
        if (!isFrozen)
        {
            mostRecentSample = sampleToLook;
            int posInArray = (bipID * eHandle.nbSam) + mostRecentSample;
            float currentValue = eHandle.eegData[posInArray] / 100;
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
