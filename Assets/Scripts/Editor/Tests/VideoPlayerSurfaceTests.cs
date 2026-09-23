using System;
using BTV.Services.VideoService;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Edit-mode tests for the video-player surface cleanup (review M-8): the buffering capability
/// replaces a type test, and VLC output paths that would break its --sout chain are refused
/// with a message instead of making VLC write somewhere else.
/// </summary>
public class VideoPlayerSurfaceTests
{
    [TestCase("/Users/reviewer/Movies/patient 12 audio.wav")]
    [TestCase(@"C:\Users\reviewer\Videos\patient-12 (seizure).mp4")]
    [TestCase(@"\\chuv-share\eeg\P12\extract.wav")]
    public void SoutPath_AcceptsOrdinaryPaths(string path)
    {
        Assert.AreEqual(path, VideoService.SoutPath(path));
    }

    [TestCase("/data/P12, run 2/audio.wav")]
    [TestCase("/data/{P12}/audio.wav")]
    [TestCase("/data/P12's audio.wav")]
    [TestCase("/data/\"quoted\"/audio.wav")]
    public void SoutPath_RefusesCharactersThatBreakTheVlcChain(string path)
    {
        var e = Assert.Throws<ArgumentException>(() => VideoService.SoutPath(path));
        StringAssert.Contains(path, e.Message, "the message names the path");
    }

    [Test]
    public void GhostPlayer_SeeksInstantly()
    {
        GameObject go = new GameObject("ghost-player-test");
        try
        {
            Assert.IsTrue(go.AddComponent<GhostVideoPlayer>().SeeksInstantly, "the ghost clock needs no buffering spinner");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
    }
}
