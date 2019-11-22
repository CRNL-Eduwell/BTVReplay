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