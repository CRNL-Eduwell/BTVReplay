using UnityEngine;

public class UiToLayoutsMessage
{
    public enum Task
    {
        None = -1,
        Load = 0,
        Save = 1,
    }
    public Task TaskToExecute { get; set; } = Task.None;
    public string Path { get; set; } = "";
}