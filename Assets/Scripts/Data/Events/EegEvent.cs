/// <summary>
/// Class representing an event.
/// An Event is a code at a given moment in time
/// </summary>

public class EegEvent
{
    public int Code { get; set; } = -1;
    public float TimeInMilliSeconds { get; set; } = -1;
    public float TimeInSeconds { get { return TimeInMilliSeconds / 1000; } }

    /// <summary>
    /// Constructor
    /// <param name="code">Code of the event</param>
    /// <param name="time">Moment in time of the event (in milliseconds)</param>
    /// </summary>
    public EegEvent(int code, float time)
    {
        Code = code;
        TimeInMilliSeconds = time;
    }

    /// <summary>
    /// Copy Constructor
    /// <param name="eegEvent">Event to copy</param>
    /// </summary>
    public EegEvent(EegEvent eegEvent)
    {
        Code = eegEvent.Code;
        TimeInMilliSeconds = eegEvent.TimeInMilliSeconds;
    }
}