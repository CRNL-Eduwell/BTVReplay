/// <summary>
/// Class representing an event.
/// An Event is a code at a given moment in time
/// </summary>

public class EegEvent
{
    public int Code { get; set; } = -1;
    public int Sample { get; set; } = -1;
    public int SamplingFrequency { get; set; } = -1;
    public float TimeInSeconds { get { return (float)Sample / SamplingFrequency; } }
    public float TimeInMilliSeconds { get { return TimeInSeconds * 1000; } }

    public EegEvent(int code, int sample = -1, int samplingFreq = -1)
    {
        Code = code;
        Sample = sample;
        SamplingFrequency = samplingFreq;
    }

    public EegEvent(EegEvent currentEvent)
    {
        Code = currentEvent.Code;
        Sample = currentEvent.Sample;
        SamplingFrequency = currentEvent.SamplingFrequency;
    }
}

public class EegEvent2
{
    public int Code { get; set; } = -1;
    public float TimeInMilliSeconds { get; set; } = -1;
    public float TimeInSeconds { get { return TimeInMilliSeconds / 1000; } }

    /// <summary>
    /// Constructor
    /// <param name="code">Code of the event</param>
    /// <param name="time">Moment in time of the event (in milliseconds)</param>
    /// </summary>
    public EegEvent2(int code, float time)
    {
        Code = code;
        TimeInMilliSeconds = time;
    }

    /// <summary>
    /// Copy Constructor
    /// <param name="eegEvent">Event to copy</param>
    /// </summary>
    public EegEvent2(EegEvent2 eegEvent)
    {
        Code = eegEvent.Code;
        TimeInMilliSeconds = eegEvent.TimeInMilliSeconds;
    }
}