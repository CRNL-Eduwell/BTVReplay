using UnityEngine;

/// <summary>
/// Pure geometry for the scrolling EEG traces and their event overlays, pulled out of EegSignal
/// and GraphEvents (review M-7) so the numbers a clinician reads off a trace are unit-tested.
/// Behaviour is unchanged: each expression is the one the views used, operand order included.
/// </summary>
public static class TraceGeometry
{
    /// <summary>Left edge of a trace panel of <paramref name="panelWidth"/>, in local x.</summary>
    public static float PanelLeftEdge(float panelWidth) => (-panelWidth / 2) + 1;

    /// <summary>Half the drawable height: samples are clamped to +/- this.</summary>
    public static float ClampLimit(float panelHeight) => (panelHeight - 6.5f) / 2;

    /// <summary>First sample of the window that ends at <paramref name="timeMs"/> (may be negative).</summary>
    public static int WindowStartSample(int timeMs, float samplingFrequency, int numberOfPoints)
    {
        int mostRecentSample = Mathf.RoundToInt(timeMs * (samplingFrequency / 1000));
        return mostRecentSample - numberOfPoints;
    }

    /// <summary>Gain and offset applied, then clamped to the panel (NaN clamps to the bottom).</summary>
    public static float ScaleAndClamp(float sample, float gain, float offset, float limit)
    {
        float value = gain * sample + offset;
        if (value >= -limit && value <= limit)
            return value;
        return value >= 0 ? limit : -limit;
    }

    /// <summary>Local x of an event starting at <paramref name="eventStartMs"/>, for a window starting at <paramref name="windowLeftMs"/>.</summary>
    public static float EventX(int windowLeftMs, float eventStartMs, float samplingFrequency, float horizontalScale, float panelWidth)
    {
        return (((windowLeftMs - eventStartMs) * samplingFrequency) / 1000) * -horizontalScale + PanelLeftEdge(panelWidth);
    }

    /// <summary>Width, in panel units, of <paramref name="spanMs"/> milliseconds shown in a window of [left, right].</summary>
    public static float SpanWidth(float spanMs, int windowLeftMs, int windowRightMs, float panelWidth)
    {
        return (spanMs / (windowRightMs - windowLeftMs)) * panelWidth;
    }
}
