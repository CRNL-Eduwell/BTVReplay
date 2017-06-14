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
    public event eventModifValidated eventModifed;
    public event imDying aaaagh;

    Text timeText = null;
    InputField codeInputField = null;
    InputField commentInputField = null;
    Button saveButton = null;
    Button delButton = null;

    eventEeg myCurrentEvent = null;

	public void init(eventEeg clickedEvent, bool isModif)
    {
        myCurrentEvent = new eventEeg(clickedEvent);

        timeText = transform.GetChild(0).GetChild(1).GetComponent<Text>();
        codeInputField = transform.GetChild(0).GetChild(3).GetComponent<InputField>();
        commentInputField = transform.GetChild(0).GetChild(5).GetComponent<InputField>();
        saveButton = transform.GetChild(0).GetChild(6).GetComponent<Button>();
        delButton = transform.GetChild(0).GetChild(7).GetComponent<Button>();

        int timeInSec = myCurrentEvent.sample / 64;
        int h = timeInSec / 3600;
        int m = (timeInSec / 60) % 60;
        int s = timeInSec % 60;

        if (h > 0)
            timeText.text = h + ":" + m + ":" + s;
        else
            timeText.text = m + ":" + s;
        codeInputField.text = myCurrentEvent.code.ToString();
        commentInputField.text = myCurrentEvent.comment;

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

        delButton.onClick.AddListener(() => { });
    }

    void OnDestroy()
    {
        aaaagh();
        saveButton.onClick.RemoveAllListeners();
        delButton.onClick.RemoveAllListeners();
    }

    void checkEventIntegrity()
    {
        int codeValue = 0;
        if (int.TryParse(codeInputField.text, out codeValue))
            myCurrentEvent.code = codeValue;
        else
            myCurrentEvent.code = 0;

        myCurrentEvent.comment = commentInputField.text;
    }

}
