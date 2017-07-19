using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public delegate void eventValidated(eventEeg validEvent);
public delegate void eventModifValidated(eventEeg validEvent);
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

    eventEeg myCurrentEvent = null;

	public void init(eventEeg clickedEvent, eventEeg memoryEvent, bool isModif)
    {
        myCurrentEvent = new eventEeg(clickedEvent);

        timeText = transform.GetChild(0).GetChild(1).GetComponent<Text>();
        codeInputField = transform.GetChild(0).GetChild(3).GetComponent<InputField>();
        durationInputField = transform.GetChild(0).GetChild(5).GetComponent<InputField>();
        commentInputField = transform.GetChild(0).GetChild(7).GetComponent<InputField>();
        saveButton = transform.GetChild(0).GetChild(8).GetComponent<Button>();
        delButton = transform.GetChild(0).GetChild(9).GetComponent<Button>();
        closeButton = transform.GetChild(0).GetChild(10).GetComponent<Button>();

        //int timeInSec = myCurrentEvent.sample / 64;
        int timeInSec = (int)myCurrentEvent.getTimeSec();
        int h = timeInSec / 3600;
        int m = (timeInSec / 60) % 60;
        int s = timeInSec % 60;

        if (h > 0)
            timeText.text = returnTimeString(h) + ":" + returnTimeString(m) + ":" + returnTimeString(s);
        else
            timeText.text = "00:" + returnTimeString(m) + ":" + returnTimeString(s);

        if (memoryEvent != null && !isModif)
            initValueUI(memoryEvent);
        else
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
            eventsToDelete(myCurrentEvent, 0);
            Destroy(gameObject);
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

    void initValueUI(eventEeg currentEvent)
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
}
