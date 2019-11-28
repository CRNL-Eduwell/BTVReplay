using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using BTV.Data;

public class EventTrace : MonoBehaviour, IPointerClickHandler
{
    GameObject m_ContextualWindowPrefabs = null;
    BtvEvent m_Event = null;
    int parentWinID = -2;

    GameObject ContextualMenuWindow = null;
    Button m_EditEvent = null;
    Button m_DeleteEvent = null;
    Button m_CloseWindow = null;

    public void init(BtvEvent currentEvent, int winID)
    {
        m_ContextualWindowPrefabs = Resources.Load("Prefabs/EventOptions", typeof(GameObject)) as GameObject;

        m_Event = new BtvEvent(currentEvent);
        parentWinID = winID;
    }

    public void UpdateEvent(BtvEvent modifyedEvent)
    {
        m_Event = new BtvEvent(modifyedEvent);
    }

    public void DeleteMe()
    {
        EventsModificationMessage message = new EventsModificationMessage
        {
            TaskToExecute = 2,
            Event = m_Event
        };
        Messenger.Default.Send(message, MessageContext.EventsModificationMessage);
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                EventsToTraceMessage message = new EventsToTraceMessage
                {
                    TaskToExecute = 5,
                    Event = m_Event
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
        ContextualMenuWindow = Instantiate(m_ContextualWindowPrefabs);
        ContextualMenuWindow.transform.SetParent(GameObject.Find("Trace" + (parentWinID + 1) + "Window").transform);
        ContextualMenuWindow.transform.localScale = new Vector3(1, 1, 1);
        ContextualMenuWindow.transform.localPosition = new Vector3(0, 0, -402);

        m_EditEvent = ContextualMenuWindow.transform.GetChild(0).GetChild(0).GetComponent<Button>();
        m_DeleteEvent = ContextualMenuWindow.transform.GetChild(0).GetChild(1).GetComponent<Button>();
        m_CloseWindow = ContextualMenuWindow.transform.GetChild(0).GetChild(2).GetComponent<Button>();

        m_EditEvent.onClick.AddListener(EditEvent);
        m_DeleteEvent.onClick.AddListener(DeleteEvent);
        m_CloseWindow.onClick.AddListener(CloseContextualMenu);
    }

    private void EditEvent()
    {
        EventsModificationMessage message = new EventsModificationMessage
        {
            TaskToExecute = 3,
            Event = m_Event
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
        Destroy(ContextualMenuWindow);
    }
}