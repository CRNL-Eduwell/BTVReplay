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
    public Text labelElectrode
    {
        get
        {
            return elecLabel;
        }
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
    public int samplingFrequency
    {
        get
        {
            return (int)eHandle.sampFreq;
        }
    }
    public int Gain
    {
        get
        {
            return (int)gain;
        }
    }
    public bool hasFocus
    {
        get
        {
            return m_window.hasFocus;
        }
    }
    public int numberOfPoint
    {
        get
        {
            return numberPoint;
        }
    }

    [SerializeField] optionsHub hub = null;
    [SerializeField] BTVMedia media = null;
    [SerializeField] VideoPlayer video = null;
    [SerializeField] int traceID = 0;
    [SerializeField] int idCurrentElec = 0;

    GameObject traceEventClick = null;
    GameObject traceEventClick2 = null;
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
    Color yellow = new Color(0.9058f, 0.8784f, 0.0f);
    Window handleOtherTrace = null;
    selectRing ring = null;
    ColorPicker colorpicker = null;

    Vector3[] dataArray;
    int mostRecentSample = 0;
    int periodSec = 10;
    int samplingFreq = 64;
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
                    samplingFreq = (int)eHandle.sampFreq;
                });
            hub.traceRemotes[traceID].gainHasChanged -= new gainChangedEventHandler(updateTraceGain);
            hub.traceRemotes[traceID].offsetHasChanged -= new offsetChangedEventHandler(updateTraceOffset);
            hub.traceRemotes[traceID].idElecHasChanged -= new idElecChangedEventHandler(updateElectrodeID);
            hub.traceRemotes[traceID].timeHasChanged -= new timePeriodChangedEventHandler(updateTimeResolution);
            hub.eventRemote.newEventToShow -= new newEventToShowHandler(addEventToTrace);

            warden.plotWasClicked -= new newPlotClicked(plotClicked);

            colorpicker.changeColor -= new colorChanged(setColorLineRenderer);

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
        traceEventClick2 = Resources.Load("Prefabs/Trace-Event2", typeof(GameObject)) as GameObject;
        ring = GameObject.Find("ringSelect").GetComponent<selectRing>();
        warden = GameObject.Find("BrainWindow").GetComponent<BrainWarden>();

        if (traceID == 0)
        {
            handleOtherTrace = GameObject.Find("Trace" + (traceID + 2) + "Window").GetComponent<Window>();
            colorpicker = GameObject.Find("Canvas").transform.GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetChild(1).GetChild(5).GetComponent<ColorPicker>();
        }
        else
        {
            handleOtherTrace = GameObject.Find("Trace" + (traceID) + "Window").GetComponent<Window>();
            colorpicker = GameObject.Find("Canvas").transform.GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetChild(1 + traceID).GetChild(5).GetComponent<ColorPicker>();
        }

        m_rectTransform = gameObject.GetComponent<RectTransform>();
        m_window = gameObject.GetComponent<Window>();
        m_eventHolder = gameObject.transform.GetChild(11);
        lineRenderer = gameObject.transform.GetChild(0).GetComponent<LineRenderer>();
        elecLabel = gameObject.transform.GetChild(9).GetComponent<Text>();
        
        eHandle = ELAN.returnFirstValidHandle(media.elanFiles);
        samplingFreq = (int)eHandle.sampFreq;
        numberPoint = samplingFreq * periodSec;
        if (eHandle.electList.Count > 0)
        {
            elecLabel.text = eHandle.electList[idCurrentElec];
            maxValChanel = eHandle.maxValues[idCurrentElec];
            hub.traceRemotes[traceID].changeButtonSMColor();
        }

        dataArray = new Vector3[numberPoint];
        lineRenderer.numPositions = numberPoint;
        lineRenderer.startWidth = 0.04f;
        lineRenderer.endWidth = 0.04f;
        updateHorizontalScale();

        #region plugEvents
        hub.traceRemotes[traceID].idFileHasChanged += new idFileChangedEventHandler(
            delegate (int newID)
            {
                eHandle = ELAN.changeHandle(eHandle, media.elanFiles, newID);
                samplingFreq = (int)eHandle.sampFreq;
            });
        hub.traceRemotes[traceID].gainHasChanged += new gainChangedEventHandler(updateTraceGain);
        hub.traceRemotes[traceID].offsetHasChanged += new offsetChangedEventHandler(updateTraceOffset);
        hub.traceRemotes[traceID].loadElectrodeInPanel(eHandle.electList);
        hub.traceRemotes[traceID].idElecHasChanged += new idElecChangedEventHandler(updateElectrodeID);
        hub.traceRemotes[traceID].timeHasChanged += new timePeriodChangedEventHandler(updateTimeResolution);
        hub.eventRemote.newEventToShow += new newEventToShowHandler(addEventToTrace);

        warden.plotWasClicked += new newPlotClicked(plotClicked);

        colorpicker.changeColor += new colorChanged(setColorLineRenderer);

        initDone = true;
        #endregion
    }

    void updateTimeResolution(int newPeriod)
    {
        periodSec = newPeriod;
        numberPoint = samplingFreq * periodSec;
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
            dataArray[i].y = 0;
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

        if(newGain / previousGain < 0)
            updateElectrodeLabel();

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
            updateElectrodeLabel();
            maxValChanel = eHandle.maxValues[idCurrentElec];
            updateTraceOffset(offsetPerTen);
        }
    }

    void updateElectrodeLabel()
    {
        if(gain > 0)
            elecLabel.text = eHandle.electList[idCurrentElec];
        else
            elecLabel.text = " - " + eHandle.electList[idCurrentElec];
    }

    //===

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 2)
            manageFocusClick();

        focusClickElecLabel();

        m_rectTransform.GetWorldCorners(m_worldCorners);
        Vector3 worldClick = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float perCentX = (worldClick.x - m_worldCorners[1].x) / (m_worldCorners[2].x - m_worldCorners[1].x);
        float sampleClicked = (mostRecentSample - numberPoint) + (perCentX * numberPoint);
        if (sampleClicked >= 0)
        {
            eventEeg currentEvent = new eventEeg(eventCode, (int)sampleClicked, samplingFreq:samplingFreq, elecOfInterest:elecLabel.text);
            eventWasClicked(currentEvent, traceID);
        }
    }

    void addEventToTrace(eventEeg currentEvent, int id)
    {
        GameObject currentEventToAdd = null;
        if (currentEvent.duration == 0)
            currentEventToAdd = Instantiate(traceEventClick);
        else
            currentEventToAdd = Instantiate(traceEventClick2);

        currentEventToAdd.name = "Event - " + currentEvent.sample;
        currentEventToAdd.transform.SetParent(m_eventHolder);
        currentEventToAdd.transform.localScale = new Vector3(1, 1, 1);
        currentEventToAdd.transform.SetSiblingIndex(id);
        eventsAdded.Insert(id, currentEventToAdd);

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
            var values = new List<eventEeg>(hub.eventRemote.userEvents.Values);

            List<int> idMove = values.Select((item, index) => new { Item = item, Index = index })
                                                             .Where(x => x.Item.sample > left && x.Item.sample < right)
                                                             .Select(x => x.Index)
                                                             .ToList();

            List<int> idLeft = values.Select((item, index) => new { Item = item, Index = index })
                                                             .Where(x => x.Item.sample < left && (x.Item.sample + (x.Item.duration * ((float)samplingFreq / 1000))) < right && 
                                                                                                 (x.Item.sample + (x.Item.duration * ((float)samplingFreq / 1000))) > left)
                                                             .Select(x => x.Index)
                                                             .ToList();

            List<int> idRight = values.Select((item, index) => new { Item = item, Index = index })
                                                             .Where(x => x.Item.sample > left && x.Item.sample < right && (x.Item.sample + (x.Item.duration * ((float)samplingFreq / 1000))) > right)
                                                             .Select(x => x.Index)
                                                             .ToList();

            hideActiveEvents();
            float sizeV = m_rectTransform.rect.height - 10;

            for (int i = 0; i < idMove.Count; i++)
            {
                float positionInsideRect = (left - keys[idMove[i]]) * -horizontalScale + ((-widthOfGameObject / 2) + 1);
                float size = ((values[idMove[i]].duration * ((float)samplingFreq / 1000)) / (right - left)) * widthOfGameObject;

                if (values[idMove[i]].duration > 0)
                {
                    if (keys[idMove[i]] <= right)
                    {
                        eventsAdded[idMove[i]].transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
                        eventsAdded[idMove[i]].transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sizeV);
                        eventsAdded[idMove[i]].SetActive(true);
                        eventsAdded[idMove[i]].transform.localPosition = new Vector3(positionInsideRect, 0, -2);
                    }
                }
                else
                {
                    eventsAdded[idMove[i]].SetActive(true);
                    eventsAdded[idMove[i]].transform.localPosition = new Vector3(positionInsideRect, dataArray[keys[idMove[i]] - left].y, -201);
                }
            }

            for (int i = 0; i < idLeft.Count; i++)
            {
                float leftevent = (values[idLeft[i]].sample + (values[idLeft[i]].duration * ((float)samplingFreq / 1000)) - left);
                float size = (leftevent / (right - left)) * widthOfGameObject;

                eventsAdded[idLeft[i]].transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
                eventsAdded[idLeft[i]].transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sizeV);
                eventsAdded[idLeft[i]].SetActive(true);
            }

            for (int i = 0; i < idRight.Count; i++)
            {
                float leftevent = right - values[idRight[i]].sample;
                float size = (leftevent / (right - left)) * widthOfGameObject;

                eventsAdded[idRight[i]].transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
                eventsAdded[idRight[i]].transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sizeV);
                eventsAdded[idRight[i]].SetActive(true);
            }
        }
    }

    public void hideActiveEvents()
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

    public void manageFocusClick()
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

    void focusClickElecLabel()
    {
        Ray r = new Ray(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector3.forward);
        RaycastHit hit;
        if (Physics.Raycast(r, out hit))
        {
            if (hit.collider.name == "ElecLabel" + (traceID + 1))
            {
                manageFocusClick();
                if (m_window.transform.position == handleOtherTrace.transform.position)
                {
                    gameObject.transform.SetSiblingIndex(1);
                    handleOtherTrace.gameObject.transform.SetSiblingIndex(0);
                }

            }
            else
            {
                handleOtherTrace.gameObject.GetComponent<TraceCurve>().manageFocusClick();
                if (m_window.transform.position == handleOtherTrace.transform.position)
                {
                    gameObject.transform.SetSiblingIndex(0);
                    handleOtherTrace.gameObject.transform.SetSiblingIndex(1);
                }
            }
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

    void setColorLineRenderer(Color color)
    {
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }
}