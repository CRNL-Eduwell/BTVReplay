using System.IO;
using BTV.Services.VideoService;
using NUnit.Framework;

/// <summary>
/// The filtered-audio cache name carries the ToHilbert output version. Caches written before
/// Framework c466993 (float32 fir2 phase, wrong envelopes beyond about 1 h) must not be loaded,
/// and the new name must never collide with the file an older version wrote.
/// </summary>
public class FilteredAudioCacheTests
{
    [Test]
    public void FilteredAudioPath_IsVersioned()
    {
        string wav = Path.Combine("data", "patient", "video.wav");

        Assert.AreEqual(Path.Combine("data", "patient", "video_audio_v2.csv"),
            VideoService.GetFilteredAudioPathFromAudioPath(wav, VideoService.FilteredAudioSuffix));
    }

    [Test]
    public void LegacyFilteredAudioPath_IsTheOneOlderVersionsWrote()
    {
        string wav = Path.Combine("data", "patient", "video.wav");

        Assert.AreEqual(Path.Combine("data", "patient", "video_audio.csv"),
            VideoService.GetFilteredAudioPathFromAudioPath(wav, VideoService.LegacyFilteredAudioSuffix));
    }

    [Test]
    public void FilteredAudioPath_NeverMatchesTheLegacyFile()
    {
        string wav = Path.Combine("data", "patient", "video.wav");

        Assert.AreNotEqual(
            VideoService.GetFilteredAudioPathFromAudioPath(wav, VideoService.LegacyFilteredAudioSuffix),
            VideoService.GetFilteredAudioPathFromAudioPath(wav, VideoService.FilteredAudioSuffix));
    }

    [Test]
    public void FilteredAudioPath_IsEmptyWithoutVideo()
    {
        Assert.AreEqual("", VideoService.GetFilteredAudioPathFromAudioPath("", VideoService.FilteredAudioSuffix));
    }
}
