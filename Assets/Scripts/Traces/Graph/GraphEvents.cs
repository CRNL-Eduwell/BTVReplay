using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GraphEvents : MonoBehaviour
{
    public List<GameObject> eventsAdded = new List<GameObject>();

    [SerializeField] optionsHub hub = null;
    [SerializeField] Transform m_eventHolder = null;

    GameObject m_traceEventClick = null, m_traceEventClick2 = null;
    RectTransform m_parentRectTransform = null;
    bool m_display = true;
    Trace m_parent = null;

    public void init(Trace parentWin)
    {
        m_parent = parentWin;
        m_traceEventClick = Resources.Load("Prefabs/Trace-Event", typeof(GameObject)) as GameObject;
        m_traceEventClick2 = Resources.Load("Prefabs/Trace-Event2", typeof(GameObject)) as GameObject;
        m_parentRectTransform = gameObject.transform.parent.GetComponent<RectTransform>();
    }

    public void showEvents(bool show)
    {
        m_display = show;
    }

    public void addEventToTrace(TraceEvent currentEvent, int id)
    {
        UnityEngine.Debug.Log("Add event to trace");
        GameObject currentEventToAdd = null;
        if (currentEvent.duration == 0)
            currentEventToAdd = Instantiate(m_traceEventClick);
        else
            currentEventToAdd = Instantiate(m_traceEventClick2);

        currentEventToAdd.name = "Event - " + currentEvent.sample;
        currentEventToAdd.transform.SetParent(m_eventHolder);
        currentEventToAdd.transform.localScale = new Vector3(1, 1, 1);
        currentEventToAdd.transform.SetSiblingIndex(id);
        eventsAdded.Insert(id, currentEventToAdd);

        currentEventToAdd.GetComponent<EventTrace>().init(currentEvent, m_parent.TraceId);
    }

    public void DeleteEventFromTrace(int IndexToDelete)
    {
        if(IndexToDelete < eventsAdded.Count)
            Destroy(eventsAdded[IndexToDelete].gameObject);
    }

    public void updateEventsDraw(int milliSecToLook)
    {
        if (false)//(hub.eventRemote.userEvents.Length > 0)
        {
            float samplingFreq = m_parent.TraceEeg.fileHandle.sampFreq;
            int numberPoint = m_parent.TraceEeg.numberOfPoint;
            float horizontalScale = m_parent.TraceEeg.horizontalScale;
            float widthOfGameObject = m_parent.TraceEeg.widthOfGameObject;

            int left = (int)(milliSecToLook * (samplingFreq / 1000)) - numberPoint;
            int right = (int)(milliSecToLook * ((float)samplingFreq / 1000));

            //var keys = new List<int>(hub.eventRemote.userEvents.Keys);
            //var values = new List<TraceEvent>(hub.eventRemote.userEvents.Values);

            List<int> idOverFlow = hub.eventRemote.userEvents.Select((item, index) => new { Item = item, Index = index })
                                         .Where(x => (x.Item.sample <= left && (x.Item.sample + (x.Item.duration * ((float)samplingFreq / 1000)) >= right)))
                                         .Select(x => x.Index)
                                         .ToList();

            List<int> idRightEnter = hub.eventRemote.userEvents.Select((item, index) => new { Item = item, Index = index })
                                           .Where(x => (x.Item.sample < right && x.Item.sample > left && (x.Item.sample + (x.Item.duration * ((float)samplingFreq / 1000)) >= right)))
                                           .Select(x => x.Index)
                                           .ToList();

            List<int> idInside = hub.eventRemote.userEvents.Select((item, index) => new { Item = item, Index = index })
                                       .Where(x => ((x.Item.sample < right) &&
                                                    (x.Item.sample > left) &&
                                                    (x.Item.sample + (x.Item.duration * ((float)x.Item.samplingFrequency / 1000)) >= left) &&
                                                    (x.Item.sample + (x.Item.duration * ((float)samplingFreq / 1000)) <= right)))
                                       .Select(x => x.Index)
                                       .ToList();

            List<int> idLeftEnter = hub.eventRemote.userEvents.Select((item, index) => new { Item = item, Index = index })
                                          .Where(x => ((x.Item.sample < left) &&
                                                       (x.Item.sample + (x.Item.duration * ((float)samplingFreq / 1000)) >= left) &&
                                                       (x.Item.sample + (x.Item.duration * ((float)samplingFreq / 1000)) <= right)))
                                          .Select(x => x.Index)
                                          .ToList();

            hideActiveEvents();
            if (m_display)
            {
                float sizeV = m_parentRectTransform.rect.height - 10;

                for (int i = 0; i < idRightEnter.Count; i++)
                {
                    float positionInsideRect = (left - hub.eventRemote.userEvents[idRightEnter[i]].sample) * -horizontalScale + ((-widthOfGameObject / 2) + 1);
                    float rightevent = right - hub.eventRemote.userEvents[idRightEnter[i]].sample;
                    float size = (rightevent / (right - left)) * widthOfGameObject;

                    eventsAdded[idRightEnter[i]].transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
                    eventsAdded[idRightEnter[i]].transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sizeV);
                    eventsAdded[idRightEnter[i]].SetActive(true);
                    eventsAdded[idRightEnter[i]].transform.localPosition = new Vector3(positionInsideRect, 0, -2);
                }

                for (int i = 0; i < idInside.Count; i++)
                {
                    float positionInsideRect = (left - hub.eventRemote.userEvents[idInside[i]].sample) * -horizontalScale + ((-widthOfGameObject / 2) + 1);
                    float size = ((hub.eventRemote.userEvents[idInside[i]].duration * ((float)samplingFreq / 1000)) / (right - left)) * widthOfGameObject;

                    if (hub.eventRemote.userEvents[idInside[i]].duration > 0)
                    {
                        eventsAdded[idInside[i]].transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
                        eventsAdded[idInside[i]].transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sizeV);
                        eventsAdded[idInside[i]].SetActive(true);
                        eventsAdded[idInside[i]].transform.localPosition = new Vector3(positionInsideRect, 0, -2);
                    }
                    else
                    {
                        eventsAdded[idInside[i]].SetActive(true);
                        eventsAdded[idInside[i]].transform.localPosition = new Vector3(positionInsideRect, m_parent.TraceEeg.dataArray[hub.eventRemote.userEvents[idInside[i]].sample - left].y, -201);
                    }
                }

                for (int i = 0; i < idLeftEnter.Count; i++)
                {
                    float positionInsideRect = ((-widthOfGameObject / 2) + 1);
                    float leftevent = (hub.eventRemote.userEvents[idLeftEnter[i]].sample + (hub.eventRemote.userEvents[idLeftEnter[i]].duration * ((float)samplingFreq / 1000)) - left);
                    float size = (leftevent / (right - left)) * widthOfGameObject;

                    eventsAdded[idLeftEnter[i]].transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
                    eventsAdded[idLeftEnter[i]].transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sizeV);
                    eventsAdded[idLeftEnter[i]].SetActive(true);
                    eventsAdded[idLeftEnter[i]].transform.localPosition = new Vector3(positionInsideRect, 0, -2);
                }

                for (int i = 0; i < idOverFlow.Count; i++)
                {
                    float positionInsideRect = ((-widthOfGameObject / 2) + 1);
                    float size = widthOfGameObject;
                    eventsAdded[idOverFlow[i]].transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
                    eventsAdded[idOverFlow[i]].transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sizeV);
                    eventsAdded[idOverFlow[i]].SetActive(true);
                    eventsAdded[idOverFlow[i]].transform.localPosition = new Vector3(positionInsideRect, 0, -2);
                }
            }
        }
    }

    public void hideActiveEvents()
    {
        List<GameObject> activeObj = eventsAdded.FindAll(x => x.activeSelf == true);
        if (activeObj.Count > 0)
        {
            for (int i = 0; i < activeObj.Count; i++)
            {
                activeObj[i].SetActive(false);
            }
        }
    }
}