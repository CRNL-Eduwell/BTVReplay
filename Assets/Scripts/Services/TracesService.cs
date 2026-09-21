using BTV.Data;
using BTV.Services.EegFileService;
using BTV.Services.VideoService;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TracesService
{
    private static Dictionary<int, TraceOption> Options { get { return BTV.Services.Session.Current.TraceOptions; } }
    private static AudioTraceOption AudioOption
    {
        get { return BTV.Services.Session.Current.AudioTraceOption; }
        set { BTV.Services.Session.Current.AudioTraceOption = value; }
    }
    private static readonly Color m_blue = new Color(0.6117f, 0.7058f, 0.7960f);
    private static readonly Color m_yellow = new Color(0.9058f, 0.8784f, 0.0f);

    public static void Reset()
    {
        BTV.Services.Session.Current.TraceOptions = new Dictionary<int, TraceOption>();
        AudioOption = null;

        VideoService.AudioDataLoaded -= OnAudioDataLoaded;
    }

    public static void InitTraces()
    {
        Options.Add(0, new TraceOption(EegFileService.ReturnFirstValidContainer(), m_blue));
        Options.Add(1, new TraceOption(EegFileService.ReturnFirstValidContainer(), m_yellow));
        AudioOption = new AudioTraceOption(null);

        VideoService.AudioDataLoaded += OnAudioDataLoaded;
    }

    private static void OnAudioDataLoaded()
    {
        AudioOption.FileHandle = VideoService.GetAudioContainer();
    }

    public static TraceOption GetOptionsFor(int traceID)
    {
        return GetOptionsFor(BTV.Services.Session.Current, traceID);
    }

    public static TraceOption GetOptionsFor(BTV.Services.Session session, int traceID)
    {
        if (session.TraceOptions.ContainsKey(traceID))
        {
            return session.TraceOptions[traceID];
        }
        else
        {
            throw new KeyNotFoundException("No Trace options for key = " + traceID.ToString());
        }
    }

    public static AudioTraceOption GetAudioOptions()
    {
        return GetAudioOptions(BTV.Services.Session.Current);
    }

    public static AudioTraceOption GetAudioOptions(BTV.Services.Session session)
    {
        if (session.AudioTraceOption != null)
        {
            return session.AudioTraceOption;
        }
        else
        {
            return null;
        }
    }

    public static string ElectrodeName(int traceID)
    {
        if (Options.ContainsKey(traceID))
        {
            int electrodeID = Options[traceID].ElectrodeID;
            BtvProgram handle = Options[traceID].FileHandle;
            return handle.GetElectrodeNameFromElectrodeID(electrodeID);
        }
        else
        {
            return "";
        }
    }

    public static int WindowInSeconds(int traceID)
    {
        return Options.ContainsKey(traceID) ? Options[traceID].WindowInSeconds : -1;
    }

    public static int SamplingFrequency(int traceID)
    {
        return Options.ContainsKey(traceID) ? Options[traceID].SamplingFrequency : -1;
    }

    public static int ElectrodeCount(int traceID)
    {
        return Options.ContainsKey(traceID) ? Options[traceID].FileHandle.NumberOfElectrodes : -1;
    }

    public static float[] ChannelData(int traceID, int electrodeID = -1)
    {
        if (Options.ContainsKey(traceID))
        {
            BtvProgram handle = Options[traceID].FileHandle;
            if (electrodeID == -1) electrodeID = Options[traceID].ElectrodeID;
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
        if (AudioOption == null) return null;
        if (AudioOption.FileID < 0) return null;

        return AudioOption.FileHandle.Channels[AudioOption.FileID].Data;
    }
}
