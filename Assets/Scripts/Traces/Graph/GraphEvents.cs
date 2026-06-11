using UnityEngine;
using System.Collections.Generic;
using BTV.Services.EventsService;
using BTV.Data;

public class GraphEvents : MonoBehaviour
{
    public bool DisplayEvents
    {
        get
        {
            return m_DisplayEvents;
        }
        set
        {
            if (value == false)
                HideActiveEvents();
            BtvLog.Log("Setting displa " + value);
            m_DisplayEvents = value;
        }
    }

    [SerializeField] Transform m_eventHolder = null;
    private GameObject m_EventZeroDurationPrefab = null, m_EventNonZeroDurationPrefab = null;
    private RectTransform m_parentRectTransform = null;
    private Trace m_parent = null;
    private List<GameObject> m_EventsAdded = new List<GameObject>();
    private bool m_DisplayEvents = true;

    public void init(Trace parentWin)
    {
        m_parent = parentWin;
        m_EventZeroDurationPrefab = Resources.Load("Prefabs/Trace-Event", typeof(GameObject)) as GameObject;
        m_EventNonZeroDurationPrefab = Resources.Load("Prefabs/Trace-Event2", typeof(GameObject)) as GameObject;
        m_parentRectTransform = gameObject.transform.parent.GetComponent<RectTransform>();
    }

    public void AddEventToTrace(BtvEvent currentEvent, int id)
    {
        BtvLog.Log("Add event to trace");
        GameObject currentEventToAdd = null;
        if (currentEvent.Duration == 0)
            currentEventToAdd = Instantiate(m_EventZeroDurationPrefab);
        else
            currentEventToAdd = Instantiate(m_EventNonZeroDurationPrefab);

        currentEventToAdd.name = "Event - " + currentEvent.TimeInSeconds;
        currentEventToAdd.transform.SetParent(m_eventHolder);
        currentEventToAdd.transform.localScale = new Vector3(1, 1, 1);
        currentEventToAdd.transform.SetSiblingIndex(id);
        m_EventsAdded.Insert(id, currentEventToAdd);

        if (currentEvent.Duration == 0)
        {
            currentEventToAdd.GetComponent<EventTrace>().Init(currentEvent, m_parent.TraceId);
        }
        else
        {
            currentEventToAdd.GetComponent<EventWithDuration>().Initialize(currentEvent, m_parent.TraceId);
        }
    }

    public void DeleteEventFromTrace(int IndexToDelete)
    {
        if (IndexToDelete < m_EventsAdded.Count)
        {
            GameObject ToRemove = m_EventsAdded[IndexToDelete].gameObject;
            m_EventsAdded.RemoveAt(IndexToDelete);
            Destroy(ToRemove);
        }
    }

