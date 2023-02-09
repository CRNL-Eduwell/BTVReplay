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

    public void Init(BtvEvent currentEvent, int winID)
    {
        EventOfInterest = new BtvEvent(currentEvent);
        ParentWindowIndex = winID;
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                {
                    //will be used later to handle margin manipulation of events
                    //with duration
                    break;
                }
            case PointerEventData.InputButton.Right:
                {
                    EventsToTraceMessage message = new EventsToTraceMessage
                    {
                        TaskToExecute = 5,
                        Event = EventOfInterest,
                        ParentWindowIndex = ParentWindowIndex
                    };
                    Messenger.Default.Send(message, MessageContext.EventsToTraceMessage);
                    break;
                }
        }
    }
}