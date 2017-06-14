using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public delegate void eventToEditHandler(eventEeg eventToEdit);

public class EventInfoDisplay : MonoBehaviour
{
    public event eventToEditHandler editEvent;
    public event imDying aaaagh;

    Text timeText = null;
    Text codeText = null;
    Text commentText = null;
    Button editEventButton = null;

    eventEeg myCurrentEvent = null;

    public void init(eventEeg clickedEvent)
    {
        myCurrentEvent = new eventEeg(clickedEvent);

        timeText = transform.GetChild(0).GetChild(1).GetComponent<Text>();
        codeText = transform.GetChild(0).GetChild(3).GetComponent<Text>();
        commentText = transform.GetChild(0).GetChild(5).GetComponent<Text>();
        editEventButton = transform.GetChild(0).GetChild(6).GetComponent<Button>();

        int timeInSec = myCurrentEvent.sample / 64;
        int h = timeInSec / 3600;
        int m = (timeInSec / 60) % 60;
        int s = timeInSec % 60;

        if (h > 0)
            timeText.text = h + ":" + m + ":" + s;
        else
            timeText.text = m + ":" + s;

        codeText.text = myCurrentEvent.code.ToString();
        commentText.text = myCurrentEvent.comment;

        editEventButton.onClick.AddListener(() =>
        {
            editEvent(myCurrentEvent);
            Destroy(gameObject);
        });
    }

    void OnDestroy()
    {
        aaaagh();
        editEventButton.onClick.RemoveAllListeners();
    }
}
