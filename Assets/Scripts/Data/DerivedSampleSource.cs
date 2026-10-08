using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SimpleExpressionEngine;
using Tools.CSharp.EEG;

namespace BTV.Data
{
    /// <summary>
    /// The montage channels of one file that are expressions of its channels (a bipolar
    /// derivation, a re-reference), evaluated on the samples being read instead of over the
    /// whole recording up front. Expressions are point-wise, so evaluating a range needs only
    /// that range of the channels they name, read in one go per source. A montage channel used
    /// to be a full-length array; it now costs its 8192 stored samples.
    /// </summary>
    public sealed class DerivedSampleSource : ISampleSource
    {
        /// <summary>
        /// One expression, checked against the base channels and evaluated on their stored
        /// samples (same positions in every channel of a file): its statistics come from those,
        /// without reading the recording again.
        /// </summary>
        public sealed class Expression
        {
            public Node Node { get; }
            /// <summary>The base channels the expression names, once each.</summary>
            public IReadOnlyList<BtvChannel> Inputs { get; }
            public ChannelStats Stats { get; }

            private Expression(Node node, List<BtvChannel> inputs, ChannelStats stats)
            {
                Node = node;
                Inputs = inputs;
                Stats = stats;
            }

            /// <summary>
            /// Throws for a name that is not a base channel (InvalidDataException, naming it),
            /// as evaluating the montage over the whole recording did.
            /// </summary>
            public static Expression Prepare(Node node, List<BtvChannel> baseChannels, long sampleCount)
            {
                int stride = ChannelStats.StrideFor(sampleCount);
                int storedCount = (int)((sampleCount + stride - 1) / stride);
                StoredSampleContext context = new StoredSampleContext(baseChannels, stride);
                float[] stored = new float[storedCount];
                for (int k = 0; k < storedCount; k++)
                {
                    context.Index = k;
                    stored[k] = (float)node.Eval(context);
                }
                return new Expression(node, context.Inputs, ChannelStats.FromStoredSamples(stored, stride));
            }
        }

        private readonly Expression[] m_Expressions;

        public DerivedSampleSource(IReadOnlyList<Expression> expressions, Frequency frequency, long sampleCount)
        {
            m_Expressions = expressions.ToArray();
            Frequency = frequency;
            SampleCount = sampleCount;
        }

        public int ChannelCount => m_Expressions.Length;
        public long SampleCount { get; }
        public Frequency Frequency { get; }

        public int ReadRange(long first, int count, int[] channels, float[][] dst)
        {
            if (first < 0)
                throw new ArgumentOutOfRangeException(nameof(first), first, "The first sample cannot be negative.");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), count, "The sample count cannot be negative.");
            if (first >= SampleCount)
                return 0;
            int read = (int)Math.Min(count, SampleCount - first);

            // Every base channel the requested expressions name, read once, one read per source.
            RangeContext context = new RangeContext();
            IEnumerable<BtvChannel> inputs = channels.SelectMany(c => m_Expressions[c].Inputs).Distinct();
            foreach (IGrouping<ISampleSource, BtvChannel> group in inputs.GroupBy(input => input.Source))
            {
                BtvChannel[] sourceInputs = group.ToArray();
                float[][] buffers = new float[sourceInputs.Length][];
                for (int i = 0; i < buffers.Length; i++)
                    buffers[i] = new float[read];
                group.Key.ReadRange(first, read, sourceInputs.Select(input => input.SourceChannel).ToArray(), buffers);
                for (int i = 0; i < sourceInputs.Length; i++)
                    context.Values[sourceInputs[i].Label] = buffers[i];
            }

            for (int k = 0; k < channels.Length; k++)
            {
                Node node = m_Expressions[channels[k]].Node;
                float[] values = dst[k];
                for (int j = 0; j < read; j++)
                {
                    context.Index = j;
                    values[j] = (float)node.Eval(context);
                }
            }
            return read;
        }

        public void Dispose()
        {
        }

        /// <summary>Resolves names to base channels' stored samples, and records which it used.</summary>
        private sealed class StoredSampleContext : IContext
        {
            private readonly List<BtvChannel> m_Channels;
            private readonly int m_Stride;
            private readonly Dictionary<string, BtvChannel> m_Resolved = new Dictionary<string, BtvChannel>();

            public StoredSampleContext(List<BtvChannel> channels, int stride)
            {
                m_Channels = channels;
                m_Stride = stride;
            }

            public int Index { get; set; }
            public List<BtvChannel> Inputs { get; } = new List<BtvChannel>();

            public double ResolveVariable(string name)
            {
                if (!m_Resolved.TryGetValue(name, out BtvChannel channel))
                {
                    channel = m_Channels.FirstOrDefault(c => c.Label == name);
                    if (channel == null)
                        throw new InvalidDataException($"Unknown channel: '{name}'");
                    if (channel.Stats.Stride != m_Stride)
                        throw new InvalidDataException($"Channel '{name}' does not have the montage's sample count");
                    m_Resolved.Add(name, channel);
                    Inputs.Add(channel);
                }
                return channel.Stats.StoredSamples[Index];
            }

            public double CallFunction(string name, double[] arguments)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>Resolves names to the range just read, at <see cref="Index"/>; one per ReadRange call.</summary>
        private sealed class RangeContext : IContext
        {
            public Dictionary<string, float[]> Values { get; } = new Dictionary<string, float[]>();
            public int Index { get; set; }

            public double ResolveVariable(string name)
            {
                return Values[name][Index];
            }

            public double CallFunction(string name, double[] arguments)
            {
                throw new NotImplementedException();
            }
        }
    }
}
