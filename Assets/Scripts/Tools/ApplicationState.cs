using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using CielaSpike;

public static class ApplicationState
{
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
            messageWindow = GameObject.Find("Canvas").transform.GetChild(7).GetChild(0).GetComponent<MessageWindow>();
    }

    public static void displayMessage(string HeaderMessage, string TypeMessage, string DetailledMessage)
    {
        if (messageWindow == null)
            messageWindow = GameObject.Find("Canvas").transform.GetChild(7).GetChild(0).GetComponent<MessageWindow>();
        coroutineManager.StartCoroutine(c_dislayMessage(HeaderMessage, TypeMessage, DetailledMessage));
    }

    static IEnumerator c_dislayMessage(string HeaderMessage, string TypeMessage, string DetailledMessage)
    {
        yield return Ninja.JumpToUnity;
        messageWindow.display(HeaderMessage, TypeMessage, DetailledMessage);
        yield return Ninja.JumpBack;

        yield return null;
    }

    public static void displayConfirmation(string HeaderMessage, string DetailledMessage, UnityAction yesAction, UnityAction cancelAction)
    {
        if (messageWindow == null)
            messageWindow = GameObject.Find("Canvas").transform.GetChild(7).GetChild(0).GetComponent<MessageWindow>();
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
