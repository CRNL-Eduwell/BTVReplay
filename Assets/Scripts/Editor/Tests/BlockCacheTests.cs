using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BTV.Data;
using BTV.Services;
using NUnit.Framework;
using Tools.CSharp.EEG;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Edit-mode tests for the window cache (windowed-loading design, phase P2). The traces and
/// sites used to read samples synchronously on every video tick; they now ask a BlockCache,
/// which loads fixed blocks on a worker and reports a miss while they load. A manual frame
/// clock and a runner that completes reads only when told replace Time.frameCount and
/// Task.Run, so each test decides when a frame starts and when a read lands.
/// </summary>
public class BlockCacheTests
{
    private const int B = BlockCache.BlockSize;

    private sealed class DeferredRunner
    {
        private readonly List<(Action work, TaskCompletionSource<bool> done)> m_Pending = new List<(Action, TaskCompletionSource<bool>)>();

        public int PendingCount => m_Pending.Count;

        public Task Run(Action work)
        {
            var done = new TaskCompletionSource<bool>();
            m_Pending.Add((work, done));
            return done.Task;
        }

        /// <summary>Runs every pending read; each continuation runs inline, as on the main thread.</summary>
        public void CompleteAll()
        {
            var pending = m_Pending.ToList();
            m_Pending.Clear();
            foreach (var (work, done) in pending)
            {
                try
                {
                    work();
                    done.SetResult(true);
                }
                catch (Exception e)
                {
                    done.SetException(e);
                }
            }
        }
    }

    private SynchronizationContext m_PreviousContext;
    private int m_Frame;
    private DeferredRunner m_Runner;

    [SetUp]
    public void SetUp()
    {
        // No context: an await on a read completed by the test continues inline.
        m_PreviousContext = SynchronizationContext.Current;
        SynchronizationContext.SetSynchronizationContext(null);
        m_Frame = 1;
        m_Runner = new DeferredRunner();
        Session.ReplaceCurrent();
    }

    [TearDown]
    public void TearDown()
    {
        SynchronizationContext.SetSynchronizationContext(m_PreviousContext);
        Session.ReplaceCurrent();
    }

    private static float[][] Ramp(int channels, int length)
    {
        // Channel c, sample i holds c * 1e6 + i: every value says where it came from.
        return Enumerable.Range(0, channels)
            .Select(c => Enumerable.Range(0, length).Select(i => (float)(c * 1000000 + i)).ToArray())
            .ToArray();
    }

    private BlockCache Cache(float[][] data, Func<float[][], ISampleSource> wrap = null)
    {
        ISampleSource source = new InMemorySampleSource(data, new Frequency(512));
        return new BlockCache(wrap != null ? wrap(data) : source, () => m_Frame, m_Runner.Run);
    }

    /// <summary>Asks for the window and completes reads until it hits (bounded), in one frame.</summary>
    private void Settle(BlockCache cache, long first, int count, int channel = 0)
    {
        float[] dst = new float[count];
        for (int i = 0; i < 20; i++)
        {
            cache.TryReadWindow(channel, first, count, dst);
            if (m_Runner.PendingCount == 0)
                return;
            m_Runner.CompleteAll();
        }
        Assert.Fail("The cache never settled");
    }

    private static long[] Loaded(BlockCache cache)
    {
        return Enumerable.Range(0, (int)cache.BlockCount).Where(b => cache.IsLoaded(b)).Select(b => (long)b).ToArray();
    }

    [Test]
    public void FirstRead_MissesThenReturnsTheSamplesOnceTheirBlocksAreLoaded()
    {
        float[][] data = Ramp(3, 3 * B);
        BlockCache cache = Cache(data);
        float[] dst = Enumerable.Repeat(-7f, 2000).ToArray();

        Assert.IsFalse(cache.TryReadWindow(2, B - 1000, 2000, dst), "nothing is loaded yet");
        CollectionAssert.AreEqual(Enumerable.Repeat(-7f, 2000), dst, "a miss leaves the destination as it was");
        Assert.AreEqual(2, cache.InFlightCount, "the window's two blocks load first");

        m_Runner.CompleteAll();

        Assert.IsTrue(cache.TryReadWindow(2, B - 1000, 2000, dst));
        CollectionAssert.AreEqual(data[2].Skip(B - 1000).Take(2000), dst);
    }

    [Test]
    public void AtMostTwoReadsRunAtOnce_TheWindowBeforeItsMargins()
    {
        BlockCache cache = Cache(Ramp(1, 8 * B));
        float[] dst = new float[3 * B];

        Assert.IsFalse(cache.TryReadWindow(0, 2 * B, 3 * B, dst));
        Assert.AreEqual(2, cache.InFlightCount);
        m_Runner.CompleteAll();
        CollectionAssert.AreEqual(new long[] { 2, 3 }, Loaded(cache), "window blocks, not the margin block 1");

        Assert.IsFalse(cache.TryReadWindow(0, 2 * B, 3 * B, dst), "block 4 is still missing");
        m_Runner.CompleteAll();
        Assert.IsTrue(cache.TryReadWindow(0, 2 * B, 3 * B, dst));
    }

