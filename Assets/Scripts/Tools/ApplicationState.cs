using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using BTV.UI.Module3D;
using BTV.Services.EventsService;
using BTV.Services.EegFileService;
using BTV.Services.VideoService;
using BTV.UI;
using Tools.Unity;
using BTV.Services.CodeMatchingService;
using BTV.Services.AnatomicalDataService;
using BTV.Services.SubjectInfoService;
using BTV.Services.TaskPerformanceService;

public static class ApplicationState
{
    #region public members
    public static BTV3DModule Module3D { get; set; }
    public static MessageWindow messageWindow { get; set; }
    public static CoroutineManager coroutineManager { get; set; }
    public static TooltipManager TooltipManager { get; set; }
    #endregion

    private static GameObject m_InputFieldWindowPrefabs = null;

    public static void init()
    {
        m_InputFieldWindowPrefabs = Resources.Load("Prefabs/UIElements/InputFieldWindow", typeof(GameObject)) as GameObject;

        if (coroutineManager == null)
            coroutineManager = Object.FindAnyObjectByType<CoroutineManager>();
        if (messageWindow == null)
            messageWindow = FindMessageWindow();
    }

    public static void ResetAllServices()
    {
        // Session isolation is established before the scene reload. These compatibility resets
        // initialize the fresh state and publish the UI reset messages expected by the new scene.
        SubjectInfoService.Reset();
        AnatomicalDataService.Reset();
        EegFileService.Reset();
        TracesService.Reset();
        TimeFrequencyService.Reset();
        VideoService.Reset();
        EventsService.Reset();
        CodeMatchingService.Reset();
        TaskPerformanceService.Reset();
    }

    // By type, including inactive objects: this used to be GameObject.Find("Canvas") then
    // GetChild(3).GetChild(0), so reordering the Canvas children broke every dialog.
    private static MessageWindow FindMessageWindow()
    {
        return Object.FindAnyObjectByType<MessageWindow>(FindObjectsInactive.Include);
    }

    // Touches the UI, so call it on the main thread (async flows: after the await, never inside
    // a Task.Run worker).
    public static void displayMessage(string HeaderMessage, string TypeMessage, string DetailledMessage)
    {
        if (messageWindow == null)
            messageWindow = FindMessageWindow();
        messageWindow.display(HeaderMessage, TypeMessage, DetailledMessage);
    }

    public static void displayConfirmation(string HeaderMessage, string DetailledMessage, UnityAction yesAction, UnityAction cancelAction)
    {
        if (messageWindow == null)
            init();
        messageWindow.displayConfirmation(HeaderMessage, DetailledMessage, yesAction, cancelAction);
    }

    public static InputFieldWindow SpawnInputFieldWindow()
    {
        m_InputFieldWindowPrefabs = Resources.Load("Prefabs/UIElements/InputFieldWindow", typeof(GameObject)) as GameObject; //enlever d'ici quand le debug de la nouvelle db est finis

        GameObject viewGameObject = GameObject.Find("Canvas");
        GameObject inputField = GameObject.Instantiate(m_InputFieldWindowPrefabs, viewGameObject.transform);
        InputFieldWindow window = inputField.GetComponent<InputFieldWindow>();
        return window;
    }
}
