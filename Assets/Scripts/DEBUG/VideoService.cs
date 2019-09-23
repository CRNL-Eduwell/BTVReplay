using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace BTV.Services.VideoService
{
	public static class VideoService
	{
		//private static AudioDataContainer[]

        public static IEnumerator c_ExtractAudio(string AudioFilePath, string VideoFilePath)
        {
            FileInfo audioFileInfo = new FileInfo(AudioFilePath);
            if (!audioFileInfo.Exists)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = "/Applications/VLC.app/Contents/MacOS/VLC";
                startInfo.Arguments = "-I dummy --sout \"#transcode{acodec=s16l,channels=2,samplerate=11025}:std{access=file,mux=wav,dst=" + AudioFilePath + "}\" " + "\"" + VideoFilePath + "\" vlc://quit";

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
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "/Applications/VLC.app/Contents/MacOS/VLC";
            startInfo.Arguments = "-I dummy screen:// --screen-fps 25 --sout \"#transcode{vcodec=h264,venc=x264, vb=1500,acodec=none,scale=1.0}:std{access=file,mux=mp4,dst=" + OutputVideoPath + "}\" --stop-time " + durationInSeconds+ " vlc://quit";

            Process m_recordProcess = new Process();
            m_recordProcess.StartInfo = startInfo;
            m_recordProcess.Start();
            m_recordProcess.WaitForExit();

            yield return null;
        }

        public static void FilterAudioFromVideo()
		{

		}

		public static void LoadAudioTrace()
		{

		}
    }
}