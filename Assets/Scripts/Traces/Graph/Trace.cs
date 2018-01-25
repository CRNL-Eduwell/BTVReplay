using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public delegate void eventsClickedHandler(TraceEvent newVal, int idWin);

public class Trace : MonoBehaviour, IPointerClickHandler
{
    public event eventsClickedHandler eventWasClicked;

    public int TraceId
    {
        get
        {
            return traceID;
        }
    }
    public EegSignal TraceEeg
    {
        get
        {
            return eegSignal;
        }
    }
    public GraphEvents EventsEeg
    {
        get
        {
            return graphEvent;
        }
    }

    [SerializeField] optionsHub hub = null;
    [SerializeField] BTVMedia media = null;
    [SerializeField] VideoPlayer video = null;
    [SerializeField] EegSignal eegSignal = null;
    [SerializeField] AudioSignal audioSignal = null;
    [SerializeField] GraphLabel graphLabel = null;
    [SerializeField] ColorPicker colorpicker = null;
    [SerializeField] selectRing ring = null;
    [SerializeField] BrainWarden warden = null;
    [SerializeField] GraphGrid graphGrid = null;
    [SerializeField] GraphEvents graphEvent = null;
    [SerializeField] GraphSonification graphSonif = null;
    [SerializeField] Window m_window = null;
    [SerializeField] int traceID = 0;

    bool m_initDone = false;
    RectTransform m_rectTransform = null;
    Vector3[] m_worldCornerOfBrainPanel = new Vector3[4];
    Vector3[] m_worldCorners = new Vector3[4];

    void Awake()
    {
        media.loadTrace += new initTrace(init);
    }

    void OnDestroy()
    {
        media.loadTrace -= new initTrace(init);
        if (m_initDone)
        {
            video.sendTime -= new timeVideo(eegSignal.updateDraw);
            video.sendTimeVideo -= new timeVideoSync(audioSignal.updateDraw);
            video.sendTime -= new timeVideo(graphEvent.updateEventsDraw);
            video.sendTime -= new timeVideo(graphSonif.updateSonif);
            video.stopTimeVideo -= new stopVideo(graphSonif.muteSonficiation);

            hub.traceRemotes[traceID].idFileHasChanged -= new idFileChangedEventHandler(changeFileID);
            hub.traceRemotes[traceID].gainHasChanged -= new gainChangedEventHandler(UpdateTraceGain);
            hub.traceRemotes[traceID].offsetHasChanged -= new offsetChangedEventHandler(eegSignal.updateOffset);
            hub.traceRemotes[traceID].idElecHasChanged -= new idElecChangedEventHandler(updateElectrodeById);
            hub.traceRemotes[traceID].timeHasChanged -= new timePeriodChangedEventHandler(updateTimeResolution);
            hub.traceRemotes[traceID].gridToggled -= new toggleGridDisplay(graphGrid.displayTimeGrid);
            hub.traceRemotes[traceID].sonifToggled -= new toggleSonification(graphSonif.toggleSonification);
            hub.traceRemotes[traceID].soundChanged -= new newSoundSonif(graphSonif.changeAudioSonification);
            hub.eventRemote.newEventToShow -= new newEventToShowHandler(graphEvent.addEventToTrace);
            hub.eventRemote.showEvents -= new showAllEventsHandler(graphEvent.showEvents);
            hub.videoRemote.audioToggled -= new toggleAudioTraceEventHandler(audioSignal.Show);
            hub.videoRemote.gainAudioHasChanged -= new gainAudioChangedEventHandler(audioSignal.updateGain);
            hub.videoRemote.smAudioHasChanged -= new idAudioSmChangedEventHandler(audioSignal.changeAudioId);
            warden.plotWasClicked -= new newPlotClicked(plotClicked);
            colorpicker.changeColor -= new colorChanged(updateColors);
            graphLabel.ElectrodeButton.onClick.RemoveAllListeners();
            hub.traceRemotes[traceID].deleteElectrodeInPanel();
        }
    }

    void Update()
    {
        if (m_initDone && isOver(Input.mousePosition) && m_window.hasFocus)
        {
            Vector2 scrollDelta = Input.mouseScrollDelta;
            if (scrollDelta.y != 0)
            {
                if (scrollDelta.y < 0)
                    updateElectrodeById(eegSignal.IdElectrode - 1);
                else
                    updateElectrodeById(eegSignal.IdElectrode + 1);
            }
        }
    }

