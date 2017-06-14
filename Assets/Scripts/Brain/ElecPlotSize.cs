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

    GameObject plotObject = null;
    BTVMedia media = null;
    optionsHub hub = null;
    VLCSharp.VLCSharp video = null;
    ELAN eHandle = null;
    int bipID = 0;
    int mostRecentSample = 0;
    float gain = 1;

    public void init(string goName)
    {
        media = GameObject.Find("Canvas").transform.GetChild(2).GetComponent<BTVMedia>();
        video = GameObject.Find("Canvas").transform.GetChild(0).GetChild(1).GetComponent<VLCSharp.VLCSharp>();
        hub = GameObject.Find("Canvas").transform.GetChild(0).GetChild(3).GetComponent<optionsHub>();

        video.sendTime += new timeVideo(updateSize);
        hub.brainRemote.gainHasChanged += new gainChangedEventHandler((newGain) => 
        {
            gain = newGain;
        });

        if (media != null)
            eHandle = ELAN.returnFirstValidHandle(media.elanFiles);

        plotObject = GameObject.Find(goName.ToLower());

        int plotID = eHandle.electList.FindIndex(x => x.ToLower().Equals(plotObject.name));

        if (plotID != -1)
            bipID = plotID;
        else
            plotObject.SetActive(false);
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
    }

    void updateSize(int sampleToLook)
    {
        mostRecentSample = sampleToLook;
        int posInArray = (bipID * eHandle.nbSam) + mostRecentSample;
        float currentValue = eHandle.eegData[posInArray] / 100;
        float scale = 2 + (gain * currentValue);

        if (scale >= 10)
            scale = 10;

        plotObject.transform.localScale = new Vector3(scale, scale, scale);
    }
}
