using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Data.Factory;
using System;
using BTV.Data;
using SimpleExpressionEngine;

public class DebugFlorian : MonoBehaviour
{
    // Update is called once per frame
    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.D))
        //    SpawnUI("NormalizeTF");
        //if (Input.GetKeyDown(KeyCode.D))
        //    SpawnUI("BugReporterWindow");
        //if (Input.GetKeyDown(KeyCode.D))
        //    throw new ArgumentException("OUPS");
    }

    private void SpawnUI(string uiName)
    {
        ShowWindowMessage message = new ShowWindowMessage
        {
            WindowName = uiName
        };
        Messenger.Default.Send(message, MessageContext.ShowWindowMessage);
    }

    string bp1String = "";
    string bp2String = "B'2 - B'1";
    string bp3String = "B'3 - B'2";
    private void TestParser()
    {
        int dataLength = 5;
        List<BtvChannel> channels = new List<BtvChannel>()
        {
            new BtvChannel("B'1", 0, 64, new float[5] { 1, 2, 3, 4, 5 }),
            new BtvChannel("B'2", 1, 64, new float[5] { 2, 3, 4, 5, 6 }),
            new BtvChannel("B'3", 2, 64, new float[5] { 10, 20, 30, 40, 50 })
        };
        List<BtvChannel> newChannels = new List<BtvChannel>()
        {
            new BtvChannel("B'1", 0, 64, new float[5] { 0, 0, 0, 0, 0 }),
            new BtvChannel("B'2", 1, 64, new float[5] { 0, 0, 0, 0, 0 }),
            new BtvChannel("B'3", 2, 64, new float[5] { 0, 0, 0, 0, 0 })
        };

        ChannelContext context = new ChannelContext(channels);
        
        for (int i = 0; i < dataLength; ++i)
        {
            context.Index = i;
            newChannels[0].Data[i] = (float)Parser.Parse(bp1String).Eval(context);
            newChannels[1].Data[i] = (float)Parser.Parse(bp2String).Eval(context);
            newChannels[2].Data[i] = (float)Parser.Parse(bp3String).Eval(context);
        }
    }
    private void Start()
    {
        TestParser();
    }
}
