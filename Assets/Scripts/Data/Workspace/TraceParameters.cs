using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;

public class TraceParameters
{
    public float Gain { get; set; } = 0;
    public float Offset { get; set; } = 0;
    public bool ShowGrid { get; set; } = false;
    /// <summary>
    /// Size of the window, in seconds
    /// </summary>
    public int Window { get; set; } = 0;
    /// <summary>
    /// Color of the trace
    /// </summary>
    public Color Color { get; set; }
    /// <summary>
    /// Width of the trace
    /// </summary>
    public float Width { get; set; } = 0;
    public string Parent { get; set; } = null;
    /// <summary>
    /// Id of the trace for which those are the parameters
    /// </summary>
    public int Id { get; set; } = -1;
    public GridLayout GridLayout { get; set; }

    public TraceParameters()
    {

    }

    public TraceParameters(float gain, float offset, bool showGrid, int window, Color color, float width)
    {
        Gain = gain;
        Offset = offset;
        ShowGrid = showGrid;
        Window = window;
        Color = color;
        Width = width;
    }

    public TraceParameters(TraceParameters parameters)
    {
        Gain = parameters.Gain;
        Offset = parameters.Offset;
        ShowGrid = parameters.ShowGrid;
        Window = parameters.Window;
        Color = parameters.Color;
        Width = parameters.Width;
        Parent = parameters.Parent;
        Id = parameters.Id;
        GridLayout = parameters.GridLayout;
    }
}