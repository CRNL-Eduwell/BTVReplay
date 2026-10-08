using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BTV.Data;
using NUnit.Framework;
using Tools.CSharp.EEG;
using UnityEngine;

/// <summary>
/// Edit-mode tests for the overview strip's normalisation baseline (review L-5). The strip used to
/// read the baseline's extremes synchronously on the main thread, on every normalise, electrode or
/// file switch: on a large multiplexed file a long baseline froze the app for seconds. These pin
/// that the read never happens on the calling thread, happens once per baseline, and is retried
/// after a failure.
/// </summary>
public class OverviewBaselineTests
{
    private sealed class CountingSource : ISampleSource
    {
        private readonly InMemorySampleSource m_Inner;
        private int m_Reads;

        public CountingSource(float[][] channels) { m_Inner = new InMemorySampleSource(channels, new Frequency(512)); }

        public int Reads => Volatile.Read(ref m_Reads);
        public int ChannelCount => m_Inner.ChannelCount;
        public long SampleCount => m_Inner.SampleCount;
        public Frequency Frequency => m_Inner.Frequency;

        public int ReadRange(long first, int count, int[] channels, float[][] dst)
        {
            Interlocked.Increment(ref m_Reads);
            return m_Inner.ReadRange(first, count, channels, dst);
        }

        public void Dispose() { }
    }

    /// <summary>Holds each read instead of running it, so a test decides when it runs.</summary>
    private sealed class HeldRunner
    {
        public readonly List<(Func<(float, float)> work, TaskCompletionSource<(float, float)> done)> Reads =
            new List<(Func<(float, float)>, TaskCompletionSource<(float, float)>)>();

        public Task<(float min, float max)> Run(Func<(float min, float max)> work)
        {
            var done = new TaskCompletionSource<(float, float)>();
            Reads.Add((work, done));
            return done.Task;
        }

        public void Complete(int index) => Reads[index].done.SetResult(Reads[index].work());
    }

    private static float[] Signal(int length, float offset)
    {
        return Enumerable.Range(0, length).Select(i => offset + 100f * (float)Math.Sin(i * 0.003)).ToArray();
    }

    private static (CountingSource source, BtvChannel a, BtvChannel b) TwoChannels()
    {
        CountingSource source = new CountingSource(new[] { Signal(20000, 0), Signal(20000, 500) });
        ChannelStats[] stats = ChannelStats.Compute(source, new[] { 0, 1 });
        BlockCache cache = new BlockCache(source);
        return (source, new BtvChannel("A1", 0, source, 0, stats[0], cache), new BtvChannel("A2", 1, source, 1, stats[1], cache));
    }

    [Test]
    public void Get_NeverReadsOnTheCallingThread()
    {
        var (source, a, _) = TwoChannels();
        int readsBefore = source.Reads;
        HeldRunner runner = new HeldRunner();
        OverviewBaseline baseline = new OverviewBaseline(runner.Run);

        Task<(float min, float max)> read = baseline.Get(a, 1000, 9000);

        Assert.IsFalse(read.IsCompleted, "the extremes are not known until the read has run");
        Assert.AreEqual(readsBefore, source.Reads, "Get must not touch the file itself");
        Assert.AreEqual(1, runner.Reads.Count);
    }

    [Test]
    public void Get_SameBaseline_ReadsOnceAndKeepsTheExtremes()
    {
        var (source, a, _) = TwoChannels();
        HeldRunner runner = new HeldRunner();
        OverviewBaseline baseline = new OverviewBaseline(runner.Run);

        Task<(float min, float max)> first = baseline.Get(a, 1000, 9000);
        Assert.AreSame(first, baseline.Get(a, 1000, 9000), "a second request while the read is in flight waits for the same read");
        Assert.AreEqual(1, runner.Reads.Count);

        runner.Complete(0);
        int readsAfter = source.Reads;
        Task<(float min, float max)> again = baseline.Get(a, 1000, 9000);

        Assert.AreEqual(TaskStatus.RanToCompletion, again.Status);
        Assert.AreEqual(readsAfter, source.Reads, "switching back to a baseline already read does not read the file again");
        Assert.AreEqual(1, runner.Reads.Count);
        Assert.AreEqual(a.MinMax(1000, 9000), again.Result);
    }

    [Test]
    public void Get_OtherChannelOrBaseline_StartsItsOwnRead()
    {
        var (_, a, b) = TwoChannels();
        HeldRunner runner = new HeldRunner();
        OverviewBaseline baseline = new OverviewBaseline(runner.Run);

        Task<(float min, float max)> onA = baseline.Get(a, 1000, 9000);
        Task<(float min, float max)> onB = baseline.Get(b, 1000, 9000);
        Task<(float min, float max)> otherWindow = baseline.Get(a, 2000, 9000);

        Assert.AreEqual(3, runner.Reads.Count);
        runner.Complete(0);
        runner.Complete(1);
        runner.Complete(2);
        Assert.AreEqual(a.MinMax(1000, 9000), onA.Result);
        Assert.AreEqual(b.MinMax(1000, 9000), onB.Result);
        Assert.AreEqual(a.MinMax(2000, 9000), otherWindow.Result);
        Assert.AreNotEqual(onA.Result, onB.Result, "each channel gets its own extremes");
    }

    [Test]
    public void Get_AfterAFailedRead_TriesAgain()
    {
        var (_, a, _) = TwoChannels();
        HeldRunner runner = new HeldRunner();
        OverviewBaseline baseline = new OverviewBaseline(runner.Run);

        Task<(float min, float max)> failed = baseline.Get(a, 1000, 9000);
        runner.Reads[0].done.SetException(new IOException("network share gone"));
        Assert.IsTrue(failed.IsFaulted);

        Task<(float min, float max)> retry = baseline.Get(a, 1000, 9000);
        Assert.AreNotSame(failed, retry);
        Assert.AreEqual(2, runner.Reads.Count);
        runner.Complete(1);
        Assert.AreEqual(a.MinMax(1000, 9000), retry.Result);
    }

    [Test]
    public void Get_DefaultRunner_ReadsOnAWorker()
    {
        var (_, a, _) = TwoChannels();
        int caller = Thread.CurrentThread.ManagedThreadId;
        int reader = caller;
        OverviewBaseline baseline = new OverviewBaseline(work => Task.Run(() => { reader = Thread.CurrentThread.ManagedThreadId; return work(); }));

        (float min, float max) extremes = baseline.Get(a, 0, 20000).Result;

        Assert.AreNotEqual(caller, reader);
        Assert.AreEqual(a.MinMax(0, 20000), extremes);
    }

    [Test]
    public void TracesDisplayer_DoesNotReadTheBaselineItself()
    {
        // Comments may name the old call; only code counts.
        string source = Regex.Replace(
            System.IO.File.ReadAllText(Path.Combine(Application.dataPath, "Scripts/Traces/TracesDisplayer/TracesDisplayer.cs")),
            @"//[^\n]*", "");

        StringAssert.DoesNotContain(".MinMax(", source, "the baseline extremes come from OverviewBaseline, read off the main thread");
        StringAssert.Contains("OverviewBaseline", source);
    }
}
