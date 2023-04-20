using System.Collections;
using System.Collections.Generic;
using BTV.Data;
using UnityEngine;

public static class TimeFrequencyService
{
    public static BtvEvent BaselineEvent { get; set; } = null;
    private static Dictionary<int, TfTraceOption> m_Options = new Dictionary<int, TfTraceOption>();

    public static void Reset()
    {
        m_Options = new Dictionary<int, TfTraceOption>();
    }

    public static void InitTraces()
    {
        m_Options.Add(0, new TfTraceOption(1f));
        m_Options.Add(1, new TfTraceOption(1f));
    }

    public static TfTraceOption GetOptionsFor(int traceID)
    {
        UnityEngine.Debug.Log("Trace " + traceID);
        if (m_Options.ContainsKey(traceID))
        {
            return m_Options[traceID];
        }
        else
        {
            throw new KeyNotFoundException("No TfTraceOption options for key = " + traceID.ToString());
        }
    }

    public static int GetFrameSizeFor(int traceID)
    {
        float Fs = TracesService.SamplingFrequency(traceID);
        return Mathf.RoundToInt(m_Options[traceID].WindowInMilliseconds * Fs / 1000);
    }
}
