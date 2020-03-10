using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using CielaSpike;
using BTV.UI.Module3D;
using BTV.Services.EventsService;
using BTV.Services.EegFileService;
using BTV.Services.VideoService;

public static class ApplicationState
{
    #region public members
    public static BTV3DModule Module3D { get; set; }
    public static MessageWindow messageWindow
    {
        get; set;
    }
    public static CoroutineManager coroutineManager
    {
        get; set;
    }
    #endregion

    public static void init()
    {
        if (coroutineManager == null)
            coroutineManager = GameObject.Find("ringSelect").GetComponent<CoroutineManager>();
        if (messageWindow == null)
            messageWindow = GameObject.Find("Canvas").transform.GetChild(4).GetChild(0).GetComponent<MessageWindow>();
    }

    public static void ResetAllServices()
    {
        EegFileService.Reset();
        VideoService.Reset();
        EventsService.Reset();
    }

    // If in coroutine, need to be as such, otherwise it trigger error : "StartCoroutine_Auto_Internal can only be called from the main thread"
    // yield return Ninja.JumpToUnity;
    // ApplicationState.displayMessage(string HeaderMessage, string TypeMessage, string DetailledMessage);
    // yield return Ninja.JumpBack;
    public static void displayMessage(string HeaderMessage, string TypeMessage, string DetailledMessage)
    {
        if (messageWindow == null)
            messageWindow = GameObject.Find("Canvas").transform.GetChild(4).GetChild(0).GetComponent<MessageWindow>();
        messageWindow.display(HeaderMessage, TypeMessage, DetailledMessage);
    }

    public static void displayConfirmation(string HeaderMessage, string DetailledMessage, UnityAction yesAction, UnityAction cancelAction)
    {
        if (messageWindow == null || coroutineManager == null)
            init();
        coroutineManager.StartCoroutine(c_displayConfirmation(HeaderMessage, DetailledMessage, yesAction, cancelAction));
    }

    static IEnumerator c_displayConfirmation(string HeaderMessage, string DetailledMessage, UnityAction yesAction, UnityAction cancelAction)
    {
        yield return Ninja.JumpToUnity;
        messageWindow.displayConfirmation(HeaderMessage, DetailledMessage, yesAction, cancelAction);
        yield return Ninja.JumpBack;

        yield return null;
    }
}
