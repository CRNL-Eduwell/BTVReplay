using System.Collections;
using System.Collections.Generic;
using BTV.Data;
using UnityEngine;
using UnityEngine.UI;

public class EventOptions : MonoBehaviour
{
    [SerializeField]
    private Button m_EditEvent = null;
    [SerializeField]
    private Button m_DeleteEvent = null;
    [SerializeField]
    private Button m_CloseWindow = null;

    private BtvEvent m_Event = null;
    private int m_ParentWindowIndex = -1;

    public void Init(BtvEvent btvEvent, int parentWindowIndex)
    {
        m_Event = btvEvent;
        m_ParentWindowIndex = parentWindowIndex;

        m_EditEvent.onClick.AddListener(EditEvent);
        m_DeleteEvent.onClick.AddListener(DeleteEvent);
        m_CloseWindow.onClick.AddListener(CloseContextualMenu);
    }

    private void EditEvent()
    {
        EventsModificationMessage message = new EventsModificationMessage
        {
            TaskToExecute = 3,
            Event = m_Event,
            ParentWindowIndex = m_ParentWindowIndex
        };
        Messenger.Default.Send(message, MessageContext.EventsModificationMessage);
        CloseContextualMenu();
    }

    private void DeleteEvent()
    {
        ApplicationState.displayConfirmation("Event Deletion", "Are You Sure You Want To Delete This Event ?", DeleteEventAndCloseContextualMenu, CloseContextualMenu);
    }

    private void DeleteEventAndCloseContextualMenu()
    {
        EventsModificationMessage message = new EventsModificationMessage
        {
            TaskToExecute = 2,
            Event = m_Event
        };
        Messenger.Default.Send(message, MessageContext.EventsModificationMessage);
        CloseContextualMenu();
    }

    private void CloseContextualMenu()
    {
        m_EditEvent.onClick.RemoveAllListeners();
        m_DeleteEvent.onClick.RemoveAllListeners();
        m_CloseWindow.onClick.RemoveAllListeners();
        Destroy(gameObject);
    }
}
