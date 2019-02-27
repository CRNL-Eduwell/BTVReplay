using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public delegate void eventValidated(TraceEvent validEvent);
public delegate void eventModifValidated(TraceEvent validEvent);
public delegate void imDying();

public class EventInfoEdit : MonoBehaviour
{
    public event eventValidated eventValid;
    public event eventsToDelete eventsToDelete;
    public event eventModifValidated eventModifed;
    public event imDying aaaagh;

    Text timeText = null;
    InputField codeInputField = null;
    InputField durationInputField = null;
    InputField commentInputField = null;
    Button saveButton = null;
    Button delButton = null;
    Button closeButton = null;

    TraceEvent myCurrentEvent = null;

    public void init(TraceEvent clickedEvent, TraceEvent memoryEvent, bool isModif)
    {
        myCurrentEvent = new TraceEvent(clickedEvent);

        timeText = transform.GetChild(0).GetChild(1).GetComponent<Text>();
        codeInputField = transform.GetChild(2).GetChild(1).GetComponent<InputField>();
        durationInputField = transform.GetChild(2).GetChild(3).GetComponent<InputField>();
        commentInputField = transform.GetChild(4).GetChild(1).GetComponent<InputField>();
        saveButton = transform.GetChild(6).GetChild(0).GetComponent<Button>();
        delButton = transform.GetChild(6).GetChild(1).GetComponent<Button>();
        closeButton = transform.GetChild(6).GetChild(2).GetComponent<Button>();

        //int timeInSec = myCurrentEvent.sample / 64;
        int timeInSec = (int)myCurrentEvent.timeSeconds();
        int h = timeInSec / 3600;
        int m = (timeInSec / 60) % 60;
        int s = timeInSec % 60;

        if (h > 0)
            timeText.text = returnTimeString(h) + ":" + returnTimeString(m) + ":" + returnTimeString(s);
        else
            timeText.text = "00:" + returnTimeString(m) + ":" + returnTimeString(s);

        if (memoryEvent != null && !isModif)
            initValueUI(memoryEvent);
        else if(memoryEvent != null)
            initValueUI(myCurrentEvent);

        saveButton.onClick.AddListener(() => 
        {
            checkEventIntegrity();
            if (!isModif)
            {
                eventValid(myCurrentEvent);
            }
            else
            {
                eventModifed(myCurrentEvent);
            }

            Destroy(gameObject);
        });

        delButton.onClick.AddListener(() => 
        {
            ApplicationState.displayConfirmation("Event Deletion", "Are You Sure You Want To Delete This Event ?", deleteAction, cancelAction);
        });

        closeButton.onClick.AddListener(() =>
        {
            Destroy(gameObject);
        });
    }

    void OnDestroy()
    {
        aaaagh();
        saveButton.onClick.RemoveAllListeners();
        delButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();
    }

    void initValueUI(TraceEvent currentEvent)
    {
        codeInputField.text = currentEvent.code.ToString();
        durationInputField.text = currentEvent.duration.ToString();
        commentInputField.text = currentEvent.comment;
    }

    string returnTimeString(int time)
    {
        if (time < 10)
        {
            return "0" + time;
        }
        else
        {
            return time.ToString();
        }
    }

    void checkEventIntegrity()
    {
        int codeValue = 0;
        if (int.TryParse(codeInputField.text, out codeValue))
            myCurrentEvent.code = codeValue;
        else
            myCurrentEvent.code = 0;

        myCurrentEvent.comment = commentInputField.text;
        myCurrentEvent.duration = int.Parse(durationInputField.text);
    }

    void deleteAction()
    {
        eventsToDelete(myCurrentEvent, 0);
        Destroy(gameObject);
    }

    void cancelAction()
    {
        Destroy(gameObject);
    }
}
