using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using BTV.Data;

public class EventTrace : MonoBehaviour, IPointerClickHandler
{
    public BtvEvent EventOfInterest { get; private set; } = null;
    public int ParentWindowIndex { get; private set; } = -1;

    private GameObject m_ContextualWindowPrefabs = null;
    private GameObject m_ContextualMenuWindow = null;
    private Button m_EditEvent = null;
    private Button m_DeleteEvent = null;
    private Button m_CloseWindow = null;

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

        m_EditEvent = m_ContextualMenuWindow.transform.GetChild(0).GetChild(0).GetComponent<Button>();
        m_DeleteEvent = m_ContextualMenuWindow.transform.GetChild(0).GetChild(1).GetComponent<Button>();
        m_CloseWindow = m_ContextualMenuWindow.transform.GetChild(0).GetChild(2).GetComponent<Button>();

        m_EditEvent.onClick.AddListener(EditEvent);
        m_DeleteEvent.onClick.AddListener(DeleteEvent);
        m_CloseWindow.onClick.AddListener(CloseContextualMenu);
    }

    private void EditEvent()
    {
        EventsModificationMessage message = new EventsModificationMessage
        {
            TaskToExecute = 3,
            Event = EventOfInterest,
            ParentWindowIndex = ParentWindowIndex
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
            Event = EventOfInterest
        };
        Messenger.Default.Send(message, MessageContext.EventsModificationMessage);
        CloseContextualMenu();
    }

    private void CloseContextualMenu()
    {
        m_EditEvent.onClick.RemoveAllListeners();
        m_DeleteEvent.onClick.RemoveAllListeners();
        m_CloseWindow.onClick.RemoveAllListeners();
        Destroy(m_ContextualMenuWindow);
    }
}