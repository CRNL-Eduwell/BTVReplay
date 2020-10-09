using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Data.Factory;
using System;

public class DebugFlorian : MonoBehaviour
{
    // Update is called once per frame
    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.D))
        //    SpawnUI("DBUserPreferences");
        //if (Input.GetKeyDown(KeyCode.D))
        //    SpawnUI("BugReporterWindow");
        //if (Input.GetKeyDown(KeyCode.D))
        //    throw new ArgumentException("OUPS");
    }

    private void SpawnUI(string uiName)
    {
        ShowWindowMessage message = new ShowWindowMessage
        {
            TaskToExecute = 0,
            WindowName = uiName
        };
        Messenger.Default.Send(message, MessageContext.ShowWindowMessage);
    }
}
