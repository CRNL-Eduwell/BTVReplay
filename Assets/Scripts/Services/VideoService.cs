using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BTV.Data;
using Tools.CSharp.Audio;
using Tools.CSharp.EEG;
using BTV.Services.CalculationService;
using Assets.Scripts.Data.Files;
using UnityEngine;
using System.Runtime.InteropServices;

namespace BTV.Services.VideoService
{
    public delegate void AudioDataLoaded();

    public static class VideoService
    {
        public static event AudioDataLoaded AudioDataLoaded
        {
            add { Session.Current.AudioDataLoadedHandlers += value; }
            remove { Session.Current.AudioDataLoadedHandlers -= value; }
        }

        public static void SubscribeAudioDataLoaded(Session session, AudioDataLoaded handler)
        {
            session.AudioDataLoadedHandlers += handler;
        }

        public static void UnsubscribeAudioDataLoaded(Session session, AudioDataLoaded handler)
        {
            session.AudioDataLoadedHandlers -= handler;
        }

        public static string OriginalVideoPath
        {
            get { return GetOriginalVideoPath(Session.Current); }
        }
        public static string AudioFromVideoPath
        {
            get { return GetAudioFromVideoPath(Session.Current); }
        }
        public static string FilteredAudioPath
        {
            get { return GetFilteredAudioPath(Session.Current); }
        }
        public static bool VideoFileExist
        {
            get { return VideoFileExists(Session.Current); }
        }
        public static bool AudioFileExist
        {
            get { return AudioFileExists(Session.Current); }
        }
        public static bool FilteredAudioFileExist
        {
            get { return FilteredAudioFileExists(Session.Current); }
        }
        public static bool FilteredDataLoaded
        {
            get { return Session.Current.FilteredDataLoaded; }
            private set { Session.Current.FilteredDataLoaded = value; }
        }

        public static bool IsFilteredDataLoaded(Session session)
        {
            return session.FilteredDataLoaded;
        }

        public static string GetOriginalVideoPath(Session session)
        {
            return SubjectInfoService.SubjectInfoService.GetVideoPath(session);
        }

        public static string GetAudioFromVideoPath(Session session)
        {
            return Path.ChangeExtension(GetOriginalVideoPath(session), ".wav");
        }

        public static string GetFilteredAudioPath(Session session)
        {
            return GetAudioFromVideoPath(session).Replace(".wav", "_audio.csv");
        }

        public static bool VideoFileExists(Session session)
        {
            string path = GetOriginalVideoPath(session);
            return path != "" && new FileInfo(path).Exists;
        }

        public static bool AudioFileExists(Session session)
        {
            string path = GetAudioFromVideoPath(session);
            return path != "" && new FileInfo(path).Exists;
        }

        public static bool FilteredAudioFileExists(Session session)
        {
            string path = GetFilteredAudioPath(session);
            return path != "" && new FileInfo(path).Exists;
        }
        /// <summary>
        /// Whether the VLC executable is present at the configured path. UI flows that need VLC
        /// check this first and show <see cref="VlcMissingMessage"/> as a plain dialog - a
        /// missing VLC is a configuration state, not a program error.
        /// </summary>
        public static bool VlcFileExist
        {
            get
            {
                string vlcPath = m_VlcPath;
                return !string.IsNullOrEmpty(vlcPath) && System.IO.File.Exists(vlcPath);
            }
        }
        public static string VlcMissingMessage
        {
            get
            {
                return "VLC was not found at \"" + m_VlcPath + "\".\nInstall VLC or set its installation folder in the user preferences.";
            }
        }

