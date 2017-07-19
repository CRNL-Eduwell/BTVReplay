using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EventTrace : MonoBehaviour, IPointerClickHandler
{
    public event eventsToDisplay eventsToDisplay;
    public event eventsToDelete eventsToDelete;

    GameObject actionEventClick = null;
    eventEeg myEvent = null;
    int parentWinID = -2;

    GameObject choiceWin = null;
    Button editButton = null;
    Button deleteButton = null;
    Button closeButton = null;

    public void init(eventEeg currentEvent, int winID)
    {
        actionEventClick = Resources.Load("Prefabs/EventOptions", typeof(GameObject)) as GameObject;

        myEvent = new eventEeg(currentEvent);
        parentWinID = winID;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                eventsToDisplay(myEvent, parentWinID);
                break;
            case PointerEventData.InputButton.Right:
                openChoiceOption();
                break;
        }
    }

    public void UpdateEvent(eventEeg modifyedEvent)
    {
        myEvent = new eventEeg(modifyedEvent);
    }

    public void openChoiceOption()
    {
        choiceWin = Instantiate(actionEventClick);
        choiceWin.transform.SetParent(GameObject.Find("Trace" + (parentWinID + 1) + "Window").transform);
        choiceWin.transform.localScale = new Vector3(1, 1, 1);
        choiceWin.transform.localPosition = new Vector3(0, 0, -402);

        editButton = choiceWin.transform.GetChild(0).GetChild(0).GetComponent<Button>();
        deleteButton = choiceWin.transform.GetChild(0).GetChild(1).GetComponent<Button>();
        closeButton = choiceWin.transform.GetChild(0).GetChild(2).GetComponent<Button>();

        editButton.onClick.AddListener(choiceEdit);
        deleteButton.onClick.AddListener(choiceDelete);
        closeButton.onClick.AddListener(choiceClose);
    }

    void choiceEdit()
    {
        eventsToDisplay(myEvent, parentWinID);
        editButton.onClick.RemoveAllListeners();
        deleteButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();
        Destroy(choiceWin);
    }

    void choiceDelete()
    {
        eventsToDelete(myEvent, parentWinID);
        editButton.onClick.RemoveAllListeners();
        deleteButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();
        Destroy(choiceWin);
    }

    void choiceClose()
    {
        editButton.onClick.RemoveAllListeners();
        deleteButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();
        Destroy(choiceWin);
    }

    public void deleteMe()
    {
        eventsToDelete(myEvent, parentWinID);
    }
}