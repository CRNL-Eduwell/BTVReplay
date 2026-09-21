using System;
using System.Collections.Generic;
using BTV.Data;
using BTV.Services.VideoService;
using Tools.CSharp.Audio;
using UnityEngine;

namespace BTV.Services
{
    /// <summary>
    /// Owns all mutable state belonging to the patient currently loaded in the viewer.
    ///
    /// Static services remain as compatibility facades for now, but their state lives here.
    /// Replacing Current gives a patient switch one atomic identity boundary and lets async
    /// continuations reject results produced for a disposed session.
    /// </summary>
    public sealed class Session : IDisposable
    {
        private static Session m_Current = new Session();

        public static Session Current { get { return m_Current; } }
        public bool IsDisposed { get; private set; }

        internal Subject Subject { get; set; }
        internal string ExamLabel { get; set; } = "";
        internal int ExamIndex { get; set; } = -1;

        internal List<BtvMontage> Montages { get; set; } = CreateDefaultMontages();
        internal int SelectedMontageID { get; set; }

        internal List<BtvEvent> Events { get; set; } = new List<BtvEvent>();
        internal Dictionary<int, TraceOption> TraceOptions { get; set; } = new Dictionary<int, TraceOption>();
        internal AudioTraceOption AudioTraceOption { get; set; }
        internal BtvEvent BaselineEvent { get; set; }
        internal Dictionary<int, TfTraceOption> TfTraceOptions { get; set; } = new Dictionary<int, TfTraceOption>();

        internal Dictionary<string, List<AnatomicalSite>> SitesPerReferential { get; set; } = new Dictionary<string, List<AnatomicalSite>>();
        internal MarsAtlas Atlas { get; set; }

        internal BtvProgram ProcessedAudio { get; set; }
        internal AudioDataContainer RawAudioData { get; set; }
        internal bool FilteredDataLoaded { get; set; }
        internal AudioDataLoaded AudioDataLoadedHandlers { get; set; }

        internal List<EegTrigger> ProcessedTriggers { get; set; }
        internal List<Color> TaskPerformanceColors { get; set; }
        internal Dictionary<int, string> CodeComments { get; set; } = new Dictionary<int, string>();

        /// <summary>
        /// Starts a fresh patient lifetime. Call on the Unity main thread before reset messages
        /// are published so every compatibility service immediately forwards to the new state.
        /// </summary>
        public static Session ReplaceCurrent()
        {
            Session previous = m_Current;
            m_Current = new Session();
            previous.Dispose();
            return m_Current;
        }

        public static bool IsCurrent(Session session)
        {
            return session != null && !session.IsDisposed && ReferenceEquals(session, m_Current);
        }

        public void Dispose()
        {
            if (IsDisposed) return;

            IsDisposed = true;
            AudioDataLoadedHandlers = null;
            Subject = null;
            Montages = null;
            Events = null;
            TraceOptions = null;
            AudioTraceOption = null;
            BaselineEvent = null;
            TfTraceOptions = null;
            SitesPerReferential = null;
            Atlas?.Dispose();
            Atlas = null;
            ProcessedAudio = null;
            RawAudioData = null;
            ProcessedTriggers = null;
            TaskPerformanceColors = null;
            CodeComments = null;
        }

        internal static List<BtvMontage> CreateDefaultMontages()
        {
            return new List<BtvMontage>
            {
                new BtvMontage("Default", new BtvProgram[6])
            };
        }
    }
}
