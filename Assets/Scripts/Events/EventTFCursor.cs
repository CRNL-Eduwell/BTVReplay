using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventTFCursor : MonoBehaviour
{
    [SerializeField] private bool m_IsSlaved = false;
    [SerializeField] private Image m_HorizontalLine = null;
    [SerializeField] private Image m_VerticalLine = null;

    private RectTransform m_Rectransform = null;

    private void Awake()
    {
        m_Rectransform = gameObject.transform.GetComponent<RectTransform>();

        //Messenger.Default.Register<UiToTFEventsMessage>(this, OnUiToTFEventsMessage, MessageContext.UiToTFEvents);
    }

    private void OnDestroy()
    {
        //Messenger.Default.Unregister(this, MessageContext.UiToTFEvents);
    }

    private void OnRectTransformDimensionsChange()
    {
        if (gameObject.transform.GetComponent<RectTransform>() == null) return;

        float width = gameObject.transform.parent.transform.GetComponent<RectTransform>().rect.width;
        float height = gameObject.transform.parent.transform.GetComponent<RectTransform>().rect.height;

        m_HorizontalLine.transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        m_VerticalLine.transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }

    private void OnGUI()
    {
        Vector2 Mouse = Input.mousePosition;
        bool isOver = RectTransformUtility.RectangleContainsScreenPoint(m_Rectransform, Mouse, Camera.main);
        if (isOver || m_IsSlaved)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(m_Rectransform, Mouse, Camera.main, out Vector2 localPosition);

            float x = localPosition.x;
            float y = localPosition.y;

            m_HorizontalLine.transform.localPosition = new Vector3(m_HorizontalLine.transform.localPosition.x, y, m_HorizontalLine.transform.localPosition.z);
            m_VerticalLine.transform.localPosition = new Vector3(x, m_VerticalLine.transform.localPosition.y, m_VerticalLine.transform.localPosition.z);
        }
    }

    private void OnUiToTFEventsMessage(UiToTFEventsMessage message)
    {
        m_IsSlaved = message.IsSlaved;
    }
}
