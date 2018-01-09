using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public delegate void eventsClickedHandler(TraceEvent newVal, int idWin);
public delegate void eventsToDisplay(TraceEvent newVal, int idWin);
public delegate void eventsToDelete(TraceEvent newVal, int idWin);

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
    public ELAN fileHandle
    {
        get
        {
            return eHandle;
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
    LineRenderer lineRenderer = null, lineRendererRMS = null;
    Text elecLabel = null;
    Image elecColor = null;
    Button elecButton = null;
    ELAN eHandle = null;
    bool initDone = false;
    public List<GameObject> eventsAdded = new List<GameObject>();

    GameObject gridLine = null, gridLineMS = null;
    GameObject gridCont = null;
    BrainWarden warden = null;
    Window m_window = null;
    Color orange = new Color(0.9058f, 0.5254f, 0.1921f);
    Color blue = new Color(0.6117f, 0.7058f, 0.7960f);
    Color yellow = new Color(0.9058f, 0.8784f, 0.0f);
    Window handleOtherTrace = null;
    selectRing ring = null;
    ColorPicker colorpicker = null;

    Vector3[] dataArray, dataArrayRMS;
    int mostRecentSample = 0;
    int periodSec = 10;
    int samplingFreq = 64;
    int numberPoint = 64 * 10;
    float widthOfGameObject = 0;
    float horizontalScale = 0;
    float gain = 1, gainAudio = 1;
    float previousGain = 1, previousGainAudio = 1;
    float maxValChanel = 0;
    float offsetCoefficient = 0;
    float offsetPerTen = 0;
    bool gridDisplay = false, showEvents = true;

    void Awake()
    {
        media.loadTrace += new initTrace(init);
    }

    void OnDestroy()
    {
        media.loadTrace -= new initTrace(init);

        if (initDone)
        {
            video.sendTime -= new timeVideo(updateDraw);
            video.sendTimeVideo -= new timeVideoSync(updateDrawRMS);
            video.sendTime -= new timeVideo(updateEventsDraw);

            hub.traceRemotes[traceID].idFileHasChanged -= new idFileChangedEventHandler(
                delegate (int newID)
                {
                    eHandle = ELAN.changeHandle(eHandle, media.elanFiles, newID);
                    samplingFreq = (int)eHandle.sampFreq;
                    updateTimeResolution(periodSec);
                });
            hub.traceRemotes[traceID].gainHasChanged -= new gainChangedEventHandler(updateTraceGain);
            hub.traceRemotes[traceID].offsetHasChanged -= new offsetChangedEventHandler(updateTraceOffset);
            hub.traceRemotes[traceID].idElecHasChanged -= new idElecChangedEventHandler(updateElectrodeID);
            hub.traceRemotes[traceID].timeHasChanged -= new timePeriodChangedEventHandler(updateTimeResolution);
            hub.traceRemotes[traceID].gridToggled -= new toggleGridDisplay(displayTimeGrid);
            hub.eventRemote.newEventToShow -= new newEventToShowHandler(addEventToTrace);
            hub.eventRemote.showEvents -= new showAllEventsHandler((bool show) =>
            {
                showEvents = show;
            });
            hub.videoRemote.audioToggled -= new toggleAudioTraceEventHandler(
                delegate (bool togg)
                {
                    lineRendererRMS.gameObject.SetActive(togg);
                });
            hub.videoRemote.gainAudioHasChanged -= new gainAudioChangedEventHandler(updateAudioGain);
            hub.videoRemote.smAudioHasChanged -= new idAudioSmChangedEventHandler(
                delegate (int newID)
                {
                    video.audioWav.idAudioHandle = newID;
                });

            warden.plotWasClicked -= new newPlotClicked(plotClicked);

            colorpicker.changeColor -= new colorChanged(setColors);

            elecButton.onClick.RemoveAllListeners();

            hub.traceRemotes[traceID].deleteElectrodeInPanel();
        }
    }

    void OnRectTransformDimensionsChange()
    {
        if (m_rectTransform != null)
        {
            updateHorizontalScale(lineRenderer, dataArray);
            updateHorizontalScale(lineRendererRMS, dataArrayRMS);
        }
    }

    void init()
    {
        traceEventClick = Resources.Load("Prefabs/Trace-Event", typeof(GameObject)) as GameObject;
        traceEventClick2 = Resources.Load("Prefabs/Trace-Event2", typeof(GameObject)) as GameObject;
        gridLine = Resources.Load("Prefabs/ImageGrid", typeof(GameObject)) as GameObject;
        gridLineMS = Resources.Load("Prefabs/ImageGridMS", typeof(GameObject)) as GameObject;

        ring = GameObject.Find("ringSelect").GetComponent<selectRing>();
        warden = GameObject.Find("BrainWindow").GetComponent<BrainWarden>();
        gridCont = gameObject.transform.GetChild(14).gameObject;

        if (traceID == 0)
        {
            handleOtherTrace = GameObject.Find("Trace" + (traceID + 2) + "Window").GetComponent<Window>();
            colorpicker = GameObject.Find("Canvas").transform.GetChild(1).GetChild(2).GetChild(0).GetChild(2).GetComponent<ColorPicker>();
        }
        else
        {
            handleOtherTrace = GameObject.Find("Trace" + (traceID) + "Window").GetComponent<Window>();
            colorpicker = GameObject.Find("Canvas").transform.GetChild(1).GetChild(2).GetChild(1).GetChild(2).GetComponent<ColorPicker>();
        }

        m_rectTransform = gameObject.GetComponent<RectTransform>();
        m_window = gameObject.GetComponent<Window>();
        m_eventHolder = gameObject.transform.GetChild(13);
        lineRenderer = gameObject.transform.GetChild(0).GetComponent<LineRenderer>();
        lineRendererRMS = gameObject.transform.GetChild(1).GetComponent<LineRenderer>();
        lineRendererRMS.gameObject.SetActive(false);

        elecLabel = gameObject.transform.GetChild(9).GetChild(0).GetComponent<Text>();
        elecColor = gameObject.transform.GetChild(9).GetChild(1).GetComponent<Image>();
        elecButton = gameObject.transform.GetChild(9).GetChild(1).GetComponent<Button>();

        eHandle = ELAN.returnFirstValidHandle(media.elanFiles);
        samplingFreq = (int)eHandle.sampFreq;
        numberPoint = samplingFreq * periodSec;
        if (eHandle.electrodes.Length > 0)
        {
            elecLabel.text = eHandle.electrodes[idCurrentElec].name;
            elecColor.gameObject.SetActive(true);
            maxValChanel = eHandle.maxValues[idCurrentElec];
            hub.traceRemotes[traceID].changeButtonSMColor();
        }

        dataArray = new Vector3[numberPoint];
        dataArrayRMS = new Vector3[numberPoint];

        lineRenderer.positionCount = numberPoint;
        lineRenderer.startWidth = 0.02f; //0.04f; = width 1
        lineRenderer.endWidth = 0.02f;

        lineRendererRMS.positionCount = numberPoint;
        lineRendererRMS.startWidth = 0.02f;
        lineRendererRMS.endWidth = 0.02f;

        updateHorizontalScale(lineRenderer, dataArray);
        updateHorizontalScale(lineRendererRMS, dataArrayRMS);
        updateGridScale(periodSec);
        displayTimeGrid(gridDisplay);

        #region plugEvents
        video.sendTime += new timeVideo(updateDraw);
        video.sendTimeVideo += new timeVideoSync(updateDrawRMS);
        video.sendTime += new timeVideo(updateEventsDraw);

        hub.traceRemotes[traceID].idFileHasChanged += new idFileChangedEventHandler(
            delegate (int newID)
            {
                eHandle = ELAN.changeHandle(eHandle, media.elanFiles, newID);
                samplingFreq = (int)eHandle.sampFreq;
                updateTimeResolution(periodSec);
            });
        hub.traceRemotes[traceID].gainHasChanged += new gainChangedEventHandler(updateTraceGain);
        hub.traceRemotes[traceID].offsetHasChanged += new offsetChangedEventHandler(updateTraceOffset);
        hub.traceRemotes[traceID].loadElectrodeInPanel(eHandle.electrodes);
        hub.traceRemotes[traceID].idElecHasChanged += new idElecChangedEventHandler(updateElectrodeID);
        hub.traceRemotes[traceID].timeHasChanged += new timePeriodChangedEventHandler(updateTimeResolution);
        hub.traceRemotes[traceID].gridToggled += new toggleGridDisplay(displayTimeGrid);
        hub.eventRemote.newEventToShow += new newEventToShowHandler(addEventToTrace);
        hub.eventRemote.showEvents += new showAllEventsHandler((bool show)=> 
        {
            showEvents = show;
        });
        hub.videoRemote.audioToggled += new toggleAudioTraceEventHandler(
            delegate (bool togg)
            {
                lineRendererRMS.gameObject.SetActive(togg);
            });
        hub.videoRemote.gainAudioHasChanged += new gainAudioChangedEventHandler(updateAudioGain);
        hub.videoRemote.smAudioHasChanged += new idAudioSmChangedEventHandler(
            delegate (int newID)
            {
                video.audioWav.idAudioHandle = newID;
            });

        warden.plotWasClicked += new newPlotClicked(plotClicked);

        colorpicker.changeColor += new colorChanged(setColors);

        elecButton.onClick.AddListener(updateTracesWidth);

        initDone = true;
        #endregion
    }

    void updateTimeResolution(int newPeriod)
    {
        periodSec = newPeriod;
        numberPoint = samplingFreq * periodSec;
        dataArray = new Vector3[numberPoint];
        dataArrayRMS = new Vector3[64 * periodSec];
        lineRenderer.positionCount = numberPoint;
        lineRendererRMS.positionCount = numberPoint;
        lineRenderer.sortingOrder = -1;
        lineRendererRMS.sortingOrder = -1;
        updateHorizontalScale(lineRenderer, dataArray);
        updateHorizontalScale(lineRendererRMS, dataArrayRMS);
        updateGridScale(periodSec);
    }

    void updateHorizontalScale(LineRenderer p_lineRenderer, Vector3[] p_dataArray)
    {
        widthOfGameObject = m_rectTransform.rect.width - 10;
        horizontalScale = widthOfGameObject / p_dataArray.Length;
        for (int i = 0; i < p_dataArray.Length; i++)
        {
            p_dataArray[i].x = ((-widthOfGameObject / 2) + 1) + i * horizontalScale;
            p_dataArray[i].y = 0;
        }
        p_lineRenderer.SetPositions(p_dataArray);
    }

    void updateGridScale(int newPeriod)
    {
        for (int i = gridCont.transform.childCount - 1; i >= 0; i--)
            Destroy(gridCont.transform.GetChild(i).gameObject);

        for (int i = 0; i < newPeriod; i++)
        {
            GameObject newLine = Instantiate(gridLine);
            newLine.name = "line " + i;
            newLine.transform.SetParent(gridCont.transform);
            newLine.transform.localScale = new Vector3(1, 1, 1);
            newLine.transform.localPosition = new Vector3(newLine.transform.localPosition.x, newLine.transform.localPosition.y, 0);
            newLine.SetActive(gridDisplay);

            if (newPeriod <= 3)
            {
                for (int j = 0; j < 4; j++)
                {
                    GameObject newLineMS = Instantiate(gridLineMS);
                    newLineMS.name = "lineMs " + i;
                    newLineMS.transform.SetParent(gridCont.transform);
                    newLineMS.transform.localScale = new Vector3(1, 1, 1);
                    newLineMS.transform.localPosition = new Vector3(newLineMS.transform.localPosition.x, newLineMS.transform.localPosition.y, 0);
                    newLineMS.SetActive(gridDisplay);
                }
            }
        }
    }

    void updateTraceOffset(float newOffset)
    {
        offsetPerTen = newOffset;
        offsetCoefficient = (offsetPerTen / 10) * maxValChanel;
    }

    void updateTraceGain(float newGain)
    {
        previousGain = gain;
        gain = newGain;
        updateElectrodeLabel();

        for (int i = 0; i < numberPoint; i++)
        {
            dataArray[i].y = (dataArray[i].y / previousGain) * gain;
        }
        lineRenderer.SetPositions(dataArray);
    }

    void updateAudioGain(float newGain)
    {
        previousGainAudio = gainAudio;
        gainAudio = newGain;

        for (int i = 0; i < numberPoint; i++)
        {
            dataArrayRMS[i].y = (dataArrayRMS[i].y / previousGainAudio) * gainAudio;
        }
        lineRendererRMS.SetPositions(dataArrayRMS);
    }

    void updateDraw(int milliSecToLook)
    {
        mostRecentSample = (int)(milliSecToLook * ((float)samplingFreq / 1000));
        int elecPosOffset = idCurrentElec * eHandle.nbSam;
        int posInArray = mostRecentSample - numberPoint + elecPosOffset;
        float limitVal = (m_rectTransform.rect.height - 6.5f) / 2;

        for (int i = 0; i < numberPoint; i++)
        {
            if (i + posInArray >= elecPosOffset)
            {
                float value = gain * (eHandle.eegData[i + posInArray] + offsetCoefficient);
                if (value >= -limitVal && value <= limitVal)
                {
                    dataArray[i].y = value;
                }
                else
                {
                    if (value >= 0)
                        dataArray[i].y = limitVal;
                    else
                        dataArray[i].y = -limitVal;
                }
            }
            else
            {
                dataArray[i].y = 0;
            }
        }
        lineRenderer.SetPositions(dataArray);
    }

    void updateDrawRMS(int milliSecToLook)
    {
        if (lineRendererRMS.gameObject.activeSelf == false || video.audioWav == null ||
            video.audioWav.filterFileExist == false || milliSecToLook == -1)
            return;

        int posInArray = (int)(milliSecToLook * ((float)64 / 1000)) - numberPoint;
        float limitVal = (m_rectTransform.rect.height - 6.5f) / 2;

        for (int i = 0; i < numberPoint; i++)
        {
            if ((i + posInArray >= 0) && (i + posInArray < video.audioWav.currentAudio.Length))
            {
                float value = gainAudio * ((float)video.audioWav.currentAudio[i + posInArray]);
                if (value >= -limitVal && value <= limitVal)
                {
                    dataArrayRMS[i].y = value;
                }
                else
                {
                    if (value >= 0)
                        dataArrayRMS[i].y = limitVal;
                    else
                        dataArrayRMS[i].y = -limitVal;
                }
            }
            else
            {
                dataArrayRMS[i].y = 0;
            }
        }
        lineRendererRMS.SetPositions(dataArrayRMS);
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
        if(gain >= 0)
            elecLabel.text = eHandle.electrodes[idCurrentElec].name;
        else
            elecLabel.text = " - " + eHandle.electrodes[idCurrentElec].name;
    }

    void updateTracesWidth()
    {
        if (lineRenderer.startWidth == 0.02f)
        {
            lineRenderer.startWidth = 0.04f;
            lineRenderer.endWidth = 0.04f;
            //==
            lineRendererRMS.startWidth = 0.04f;
            lineRendererRMS.endWidth = 0.04f;
        }
        else
        {
            lineRenderer.startWidth = 0.02f;
            lineRenderer.endWidth = 0.02f;
            //==
            lineRendererRMS.startWidth = 0.02f;
            lineRendererRMS.endWidth = 0.02f;
        }

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
            //eventEeg currentEvent = new eventEeg(eventCode, (int)sampleClicked, samplingFreq:samplingFreq, elecOfInterest:elecLabel.text);
            TraceEvent currentEvent = new TraceEvent(new eventEeg(eventCode, (int)sampleClicked, samplingFreq), elecOfInterest:elecLabel.text);
            eventWasClicked(currentEvent, traceID);
        }
    }

    void addEventToTrace(TraceEvent currentEvent, int id)
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

    void updateEventsDraw(int milliSecToLook)
    {
        if (hub.eventRemote.userEvents.Count > 0)
        {
            int left = (int)(milliSecToLook * ((float)samplingFreq / 1000)) - numberPoint;
            int right = (int)(milliSecToLook * ((float)samplingFreq / 1000));

            var keys = new List<int>(hub.eventRemote.userEvents.Keys);
            var values = new List<TraceEvent>(hub.eventRemote.userEvents.Values);

            List<int> idOverFlow = values.Select((item, index) => new { Item = item, Index = index })
                                         .Where(x => (x.Item.sample <= left && (x.Item.sample + (x.Item.duration * ((float)samplingFreq / 1000)) >= right)))
                                         .Select(x => x.Index)
                                         .ToList();

            List<int> idRightEnter = values.Select((item, index) => new { Item = item, Index = index })
                                           .Where(x => (x.Item.sample < right && x.Item.sample > left && (x.Item.sample + (x.Item.duration * ((float)samplingFreq / 1000)) >= right)))
                                           .Select(x => x.Index)
                                           .ToList();

            List<int> idInside = values.Select((item, index) => new { Item = item, Index = index })
                                       .Where(x => ((x.Item.sample < right) &&
                                                    (x.Item.sample > left) &&
                                                    (x.Item.sample + (x.Item.duration * ((float)samplingFreq / 1000)) >= left) &&
                                                    (x.Item.sample + (x.Item.duration * ((float)samplingFreq / 1000)) <= right)))
                                       .Select(x => x.Index)
                                       .ToList();

            List<int> idLeftEnter = values.Select((item, index) => new { Item = item, Index = index })
                                          .Where(x => ((x.Item.sample < left) &&
                                                       (x.Item.sample + (x.Item.duration * ((float)samplingFreq / 1000)) >= left) &&
                                                       (x.Item.sample + (x.Item.duration * ((float)samplingFreq / 1000)) <= right)))
                                          .Select(x => x.Index)
                                          .ToList();

            hideActiveEvents();
            if (showEvents)
            {
                float sizeV = m_rectTransform.rect.height - 10;

                for (int i = 0; i < idRightEnter.Count; i++)
                {
                    float positionInsideRect = (left - keys[idRightEnter[i]]) * -horizontalScale + ((-widthOfGameObject / 2) + 1);
                    float rightevent = right - values[idRightEnter[i]].sample;
                    float size = (rightevent / (right - left)) * widthOfGameObject;

                    eventsAdded[idRightEnter[i]].transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
                    eventsAdded[idRightEnter[i]].transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sizeV);
                    eventsAdded[idRightEnter[i]].SetActive(true);
                    eventsAdded[idRightEnter[i]].transform.localPosition = new Vector3(positionInsideRect, 0, -2);
                }

                for (int i = 0; i < idInside.Count; i++)
                {
                    float positionInsideRect = (left - keys[idInside[i]]) * -horizontalScale + ((-widthOfGameObject / 2) + 1);
                    float size = ((values[idInside[i]].duration * ((float)samplingFreq / 1000)) / (right - left)) * widthOfGameObject;

                    if (values[idInside[i]].duration > 0)
                    {
                        eventsAdded[idInside[i]].transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
                        eventsAdded[idInside[i]].transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sizeV);
                        eventsAdded[idInside[i]].SetActive(true);
                        eventsAdded[idInside[i]].transform.localPosition = new Vector3(positionInsideRect, 0, -2);
                    }
                    else
                    {
                        eventsAdded[idInside[i]].SetActive(true);
                        eventsAdded[idInside[i]].transform.localPosition = new Vector3(positionInsideRect, dataArray[keys[idInside[i]] - left].y, -201);
                    }
                }

                for (int i = 0; i < idLeftEnter.Count; i++)
                {
                    float positionInsideRect = ((-widthOfGameObject / 2) + 1);
                    float leftevent = (values[idLeftEnter[i]].sample + (values[idLeftEnter[i]].duration * ((float)samplingFreq / 1000)) - left);
                    float size = (leftevent / (right - left)) * widthOfGameObject;

                    eventsAdded[idLeftEnter[i]].transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
                    eventsAdded[idLeftEnter[i]].transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sizeV);
                    eventsAdded[idLeftEnter[i]].SetActive(true);
                    eventsAdded[idLeftEnter[i]].transform.localPosition = new Vector3(positionInsideRect, 0, -2);
                }

                for (int i = 0; i < idOverFlow.Count; i++)
                {
                    float positionInsideRect = ((-widthOfGameObject / 2) + 1);
                    float size = widthOfGameObject;
                    eventsAdded[idOverFlow[i]].transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
                    eventsAdded[idOverFlow[i]].transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sizeV);
                    eventsAdded[idOverFlow[i]].SetActive(true);
                    eventsAdded[idOverFlow[i]].transform.localPosition = new Vector3(positionInsideRect, 0, -2);
                }
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

            plotClicked(GameObject.Find(eHandle.electrodes[idCurrentElec].name.ToLower()));
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

    void setColors(Color color)
    {
        elecColor.color = color;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }

    void displayTimeGrid(bool isGridOn)
    {
        gridDisplay = isGridOn;
        for (int i = 0; i < gridCont.transform.childCount; i++)
            gridCont.transform.GetChild(i).gameObject.SetActive(isGridOn);
    }
}