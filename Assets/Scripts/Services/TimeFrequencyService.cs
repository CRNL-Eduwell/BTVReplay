using System.Collections;
using System.Collections.Generic;
using BTV.Data;
using UnityEngine;

public static class TimeFrequencyService
{
    public static BtvEvent BaselineEvent
    {
        get { return BTV.Services.Session.Current.BaselineEvent; }
        set { BTV.Services.Session.Current.BaselineEvent = value; }
    }
    private static Dictionary<int, TfTraceOption> Options { get { return BTV.Services.Session.Current.TfTraceOptions; } }

    public static void Reset()
    {
        BaselineEvent = null;
        BTV.Services.Session.Current.TfTraceOptions = new Dictionary<int, TfTraceOption>();
    }

    public static void InitTraces()
    {
        Options.Add(0, new TfTraceOption(1f));
        Options.Add(1, new TfTraceOption(1f));
    }

    public static TfTraceOption GetOptionsFor(int traceID)
    {
        return GetOptionsFor(BTV.Services.Session.Current, traceID);
    }

    public static TfTraceOption GetOptionsFor(BTV.Services.Session session, int traceID)
    {
        BtvLog.Log("Trace " + traceID);
        if (session.TfTraceOptions.ContainsKey(traceID))
        {
            return session.TfTraceOptions[traceID];
        }
        else
        {
            throw new KeyNotFoundException("No TfTraceOption options for key = " + traceID.ToString());
        }
    }

    public static int GetFrameSizeFor(int traceID)
    {
        return GetFrameSizeFor(BTV.Services.Session.Current, traceID);
    }

    public static int GetFrameSizeFor(BTV.Services.Session session, int traceID)
    {
        float Fs = TracesService.SamplingFrequency(session, traceID);
        return Mathf.RoundToInt(session.TfTraceOptions[traceID].WindowInMilliseconds * Fs / 1000);
    }
}
