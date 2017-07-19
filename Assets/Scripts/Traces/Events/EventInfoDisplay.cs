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
    Text elecText = null;
    Text codeText = null;
    Text durationText = null;
    Text commentText = null;
    Button editEventButton = null;
    Button closeButton = null;

    eventEeg myCurrentEvent = null;

    public void init(eventEeg clickedEvent)
    {
        myCurrentEvent = new eventEeg(clickedEvent);

        timeText = transform.GetChild(0).GetChild(1).GetComponent<Text>();
        elecText = transform.GetChild(0).GetChild(3).GetComponent<Text>();
        codeText = transform.GetChild(0).GetChild(5).GetComponent<Text>();
        durationText = transform.GetChild(0).GetChild(7).GetComponent<Text>();
        commentText = transform.GetChild(0).GetChild(9).GetComponent<Text>();
        editEventButton = transform.GetChild(0).GetChild(10).GetComponent<Button>();
        closeButton = transform.GetChild(0).GetChild(11).GetComponent<Button>();

        //int timeInSec = myCurrentEvent.sample / 64;
        int timeInSec = (int)myCurrentEvent.getTimeSec();
        int h = timeInSec / 3600;
        int m = (timeInSec / 60) % 60;
        int s = timeInSec % 60;

        if (h > 0)
            timeText.text = returnTimeString(h) + ":" + returnTimeString(m) + ":" + returnTimeString(s);
        else
            timeText.text = "00:" + returnTimeString(m) + ":" + returnTimeString(s);

        elecText.text = myCurrentEvent.elecOfInterest;
        codeText.text = myCurrentEvent.code.ToString();
        durationText.text = myCurrentEvent.duration.ToString();
        commentText.text = myCurrentEvent.comment;

        editEventButton.onClick.AddListener(() =>
        {
            editEvent(myCurrentEvent);
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
        editEventButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();
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
}
