using System;
using System.Threading.Tasks;
using BTV.Services.CalculationService;
using UnityEngine;

/// <summary>
/// Runs the time-frequency computations requested by the UI. Each request follows the same
/// shape: gather inputs on the main thread, compute on a worker via Task.Run (the await
/// continuation comes back on the Unity main thread), then publish the result message. The
/// worker only ever touches data gathered for it - it never reads or writes service state,
/// which is what the previous ThreadNinja version did through shared fields.
/// </summary>
public class CalculationManager : MonoBehaviour
{
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
                    ProcessShortTermFourrier(message.EventOfInterest, message.TraceIndex);
                }
                break;
            case Calculations.NormalizedTF:
                {
                    ProcessZscoreTF(message.BaselineEvent, message.EventOfInterest, message.TraceIndex);
                }
                break;
            case Calculations.Unknown:
                {

                }
                break;
        }
    }

    private async void ProcessShortTermFourrier(BTV.Data.BtvEvent eventOfInterest, int TraceIndex)
    {
        try
        {
            int frameSize = TimeFrequencyService.GetFrameSizeFor(TraceIndex);
            float fs = TracesService.SamplingFrequency(TraceIndex);
            float[] dataToProcess = SliceEventData(eventOfInterest, TraceIndex, fs);

            TimeFrequencyDataStructure result = await Task.Run(() =>
            {
                TimeFrequencyDataStructure tf = ComputeShortTermFourier(dataToProcess, frameSize, fs);
                // TopValue sorts every value of the map and is cached; paying it here keeps the
                // first render of the TF map off the main thread.
                _ = tf.TopValue;
                return tf;
            });

            if (this == null) return; // scene was reloaded during the computation: drop the result

            BtvLog.Log("TF Done");
            TimeFrequencyResultMessage message = new TimeFrequencyResultMessage
            {
                TFDataStructure = result,
                TraceIndex = TraceIndex,
                EventOfInterest = eventOfInterest
            };
            Messenger.Default.Send(message, MessageContext.TimeFrequencyResultMessage);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("Error while processing TF");
            UnityEngine.Debug.LogException(ex);
            ApplicationState.displayMessage("Time-frequency computation failed", "NOK", ex.Message);
        }
    }

    private async void ProcessZscoreTF(BTV.Data.BtvEvent baseline, BTV.Data.BtvEvent eventOfInterest, int TraceIndex)
    {
        try
        {
            int frameSize = TimeFrequencyService.GetFrameSizeFor(TraceIndex);
            float fs = TracesService.SamplingFrequency(TraceIndex);
            float[] baselineData = SliceEventData(baseline, TraceIndex, fs);
            float[] eventData = SliceEventData(eventOfInterest, TraceIndex, fs);

            TimeFrequencyDataStructure result = await Task.Run(() =>
            {
                TimeFrequencyDataStructure tfBaseline = ComputeShortTermFourier(baselineData, frameSize, fs);
                TimeFrequencyDataStructure tfEvent = ComputeShortTermFourier(eventData, frameSize, fs);
                ApplyZscoreNormalization(tfBaseline, tfEvent);
                // The copy constructor pays the (expensive, cached) TopValue eagerly - on the
                // worker, like the previous implementation did.
                return new TimeFrequencyDataStructure(tfEvent);
            });

            if (this == null) return; // scene was reloaded during the computation: drop the result

            BtvLog.Log("TF Normalization Done");
            TimeFrequencyResultMessage message = new TimeFrequencyResultMessage
            {
                TFDataStructure = result,
                TraceIndex = TraceIndex,
                EventOfInterest = eventOfInterest
            };
            Messenger.Default.Send(message, MessageContext.TimeFrequencyResultMessage);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("Error while processing TF Normalization");
            UnityEngine.Debug.LogException(ex);
            ApplicationState.displayMessage("Time-frequency normalization failed", "NOK", ex.Message);
        }
    }

    /// <summary>
    /// Copies the event's slice of the channel into a buffer owned by the computation, so the
    /// worker never reads TracesService state.
    /// </summary>
    private static float[] SliceEventData(BTV.Data.BtvEvent btvEvent, int TraceIndex, float fs)
    {
        int beginSample = Mathf.RoundToInt(btvEvent.TimeInSeconds * fs);
        int endSample = Mathf.RoundToInt(beginSample + ((float)(btvEvent.Duration / 1000) * fs));
        float[] rawData = TracesService.ChannelData(TraceIndex);
        float[] dataToProcess = new float[endSample - beginSample];
        Array.Copy(rawData, beginSample, dataToProcess, 0, dataToProcess.Length);
        return dataToProcess;
    }

    private static TimeFrequencyDataStructure ComputeShortTermFourier(float[] dataToProcess, int frameSize, float fs)
    {
        int hopSize = frameSize / 2;
        int frameCount = ((dataToProcess.Length - frameSize) / hopSize) + 1;
        int freqBinCount = (frameSize / 2) + 1;

        BtvLog.Log("Frame Count : " + frameCount);
        BtvLog.Log("Freq Bin Count : " + freqBinCount);
        BtvLog.Log("Sampling Frequency : " + fs);

        TimeFrequencyDataStructure tfDataStruct = new TimeFrequencyDataStructure(fs, freqBinCount, frameCount);
        for (int i = 0; i < frameCount; i++)
        {
            float[] input = new float[frameSize];
            Array.Copy(dataToProcess, i * hopSize, input, 0, frameSize);
            float[] output = new float[frameSize];

            CalculationService.FFT_Magnitude(input, input.Length, output);

            tfDataStruct.SetTf_Frame(i, output);
        }
        return tfDataStruct;
    }

    private static void ApplyZscoreNormalization(TimeFrequencyDataStructure tfBaseline, TimeFrequencyDataStructure tfEvent)
    {
        for (int i = 0; i < tfBaseline.FrequencyBinCount; i++)
        {
            float[] baselineData = tfBaseline.GetFrequencyBinData(i);
            float baselineMean = CalculationService.Mean(baselineData, baselineData.Length);
            float baselineStdDev = CalculationService.StandardDeviation(baselineData, baselineData.Length);

            float[] eventData = tfEvent.GetFrequencyBinData(i);
            for (int j = 0; j < eventData.Length; j++)
            {
                eventData[j] = (eventData[j] - baselineMean) / baselineStdDev;
            }

            tfEvent.SetFrequencyBinData(i, eventData);
        }
    }
}
