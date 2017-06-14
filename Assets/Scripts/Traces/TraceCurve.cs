using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using VLCSharp;
using System.Linq;

public delegate void eventsClickedHandler(eventEeg newVal, int idWin);
public delegate void eventsToDisplay(eventEeg newVal, int idWin);
public delegate void eventsToDelete(eventEeg newVal, int idWin);

public class TraceCurve : MonoBehaviour, IPointerClickHandler
{
    public event eventsClickedHandler eventWasClicked;
    public event eventsToDisplay eventsToDisplay;
    public event eventsToDelete eventsToDelete;

    public int eventCode
    {
        get;
        set;
    }
    public int idElectrode
    {
        get
        {
            return idCurrentElec;
        }
        set
        {
            idCurrentElec = value;
        }
    }
    public int idTrace
    {
        get
        {
            return traceID;
        }
    }

    [SerializeField] optionsHub hub = null;
    [SerializeField] BTVMedia media = null;
    [SerializeField] VLCSharp.VLCSharp video = null;
    [SerializeField] int traceID = 0;
    [SerializeField] int idCurrentElec = 0;

    GameObject traceEventClick = null;
    RectTransform m_rectTransform = null;
    Vector3[] m_worldCorners = new Vector3[4];
    Transform m_eventHolder = null;
    LineRenderer lineRenderer = null;
    Text elecLabel = null;
    ELAN eHandle = null;
    bool initDone = false;
    public List<GameObject> eventsAdded = new List<GameObject>();

    BrainWarden warden = null;
    Window m_window = null;
    Color orange = new Color(0.9058f, 0.5254f, 0.1921f);
    Color blue = new Color(0.6117f, 0.7058f, 0.7960f);
    Window handleOtherTrace = null;
    selectRing ring = null;

    Vector3[] dataArray;
    int mostRecentSample = 0;
    int periodSec = 10;
    int numberPoint = 64 * 10;
    float widthOfGameObject = 0;
    float horizontalScale = 0;
    float gain = 1;
    float previousGain = 1;
    float maxValChanel = 0;
    float offsetCoefficient = 0;
    float offsetPerTen = 0;

    void Awake()
    {
        media.loadTrace += new initTrace(init);
        video.sendTime += new timeVideo(updateDraw);
        video.sendTime += new timeVideo(updateEventsDraw);
    }

    void OnDestroy()
    {
        media.loadTrace -= new initTrace(init);
        video.sendTime -= new timeVideo(updateDraw);
        video.sendTime -= new timeVideo(updateEventsDraw);

        if (initDone)
        {
            hub.traceRemotes[traceID].idFileHasChanged -= new idFileChangedEventHandler(
                delegate (int newID)
                {
                    eHandle = ELAN.changeHandle(eHandle, media.elanFiles, newID);
                });
            hub.traceRemotes[traceID].gainHasChanged -= new gainChangedEventHandler(updateTraceGain);
            hub.traceRemotes[traceID].offsetHasChanged -= new offsetChangedEventHandler(updateTraceOffset);
            hub.traceRemotes[traceID].idElecHasChanged -= new idElecChangedEventHandler(updateElectrodeID);
            hub.traceRemotes[traceID].timeHasChanged -= new timePeriodChangedEventHandler(updateTimeResolution);
            hub.eventRemote.newEventToShow -= new newEventToShowHandler(addEventToTrace);

            warden.plotWasClicked -= new newPlotClicked(plotClicked);

            hub.traceRemotes[traceID].deleteElectrodeInPanel();
        }
    }

    void OnRectTransformDimensionsChange()
    {
        if(m_rectTransform != null)
            updateHorizontalScale();
    }

