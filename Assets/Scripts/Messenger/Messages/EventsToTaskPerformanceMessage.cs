
class EventsToTaskPerformanceMessage
{
    public enum Task
    {
        ResetAll = 0,
        MarkOutOfDate = 1,
    }

    public Task TaskToExecute
    {
        get;
        set;
    }
}
