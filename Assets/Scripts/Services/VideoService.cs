using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BTV.Data;
using Tools.CSharp.Audio;
using Tools.CSharp.EEG;
using BTV.Services.CalculationService;
using Assets.Scripts.Data.Files;
using System;
using UnityEngine.Events;
using CielaSpike;
using UnityEngine;
using System.Runtime.InteropServices;

namespace BTV.Services.VideoService
{
    public delegate void AudioDataLoaded();

    public static class VideoService
    {
        public static event AudioDataLoaded AudioDataLoaded;

        public static string OriginalVideoPath
        {
            get
            {
                return SubjectInfoService.SubjectInfoService.VideoPath;
            }
        }
        public static string AudioFromVideoPath
        {
            get
            {
                return Path.ChangeExtension(OriginalVideoPath, ".wav");
            }
        }
        public static string FilteredAudioPath
        {
            get
            {
                return AudioFromVideoPath.Replace(".wav", "_audio.csv");
            }
        }
        public static bool VideoFileExist
        {
            get
            {
                return OriginalVideoPath != "" ? new FileInfo(OriginalVideoPath).Exists : false;
            }
        }
        public static bool AudioFileExist
        {
            get
            {
                return AudioFromVideoPath != "" ? new FileInfo(AudioFromVideoPath).Exists : false;
            }
        }
        public static bool FilteredAudioFileExist
        {
            get
            {
                return FilteredAudioPath != "" ? new FileInfo(FilteredAudioPath).Exists : false;
            }
        }
        public static bool FilteredDataLoaded { get; private set; } = false;

        private static BtvProgram m_ProcessedAudio = null;
        private static AudioDataContainer m_RawAudioData = null;
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
            m_ProcessedAudio = null;
            m_RawAudioData = null;
            FilteredDataLoaded = false;
        }

        #region AudioProcessing
        public static IEnumerator c_ExtractAudio(string AudioFilePath, string VideoFilePath)
        {
            FileInfo audioFileInfo = new FileInfo(AudioFilePath);
            if (!audioFileInfo.Exists)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = m_VlcPath;
                //startInfo.Arguments = "-I dummy --sout \"#transcode{acodec=s16l,channels=2,samplerate=11025}:std{access=file,mux=wav,dst=" + AudioFilePath + "}\" " + "\"" + VideoFilePath + "\" vlc://quit";
                startInfo.Arguments = "-I dummy --sout \"#transcode{acodec=s16l,samplerate=11025}:std{access=file,mux=wav,dst=" + AudioFilePath + "}\" " + "\"" + VideoFilePath + "\" vlc://quit";

                Process process = new Process();
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
                yield return null;
            }
            else
            {
                yield return null;
            }
        }

        public static IEnumerator c_RecordVideoSnippet(string OutputVideoPath, string durationInSeconds)
        {
            BtvLog.Log("Record " + OutputVideoPath + " et duree " + durationInSeconds);
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = m_VlcPath;
            startInfo.Arguments = "-I dummy screen:// --screen-fps 25 --sout \"#transcode{vcodec=h264,venc=x264, vb=1500,acodec=none,scale=1.0}:std{access=file,mux=mp4,dst=" + OutputVideoPath + "}\" --stop-time " + durationInSeconds+ " vlc://quit";

            Process m_recordProcess = new Process();
            m_recordProcess.StartInfo = startInfo;
            m_recordProcess.Start();
            m_recordProcess.WaitForExit();

            yield return null;
        }

        public static IEnumerator c_LoadRawAudioFromFile(string RawAudioFromVideoPath)
        {
            m_RawAudioData = new AudioDataContainer(RawAudioFromVideoPath, AudioFile.AudioFileType.Wav);

            yield return null;
        }

        public static IEnumerator c_FilterAudioFromVideo(string FrequencyBands, int DownsampFreq)
        {
            Frequency DownsampledFrequency = new Frequency(DownsampFreq);

            float[] RawAudio = m_RawAudioData.ValuesByChannel.Values.ElementAt(0);
            int FilteredLength = Mathf.CeilToInt((float)RawAudio.Length / m_RawAudioData.Frequency.Value * DownsampledFrequency.Value);
            float[][] FilteredData = new float[6][];

            //Process Hilbert Enveloppe from raw signal
            FilteredData[0] = new float[FilteredLength];
            CalculationService.CalculationService.ToHilbert(RawAudio, RawAudio.Length, m_RawAudioData.Frequency.Value, FilteredData[0], FilteredLength, DownsampledFrequency.Value, FrequencyBands);

            //Convolve Hilbert Enveloppe according to different smoothing coefficient
            int[] WindowSmoothinginMs = { 0, 250, 500, 1000, 2500, 5000 };
            for (int i = 1; i < 6; i++)
            {
                FilteredData[i] = new float[FilteredLength];
                int NumberSample = DownsampledFrequency.ConvertToCeiledNumberOfSamples(WindowSmoothinginMs[i]);
                CalculationService.CalculationService.Convolution(FilteredData[0], FilteredLength, FilteredData[i], NumberSample);
            }

            //Save Data to prevent reprocessing each time
            CsvFile.SaveFloatDataHorizontally(FilteredAudioPath, FilteredData);

            //Load for use in traces and UI objects
            AudioDataContainer container = new AudioDataContainer(FilteredAudioPath, AudioFile.AudioFileType.Processed);
            m_ProcessedAudio = new BtvProgram(container);

            yield return Ninja.JumpToUnity;
            AudioDataLoaded.Invoke();
            FilteredDataLoaded = true;
            yield return Ninja.JumpBack;

            yield return null;
        }

        public static IEnumerator c_LoadFilteredAudioFromFile(string FilteredAudioFilePath)
        {
            AudioDataContainer container = new AudioDataContainer(FilteredAudioFilePath, AudioFile.AudioFileType.Processed);
            m_ProcessedAudio = new BtvProgram(container);

            yield return Ninja.JumpToUnity;
            AudioDataLoaded.Invoke();
            FilteredDataLoaded = true;
            yield return Ninja.JumpBack;

            yield return null;
        }
        #endregion

        public static BtvChannel GetSmoothedAudio(int ID)
        {
            if (m_ProcessedAudio == null)
                return null;

            if (ID >= m_ProcessedAudio.NumberOfElectrodes)
                throw new Exception("Error , wanted Audio channel ID is greater than the total number of channels");

            return m_ProcessedAudio.Channels[ID];
        }

        public static BtvProgram GetAudioContainer()
        {
            if (m_ProcessedAudio == null)
                return null;

            return m_ProcessedAudio;
        }
    }
}