using UnityEngine;
using System.Collections.Generic;
using BTV.Services.EventsService;
using BTV.Services;
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
    private Session m_Session = null;
    private List<GameObject> m_EventsAdded = new List<GameObject>();
    private bool m_DisplayEvents = true;

    // Reused every tick by UpdateEventsOnTrace so the per-frame event-window query allocates nothing.
    private readonly List<int> m_IdOverFlow = new List<int>();
    private readonly List<int> m_IdRightEnter = new List<int>();
    private readonly List<int> m_IdInside = new List<int>();
    private readonly List<int> m_IdLeftEnter = new List<int>();

    public void init(Trace parentWin, Session session)
    {
        m_parent = parentWin;
        m_Session = session;
        m_EventZeroDurationPrefab = Resources.Load("Prefabs/Trace-Event", typeof(GameObject)) as GameObject;
        m_EventNonZeroDurationPrefab = Resources.Load("Prefabs/Trace-Event2", typeof(GameObject)) as GameObject;
        m_parentRectTransform = gameObject.transform.parent.GetComponent<RectTransform>();
    }

    public void AddEventToTrace(BtvEvent currentEvent, int id)
    {
        if (id < 0 || id > m_EventsAdded.Count)
        {
            // The insert index comes from EventsService.Events; if it does not fit this trace's
            // list the two are already out of sync - rebuild from the service instead of throwing.
            Debug.LogWarning("GraphEvents: event insert index " + id + " does not fit the trace list (" + m_EventsAdded.Count + " events), rebuilding from EventsService.");
            RebuildEventsFromService();
            return;
        }

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
            currentEventToAdd.GetComponent<EventWithDuration>().Initialize(currentEvent, m_parent.TraceId, m_Session);
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
        // m_EventsAdded must stay index-parallel with EventsService.Events. If any upstream
        // add/delete/load path desynced them (an exception mid-add, a reset or an events-file
        // reload that this trace never heard about), rebuild from the service instead of
        // indexing out of range on every video tick.
        if (m_EventsAdded.Count != m_Session.Events.Count)
        {
            Debug.LogWarning("GraphEvents: trace event objects out of sync with its session (" + m_EventsAdded.Count + " vs " + m_Session.Events.Count + "), rebuilding.");
            RebuildEventsFromService();
        }

        int EventCount = m_Session.Events.Count;
        if (EventCount > 0)
        {
            float samplingFreq = TracesService.SamplingFrequency(m_Session, m_parent.TraceId);
            int PeriodInSeconds = TracesService.WindowInSeconds(m_Session, m_parent.TraceId);
            float widthOfGameObject = m_parent.gameObject.transform.GetComponent<RectTransform>().rect.width - 10; //TODO : do we get some way of getting that from parent or not
            float horizontalScale = widthOfGameObject / TracesService.GetOptionsFor(m_Session, m_parent.TraceId).NumberOfPoint;

            int left = milliSecToLook - (PeriodInSeconds * 1000);
            int right = milliSecToLook;

            HideActiveEvents();
            if (DisplayEvents)
            {
                //We get all relevant events in a single allocation-free pass over reused buffers
                EventsService.CollectEventIdsForWindow(m_Session, left, right, m_IdOverFlow, m_IdRightEnter, m_IdInside, m_IdLeftEnter);
                List<int> idOverFlow = m_IdOverFlow;
                List<int> idRightEnter = m_IdRightEnter;
                List<int> idInside = m_IdInside;
                List<int> idLeftEnter = m_IdLeftEnter;

                //Then we display
                float sizeV = m_parentRectTransform.rect.height - 10;

                for (int i = 0; i < idRightEnter.Count; i++)
                {
                    float positionInsideRect = (((left - m_Session.Events[idRightEnter[i]].TimeInMilliSeconds) * samplingFreq) / 1000) * -horizontalScale + ((-widthOfGameObject / 2) + 1);
                    float rightevent = right - m_Session.Events[idRightEnter[i]].TimeInMilliSeconds;
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
                    float positionInsideRect = (((left - m_Session.Events[idInside[i]].TimeInMilliSeconds) * samplingFreq) / 1000) * -horizontalScale + ((-widthOfGameObject / 2) + 1);
                    float size = ((float)m_Session.Events[idInside[i]].Duration / (right - left)) * widthOfGameObject;

                    if (m_Session.Events[idInside[i]].Duration > 0)
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
                    float leftevent = (m_Session.Events[idLeftEnter[i]].TimeInMilliSeconds + m_Session.Events[idLeftEnter[i]].Duration) - left;
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

    private void RebuildEventsFromService()
    {
        for (int i = 0; i < m_EventsAdded.Count; i++)
        {
            Destroy(m_EventsAdded[i]);
        }
        m_EventsAdded.Clear();

        for (int i = 0; i < m_Session.Events.Count; i++)
        {
            AddEventToTrace(m_Session.Events[i], i);
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