    void init()
    {
        traceEventClick = Resources.Load("Prefabs/Trace-Event", typeof(GameObject)) as GameObject;
        ring = GameObject.Find("ringSelect").GetComponent<selectRing>();
        warden = GameObject.Find("BrainWindow").GetComponent<BrainWarden>();

        if (traceID == 0)
            handleOtherTrace = GameObject.Find("Trace" + (traceID + 2) + "Window").GetComponent<Window>();
        else
            handleOtherTrace = GameObject.Find("Trace" + (traceID) + "Window").GetComponent<Window>();

        m_rectTransform = gameObject.GetComponent<RectTransform>();
        m_window = gameObject.GetComponent<Window>();
        m_eventHolder = gameObject.transform.GetChild(11);
        lineRenderer = gameObject.transform.GetChild(0).GetComponent<LineRenderer>();
        elecLabel = gameObject.transform.GetChild(9).GetComponent<Text>();

        dataArray = new Vector3[numberPoint];
        lineRenderer.numPositions = numberPoint;
        lineRenderer.startWidth = 0.04f;
        lineRenderer.endWidth = 0.04f;
        updateHorizontalScale();
        
        eHandle = ELAN.returnFirstValidHandle(media.elanFiles);
        if (eHandle.electList.Count > 0)
        {
            elecLabel.text = eHandle.electList[idCurrentElec];
            maxValChanel = eHandle.maxValues[idCurrentElec];
        }

        #region plugEvents
        hub.traceRemotes[traceID].idFileHasChanged += new idFileChangedEventHandler(
            delegate (int newID)
            {
                eHandle = ELAN.changeHandle(eHandle, media.elanFiles, newID);
            });
        hub.traceRemotes[traceID].gainHasChanged += new gainChangedEventHandler(updateTraceGain);
        hub.traceRemotes[traceID].offsetHasChanged += new offsetChangedEventHandler(updateTraceOffset);
        hub.traceRemotes[traceID].loadElectrodeInPanel(eHandle.electList);
        hub.traceRemotes[traceID].idElecHasChanged += new idElecChangedEventHandler(updateElectrodeID);
        hub.traceRemotes[traceID].timeHasChanged += new timePeriodChangedEventHandler(updateTimeResolution);
        hub.eventRemote.newEventToShow += new newEventToShowHandler(addEventToTrace);

        warden.plotWasClicked += new newPlotClicked(plotClicked);

        initDone = true;
        #endregion
    }

    void updateTimeResolution(int newPeriod)
    {
        periodSec = newPeriod;
        numberPoint = 64 * periodSec;
        dataArray = new Vector3[numberPoint];
        lineRenderer.numPositions = numberPoint;
        lineRenderer.sortingOrder = -1;
        updateHorizontalScale();
    }

    void updateHorizontalScale()
    {
        widthOfGameObject = m_rectTransform.rect.width - 10;
        horizontalScale = widthOfGameObject / numberPoint;
        for (int i = 0; i < numberPoint; i++)
        {
            dataArray[i].x = ((-widthOfGameObject / 2) + 1) + i * horizontalScale;
        }
        lineRenderer.SetPositions(dataArray);
    }

    void updateTraceOffset(float newOffset)
    {
        offsetPerTen = newOffset;
        offsetCoefficient = (offsetPerTen / 10) * maxValChanel;
    }

    void updateTraceGain(int newGain)
    {
        previousGain = gain;
        gain = newGain;

        for (int i = 0; i < numberPoint; i++)
        {
            dataArray[i].y = (dataArray[i].y / previousGain) * gain;
        }
        lineRenderer.SetPositions(dataArray);
    }

    void updateDraw(int sampleToLook)
    {
        mostRecentSample = sampleToLook;
        int elecPosOffset = idCurrentElec * eHandle.nbSam;
        int posInArray = sampleToLook - numberPoint + elecPosOffset;

        for (int i = 0; i < numberPoint; i++)
        {
            if (i + posInArray >= elecPosOffset)
            {
                dataArray[i].y = gain * (eHandle.eegData[i + posInArray] + offsetCoefficient);
            }
            else
            {
                dataArray[i].y = 0;
            }
        }
        lineRenderer.SetPositions(dataArray);
    }

    void updateElectrodeID(int newID)
    {
        if (newID != -1)
        {
            idCurrentElec = newID;
            elecLabel.text = eHandle.electList[idCurrentElec];
            maxValChanel = eHandle.maxValues[idCurrentElec];
            updateTraceOffset(offsetPerTen);
        }
    }

