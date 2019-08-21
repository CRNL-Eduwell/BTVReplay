using UnityEngine;
using UnityEngine.UI;
using BrainTV.Tools.NumberExtensions;

public class EventInfoEdit : MonoBehaviour
{
    [SerializeField]
    private Text m_Time = null;
    [SerializeField]
    private InputField m_Code = null;
    [SerializeField]
    private InputField m_Duration = null;
    [SerializeField]
    private InputField m_Comment = null;
    [SerializeField]
    private Button m_SaveEvent = null;
    [SerializeField]
    private Button m_DeleteEvent = null;
    [SerializeField]
    private Button m_CloseWindow = null;

    private TraceEvent m_Event = null;
    private TraceEvent m_OriginalEvent = null;
    private bool m_IsModif = false;

    public void init(TraceEvent clickedEvent, bool isModif)
    {
        m_Event = new TraceEvent(clickedEvent);
        m_OriginalEvent = new TraceEvent(clickedEvent);
        m_IsModif = isModif;

        InitTimeDisplay((int)m_Event.timeSeconds());

        if (isModif && ApplicationState.MemoryEvent != null)
            InitUiValues(ApplicationState.MemoryEvent);
        else
            InitUiValues(m_Event);

        m_SaveEvent.onClick.AddListener(SaveEvent);
        m_DeleteEvent.onClick.AddListener(DeleteEvent);
        m_CloseWindow.onClick.AddListener(CloseWindow);
    }

    private void OnDestroy()
    {
        m_SaveEvent.onClick.RemoveAllListeners();
        m_DeleteEvent.onClick.RemoveAllListeners();
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

    private void InitUiValues(TraceEvent currentEvent)
    {
        m_Code.text = currentEvent.code.ToString();
        m_Duration.text = currentEvent.duration.ToString();
        m_Comment.text = currentEvent.comment;
    }

    private void SaveEvent()
    {
        CheckEventIntegrity();
        if (!m_IsModif)
        {
            EventsModificationMessage message = new EventsModificationMessage
            {
                TaskToExecute = 0,
                Event = m_Event
            };
            Messenger.Default.Send(message, MessageContext.EventsModificationMessage);
        }
        else
        {
            EventsModificationMessage message = new EventsModificationMessage
            {
                TaskToExecute = 1,
                Event = m_Event,
                EventMemory = m_OriginalEvent
            };
            Messenger.Default.Send(message, MessageContext.EventsModificationMessage);
        }

        CloseWindow();
    }

    private void CheckEventIntegrity()
    {
        if (int.TryParse(m_Code.text, out int codeValue))
            m_Event.code = codeValue;
        else
            m_Event.code = 0;

        m_Event.comment = m_Comment.text;
        m_Event.duration = int.Parse(m_Duration.text);
    }

    private void DeleteEvent()
    {
        ApplicationState.displayConfirmation("Event Deletion", "Are You Sure You Want To Delete This Event ?", DeleteAction, CancelAction);
    }

    private void CloseWindow()
    {
        Destroy(gameObject);
    }

    private void DeleteAction()
    {
        EventsModificationMessage message = new EventsModificationMessage
        {
            TaskToExecute = 2,
            Event = m_Event
        };
        Messenger.Default.Send(message, MessageContext.EventsModificationMessage);

        CloseWindow();
    }

    private void CancelAction()
    {
        Destroy(gameObject);
    }
}