    void init()
    {
        m_rectTransform = gameObject.GetComponent<RectTransform>();

        eegSignal.init();
        audioSignal.init();
        graphLabel.init(eegSignal.LabelElectrode);
        graphGrid.init(eegSignal.PeriodInSeconds);
        graphEvent.init(this);
        graphSonif.init(this);

        #region plugEvents
        video.sendTime += new timeVideo(eegSignal.updateDraw);
        video.sendTimeVideo += new timeVideoSync(audioSignal.updateDraw);
        video.sendTime += new timeVideo(graphEvent.updateEventsDraw);
        video.sendTime += new timeVideo(graphSonif.updateSonif);
        video.stopTimeVideo += new stopVideo(graphSonif.muteSonficiation);

        hub.traceRemotes[traceID].idFileHasChanged += new idFileChangedEventHandler(changeFileID);
        hub.traceRemotes[traceID].gainHasChanged += new gainChangedEventHandler(UpdateTraceGain);
        hub.traceRemotes[traceID].offsetHasChanged += new offsetChangedEventHandler(eegSignal.updateOffset);
        hub.traceRemotes[traceID].idElecHasChanged += new idElecChangedEventHandler(updateElectrodeById);
        hub.traceRemotes[traceID].timeHasChanged += new timePeriodChangedEventHandler(updateTimeResolution);
        hub.traceRemotes[traceID].gridToggled += new toggleGridDisplay(graphGrid.displayTimeGrid);
        hub.traceRemotes[traceID].sonifToggled += new toggleSonification(graphSonif.toggleSonification);
        hub.traceRemotes[traceID].soundChanged += new newSoundSonif(graphSonif.changeAudioSonification);
        hub.eventRemote.newEventToShow += new newEventToShowHandler(graphEvent.addEventToTrace);
        hub.eventRemote.showEvents += new showAllEventsHandler(graphEvent.showEvents);
        hub.videoRemote.audioToggled += new toggleAudioTraceEventHandler(audioSignal.Show);
        hub.videoRemote.gainAudioHasChanged += new gainAudioChangedEventHandler(audioSignal.updateGain);
        hub.videoRemote.smAudioHasChanged += new idAudioSmChangedEventHandler(audioSignal.changeAudioId);
        warden.plotWasClicked += new newPlotClicked(plotClicked);
        colorpicker.changeColor += new colorChanged(updateColors);
        graphLabel.ElectrodeButton.onClick.AddListener(updateTracesWidth);
        hub.traceRemotes[traceID].loadElectrodeInPanel(eegSignal.fileHandle.electrodes);
        #endregion

        m_initDone = true;
    }

    void changeFileID(int newId)
    {
        eegSignal.updateFileId(newId);
        updateTimeResolution(eegSignal.PeriodInSeconds);
    }

    void updateTimeResolution(int newPeriod)
    {
        eegSignal.updateTimeResolution(newPeriod);
        eegSignal.updateHorizontalScale();
        audioSignal.updateTimeResolution(newPeriod);
        audioSignal.updateHorizontalScale();
        graphGrid.updateGridScale(newPeriod);
    }

    void UpdateTraceGain(float newGain)
    {
        eegSignal.updateGain(newGain);
        graphLabel.setName(eegSignal.LabelElectrode);
    }

    void updateElectrodeById(int newId)
    {
        eegSignal.IdElectrode = newId;
        eegSignal.updateOffset();
        graphLabel.setName(eegSignal.LabelElectrode);
    }

    void updateTracesWidth()
    {
        eegSignal.updateLineWidth();
        audioSignal.updateLineWidth();
    }

    void updateColors(Color color)
    {
        graphLabel.setColor(color);
        eegSignal.updateLineColor(color);
    }

    void plotClicked(GameObject plot)
    {
        if (m_window.hasFocus)
        {
            if (plot != null)
            {
                int hitID = plot.GetComponent<ElecPlotSize>().ID;
                updateElectrodeById(hitID);
            }
            ring.setSelectedPlot(plot);
        }
    }

    bool isOver(Vector3 mousePos)
    {
        m_rectTransform.GetWorldCorners(m_worldCornerOfBrainPanel);
        Vector3 worldClick = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (worldClick.x > m_worldCornerOfBrainPanel[1].x && worldClick.x < m_worldCornerOfBrainPanel[2].x
            && worldClick.y > m_worldCornerOfBrainPanel[3].y && worldClick.y < m_worldCornerOfBrainPanel[2].y)
            return true;
        else
            return false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //if (eventData.clickCount == 2)
        //    manageFocusClick();

        //focusClickElecLabel();

        m_rectTransform.GetWorldCorners(m_worldCorners);
        Vector3 worldClick = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float perCentX = (worldClick.x - m_worldCorners[1].x) / (m_worldCorners[2].x - m_worldCorners[1].x);
        float sampleClicked = (eegSignal.mostRecentSample - eegSignal.numberOfPoint) + (perCentX * eegSignal.numberOfPoint);
        if (sampleClicked >= 0)
        {
            TraceEvent currentEvent = new TraceEvent(new eventEeg(0, (int)sampleClicked, eegSignal.SamplingFrequency), elecOfInterest:eegSignal.LabelElectrode);
            eventWasClicked(currentEvent, traceID);
        }
    }
}