class UiToTFEventsMessage
{
    // 0 : Toggle Curso slave
    public int TaskToExecute
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
    public float FrequencySlider { get; set; }
    public float WindowInMs { get; set; }
}