    //===

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 2)
            manageFocusClick();

        m_rectTransform.GetWorldCorners(m_worldCorners);
        Vector3 worldClick = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float perCentX = (worldClick.x - m_worldCorners[1].x) / (m_worldCorners[2].x - m_worldCorners[1].x);
        float sampleClicked = (mostRecentSample - numberPoint) + (perCentX * numberPoint);
        if (sampleClicked >= 0)
        {
            eventEeg currentEvent = new eventEeg(eventCode, (int)sampleClicked, elecOfInterest:elecLabel.text);
            eventWasClicked(currentEvent, traceID);
        }
    }

    void addEventToTrace(eventEeg currentEvent, int id)
    {
        GameObject currentEventToAdd = Instantiate(traceEventClick);
        currentEventToAdd.name = "Event - " + currentEvent.sample;
        currentEventToAdd.transform.SetParent(m_eventHolder);
        currentEventToAdd.transform.localScale = new Vector3(1, 1, 1);
        currentEventToAdd.transform.SetSiblingIndex(id);
        eventsAdded.Insert(id,currentEventToAdd);

        currentEventToAdd.GetComponent<EventTrace>().init(currentEvent, traceID);
        currentEventToAdd.GetComponent<EventTrace>().eventsToDisplay += new eventsToDisplay((eventToDisp, winID) => 
        {
            eventsToDisplay(eventToDisp, winID);
        });
        currentEventToAdd.GetComponent<EventTrace>().eventsToDelete += new eventsToDelete((eventToDisp, winID) =>
        {
            eventsToDelete(eventToDisp, winID);
        });
    }

    void updateEventsDraw(int sampleToLook)
    {
        if (hub.eventRemote.userEvents.Count > 0)
        {
            int left = sampleToLook - numberPoint;
            int right = sampleToLook;

            var keys = new List<int>(hub.eventRemote.userEvents.Keys);
            List <int> currentIndex = keys.Select((item, index) => new { Item = item, Index = index })
                                                         .Where(x => x.Item > left && x.Item < right)
                                                         .Select(x => x.Index)
                                                         .ToList();

            if (currentIndex.Count > 0)
            {
                for (int i = currentIndex[0] - 1; i >= 0; i--)
                {
                    if (eventsAdded[i].activeSelf == true)
                        eventsAdded[i].SetActive(false);
                }

                for (int i = currentIndex[currentIndex.Count - 1] + 1; i < eventsAdded.Count; i++)
                {
                    if (eventsAdded[i].activeSelf == true)
                        eventsAdded[i].SetActive(false);
                }

                for (int i = 0; i < currentIndex.Count; i++)
                {
                    float positionInsideRect = (left - keys[currentIndex[i]]) * -horizontalScale + ((-widthOfGameObject / 2) + 1);

                    if (keys[currentIndex[i]] <= right)
                    {
                        eventsAdded[currentIndex[i]].SetActive(true);
                        eventsAdded[currentIndex[i]].transform.localPosition = new Vector3(positionInsideRect, dataArray[keys[currentIndex[i]] - left].y, -201);
                    }
                }
            }
            else //No New obj, we clean if there is some left
            {
                List<GameObject> activeObj = eventsAdded.FindAll(x => x.activeSelf == true);
                if (activeObj.Count > 0)
                {
                    for (int i = 0; i < activeObj.Count; i++)
                    {
                        activeObj[i].SetActive(false);
                    }
                }
            }
        }
    }

    public void removeEventConnections(GameObject objToDel)
    {
        objToDel.GetComponent<EventTrace>().eventsToDisplay -= new eventsToDisplay((eventToDisp, winID) =>
        {
            eventsToDisplay(eventToDisp, winID);
        });
        objToDel.GetComponent<EventTrace>().eventsToDelete -= new eventsToDelete((eventToDisp, winID) =>
        {
            eventsToDelete(eventToDisp, winID);
        });
    }

    void manageFocusClick()
    {
        if (!m_window.hasFocus)
        {
            m_window.setBorderColor(orange);
            m_window.hasFocus = !m_window.hasFocus;

            if (handleOtherTrace != null)
            {
                handleOtherTrace.hasFocus = false;
                handleOtherTrace.setBorderColor(blue);
            }

            plotClicked(GameObject.Find(eHandle.electList[idCurrentElec].ToLower()));
        }
        else
        {
            plotClicked(null);
            m_window.setBorderColor(blue);
            m_window.hasFocus = !m_window.hasFocus;
        }
    }

    void plotClicked(GameObject plot)
    {
        if (m_window.hasFocus)
        {
            if (plot != null)
            {
                int hitID = plot.GetComponent<ElecPlotSize>().ID;
                updateElectrodeID(hitID);
            }
            ring.setSelectedPlot(plot);
        }
    }
}