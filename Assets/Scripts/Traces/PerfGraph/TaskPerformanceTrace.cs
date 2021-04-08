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
    /// Gameobject used to display a message if events have been updated since
    /// the calculation of the task performance
    /// </summary>
    [SerializeField]
    private GameObject m_InfoDisplay = null;

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
    private int m_State = 1;

    public void UpdateWindowState(int state)
    {
        UnityEngine.Debug.Log("Updating Task performance Ui State");
        m_State = state;
        switch (m_State)
        {
            case 0:
                gameObject.SetActive(false);
                break;
            case 1:
                gameObject.SetActive(true);
                break;
            case 2:
                gameObject.SetActive(true);
                break;
        }
    }

    private void Awake()
    {
        m_TriggerBarplotPrefabs = Resources.Load("Prefabs/PerfTrace", typeof(TriggerBarplot)) as TriggerBarplot;
        m_DefaultEventPicturePrefabs = Resources.Load("Pictures/EventDefault", typeof(Texture2D)) as Texture2D;

        m_signalWindow1 = GameObject.Find("Trace1Window").GetComponent<Trace>();

        Messenger.Default.Register<UiToTaskPerformanceMessage>(this, OnUiToTaskPerformanceMessage, MessageContext.UiToTaskPerformanceMessage);
        Messenger.Default.Register<EventsToTaskPerformanceMessage>(this, OnEventsToTaskPerformanceMessage, MessageContext.EventsToTaskPerformanceMessage);
        Messenger.Default.Register<VideoToModulesMessage>(this, OnVideoToModulesMessage, MessageContext.VideoToModulesMessage);
    }

    private void OnDestroy()
    {
        Messenger.Default.Unregister(this, MessageContext.UiToTaskPerformanceMessage);
        Messenger.Default.Unregister(this, MessageContext.EventsToTaskPerformanceMessage);
        Messenger.Default.Unregister(this, MessageContext.VideoToModulesMessage);
    }

    private void OnRectTransformDimensionsChange()
    {
        if (m_State > 0)
        {
            gameObject.SetActive(gameObject.GetComponent<RectTransform>().rect.width > 100);
        }

        UpdateScales();
    }

    private void OnUiToTaskPerformanceMessage(UiToTaskPerformanceMessage message)
    {
        switch (message.TaskToExecute)
        {
            case 0:
                ClearTrace();
                UpdateEventsForProtocol(message.NewProtocol);
                UpdateScales();
                break;
            case 1:
                UpdateTimeResolution(message.TimeWindow);
                UpdateScales();
                break;
        }
    }

    private void OnEventsToTaskPerformanceMessage(EventsToTaskPerformanceMessage message)
    {
        switch (message.TaskToExecute)
        {
            case 0:
                UnityEngine.Debug.Log("Task deactivated, events have been reseted");
                ClearTrace();
                break;
            case 1:
                UnityEngine.Debug.Log("Task not up to date, events have been modifyed (add, delete, update)");
                if(m_HasDataToDisplay)
                    m_InfoDisplay.SetActive(true);
                break;
        }
    }

    private void OnVideoToModulesMessage(VideoToModulesMessage message)
    {
        int timeInMilliseconds = (int)message.TimeMilliseconds;
        UpdateSpawn(timeInMilliseconds);
        UpdatePicEvent(timeInMilliseconds);
    }

    private void ClearTrace()
    {
        m_HasDataToDisplay = false;

        //Clear barplots
        int BarplotCount = m_Triggers.Count;
        for (int i = BarplotCount - 1; i >= 0; i--)
        {
            m_Triggers[i].SelfDestruct();
        }
        m_Triggers.Clear();

        //Clear Event pics and main codes
        int EventPicturesCount = m_EventPictures.Count;
        for (int i = EventPicturesCount - 1; i >= 0; i--)
        {
            Resources.UnloadAsset(m_EventPictures[i]);
        }
        m_EventPictures.Clear();
        m_EventMainCodes.Clear();

        m_InfoDisplay.SetActive(false);
    }

    private void UpdateEventsForProtocol(Protocol protocol)
    {
        UnityEngine.Debug.Log("Update Protocol Events");
        m_HasDataToDisplay = false;
        TaskPerformanceService.ProcessEventsForExperiment(protocol);
        UpdateEvents();
        UpdateProtocolPicturesAndCodes(protocol);
        m_HasDataToDisplay = (TaskPerformanceService.ProcessedTriggers.Count == 0) ? false : true;
    }

    private void UpdateEvents()
    {
        int TriggerCount = TaskPerformanceService.ProcessedTriggers.Count;
        UnityEngine.Debug.Log("Update Events " + TriggerCount);

        for (int i = 0; i < TriggerCount; i++)
        {
            TriggerBarplot trigger = Instantiate(m_TriggerBarplotPrefabs, m_TaskBarHolder); //instancier avec parent dzans les paramètres
            trigger.Trigger = TaskPerformanceService.ProcessedTriggers[i];
            trigger.UpdatePosition(0, i, 0, -2);
            trigger.UpdatePosition(0, i, 100, -2);
            trigger.Show(false);

            m_Triggers.Add(trigger);
        }
    }

    private void UpdateProtocolPicturesAndCodes(Protocol protocol)
    {
        UnityEngine.Debug.Log("Update Protocol Pics and code");

        for (int i = 0; i < protocol.Blocs.Count; i++)
        {
            if (File.Exists(protocol.Blocs[i].dispBloc.path))
            {
                Texture2D current = new Texture2D(256, 256);
                current.LoadImage(File.ReadAllBytes(protocol.Blocs[i].dispBloc.path));

                m_EventPictures.Add(current);
            }
            else
            {
                m_EventPictures.Add(m_DefaultEventPicturePrefabs);
            }
            m_EventMainCodes.Add(protocol.Blocs[i].mainEvent.code);
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
            int currentRtMs = (int)m_Triggers[i].Trigger.ReactionTimeInMs;
            if (currentRtMs > MaxReactionTime)
                MaxReactionTime = currentRtMs;
        }
        m_HorizontalScale = m_TaskBarHolder.rect.width / (m_PeriopdInSec * 1000);
        m_VerticalScale = m_TaskBarHolder.rect.height / MaxReactionTime;
    }

    private void UpdateSpawn(int milliSecToLook)
    {
        if (!m_HasDataToDisplay)
            return;

        int left = milliSecToLook - (m_PeriopdInSec * 1000);
        int right = milliSecToLook;

        List<int> currentIndex = m_Triggers.Select((item, index) => new { Item = item, Index = index })
                                            .Where(x => x.Item.Trigger.ResponsTimeInMilliSeconds > left && x.Item.Trigger.ResponsTimeInMilliSeconds < right)
                                            .Select(x => x.Index)
                                            .ToList();

        DeactivateSpawn();
        if (currentIndex.Count != 0)
        {
            for (int i = 0; i < currentIndex.Count; i++)
            {
                float posiionSample = (left - m_Triggers[currentIndex[i]].Trigger.ResponsTimeInMilliSeconds);
                float positionInsideRect = posiionSample * -m_HorizontalScale;

                if (m_Triggers[currentIndex[i]].Trigger.ResponsTimeInMilliSeconds <= right)
                {
                    m_Triggers[currentIndex[i]].gameObject.SetActive(true);
                    m_Triggers[currentIndex[i]].UpdatePosition(0, positionInsideRect, 5, -2);

                    float value = m_VerticalScale * (m_Triggers[currentIndex[i]].Trigger.ReactionTimeInMs - 750);
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
                ActiveObjects[i].Show(false);
        }
    }

    private void UpdatePicEvent(int milliSecToLook)
    {
        if (!m_HasDataToDisplay)
            return;

        int found = m_Triggers.FindIndex(x => x.Trigger.MainEventTimeInMilliSeconds >= milliSecToLook - 8 && x.Trigger.MainEventTimeInMilliSeconds < milliSecToLook + 8);
        if (found == -1)
        {
            if (m_EventPicture.activeSelf == true)
                m_EventPicture.SetActive(false);

            return;
        }

        if (m_EventPicture.activeSelf == false)
        {
            m_EventPicture.SetActive(true);
            m_EventRawImage.texture = m_EventPictures[m_EventMainCodes.IndexOf(m_Triggers[found].Trigger.MainEnventCode)];
        }
    }
}