    [Test]
    public void WindowEdges_ReadAsZerosOutsideTheRecording()
    {
        float[][] data = Ramp(1, 2 * B + 100);   // the last block holds 100 samples
        BlockCache cache = Cache(data);

        Settle(cache, -50, 200);
        float[] start = new float[200];
        Assert.IsTrue(cache.TryReadWindow(0, -50, 200, start));
        CollectionAssert.AreEqual(Enumerable.Repeat(0f, 50).Concat(data[0].Take(150)), start);

        Settle(cache, 2 * B + 50, 200);
        float[] end = new float[200];
        Assert.IsTrue(cache.TryReadWindow(0, 2 * B + 50, 200, end));
        CollectionAssert.AreEqual(data[0].Skip(2 * B + 50).Take(50).Concat(Enumerable.Repeat(0f, 150)), end);

        float[] outside = Enumerable.Repeat(-7f, 10).ToArray();
        Assert.IsTrue(cache.TryReadWindow(0, -100, 10, outside), "nothing to load before the start");
        CollectionAssert.AreEqual(new float[10], outside);
    }

    [Test]
    public void BlockSet_FollowsTheLongestWindowAskedFor()
    {
        // A 1-block trace window ending in block 50, then a 5-block one ending at the same
        // time (a longer trace period), then the short one again.
        BlockCache cache = Cache(Ramp(1, 100 * B));

        Settle(cache, 50 * B, 100);
        CollectionAssert.AreEqual(new long[] { 49, 50, 51 }, Loaded(cache), "the window and one block on each side");

        m_Frame++;
        Settle(cache, 46 * B, 4 * B + 100);
        CollectionAssert.AreEqual(new long[] { 45, 46, 47, 48, 49, 50, 51 }, Loaded(cache), "a longer period grows the set");

        m_Frame++;
        Settle(cache, 50 * B, 100);
        m_Frame++;
        Settle(cache, 50 * B, 100);
        CollectionAssert.AreEqual(new long[] { 49, 50, 51 }, Loaded(cache), "a shorter one shrinks it on the next frame");
    }

    [Test]
    public void BlockSet_IsKeptWhileNothingDraws()
    {
        BlockCache cache = Cache(Ramp(1, 100 * B));
        Settle(cache, 50 * B, 100);

        // Paused: frames go by without any request.
        m_Frame += 100;

        CollectionAssert.AreEqual(new long[] { 49, 50, 51 }, Loaded(cache));
        float[] dst = new float[100];
        Assert.IsTrue(cache.TryReadWindow(0, 50 * B, 100, dst), "resuming hits at once");
    }

    [Test]
    public void ABlockReadForAPreviousPatient_IsDropped()
    {
        BlockCache cache = Cache(Ramp(1, 4 * B));
        float[] dst = new float[100];
        Assert.IsFalse(cache.TryReadWindow(0, 0, 100, dst));

        Session.ReplaceCurrent();   // patient switch while the read runs
        m_Runner.CompleteAll();

        Assert.AreEqual(0, cache.LoadedBlockCount);
        Assert.AreEqual(0, cache.InFlightCount);
    }

    private sealed class FailingSource : ISampleSource
    {
        public int ChannelCount => 1;
        public long SampleCount => B;   // one block: one read, one warning
        public Frequency Frequency { get; } = new Frequency(512);
        public int ReadRange(long first, int count, int[] channels, float[][] dst) => throw new System.IO.IOException("share went away");
        public void Dispose() { }
    }

    [Test]
    public void AFailedRead_IsReportedAndAskedForAgain()
    {
        BlockCache cache = new BlockCache(new FailingSource(), () => m_Frame, m_Runner.Run);
        float[] dst = new float[100];
        Assert.IsFalse(cache.TryReadWindow(0, 0, 100, dst));

        LogAssert.Expect(LogType.Warning, new Regex("share went away"));
        m_Runner.CompleteAll();

        Assert.AreEqual(0, cache.LoadedBlockCount);
        Assert.AreEqual(0, cache.InFlightCount);
        Assert.IsFalse(cache.TryReadWindow(0, 0, 100, dst));
        Assert.AreEqual(1, cache.InFlightCount, "the next tick tries again");
    }

    [Test]
    public void Channel_TryReadWindowMatchesReadWindowOnceLoaded()
    {
        // EegSignal draws from TryReadWindow; centred, it must give what ReadWindow gives.
        float[][] data = Ramp(2, 3 * B);
        ISampleSource source = new InMemorySampleSource(data, new Frequency(512));
        BlockCache cache = new BlockCache(source, () => m_Frame, m_Runner.Run);
        ChannelStats[] stats = ChannelStats.Compute(source, new[] { 0, 1 });
        BtvChannel channel = new BtvChannel("A2", 1, source, 1, stats[1], cache);

        Settle(cache, -200, 2 * B, channel: 1);
        float[] cached = new float[2 * B];
        float[] direct = new float[2 * B];
        Assert.IsTrue(channel.TryReadWindow(-200, cached.Length, cached, true));
        channel.ReadWindow(-200, direct.Length, direct, true);

        CollectionAssert.AreEqual(direct, cached);
        Assert.IsTrue(channel.TryGetSample(B + 3, out float sample));
        Assert.AreEqual(data[1][B + 3], sample);
    }
}
