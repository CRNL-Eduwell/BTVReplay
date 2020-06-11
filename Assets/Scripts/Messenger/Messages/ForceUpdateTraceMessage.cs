using UnityEngine;
using UnityEditor;

class ForceUpdateTraceMessage 
{
    public int TraceID { get; set; } = -1;
    public float Gain { get; set; }
    public float Offset { get; set; }
    public bool ShowGrid { get; set; }
    public int Period { get; set; }
    public Color Color { get; set; }
}