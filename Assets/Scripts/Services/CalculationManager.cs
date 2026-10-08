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
            BtvLog.Handled("Error while processing TF", ex);
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

            (TimeFrequencyDataStructure result, int flatBins) = await Task.Run(() =>
            {
                TimeFrequencyDataStructure tfBaseline = ComputeShortTermFourier(baselineData, frameSize, fs);
                TimeFrequencyDataStructure tfEvent = ComputeShortTermFourier(eventData, frameSize, fs);
                int flat = TimeFrequencyNormalization.ApplyZscore(tfBaseline, tfEvent);
                // The copy constructor pays the (expensive, cached) TopValue eagerly - on the
                // worker, like the previous implementation did.
                return (new TimeFrequencyDataStructure(tfEvent), flat);
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

            if (flatBins > 0)
                ApplicationState.displayMessage("Flat baseline", "NOK", string.Format(
                    "{0} of {1} frequency bins have a flat baseline (a disconnected or zero-filled contact?). A z-score is undefined there, so they are shown in grey.",
                    flatBins, (int)result.FrequencyBinCount));
        }
        catch (Exception ex)
        {
            BtvLog.Handled("Error while processing TF Normalization", ex);
            ApplicationState.displayMessage("Time-frequency normalization failed", "NOK", ex.Message);
        }
    }

    /// <summary>
    /// Reads the event's slice of the channel into a buffer owned by the computation, so the
    /// worker never reads TracesService state.
    /// </summary>
    private static float[] SliceEventData(BTV.Data.BtvEvent btvEvent, int TraceIndex, float fs)
    {
        int beginSample = Mathf.RoundToInt(btvEvent.TimeInSeconds * fs);
        int endSample = Mathf.RoundToInt(beginSample + ((float)(btvEvent.Duration / 1000) * fs));
        BTV.Data.BtvChannel channel = TracesService.Channel(TraceIndex);
        // Reported as an error, as the whole-array copy did, rather than zero-filled.
        if (beginSample < 0 || endSample > channel.NumberOfSample)
            throw new ArgumentOutOfRangeException(nameof(btvEvent), "The event runs outside the recording.");
        float[] dataToProcess = new float[endSample - beginSample];
        channel.ReadWindow(beginSample, dataToProcess.Length, dataToProcess);
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
}
