using UnityEngine;
using UnityEngine.UI;
using BrainTV.Tools.NumberExtensions;
using BTV.Data;
using BTV.Services.CodeMatchingService;

public class EventInfoAdd : MonoBehaviour
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
    private Button m_CloseWindow = null;

    private BtvEvent m_Event = null;

    public void Init(BtvEvent clickedEvent)
    {
        m_Event = new BtvEvent(clickedEvent);

        InitTimeDisplay((int)m_Event.TimeInSeconds);
        SetUiValues(m_Event);

        m_Code.onEndEdit.AddListener(OnEndEditCodefield);
        m_SaveEvent.onClick.AddListener(SaveEvent);
        m_CloseWindow.onClick.AddListener(CloseWindow);
    }

    private void OnDestroy()
    {
        m_Code.onEndEdit.RemoveAllListeners();
        m_SaveEvent.onClick.RemoveAllListeners();
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

    private void SetUiValues(BtvEvent currentEvent)
    {
        m_Code.text = currentEvent.Code.ToString();
        m_Duration.text = currentEvent.Duration.ToString();
        m_Comment.text = currentEvent.Comment;
    }

    private void OnEndEditCodefield(string str)
    {
        if (!CodeMatchingService.HasCodes) return;

        if (int.TryParse(m_Code.text, out int codeValue))
        {
            m_Comment.text = CodeMatchingService.GetCommentFromCode(codeValue);
        }
    }

    private void SaveEvent()
    {
        CheckEventIntegrity();

        EventsModificationMessage message = new EventsModificationMessage
        {
            TaskToExecute = EventsModificationMessage.Task.AddEvent,
            Event = m_Event
        };
        Messenger.Default.Send(message, MessageContext.EventsModificationMessage);
       
        CloseWindow();
    }

    private void CheckEventIntegrity()
    {
        if (int.TryParse(m_Code.text, out int codeValue))
            m_Event.Code = codeValue;
        else
            m_Event.Code = 0;

        m_Event.Comment = m_Comment.text;
        m_Event.Duration = int.Parse(m_Duration.text);
    }

    private void CloseWindow()
    {
        Destroy(gameObject);
    }
}
