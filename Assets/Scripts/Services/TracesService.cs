using BTV.Data;
using BTV.Services.EegFileService;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TracesService
{
    private static Dictionary<int, TraceOption> m_Options = new Dictionary<int, TraceOption>();

    public static void Reset()
    {
        m_Options = new Dictionary<int, TraceOption>();
    }

    public static void InitTraces()
    {
        m_Options.Add(0, new TraceOption(EegFileService.ReturnFirstValidContainer()));
        m_Options.Add(1, new TraceOption(EegFileService.ReturnFirstValidContainer()));
    }

    public static TraceOption GetOptionsFor(int traceID)
    {
        if (m_Options.ContainsKey(traceID))
        {
            return m_Options[traceID];
        }
        else
        {
            throw new KeyNotFoundException("No Trace options for key = " + traceID.ToString());
        }
    }

    public static string ElectrodeName(int traceID)
    {
        if (m_Options.ContainsKey(traceID))
        {
            int electrodeID = m_Options[traceID].ElectrodeID;
            BtvProgram handle = m_Options[traceID].FileHandle;
            return handle.GetElectrodeNameFromElectrodeID(electrodeID);
        }
        else
        {
            return "";
        }
    }
    public static void SamplingFrequency(int traceID)
    {
        throw new NotImplementedException("Oupsi, need to implement TraceService.SamplingFrequency getter");
    }

    public static void ElectrodeCount(int traceID)
    {
        throw new NotImplementedException("Oupsi, need to implement TraceService.ElectrodeCount getter");
    }

    public static void ChannelData(int traceID)
    {
        throw new NotImplementedException("Oupsi, need to implement TraceService.ChannelData getter");
    }
}