        private static BtvProgram ProcessedAudio
        {
            get { return Session.Current.ProcessedAudio; }
            set { Session.Current.ProcessedAudio = value; }
        }
        private static AudioDataContainer RawAudioData
        {
            get { return Session.Current.RawAudioData; }
            set { Session.Current.RawAudioData = value; }
        }
        private static string m_VlcPath
        {
            get
            {
                string userPrefPath = UserPreferencesService.UserPreferencesService.UserPreferences.GeneralPreferences.VlcPath;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return string.IsNullOrEmpty(userPrefPath) ? "C:\\Program Files (x86)\\VideoLAN\\VLC\\vlc.exe" : (userPrefPath + "\\vlc.exe");
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return "";
                }
                else if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return string.IsNullOrEmpty(userPrefPath) ? "/Applications/VLC.app/Contents/MacOS/VLC" : (userPrefPath + "/VLC.app/Contents/MacOS/VLC");
                }
                else
                {
                    return "";
                }
            }
        }

        public static void Reset()
        {
            ProcessedAudio = null;
            RawAudioData = null;
            FilteredDataLoaded = false;
        }

        #region AudioProcessing
        // Each method gathers what it needs on the main thread, runs the blocking work (VLC
        // process, file IO, DSP) inside Task.Run, and publishes results to the static fields
        // after the await - i.e. back on the Unity main thread.

        // The output path is spliced into VLC's --sout chain, where , { } separate or close
        // options and quotes end the argument: such a path used to make VLC silently write
        // somewhere else or fail. VLC's own quoting treats backslashes specially, which would
        // risk Windows paths, so refuse those characters with a message instead.
        public static string SoutPath(string path)
        {
            if (path.IndexOfAny(new[] { ',', '{', '}', '"', '\'' }) >= 0)
                throw new ArgumentException("VLC cannot write to a path containing , { } or quotes. Choose another folder or file name:\n" + path);
            return path;
        }

        // Backstop for callers that skipped the VlcFileExist check. Reads the user preferences,
        // so call it on the main thread. Without this check, Process.Start fails with an
        // unhelpful "Cannot find the specified file".
        private static string ResolveVlcPathOrThrow()
        {
            if (!VlcFileExist)
                throw new FileNotFoundException(VlcMissingMessage);
            return m_VlcPath;
        }
        public static async Task ExtractAudioAsync(string AudioFilePath, string VideoFilePath)
        {
            FileInfo audioFileInfo = new FileInfo(AudioFilePath);
            if (audioFileInfo.Exists)
                return;

            string vlcPath = ResolveVlcPathOrThrow();
            await Task.Run(() =>
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = vlcPath;
                //startInfo.Arguments = "-I dummy --sout \"#transcode{acodec=s16l,channels=2,samplerate=11025}:std{access=file,mux=wav,dst=" + AudioFilePath + "}\" " + "\"" + VideoFilePath + "\" vlc://quit";
                startInfo.Arguments = "-I dummy --sout \"#transcode{acodec=s16l,samplerate=11025}:std{access=file,mux=wav,dst=" + SoutPath(AudioFilePath) + "}\" " + "\"" + VideoFilePath + "\" vlc://quit";

                using (Process process = new Process())
                {
                    process.StartInfo = startInfo;
                    process.Start();
                    process.WaitForExit();
                }
            });
        }

        public static async Task RecordVideoSnippetAsync(string OutputVideoPath, string durationInSeconds)
        {
            BtvLog.Log("Record " + OutputVideoPath + " et duree " + durationInSeconds);
            string vlcPath = ResolveVlcPathOrThrow();
            await Task.Run(() =>
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = vlcPath;
                startInfo.Arguments = "-I dummy screen:// --screen-fps 25 --sout \"#transcode{vcodec=h264,venc=x264, vb=1500,acodec=none,scale=1.0}:std{access=file,mux=mp4,dst=" + SoutPath(OutputVideoPath) + "}\" --stop-time " + durationInSeconds + " vlc://quit";

                using (Process recordProcess = new Process())
                {
                    recordProcess.StartInfo = startInfo;
                    recordProcess.Start();
                    recordProcess.WaitForExit();
                }
            });
        }

        public static async Task LoadRawAudioFromFileAsync(string RawAudioFromVideoPath)
        {
            await LoadRawAudioFromFileAsync(Session.Current, RawAudioFromVideoPath);
        }

        public static async Task LoadRawAudioFromFileAsync(Session session, string RawAudioFromVideoPath)
        {
            AudioDataContainer rawAudioData = await Task.Run(() => new AudioDataContainer(RawAudioFromVideoPath, AudioFile.AudioFileType.Wav));
            TryPublishRawAudio(session, rawAudioData);
        }

        private static bool TryPublishRawAudio(Session session, AudioDataContainer rawAudioData)
        {
            if (!Session.IsCurrent(session))
            {
                BtvLog.Log("Discarded raw audio loaded for a previous patient session.");
                return false;
            }

            session.RawAudioData = rawAudioData;
            return true;
        }

        public static async Task FilterAudioFromVideoAsync(string FrequencyBands, int DownsampFreq)
        {
            await FilterAudioFromVideoAsync(Session.Current, FrequencyBands, DownsampFreq);
        }

        public static async Task FilterAudioFromVideoAsync(Session session, string FrequencyBands, int DownsampFreq)
        {
            AudioDataContainer rawAudioData = session.RawAudioData;
            // Resolves SubjectInfoService-backed patient state on the main thread; do not move
            // this lookup into the Task.Run worker below.
            string filteredAudioPath = GetFilteredAudioPath(session);

            BtvProgram processedAudio = await Task.Run(() =>
            {
                Frequency DownsampledFrequency = new Frequency(DownsampFreq);

                float[] RawAudio = rawAudioData.ValuesByChannel.Values.ElementAt(0);
                int FilteredLength = Mathf.CeilToInt((float)RawAudio.Length / rawAudioData.Frequency.Value * DownsampledFrequency.Value);
                float[][] FilteredData = new float[6][];

                //Process Hilbert Enveloppe from raw signal
                FilteredData[0] = new float[FilteredLength];
                CalculationService.CalculationService.ToHilbert(RawAudio, RawAudio.Length, rawAudioData.Frequency.Value, FilteredData[0], FilteredLength, DownsampledFrequency.Value, FrequencyBands);

                //Convolve Hilbert Enveloppe according to different smoothing coefficient
                int[] WindowSmoothinginMs = { 0, 250, 500, 1000, 2500, 5000 };
                for (int i = 1; i < 6; i++)
                {
                    FilteredData[i] = new float[FilteredLength];
                    int NumberSample = DownsampledFrequency.ConvertToCeiledNumberOfSamples(WindowSmoothinginMs[i]);
                    CalculationService.CalculationService.Convolution(FilteredData[0], FilteredLength, FilteredData[i], NumberSample);
                }

                //Save Data to prevent reprocessing each time
                CsvFile.SaveFloatDataHorizontally(filteredAudioPath, FilteredData);

                //Load for use in traces and UI objects
                AudioDataContainer container = new AudioDataContainer(filteredAudioPath, AudioFile.AudioFileType.Processed);
                return new BtvProgram(container);
            });

            TryPublishProcessedAudio(session, processedAudio, "produced");
        }

        public static async Task LoadFilteredAudioFromFileAsync(string FilteredAudioFilePath)
        {
            await LoadFilteredAudioFromFileAsync(Session.Current, FilteredAudioFilePath);
        }

        public static async Task LoadFilteredAudioFromFileAsync(Session session, string FilteredAudioFilePath)
        {
            BtvProgram processedAudio = await Task.Run(() =>
            {
                AudioDataContainer container = new AudioDataContainer(FilteredAudioFilePath, AudioFile.AudioFileType.Processed);
                return new BtvProgram(container);
            });

            TryPublishProcessedAudio(session, processedAudio, "loaded");
        }

        private static bool TryPublishProcessedAudio(Session session, BtvProgram processedAudio, string operation)
        {
            if (!Session.IsCurrent(session))
            {
                BtvLog.Log("Discarded filtered audio " + operation + " for a previous patient session.");
                return false;
            }

            session.ProcessedAudio = processedAudio;
            session.AudioDataLoadedHandlers?.Invoke();
            session.FilteredDataLoaded = true;
            return true;
        }
        #endregion

        public static BtvChannel GetSmoothedAudio(int ID)
        {
            return GetSmoothedAudio(Session.Current, ID);
        }

        public static BtvChannel GetSmoothedAudio(Session session, int ID)
        {
            BtvProgram processedAudio = session.ProcessedAudio;
            if (processedAudio == null)
                return null;

            if (ID >= processedAudio.NumberOfElectrodes)
                throw new Exception("Error , wanted Audio channel ID is greater than the total number of channels");

            return processedAudio.Channels[ID];
        }

        public static BtvProgram GetAudioContainer()
        {
            return GetAudioContainer(Session.Current);
        }

        public static BtvProgram GetAudioContainer(Session session)
        {
            return session.ProcessedAudio;
        }
    }
}
