using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using CielaSpike;
using BTV.UI.Module3D;

public static class ApplicationState
{
    public static Trace Window1
    {
        get;
        set;
    }

    public static Trace Window2
    {
        get;
        set;
    }

    public static Patient Patient
    {
        get;
        set;
    }

    public static ELAN[] EegFiles
    {
        get;
        set;
    }

    public static TraceEvent MemoryEvent
    {
        get;
        set;
    }

    //For the moment , only the file paths
    //later , will load the audio clips only once
    //and then distribute a pointer to it
    public static List<string> SoundFilePaths
    {
        get;
        set;
    }

    #region public members
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
            messageWindow = GameObject.Find("Canvas").transform.GetChild(5).GetChild(0).GetComponent<MessageWindow>();

        Window1 = GameObject.Find("Trace1Window").GetComponent<Trace>();
        Window2 = GameObject.Find("Trace2Window").GetComponent<Trace>();
    }

    // If in coroutine, need to be as such, otherwise it trigger error : "StartCoroutine_Auto_Internal can only be called from the main thread"
    // yield return Ninja.JumpToUnity;
    // ApplicationState.displayMessage(string HeaderMessage, string TypeMessage, string DetailledMessage);
    // yield return Ninja.JumpBack;
    public static void displayMessage(string HeaderMessage, string TypeMessage, string DetailledMessage)
    {
        if (messageWindow == null)
            messageWindow = GameObject.Find("Canvas").transform.GetChild(6).GetChild(0).GetComponent<MessageWindow>();
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
