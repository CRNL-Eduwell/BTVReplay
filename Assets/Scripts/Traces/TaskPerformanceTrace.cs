using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BTV.Services.TaskPerformanceService;
using UnityEngine;
using UnityEngine.UI;

public class TaskPerformanceTrace : MonoBehaviour
{
    /// <summary>
    /// Object containing all the barplots to display
    /// </summary>
    [SerializeField]
    private RectTransform m_TaskBarHolder = null;
    /// <summary>
    /// Object that will contain the event picture
    /// </summary>
    [SerializeField]
    private GameObject m_EventPicture = null;
    /// <summary>
    /// RawImage attached to m_EventPicture to load/unload pictures
    /// </summary>
    [SerializeField]
    private RawImage m_EventRawImage = null;
    /// <summary>
    /// Videoplayer for timing informations
    /// </summary>
    [SerializeField]
    private CustomVideoPlayer video = null;

    private Trace m_signalWindow1 = null;

    private TriggerBarplot m_TriggerBarplotPrefabs = null;
    private List<TriggerBarplot> m_Triggers = new List<TriggerBarplot>();

    private Texture2D m_DefaultEventPicturePrefabs = null;
    private List<Texture2D> m_EventPictures = new List<Texture2D>();
    private List<int> m_EventMainCodes = new List<int>();

    private bool m_HasDataToDisplay = false;
    private int m_PeriopdInSec = 10;
    private int m_NumberOfPoint = 64 * 10;
    private float m_HorizontalScale = 0;
    private float m_VerticalScale = 0;

    private void Awake()
    {
        m_TriggerBarplotPrefabs = Resources.Load("Prefabs/PerfTrace", typeof(TriggerBarplot)) as TriggerBarplot;
        m_DefaultEventPicturePrefabs = Resources.Load("Pictures/EventDefault", typeof(Texture2D)) as Texture2D;

        m_signalWindow1 = GameObject.Find("Trace1Window").GetComponent<Trace>();

        video.sendTime += UpdateSpawn;
        video.sendTime += UpdatePicEvent;
        Messenger.Default.Register<UiToTaskPerformanceMessage>(this, OnUiToTaskPerformanceMessage, MessageContext.UiToTaskPerformanceMessage);
    }

    private void OnDestroy()
    {
        video.sendTime -= UpdateSpawn;
        video.sendTime -= UpdatePicEvent;
        Messenger.Default.Unregister(this, MessageContext.UiToTaskPerformanceMessage);
    }

    private void OnRectTransformDimensionsChange()
    {
        UpdateScales();
    }

    private void OnUiToTaskPerformanceMessage(UiToTaskPerformanceMessage message)
    {
        switch (message.TaskToExecute)
        {
            case 0:
                UpdateEventsForProtocol(message.NewProtocol);
                UpdateScales();
                break;
            case 1:
                UpdateTimeResolution(message.TimeWindow);
                UpdateScales();
                break;
        }
    }

    private void UpdateEventsForProtocol(ProvFile protocol)
    {
        UnityEngine.Debug.Log("Update Protocol Events");
        m_HasDataToDisplay = false;
        TaskPerformanceService.ProcessEventsForExperiment(protocol);
        UpdateEvents(protocol);
        UpdateProtocolPicturesAndCodes(protocol);
        m_HasDataToDisplay = (TaskPerformanceService.ProcessedTriggers.Count == 0) ? false : true;
    }

    private void UpdateEvents(ProvFile protocol)
    {
        UnityEngine.Debug.Log("Update Events");

        int TriggerCount = TaskPerformanceService.ProcessedTriggers.Count;
        UnityEngine.Debug.Log("Update Events " + TriggerCount);

        for (int i = 0; i < TriggerCount; i++)
        {
            TriggerBarplot trigger = Instantiate(m_TriggerBarplotPrefabs, m_TaskBarHolder); //instancier avec parent dzans les paramètres
            trigger.Trigger = TaskPerformanceService.ProcessedTriggers[i];

            //trigger.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
            //trigger.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0);
            //trigger.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
            //trigger.transform.localPosition = new Vector3(0, 0, 0);
            //trigger.name = "Perf" + (m_Triggers.Count);

            //trigger.transform.GetComponent<LineRenderer>().SetPosition(0, new Vector3(i, 0, -2));
            //trigger.transform.GetComponent<LineRenderer>().SetPosition(1, new Vector3(i, 100, -2));
            trigger.UpdatePosition(0, i, 0, -2);
            trigger.UpdatePosition(0, i, 100, -2);

            //trigger.transform.localScale = new Vector3(1, 1, 1);

            trigger.gameObject.SetActive(false);
            m_Triggers.Add(trigger);
        }
    }

