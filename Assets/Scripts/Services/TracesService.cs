using BTV.Data;
using BTV.Services.EegFileService;
using BTV.Services.VideoService;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TracesService
{
    private static Dictionary<int, TraceOption> m_Options = new Dictionary<int, TraceOption>();
    private static AudioTraceOption m_AudioOption = null;
    private static Color m_blue = new Color(0.6117f, 0.7058f, 0.7960f);
    private static Color m_yellow = new Color(0.9058f, 0.8784f, 0.0f);

    public static void Reset()
    {
        m_Options = new Dictionary<int, TraceOption>();
        m_AudioOption = null;

        VideoService.AudioDataLoaded -= OnAudioDataLoaded;
    }

    public static void InitTraces()
    {
        m_Options.Add(0, new TraceOption(EegFileService.ReturnFirstValidContainer(), m_blue));
        m_Options.Add(1, new TraceOption(EegFileService.ReturnFirstValidContainer(), m_yellow));
        m_AudioOption = new AudioTraceOption(null);

        VideoService.AudioDataLoaded += OnAudioDataLoaded;
    }

    private static void OnAudioDataLoaded()
    {
        m_AudioOption.FileHandle = VideoService.GetAudioContainer();
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

    public static AudioTraceOption GetAudioOptions()
    {
        if (m_AudioOption != null)
        {
            return m_AudioOption;
        }
        else
        {
            return null;
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

    public static float[] ChannelData(int traceID, int electrodeID = -1)
    {
        if (m_Options.ContainsKey(traceID))
        {
            BtvProgram handle = m_Options[traceID].FileHandle;
            if (electrodeID == -1) electrodeID = m_Options[traceID].ElectrodeID;
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

    public static float[] AudioChannelData()
    {
        if (m_AudioOption == null) return null;
        if (m_AudioOption.FileID < 0) return null;

        return m_AudioOption.FileHandle.Channels[m_AudioOption.FileID].Data;
    }
}
