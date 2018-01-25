using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using VLCSharp;
using UnityEngine.UI;

public class TracePerf : MonoBehaviour
{
    public bool isUsed
    {
        get;set;
    }
    [SerializeField] optionsHub hub = null;
    [SerializeField] BTVMedia media = null;
    [SerializeField] VideoPlayer video = null;

    List<int> mainCodes = new List<int>();
    Texture2D defaultEventPic = null;
    List<Texture2D> picEvent = new List<Texture2D>();
    List<GameObject> perfLine = new List<GameObject>();
    GameObject eventPicHolder = null;
    RawImage eventImage = null;
    GameObject perfLRPrefab = null;
    RectTransform m_rectTransform = null;
    RectTransform m_perfHolder = null;
    float horizontalScale = 0;
    float verticalScale = 0;
    int periodSec = 10;
    int numberPoint = 64 * 10;
    bool initDone = false;
    //bool isUsed = false;
    Trace curveTrace1 = null;
     
    void Awake()
    {
        media.loadPerf += new initPerf(init);
    }

    void OnDestroy()
    {
        media.loadPerf -= new initPerf(init);

        if (initDone)
        {
            if (isUsed)
            {
                video.sendTime -= new timeVideo(UpdateSpawn);
                video.sendTime -= new timeVideo(UpdatePicEvent);
                //hub.perfRemote.iAmHiden -= new hideMe((isHidden) =>
                //{
                //    gameObject.SetActive(isHidden);
                //});
                hub.perfRemote.timeHasChanged -= new timePeriodChangedEventHandler(updateTimeResolution);
            }
            else
            {
                hub.perfRemote.timeHasChanged -= new timePeriodChangedEventHandler((int newPeriod) => { });
            }
        }

        for (int i = 0; i < transform.childCount; i++)
            Destroy(transform.GetChild(i));
    }

    void OnRectTransformDimensionsChange()
    {
        if(m_rectTransform != null && isUsed)
            updateScales();
    }

    void init(bool initMe)
    {
        isUsed = initMe;
        perfLRPrefab = Resources.Load("Prefabs/PerfTrace", typeof(GameObject)) as GameObject;
        defaultEventPic = Resources.Load("Pictures/EventDefault", typeof(Texture2D)) as Texture2D;
        curveTrace1 = GameObject.Find("Trace1Window").GetComponent<Trace>();
        numberPoint = curveTrace1.TraceEeg.SamplingFrequency * periodSec;

        m_rectTransform = gameObject.GetComponent<RectTransform>();
        m_perfHolder = m_rectTransform.GetChild(10).GetComponent<RectTransform>();
        eventPicHolder = m_rectTransform.GetChild(11).GetChild(0).gameObject;
        eventImage = eventPicHolder.transform.GetChild(0).GetComponent<RawImage>();

        //hub.perfRemote.iAmHiden += new hideMe((isHidden) =>
        //{
        //    gameObject.SetActive(isHidden);
        //});

        if(isUsed)
        {
            hub.perfRemote.timeHasChanged += new timePeriodChangedEventHandler(updateTimeResolution);
            video.sendTime += new timeVideo(UpdateSpawn);
            video.sendTime += new timeVideo(UpdatePicEvent);

            updateScales();

            for (int i = 0; i < media.posFile.Triggers.Count; i++)
            {
                GameObject currentPerf = Instantiate(perfLRPrefab);
                currentPerf.transform.SetParent(m_perfHolder);
                currentPerf.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
                currentPerf.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0);
                currentPerf.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
                currentPerf.transform.localPosition = new Vector3(0, 0, 0);
                currentPerf.name = "Perf" + (perfLine.Count);

                currentPerf.transform.GetComponent<LineRenderer>().SetPosition(0, new Vector3(i, 0, -2));
                currentPerf.transform.GetComponent<LineRenderer>().SetPosition(1, new Vector3(i, 100, -2));
                currentPerf.transform.localScale = new Vector3(1, 1, 1);

                currentPerf.SetActive(false);
                perfLine.Add(currentPerf);
            }

            for (int i = 0; i < media.provFile.blocs.Count; i++)
            {
                if (File.Exists(media.provFile.blocs[i].dispBloc.path))
                {
                    Texture2D current = new Texture2D(256, 256);
                    current.LoadImage(File.ReadAllBytes(media.provFile.blocs[i].dispBloc.path));

                    picEvent.Add(current);
                }
                else
                {
                    picEvent.Add(defaultEventPic);
                }
                mainCodes.Add(media.provFile.blocs[i].mainEvent.code);
            }

        }
        else
        {
            gameObject.SetActive(false);
            //hub.perfRemote.hideTog.isOn = false;
            hub.perfRemote.timeHasChanged += new timePeriodChangedEventHandler((int newPeriod) => { });
        }

