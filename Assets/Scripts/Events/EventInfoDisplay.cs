using BrainTV.Tools.NumberExtensions;
using BTV.Data;
using BTV.Services.EegFileService;
using BTV.Services.VideoService;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EventInfoDisplay : MonoBehaviour
{
    [SerializeField]
    private Text m_Time = null;
    [SerializeField]
    private Dropdown m_Electrodes = null;
    [SerializeField]
    private Text m_Code = null;
    [SerializeField]
    private Text m_Duration = null;
    [SerializeField]
    private Text m_Comment = null;
    [SerializeField]
    private Button m_EditEvent = null;
    [SerializeField]
    private Button m_CalculateCorrelation = null;
    [SerializeField]
    private Button m_Calculate2DCorrelation = null;
    [SerializeField]
    private Button m_CloseWindow = null;

    private BtvEvent m_Event = null;

    public void init(BtvEvent clickedEvent)
    {
        m_Event = new BtvEvent(clickedEvent);

        InitTimeDisplay((int)m_Event.TimeInSeconds);
        InitElectrodeDropDown();
        InitUiValues(m_Event);
        SetButtonsInteractibility(m_Event.Duration);

        m_Electrodes.onValueChanged.AddListener(UpdateEventMainElectrode);
        m_EditEvent.onClick.AddListener(EditEvent);
        m_CalculateCorrelation.onClick.AddListener(CalculateCorrelation);
        m_Calculate2DCorrelation.onClick.AddListener(Calculate2DCorrelation);
        m_CloseWindow.onClick.AddListener(CloseWindow);
    }

    private void OnDestroy()
    {
        m_Electrodes.onValueChanged.RemoveAllListeners();
        m_EditEvent.onClick.RemoveAllListeners();
        m_CalculateCorrelation.onClick.RemoveAllListeners();
        m_Calculate2DCorrelation.onClick.RemoveAllListeners();
        m_CloseWindow.onClick.RemoveAllListeners();
    }

    private void InitTimeDisplay(int TimeInSeconds)
    {
        int h = TimeInSeconds / 3600;
        int m = (TimeInSeconds / 60) % 60;
        int s = TimeInSeconds % 60;

        if (h > 0)
            m_Time.text = h.FormatToTimeString() + ":" + m.FormatToTimeString() + ":" + s.FormatToTimeString();
        else
            m_Time.text = "00:" + m.FormatToTimeString() + ":" + s.FormatToTimeString();
    }

    private void InitElectrodeDropDown()
    {
        BtvProgram container = EegFileService.ReturnFirstValidContainer();

        m_Electrodes.options.Clear();
        for (int i = 0; i < container.NumberOfElectrodes; i++)
            m_Electrodes.options.Add(new Dropdown.OptionData(container.GetElectrodeNameFromElectrodeID(i)));

        if(VideoService.FilteredDataLoaded)
            m_Electrodes.options.Add(new Dropdown.OptionData("AUD"));

        m_Electrodes.value = container.GetElectrodeIDFromElectrodeName(m_Event.SiteOfInterest);
    }

    private void InitUiValues(BtvEvent currentEvent)
    {
        m_Code.text = currentEvent.Code.ToString();
        m_Duration.text = currentEvent.Duration.ToString();
        m_Comment.text = currentEvent.Comment;
    }

    private void UpdateEventMainElectrode(int ElectrodeID)
    {
        BtvEvent modifyEvent = new BtvEvent(m_Event)
        {
            SiteOfInterest = m_Electrodes.options[ElectrodeID].text
        };

        EventsModificationMessage message = new EventsModificationMessage
        {
            TaskToExecute = 1,
            Event = modifyEvent,
            EventMemory = m_Event
        };
        Messenger.Default.Send(message, MessageContext.EventsModificationMessage);

        m_Event = new BtvEvent(modifyEvent);
    }

    private void EditEvent()
    {
        EventsModificationMessage message = new EventsModificationMessage
        {
            TaskToExecute = 3,
            Event = m_Event,
            ParentWindowIndex = gameObject.GetComponentInParent<Trace>().TraceId
        };
        Messenger.Default.Send(message, MessageContext.EventsModificationMessage);
        CloseWindow();
    }

    private void CalculateCorrelation()
    {
        EventsModificationMessage message = new EventsModificationMessage
        {
            TaskToExecute = 4,
            Event = m_Event
        };
        Messenger.Default.Send(message, MessageContext.EventsModificationMessage);
    }

    private void Calculate2DCorrelation()
    {
        EventsModificationMessage message = new EventsModificationMessage
        {
            TaskToExecute = 5,
            Event = m_Event
        };
        Messenger.Default.Send(message, MessageContext.EventsModificationMessage);
    }

    private void CloseWindow()
    {
        Destroy(gameObject);
    }

    private void SetButtonsInteractibility(int EventDuration)
    {
        bool IsInteractable = EventDuration > 0 ? true : false;
        m_CalculateCorrelation.interactable = IsInteractable;
        m_Calculate2DCorrelation.interactable = IsInteractable;
    }
}
