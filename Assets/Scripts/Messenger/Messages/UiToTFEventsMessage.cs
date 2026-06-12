class UiToTFEventsMessage
{
    public enum Task
    {
        ToggleCursorSlave = 0,
        UpdateAlpha = 1,
        UpdateFrequencyBand = 2,
        UpdateTfWindow = 3,
        UpdateAmplitude = 4,
    }

    public Task TaskToExecute
    {
        get;
        set;
    }

    public bool IsSlaved
    {
        get;
        set;
    }

    public int ParentWindowIndex { get; set; }
    public float Alpha { get; set; }
    public int HighFrequency { get; set; }
    public int LowFrequency { get; set; }
    public float WindowInMs { get; set; }
    public float MinValueFactor { get; set; }
    public float MaxValueFactor { get; set; }
}
