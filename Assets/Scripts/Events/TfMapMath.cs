using UnityEngine;

/// <summary>
/// Pure arithmetic behind a time-frequency map, pulled out of EventWithDuration (review M-7):
/// which part of the event the view window shows, which analysis frames that is, and how a value
/// maps onto the jet colormap. Behaviour is unchanged; each expression is the view's own.
/// </summary>
public static class TfMapMath
{
    public const int ColorCount = 512;

    /// <summary>
    /// The visible part of an event, in samples from the event's start: the view window
    /// [<paramref name="windowLeftMs"/>, <paramref name="windowRightMs"/>] intersected with the event.
    /// </summary>
    public static (float beg, float end) VisibleSampleRange(int windowLeftMs, int windowRightMs, float samplingFrequency, float eventStartSeconds, int eventDurationMs)
    {
        float leftClockInSample = (windowLeftMs * samplingFrequency) / 1000;
        float rightClockInSample = (windowRightMs * samplingFrequency) / 1000;
        float begInSample = eventStartSeconds * samplingFrequency;
        float endInSample = begInSample + ((eventDurationMs * samplingFrequency) / 1000);
        float beg = (leftClockInSample - begInSample) < 0 ? 0 : leftClockInSample - begInSample;
        float end = (rightClockInSample - endInSample) < 0 ? (rightClockInSample - begInSample) : (endInSample - begInSample);
        return (beg, end);
    }

    /// <summary>First and last analysis frame for a sample range (frames overlap by half: hop = frameSize / 2).</summary>
    public static (int begI, int endI) FrameRange(float beg, float end, int frameSize)
    {
        int hopSize = frameSize / 2;
        int begI = Mathf.RoundToInt((beg / frameSize) * (frameSize / hopSize));
        int endI = Mathf.RoundToInt((end / frameSize) * (frameSize / hopSize)) - 1;
        return (begI, endI);
    }

    /// <summary>
    /// Colour-scale bounds: the amplitude factors scale the map's 90th percentile, floored at 256.
    /// (The floor suits FFT magnitudes; review B-6 notes it squashes z-score maps.)
    /// </summary>
    public static (float min, float max) ScaleBounds(float topValue, float minValueFactor, float maxValueFactor)
    {
        float maxBoundary = topValue > 256 ? topValue : 256;
        float maxValue = maxValueFactor * maxBoundary;
        float minValue = minValueFactor * maxBoundary;
        if (maxValue == minValue) maxValue = minValue + 1; // to prevent some kind of discontinuity
        return (minValue, maxValue);
    }

    /// <summary>Index into the jet colormap for a value, clamped to [0, 511]; -1 for no data (NaN or infinity).</summary>
    public static int ColorIndex(float value, float minValue, float maxValue)
    {
        if (float.IsNaN(value) || float.IsInfinity(value)) return -1;
        float r = (value - minValue) / (maxValue - minValue);
        int col = Mathf.RoundToInt(0 + (511 * r));
        if (col < 0) return 0;
        if (col > 511) return 511;
        return col;
    }

    /// <summary>The 512-entry jet colormap, dark blue to dark red.</summary>
    public static Color[] JetColorMap()
    {
        Color[] colorMap = new Color[ColorCount];

        int compteur = 0;
        for (int i = 0; i < 57; i++)
        {
            float r = 0;
            float g = 0;
            float b = 143.4375f + (i * 1.9649f);
            colorMap[i] = new Color(r, g, b);
        }

        compteur = 57;
        for (int i = 0; i < 130; i++)
        {
            float r = 0;
            float g = 0.4366f + (i * 1.9649f);
            float b = 255;
            colorMap[compteur] = new Color(r, g, b);
            compteur++;
        }

        compteur = 187;
        for (int i = 0; i < 130; i++)
        {
            float r = 0.8733f + (i * 1.9649f);
            float g = 255;
            float b = 254.1267f - (i * 1.9649f);
            colorMap[compteur] = new Color(r, g, b);
            compteur++;
        }

        compteur = 317;
        for (int i = 0; i < 130; i++)
        {
            float r = 255;
            float g = 253.6901f - (i * 1.9649f);
            float b = 0;
            colorMap[compteur] = new Color(r, g, b);
            compteur++;
        }

        compteur = 447;
        for (int i = 0; i < 65; i++)
        {
            float r = 253.2534f - (i * 1.9649f);
            float g = 0;
            float b = 0;
            colorMap[compteur] = new Color(r, g, b);
            compteur++;
        }

        // The bands above are authored in 0-255; Unity's Color expects 0-1, so without this the
        // jet map clamped to a few saturated colours. Normalise (alpha stays opaque).
        for (int i = 0; i < colorMap.Length; i++)
            colorMap[i] = new Color(colorMap[i].r / 255f, colorMap[i].g / 255f, colorMap[i].b / 255f, 1f);

        return colorMap;
    }
}