    private void UpdateProtocolPicturesAndCodes(ProvFile protocol)
    {
        UnityEngine.Debug.Log("Update Protocol Pics and code");

        for (int i = 0; i < protocol.blocs.Count; i++)
        {
            if (File.Exists(protocol.blocs[i].dispBloc.path))
            {
                Texture2D current = new Texture2D(256, 256);
                current.LoadImage(File.ReadAllBytes(protocol.blocs[i].dispBloc.path));

                m_EventPictures.Add(current);
            }
            else
            {
                m_EventPictures.Add(m_DefaultEventPicturePrefabs);
            }
            m_EventMainCodes.Add(protocol.blocs[i].mainEvent.code);
        }
    }

    private void UpdateTimeResolution(int periodInSecond)
    {
        m_PeriopdInSec = periodInSecond;
        m_NumberOfPoint = m_signalWindow1.TraceEeg.SamplingFrequency * m_PeriopdInSec;
    }

    private void UpdateScales()
    {
        if (!m_HasDataToDisplay)
            return;

        int MaxReactionTime = 0;
        for (int i = 0; i < m_Triggers.Count; i++)
        {
            int currentRtMs = m_Triggers[i].Trigger.ReactionTimeInMs(64);
            if (currentRtMs > MaxReactionTime)
                MaxReactionTime = currentRtMs;
        }
        m_HorizontalScale = m_TaskBarHolder.rect.width / m_NumberOfPoint;
        m_VerticalScale = m_TaskBarHolder.rect.height / MaxReactionTime;
    }

    private void UpdateSpawn(int milliSecToLook)
    {
        if (!m_HasDataToDisplay)
            return;

        int leftTime = (int)(milliSecToLook * ((float)m_signalWindow1.TraceEeg.SamplingFrequency / 1000)) - m_NumberOfPoint;
        int rightTime = (int)(milliSecToLook * ((float)m_signalWindow1.TraceEeg.SamplingFrequency / 1000));

        List<int> currentIndex = m_Triggers.Select((item, index) => new { Item = item, Index = index })
                                                         .Where(x => x.Item.Trigger.Response.Sample > leftTime && x.Item.Trigger.Response.Sample < rightTime)
                                                         .Select(x => x.Index)
                                                         .ToList();
        DeactivateSpawn();
        if (currentIndex.Count != 0)
        {
            for (int i = 0; i < currentIndex.Count; i++)
            {
                float posiionSample = (leftTime - m_Triggers[currentIndex[i]].Trigger.Response.Sample);
                float positionInsideRect = posiionSample * -m_HorizontalScale;

                if (m_Triggers[currentIndex[i]].Trigger.Response.Sample <= rightTime)
                {
                    m_Triggers[currentIndex[i]].gameObject.SetActive(true);
                    m_Triggers[currentIndex[i]].UpdatePosition(0, positionInsideRect, 5, -2);

                    float value = m_VerticalScale * (m_Triggers[currentIndex[i]].Trigger.ReactionTimeInMs() - 750);
                    m_Triggers[currentIndex[i]].UpdatePosition(1, positionInsideRect, Mathf.Abs(value), -2);
                }
            }
        }
    }

    private void DeactivateSpawn()
    {
        List<TriggerBarplot> ActiveObjects = m_Triggers.FindAll(x => x.gameObject.activeSelf == true);
        if (ActiveObjects.Count > 0)
        {
            for (int i = 0; i < ActiveObjects.Count; i++)
                ActiveObjects[i].gameObject.SetActive(false);
        }
    }

    private void UpdatePicEvent(int milliSecToLook)
    {
        if (!m_HasDataToDisplay)
            return;

        int sampleToLook = (int)(milliSecToLook * ((float)m_signalWindow1.TraceEeg.SamplingFrequency / 1000));
        UnityEngine.Debug.Log(sampleToLook);
        int found = m_Triggers.FindIndex(x => x.Trigger.Trigger.Sample >= sampleToLook - 8 && x.Trigger.Trigger.Sample < sampleToLook + 8);
        UnityEngine.Debug.Log("found " + found);
        if (found == -1)
        {
            if (m_EventPicture.activeSelf == true)
                m_EventPicture.SetActive(false);

            return;
        }

        if (m_EventPicture.activeSelf == false)
        {
            m_EventPicture.SetActive(true);
            m_EventRawImage.texture = m_EventPictures[m_EventMainCodes.IndexOf(m_Triggers[found].Trigger.Trigger.Code)];
        }
    }
}
