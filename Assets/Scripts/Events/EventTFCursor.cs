using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EventTFCursor : MonoBehaviour
{
    public bool ShowCursor
    {
        get
        {
            return m_ShowCursor;
        }
        set
        {
            m_ShowCursor = value;
            m_HorizontalLine.gameObject.SetActive(m_ShowCursor);
            m_VerticalLine.gameObject.SetActive(m_ShowCursor);
        }
    }
    public bool IsSlaved { get; set; } = false;
    public static Dictionary<int, bool> IsOverDictionary = new Dictionary<int, bool>();
    public bool IsOver { get { return RectTransformUtility.RectangleContainsScreenPoint(m_Rectransform, Input.mousePosition, Camera.main); } }

    [SerializeField] private Image m_HorizontalLine = null;
    [SerializeField] private Image m_VerticalLine = null;
    [SerializeField] private EventTfValueDisplay m_Display = null;

    private RectTransform m_Rectransform = null;
    private RectTransform m_ParentRecttransform = null;
    private bool m_ShowCursor = false;

    private void Awake()
    {
        m_Rectransform = gameObject.transform.GetComponent<RectTransform>();
        m_ParentRecttransform = gameObject.transform.parent.transform.GetComponent<RectTransform>();

        Messenger.Default.Register<EventsToEventsMessage>(this, OnEventsToEventsMessage, MessageContext.EventsToEventsMessage);
    }

    private void OnDestroy()
    {
        Messenger.Default.Unregister(this, MessageContext.EventsToEventsMessage);
    }

    private void OnRectTransformDimensionsChange()
    {
        if (m_Rectransform == null) return;

        m_HorizontalLine.transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, m_ParentRecttransform.rect.width);
        m_VerticalLine.transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, m_ParentRecttransform.rect.height);
    }

    private void OnGUI()
    {
        if (!m_ShowCursor) return;

        IsOverDictionary[transform.parent.GetComponent<EventWithDuration>().ParentWindowIndex] = IsOver;
        if (IsSlaved)
        {
            bool isOneOver = IsOverDictionary.Values.Any(isOver => isOver);
            m_HorizontalLine.gameObject.SetActive(isOneOver);
            m_VerticalLine.gameObject.SetActive(isOneOver);
        }
        else
        {
            m_HorizontalLine.gameObject.SetActive(IsOver);
            m_VerticalLine.gameObject.SetActive(IsOver);
        }

        if (IsOver)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(m_Rectransform, Input.mousePosition, Camera.main, out Vector2 localPosition);

            m_HorizontalLine.transform.localPosition = new Vector3(m_HorizontalLine.transform.localPosition.x, localPosition.y, m_HorizontalLine.transform.localPosition.z);
            m_VerticalLine.transform.localPosition = new Vector3(localPosition.x, m_VerticalLine.transform.localPosition.y, m_VerticalLine.transform.localPosition.z);

            if (IsSlaved)
            {
                EventsToEventsMessage message = new EventsToEventsMessage
                {
                    TaskToExecute = EventsToEventsMessage.Task.MoveCursor,
                    XPositionPercentage = localPosition.x / m_ParentRecttransform.rect.width,
                    YPositionPercentage = localPosition.y / m_ParentRecttransform.rect.height,
                    BTVEvent = transform.parent.GetComponent<EventWithDuration>().EventOfInterest,
                    ParentWindowIndex = transform.parent.GetComponent<EventWithDuration>().ParentWindowIndex
                };
                Messenger.Default.Send(message, MessageContext.EventsToEventsMessage);
            }

            float x_not_centered = (localPosition.x + (0.5f * m_ParentRecttransform.rect.width)) / m_ParentRecttransform.rect.width;
            float y_not_centered = (localPosition.y + (0.5f * m_ParentRecttransform.rect.height)) / m_ParentRecttransform.rect.height;

            m_Display.Show(true);
            transform.parent.GetComponent<EventWithDuration>().DisplayTfInfo(x_not_centered, y_not_centered);
        }
    }

    private void OnEventsToEventsMessage(EventsToEventsMessage message)
    {
        if (message.TaskToExecute == EventsToEventsMessage.Task.MoveCursor)
        {
            if (!m_ShowCursor) return;

            bool sameWindow = message.ParentWindowIndex == transform.parent.GetComponent<EventWithDuration>().ParentWindowIndex;
            bool sameEvent = message.BTVEvent == transform.parent.GetComponent<EventWithDuration>().EventOfInterest;
            if (!sameWindow && sameEvent)
            {
                float x = message.XPositionPercentage * m_ParentRecttransform.rect.width;
                float y = message.YPositionPercentage * m_ParentRecttransform.rect.height;
                m_HorizontalLine.transform.localPosition = new Vector3(m_HorizontalLine.transform.localPosition.x, y, m_HorizontalLine.transform.localPosition.z);
                m_VerticalLine.transform.localPosition = new Vector3(x, m_VerticalLine.transform.localPosition.y, m_VerticalLine.transform.localPosition.z);

                float x_not_centered = (x + (0.5f * m_ParentRecttransform.rect.width)) / m_ParentRecttransform.rect.width;
                float y_not_centered = (y + (0.5f * m_ParentRecttransform.rect.height)) / m_ParentRecttransform.rect.height;

                m_Display.Show(true);
                transform.parent.GetComponent<EventWithDuration>().DisplayTfInfo(x_not_centered, y_not_centered);
            }
        }
    }
}
