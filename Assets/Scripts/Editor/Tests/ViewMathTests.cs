using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Edit-mode tests for the calculations behind what a clinician reads off the screen (review
/// M-7): trace samples and event overlays, the time-frequency map's frames and colours, the
/// correlation colour on electrodes, and loop-mode seeking. They pin today's behaviour, quirks
/// included, so a later change to any of it is deliberate.
/// </summary>
public class ViewMathTests
{
    // ---- Traces
    [Test]
    public void WindowStartSample_EndsTheWindowAtTheCurrentTime()
    {
        // 10 s at 512 Hz ends at sample 5120; a 1024-point window starts 1024 earlier.
        Assert.AreEqual(4096, TraceGeometry.WindowStartSample(10000, 512f, 1024));
        Assert.AreEqual(-1024 + 256, TraceGeometry.WindowStartSample(500, 512f, 1024), "early in the recording the window starts before sample 0");
    }

    [Test]
    public void ScaleAndClamp_AppliesGainAndOffset_ThenClampsToThePanel()
    {
        Assert.AreEqual(25f, TraceGeometry.ScaleAndClamp(10f, 2f, 5f, 100f));
        Assert.AreEqual(100f, TraceGeometry.ScaleAndClamp(80f, 2f, 0f, 100f));
        Assert.AreEqual(-100f, TraceGeometry.ScaleAndClamp(-80f, 2f, 0f, 100f));
        Assert.AreEqual(-100f, TraceGeometry.ScaleAndClamp(float.NaN, 1f, 0f, 100f), "quirk pinned: NaN clamps to the bottom");
    }

    [Test]
    public void PanelEdgeAndLimit()
    {
        Assert.AreEqual(-49f, TraceGeometry.PanelLeftEdge(100f));
        Assert.AreEqual(96.75f, TraceGeometry.ClampLimit(200f));
    }

    [Test]
    public void EventOverlay_PositionAndWidth()
    {
        // Window [0, 10000] ms on a 1000-unit panel, 1000 Hz, 10 000 points: 0.1 unit per sample.
        float x = TraceGeometry.EventX(0, 2500f, 1000f, 0.1f, 1000f);
        Assert.AreEqual(-499f + 250f, x, 1e-3f, "an event at 2.5 s sits a quarter of the way in");
        Assert.AreEqual(100f, TraceGeometry.SpanWidth(1000f, 0, 10000, 1000f), 1e-4f, "one second is a tenth of the panel");
    }

    // ---- Time-frequency map
    [Test]
    public void VisibleSampleRange_IntersectsTheWindowWithTheEvent()
    {
        // Event at 2 s lasting 4 s, 1000 Hz. Window [3 s, 5 s] shows samples 1000..3000 of it.
        (float beg, float end) = TfMapMath.VisibleSampleRange(3000, 5000, 1000f, 2f, 4000);
        Assert.AreEqual(1000f, beg);
        Assert.AreEqual(3000f, end);

        // Window [0, 10 s] covers the whole event.
        (beg, end) = TfMapMath.VisibleSampleRange(0, 10000, 1000f, 2f, 4000);
        Assert.AreEqual(0f, beg);
        Assert.AreEqual(4000f, end);
    }

    [Test]
    public void FrameRange_UsesHalfOverlappingFrames()
    {
        // frameSize 256, hop 128: samples 0..1280 are frames 0..9.
        (int begI, int endI) = TfMapMath.FrameRange(0f, 1280f, 256);
        Assert.AreEqual(0, begI);
        Assert.AreEqual(9, endI);
    }

    [Test]
    public void ScaleBounds_FloorsAt256_AndNeverCollapses()
    {
        Assert.AreEqual((0f, 256f), TfMapMath.ScaleBounds(10f, 0f, 1f), "quirk pinned: small maps are scaled to 256");
        Assert.AreEqual((0f, 1000f), TfMapMath.ScaleBounds(1000f, 0f, 1f));
        Assert.AreEqual((128f, 129f), TfMapMath.ScaleBounds(10f, 0.5f, 0.5f), "equal bounds get a one-unit range");
    }

    [Test]
    public void ColorIndex_MapsLinearlyAndClamps()
    {
        Assert.AreEqual(0, TfMapMath.ColorIndex(0f, 0f, 511f));
        Assert.AreEqual(256, TfMapMath.ColorIndex(256f, 0f, 511f));
        Assert.AreEqual(511, TfMapMath.ColorIndex(9999f, 0f, 511f));
        Assert.AreEqual(0, TfMapMath.ColorIndex(-5f, 0f, 511f));
        Assert.AreEqual(-1, TfMapMath.ColorIndex(float.NaN, 0f, 511f), "no data");
        Assert.AreEqual(-1, TfMapMath.ColorIndex(float.PositiveInfinity, 0f, 511f), "no data");
    }

    [Test]
    public void JetColorMap_RunsFromDarkBlueToDarkRed()
    {
        Color[] map = TfMapMath.JetColorMap();
        Assert.AreEqual(TfMapMath.ColorCount, map.Length);
        Assert.That(map[0].b, Is.GreaterThan(0.5f)); Assert.AreEqual(0f, map[0].r);
        Assert.That(map[511].r, Is.GreaterThan(0.45f)); Assert.AreEqual(0f, map[511].g); Assert.AreEqual(0f, map[511].b);
        foreach (Color c in map)
        {
            Assert.That(c.r, Is.InRange(0f, 1f)); Assert.That(c.g, Is.InRange(0f, 1f)); Assert.That(c.b, Is.InRange(0f, 1f));
        }
    }

    // ---- Correlations
    [Test]
    public void CorrelationColor_WhiteToRedAndWhiteToBlue()
    {
        Assert.AreEqual(Color.red, CorrelationColor.For(1f));
        Assert.AreEqual(Color.blue, CorrelationColor.For(-1f));
        Assert.AreEqual(new Color(1f, 0.5f, 0.5f, 1f), CorrelationColor.For(0.5f));
        Assert.AreEqual(Color.green, CorrelationColor.For(0f), "quirk pinned: exactly zero is green, near zero is white");
    }

    // ---- Video loop mode
    [Test]
    public void LoopWindow_SnapsSeeksToTheLoopEdges()
    {
        Assert.AreEqual((5000L, false), LoopWindow.Clamp(5000, 3000, 7000, 60000));
        Assert.AreEqual((7000L, true), LoopWindow.Clamp(9000, 3000, 7000, 60000));
        Assert.AreEqual((3000L, true), LoopWindow.Clamp(1000, 3000, 7000, 60000));
        Assert.AreEqual((0L, true), LoopWindow.Clamp(-3000, -2000, 2000, 60000), "below a loop that starts before 0, the seek clamps to 0");
        Assert.AreEqual((60000L, true), LoopWindow.Clamp(63000, 58000, 62000, 60000), "past a loop that ends after the video, the seek clamps to its end");
    }
}
