using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public delegate void eventToEditHandler(TraceEvent eventToEdit);
public delegate void calculateCorrelation(TraceEvent eventCorrelation);
public delegate void calculateCorrelation2D(TraceEvent eventCorrelation);
public delegate void eventModifPlot(TraceEvent modifyEvent, TraceEvent previousEvent);

public class EventInfoDisplay : MonoBehaviour
{
    public event eventToEditHandler editEvent;
    public event calculateCorrelation processCorrelation;
    public event calculateCorrelation2D processCorrelation2D;
    public event eventModifPlot eventModifed;
    public event imDying aaaagh;

    Text timeText = null;
    Dropdown elecText = null;
    Text codeText = null;
    Text durationText = null;
    Text commentText = null;
    Button editEventButton = null;
    Button correlationButton = null;
    Button correlation2DButton = null;
    Button closeButton = null;

    TraceEvent myCurrentEvent = null;
    BTVMedia media = null;
    VideoPlayer video = null;

    public void init(TraceEvent clickedEvent)
    {
        media = GameObject.Find("Canvas").transform.GetChild(3).GetComponent<BTVMedia>();
        video = GameObject.Find("Canvas").transform.GetChild(1).GetChild(1).GetComponent<VideoPlayer>();
        myCurrentEvent = new TraceEvent(clickedEvent);

        timeText = transform.GetChild(0).GetChild(1).GetComponent<Text>();
        elecText = transform.GetChild(0).GetChild(3).GetComponent<Dropdown>();
        codeText = transform.GetChild(2).GetChild(1).GetComponent<Text>();
        durationText = transform.GetChild(2).GetChild(3).GetComponent<Text>();
        commentText = transform.GetChild(4).GetChild(1).GetComponent<Text>();
        editEventButton = transform.GetChild(6).GetChild(0).GetComponent<Button>();
        correlationButton = transform.GetChild(6).GetChild(1).GetComponent<Button>();
        correlation2DButton = transform.GetChild(6).GetChild(2).GetComponent<Button>();
        closeButton = transform.GetChild(6).GetChild(3).GetComponent<Button>();

        //int timeInSec = myCurrentEvent.sample / 64;
        int timeInSec = (int)myCurrentEvent.timeSeconds();
        int h = timeInSec / 3600;
        int m = (timeInSec / 60) % 60;
        int s = timeInSec % 60;

        if (h > 0)
            timeText.text = returnTimeString(h) + ":" + returnTimeString(m) + ":" + returnTimeString(s);
        else
            timeText.text = "00:" + returnTimeString(m) + ":" + returnTimeString(s);


        int handleID = ELAN.returnFirstValidHandleId(media.elanFiles);
        int currentHandle = media.elanFiles[handleID].electrodes.ToList().FindIndex(x => x.name == myCurrentEvent.elecOfInterest);
        elecText.options.Clear();
        for (int i = 0; i < media.elanFiles[handleID].electrodes.Length; i++)
            elecText.options.Add(new Dropdown.OptionData(media.elanFiles[handleID].electrodes[i].name));

        if (video.audioWav != null)
        {
            if (video.audioWav.filterFileExist == true)
                elecText.options.Add(new Dropdown.OptionData("AUD"));
        }

        elecText.value = currentHandle;
        elecText.transform.GetChild(0).GetComponent<Text>().text = elecText.options[elecText.value].text;
        elecText.onValueChanged.AddListener((int id) =>
        {
            TraceEvent modifyEvent = new TraceEvent(myCurrentEvent);
            modifyEvent.elecOfInterest = elecText.options[id].text;
            eventModifed(modifyEvent, myCurrentEvent);
            myCurrentEvent = new TraceEvent(modifyEvent);
        });

        codeText.text = myCurrentEvent.code.ToString();
        durationText.text = myCurrentEvent.duration.ToString();
        commentText.text = myCurrentEvent.comment;

        editEventButton.onClick.AddListener(() =>
        {
            editEvent(myCurrentEvent);
            Destroy(gameObject);
        });

        correlationButton.onClick.AddListener(() =>
        {
            processCorrelation(myCurrentEvent);
        });

        correlation2DButton.onClick.AddListener(() =>
        {
            processCorrelation2D(myCurrentEvent);
        });

        closeButton.onClick.AddListener(() =>
        {
            Destroy(gameObject);
        });

        if (myCurrentEvent.duration > 0)
        {
            correlationButton.interactable = true;
            correlation2DButton.interactable = true;
        }
        else
        {
            correlationButton.interactable = false;
            correlation2DButton.interactable = false;
        }
    }

    void OnDestroy()
    {
        aaaagh();
        editEventButton.onClick.RemoveAllListeners();
        correlationButton.onClick.RemoveAllListeners();
        correlation2DButton.onClick.RemoveAllListeners();
        elecText.onValueChanged.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();
    }

    string returnTimeString(int time)
    {
        if (time < 10)
            return "0" + time;
        else
            return time.ToString();
    }
}
