using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts.Data.Files;
using BTV.Services.CalculationService;
using CielaSpike;
using UnityEngine;

public class CalculationManager : MonoBehaviour
{
    private int m_FrameSize = 0;
    private int m_HopSize = 0;

    private TimeFrequencyDataStructure m_TfDataStruct = null;

    private void Start()
    {
        Messenger.Default.Register<ProcessCalculationMessage>(this, OnProcessCalculationMessage, MessageContext.ProcessCalculationMessage);
    }

    private void OnDestroy()
    {
        Messenger.Default.Unregister(this, MessageContext.ProcessCalculationMessage);
    }

    private void OnProcessCalculationMessage(ProcessCalculationMessage message)
    {
        switch (message.Task)
        {
            case Calculations.TF:
                {
                    StartCoroutine(c_ProcessShortTermFourrier(message.EventOfInterest, message.TraceIndex));
                }
                break;
            case Calculations.Unknown:
                {

                }
                break;
        }
    }

    private IEnumerator c_ProcessShortTermFourrier(BTV.Data.BtvEvent eventOfInterest, int TraceIndex)
    {
        yield return this.StartCoroutineAsync(c_ShortTermFourrier(eventOfInterest, TraceIndex), out Task SignalprocessingTask);

        switch (SignalprocessingTask.State)
        {
            case TaskState.Done:
                {
                    yield return Ninja.JumpToUnity;
                    UnityEngine.Debug.Log("TF Done");
                    TimeFrequencyResultMessage message = new TimeFrequencyResultMessage
                    {
                        TFDataStructure = m_TfDataStruct,
                        TraceIndex = TraceIndex,
                        EventOfInterest = eventOfInterest
                    };
                    Messenger.Default.Send(message, MessageContext.TimeFrequencyResultMessage);
                    yield return Ninja.JumpBack;
                    break;
                }
            case TaskState.Error:
                {
                    yield return Ninja.JumpToUnity;
                    //ApplicationState.displayMessage("Error Processing Correlations", "NOK", SignalprocessingTask.Exception.Message.ToString());
                    UnityEngine.Debug.LogError("Error while processing TF");
                    yield return Ninja.JumpBack;
                    break;
                }
        }
    }

    private IEnumerator c_ShortTermFourrier(BTV.Data.BtvEvent eventOfInterest, int TraceIndex)
    {
        m_FrameSize = TimeFrequencyService.GetFrameSizeFor(TraceIndex);
        m_HopSize = m_FrameSize/2;

        float Fs = TracesService.SamplingFrequency(TraceIndex);
        int BeginSample = Mathf.RoundToInt(eventOfInterest.TimeInSeconds * Fs);
        int EndSample = Mathf.RoundToInt(BeginSample + ((float)(eventOfInterest.Duration / 1000) * Fs));
        float[] RawData = TracesService.ChannelData(TraceIndex);
        float[] DataToProcess = new float[EndSample - BeginSample];
        Array.Copy(RawData, BeginSample, DataToProcess, 0, DataToProcess.Length);

        int FrameCount = ((DataToProcess.Length - m_FrameSize) / m_HopSize) + 1;
        int FreqBinCount = (m_FrameSize / 2) + 1;

        UnityEngine.Debug.Log("Frame Count : " + FrameCount);
        UnityEngine.Debug.Log("Freq Bin Count : " + FreqBinCount);
        UnityEngine.Debug.Log("Sampling Frequency : " + Fs);

        m_TfDataStruct = new TimeFrequencyDataStructure(Fs, FreqBinCount, FrameCount);
        for (int i = 0; i < FrameCount; i++)
        {
            int beg = 0 + (i * m_HopSize);
            int end = beg + m_FrameSize;
            float[] input = new float[m_FrameSize];
            Array.Copy(DataToProcess, beg, input, 0, m_FrameSize);
            float[] output = new float[m_FrameSize];

            CalculationService.FFT_Magnitude(input, input.Length, output);

            m_TfDataStruct.SetTf_Frame(i, output);
        }

        yield return null;
    }
}
