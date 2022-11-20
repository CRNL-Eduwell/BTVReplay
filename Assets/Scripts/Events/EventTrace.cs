using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using BTV.Data;

public class EventTrace : MonoBehaviour, IPointerClickHandler
{
    public BtvEvent EventOfInterest { get; set; } = null;
    public int ParentWindowIndex { get; private set; } = -1;

    private GameObject m_ContextualWindowPrefabs = null;
    private GameObject m_ContextualMenuWindow = null;

    public void Init(BtvEvent currentEvent, int winID)
    {
        m_ContextualWindowPrefabs = Resources.Load("Prefabs/EventOptions", typeof(GameObject)) as GameObject;

        EventOfInterest = new BtvEvent(currentEvent);
        ParentWindowIndex = winID;
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                EventsToTraceMessage message = new EventsToTraceMessage
                {
                    TaskToExecute = 5,
                    Event = EventOfInterest,
                    ParentWindowIndex = ParentWindowIndex
                };
                Messenger.Default.Send(message, MessageContext.EventsToTraceMessage);
                break;
            case PointerEventData.InputButton.Right:
                OpenContextualMenu();
                break;
        }
    }

    private void OpenContextualMenu()
    {
        Transform parent = GameObject.Find("Trace" + (ParentWindowIndex + 1) + "Window").transform;
        m_ContextualMenuWindow = Instantiate(m_ContextualWindowPrefabs, parent);
        EventOptions eventOptions = m_ContextualMenuWindow.GetComponent<EventOptions>();
        eventOptions.Init(EventOfInterest, ParentWindowIndex);
    }
}