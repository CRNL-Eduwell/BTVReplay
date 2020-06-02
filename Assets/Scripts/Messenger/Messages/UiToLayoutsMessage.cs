using UnityEngine;
using UnityEditor;

public class UiToLayoutsMessage
{
    // 0 : Load
    // 1 : Save
    public int TaskToExecute { get; set; } = -1;
    public string Path { get; set; } = "";
}