        initDone = true;
    }

    void updateScales()
    {
        horizontalScale = (m_perfHolder.rect.width) / numberPoint;
        verticalScale = m_perfHolder.rect.height / media.posFile.rtMsMax;
    }

    void updateTimeResolution(int newPeriod)
    {
        periodSec = newPeriod;
        numberPoint = curveTrace1.TraceEeg.SamplingFrequency * periodSec;
        updateScales();
    }

    void UpdateSpawn(int milliSecToLook)
    {
        int leftTime = (int)(milliSecToLook * ((float)curveTrace1.TraceEeg.SamplingFrequency / 1000)) - curveTrace1.TraceEeg.numberOfPoint;
        int rightTime = (int)(milliSecToLook * ((float)curveTrace1.TraceEeg.SamplingFrequency / 1000));

        List<int> currentIndex = media.posFile.Triggers.Select((item, index) => new { Item = item, Index = index })
                                                         .Where(x => x.Item.response.sample > leftTime && x.Item.response.sample < rightTime)
                                                         .Select(x => x.Index)
                                                         .ToList();

        if (currentIndex.Count != 0)
        {
            deactivateSpawn();
            for (int i = 0; i < currentIndex.Count; i++)
            {
                float posiionSample = (leftTime - media.posFile.Triggers[currentIndex[i]].response.sample);
                float positionInsideRect = posiionSample * -horizontalScale;

                if (media.posFile.Triggers[currentIndex[i]].response.sample <= rightTime)
                {
                    perfLine[currentIndex[i]].SetActive(true);
                    perfLine[currentIndex[i]].transform.GetComponent<LineRenderer>().SetPosition(0, new Vector3(positionInsideRect, 5, -2));

                    float value = verticalScale * (media.posFile.Triggers[currentIndex[i]].rtMs() - 750);
                    perfLine[currentIndex[i]].transform.GetComponent<LineRenderer>().SetPosition(1, new Vector3(positionInsideRect, value, -2));
                }
            }
        }
        else //No New obj, we clean if there is some left
        {
            deactivateSpawn();
        }
    }

    void deactivateSpawn()
    {
        List<GameObject> activeObj = perfLine.FindAll(x => x.activeSelf == true);
        if (activeObj.Count > 0)
        {
            for (int i = 0; i < activeObj.Count; i++)
                activeObj[i].SetActive(false);
        }
    }

    void UpdatePicEvent(int milliSecToLook)
    {
        int sampleToLook = (int)(milliSecToLook * ((float)curveTrace1.TraceEeg.SamplingFrequency / 1000));
        int found = media.posFile.Triggers.FindIndex(x => x.trigger.sample >= sampleToLook - 8 && x.trigger.sample < sampleToLook + 8);

        if (found != -1 && mainCodes.Contains(media.posFile.Triggers[found].trigger.code))
        {
            if (eventPicHolder.activeSelf == false)
            {
                eventPicHolder.SetActive(true);
                eventImage.texture = picEvent[mainCodes.IndexOf(media.posFile.Triggers[found].trigger.code)];
            }
        }
        else
        {
            if (eventPicHolder.activeSelf == true)
                eventPicHolder.SetActive(false);
        }
    }
}
