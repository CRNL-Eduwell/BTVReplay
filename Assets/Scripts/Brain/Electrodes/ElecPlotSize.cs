using UnityEngine;
using System.Linq;

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
    public string MarsAtlasName
    {
        get
        {
            if (m_plot is Intra_Plot)
            {
                Intra_Plot currentPlot = (Intra_Plot)m_plot;
                if (currentPlot.Atlas.nameFull != "")
                    return currentPlot.Atlas.nameFull;
                else
                    return "";
            }
            else
                return "";
        }
    }
    public string BroadmanName
    {
        get
        {
            if (m_plot is Intra_Plot)
            {
                Intra_Plot currentPlot = (Intra_Plot)m_plot;
                if (currentPlot.Atlas.broadman != "")
                    return currentPlot.Atlas.broadman;
                else
                    return "";
            }
            else
                return "";
        }
    }
    public Vector3 Coordinates
    {
        get
        {
            return plotObject.transform.localPosition;
        }
    }

    GameObject plotObject = null;
    Trace curve = null;
    BTVMedia media = null;
    optionsHub hub = null;
    VideoPlayer video = null;
    BrainWarden warden = null;
    object m_plot = null;
    ELAN eHandle = null;
    int bipID = 0;
    int mostRecentSample = 0;
    float gain = 1;

    public float scale = 0;

    MeshRenderer mySphereRenderer = null;

    public void init(string goName, object plot)
    {
        hub = GameObject.Find("Canvas").transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<optionsHub>();
        warden = GameObject.Find("Workable Part").transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<BrainWarden>();
        video = GameObject.Find("Workable Part").transform.GetChild(0).GetChild(1).GetComponent<VideoPlayer>();
        media = GameObject.Find("Canvas").transform.GetChild(3).GetComponent<BTVMedia>();
        curve = GameObject.Find("Workable Part").transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<Trace>();

        video.sendTime += new timeVideo(updateSize);
        hub.brainRemote.gainHasChanged += new gainChangedEventHandler((newGain) => 
        {
            gain = newGain;
        });
        warden.changeColorEvent += new changeColorPlotEvent(updateColorForEvent);

        setPlot(plot);
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
            mostRecentSample = (int)(milliSecToLook * (curve.TraceEeg.fileHandle.sampFreq / 1000));
            int posInArray = (bipID * curve.TraceEeg.fileHandle.nbSam) + mostRecentSample;
            float currentValue = curve.TraceEeg.fileHandle.eegData[posInArray] / 100;
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

    public void setPlot(object plot)
    {
        m_plot = plot;
    }
}