    public void UpdateEventsOnTrace(int milliSecToLook)
    {
        int EventCount = EventsService.Events.Count;
        if (EventCount > 0)
        {
            float samplingFreq = TracesService.SamplingFrequency(m_parent.TraceId);
            int PeriodInSeconds = TracesService.WindowInSeconds(m_parent.TraceId);
            float widthOfGameObject = m_parent.gameObject.transform.GetComponent<RectTransform>().rect.width - 10; //TODO : do we get some way of getting that from parent or not
            float horizontalScale = widthOfGameObject / TracesService.GetOptionsFor(m_parent.TraceId).NumberOfPoint;

            int left = milliSecToLook - (PeriodInSeconds * 1000);
            int right = milliSecToLook;

            HideActiveEvents();
            if (DisplayEvents)
            {
                //We get all relevant events
                List<int> idOverFlow = EventsService.GetEventIdsBiggerThanWindow(left, right);
                List<int> idRightEnter = EventsService.GetEventIdsEnteringWindow(left, right);
                List<int> idInside = EventsService.GetEventIdsInsideWindow(left, right);
                List<int> idLeftEnter = EventsService.GetEventIdsExitingWindow(left, right);

                //Then we display
                float sizeV = m_parentRectTransform.rect.height - 10;

                for (int i = 0; i < idRightEnter.Count; i++)
                {
                    float positionInsideRect = (((left - EventsService.Events[idRightEnter[i]].TimeInMilliSeconds) * samplingFreq) / 1000) * -horizontalScale + ((-widthOfGameObject / 2) + 1);
                    float rightevent = right - EventsService.Events[idRightEnter[i]].TimeInMilliSeconds;
                    float size = (rightevent / (right - left)) * widthOfGameObject;

                    m_EventsAdded[idRightEnter[i]].transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
                    m_EventsAdded[idRightEnter[i]].transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sizeV);
                    m_EventsAdded[idRightEnter[i]].SetActive(true);
                    m_EventsAdded[idRightEnter[i]].transform.localPosition = new Vector3(positionInsideRect, 0, -3);
                    EventWithDuration tf = m_EventsAdded[idRightEnter[i]].transform.GetComponent<EventWithDuration>();
                    if(tf != null) tf.UpdateTfMap(left, right);
                }

                for (int i = 0; i < idInside.Count; i++)
                {
                    float positionInsideRect = (((left - EventsService.Events[idInside[i]].TimeInMilliSeconds) * samplingFreq) / 1000) * -horizontalScale + ((-widthOfGameObject / 2) + 1);
                    float size = ((float)EventsService.Events[idInside[i]].Duration / (right - left)) * widthOfGameObject;

                    if (EventsService.Events[idInside[i]].Duration > 0)
                    {
                        m_EventsAdded[idInside[i]].transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
                    }

                    m_EventsAdded[idInside[i]].transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sizeV);
                    m_EventsAdded[idInside[i]].SetActive(true);
                    m_EventsAdded[idInside[i]].transform.localPosition = new Vector3(positionInsideRect, 0, -3);
                    EventWithDuration tf = m_EventsAdded[idInside[i]].transform.GetComponent<EventWithDuration>();
                    if (tf != null) tf.UpdateTfMap(left, right);
                }

                for (int i = 0; i < idLeftEnter.Count; i++)
                {
                    float positionInsideRect = (-widthOfGameObject / 2) + 1;
                    float leftevent = (EventsService.Events[idLeftEnter[i]].TimeInMilliSeconds + EventsService.Events[idLeftEnter[i]].Duration) - left;
                    float size = (leftevent / (right - left)) * widthOfGameObject;

                    m_EventsAdded[idLeftEnter[i]].transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
                    m_EventsAdded[idLeftEnter[i]].transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sizeV);
                    m_EventsAdded[idLeftEnter[i]].SetActive(true);
                    m_EventsAdded[idLeftEnter[i]].transform.localPosition = new Vector3(positionInsideRect, 0, -3);
                    EventWithDuration tf = m_EventsAdded[idLeftEnter[i]].transform.GetComponent<EventWithDuration>();
                    if (tf != null) tf.UpdateTfMap(left, right);
                }

                for (int i = 0; i < idOverFlow.Count; i++)
                {
                    float positionInsideRect = (-widthOfGameObject / 2) + 1;
                    float size = widthOfGameObject;
                    m_EventsAdded[idOverFlow[i]].transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
                    m_EventsAdded[idOverFlow[i]].transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sizeV);
                    m_EventsAdded[idOverFlow[i]].SetActive(true);
                    m_EventsAdded[idOverFlow[i]].transform.localPosition = new Vector3(positionInsideRect, 0, -3);
                    EventWithDuration tf = m_EventsAdded[idOverFlow[i]].transform.GetComponent<EventWithDuration>();
                    if (tf != null) tf.UpdateTfMap(left, right);
                }
            }
        }
    }

    private void HideActiveEvents()
    {
        List<GameObject> activeObj = m_EventsAdded.FindAll(x => x.activeSelf == true);
        if (activeObj.Count > 0)
        {
            for (int i = 0; i < activeObj.Count; i++)
            {
                activeObj[i].SetActive(false);
            }
        }
    }
}