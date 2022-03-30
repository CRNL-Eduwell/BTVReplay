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

    public static int WindowInSeconds(int traceID)
    {
        return m_Options.ContainsKey(traceID) ? m_Options[traceID].WindowInSeconds : -1;
    }

    public static int SamplingFrequency(int traceID)
    {
        return m_Options.ContainsKey(traceID) ? m_Options[traceID].SamplingFrequency : -1;
    }

    public static int ElectrodeCount(int traceID)
    {
        return m_Options.ContainsKey(traceID) ? m_Options[traceID].FileHandle.NumberOfElectrodes : -1;
    }

    public static float[] ChannelData(int traceID, int electrodeID)
    {
        if (m_Options.ContainsKey(traceID))
        {
            BtvProgram handle = m_Options[traceID].FileHandle;
            if (electrodeID < handle.NumberOfElectrodes)
            {
                return handle.Channels[electrodeID].Data;
            }
            else
            {
                throw new ArgumentOutOfRangeException("ElectrodeID is bigger than the number of electrode => " + electrodeID.ToString() + " and elecCount = " + handle.NumberOfElectrodes.ToString());
            }
        }
        else
        {
            throw new KeyNotFoundException("No Data for key = " + traceID.ToString());
        }
    }
}
