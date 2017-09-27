using System;
using System.IO;
using System.Drawing;
using System.Diagnostics; //Requiered for Stopwatch
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;//Requiered for Event data.

using VLCSharp.Tools;
using VLCSharp.Interface;
using VLCSharp.VLCMemory;

namespace VLCSharp
{
    static class LibVlc
    {
        #region core
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_new(int argc, [MarshalAs(UnmanagedType.LPArray,
          ArraySubType = UnmanagedType.LPStr)] string[] argv);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_release(IntPtr instance);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_video_get_track(IntPtr instance);
        #endregion

        #region media
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_new_location(IntPtr p_instance,
          [MarshalAs(UnmanagedType.LPStr)] string psz_mrl);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_new_path(IntPtr p_instance,
          [MarshalAs(UnmanagedType.LPStr)] string path);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_release(IntPtr p_meta_desc);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_add_option(IntPtr media,
            [MarshalAs(UnmanagedType.LPStr)] string psz_options);
        #endregion

        #region video
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_video_set_callbacks(
            IntPtr player,
            IntPtr @lock,
            IntPtr unlock,
            IntPtr display,
            IntPtr opaque);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_video_set_format(IntPtr player,
            [MarshalAs(UnmanagedType.LPArray)] byte[] chroma,
            int width,
            int height,
            int pitch);
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_video_get_size(IntPtr player,
            int num,
            UInt32[] px,
            UInt32[] py);
        #endregion

        #region audio
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_audio_set_volume(IntPtr p_mi, int volume);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_audio_get_volume(IntPtr p_mi);
        #endregion

        #region media player
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_player_new_from_media(IntPtr media);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_player_release(IntPtr player);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_player_set_hwnd(IntPtr player, IntPtr drawable);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_player_get_media(IntPtr player);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_player_set_media(IntPtr player, IntPtr media);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_media_player_play(IntPtr player);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_player_pause(IntPtr player);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_player_stop(IntPtr player);
        #endregion

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_video_set_format_callbacks(IntPtr p_mi, IntPtr setup, IntPtr cleanup);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern long libvlc_media_player_get_time(IntPtr p_mi);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_player_set_time(IntPtr p_mi, long time);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern long libvlc_media_player_get_length(IntPtr p_mi);


        #region exception
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_clearerr();

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_errmsg();
    #endregion
}

    class VlcException : Exception
    {
        protected string _err;

        public VlcException()
            : base()
        {
            IntPtr errorPointer = LibVlc.libvlc_errmsg();
            _err = errorPointer == IntPtr.Zero ? "VLC Exception"
                : Marshal.PtrToStringAuto(errorPointer);
        }

        public override string Message { get { return _err; } }
    }

    public class VlcInstance : IDisposable
    {
        internal IntPtr Handle;

        public VlcInstance(string[] args)
        {
            Handle = LibVlc.libvlc_new(0, args);
            if (Handle == IntPtr.Zero)
            {
                throw new VlcException();
            }
        }

        public void Dispose()
        {
            LibVlc.libvlc_release(Handle);
            //GC.SuppressFinalize(this);
        }
    }

    public class VlcMedia : IDisposable
    {
        internal IntPtr Handle;

        public VlcMedia(VlcInstance instance, string url)
        {
            Handle = LibVlc.libvlc_media_new_location(instance.Handle, url);
            if (Handle == IntPtr.Zero)
            {
                throw new VlcException();
            }
        }

        internal VlcMedia(IntPtr handle)
        {
            this.Handle = handle;
        }

        public void Dispose()
        {
            LibVlc.libvlc_media_release(Handle);
            //GC.SuppressFinalize(this);
        }
    }

    public class VlcMediaPlayer : IDisposable
    {
        internal IntPtr Handle;
        private IntPtr drawable;
        private bool playing, paused;

        MemoryRenderer m_memRender = null;
        MemoryRendererEx m_memRenderEx = null;

        public VlcMediaPlayer(VlcMedia media)
        {
            Handle = LibVlc.libvlc_media_player_new_from_media(media.Handle);
            if (Handle == IntPtr.Zero) throw new VlcException();
        }

        public IMemoryRenderer CustomRenderer
        {
            get
            {
                if (m_memRenderEx != null)
                {
                    throw new InvalidOperationException("CustomRenderer is mutually exclusive with CustomRendererEx");
                }

                if (m_memRender == null)
                {
                    m_memRender = new MemoryRenderer(Handle);
                }
                return m_memRender;
            }
        }

        public IMemoryRendererEx CustomRendererEx
        {
            get
            {
                if (m_memRender != null)
                {
                    throw new InvalidOperationException("CustomRendererEx is mutually exclusive with CustomRenderer");
                }

                if (m_memRenderEx == null)
                {
                    m_memRenderEx = new MemoryRendererEx(Handle);
                }
                return m_memRenderEx;
            }
        }

        public void Dispose()
        {
            LibVlc.libvlc_media_player_release(Handle);
            GC.SuppressFinalize(this);
        }

        public IntPtr Drawable
        {
            get
            {
                return drawable;
            }
            set
            {
                LibVlc.libvlc_media_player_set_hwnd(Handle, value);
                drawable = value;
            }
        }

        public VlcMedia Media
        {
            get
            {
                IntPtr media = LibVlc.libvlc_media_player_get_media(Handle);
                if (media == IntPtr.Zero) return null;
                return new VlcMedia(media);
            }
            set
            {
                LibVlc.libvlc_media_player_set_media(Handle, value.Handle);
            }
        }

        public bool IsPlaying { get { return playing && !paused; } }

        public bool IsPaused { get { return playing && paused; } }

        public bool IsStopped { get { return !playing; } }

        public long currentTime { get { return LibVlc.libvlc_media_player_get_time(Handle); } }

        public long totalVideoTime { get { return LibVlc.libvlc_media_player_get_length(Handle); } }

        public int volume { get { return LibVlc.libvlc_audio_get_volume(Handle); } }

        public void setTime(long timeMS)
        {
            LibVlc.libvlc_media_player_set_time(Handle, timeMS);
        }

        public bool SetVolume(int volume)
        {
            int volumeSet = LibVlc.libvlc_audio_set_volume(Handle, volume);

            if (volumeSet == 0)
                return true;
            else
                return false;
        }

        //TODO : use trackID and getSize to init video and texture to real size of video
        public int TrackId
        {
            get
            {
                return LibVlc.libvlc_video_get_track(Handle);
            }
        }

        public void getSize(uint[] width, uint[] height)
        {
            LibVlc.libvlc_video_get_size(Handle, TrackId, width, height);
        }

        public void Play()
        {
            int ret = LibVlc.libvlc_media_player_play(Handle);
            if (ret == -1)
                throw new VlcException();

            playing = true;
            paused = false;
        }

        public void Pause()
        {
            LibVlc.libvlc_media_player_pause(Handle);

            if (playing)
                paused ^= true;
        }

        public void Stop()
        {
            LibVlc.libvlc_media_player_stop(Handle);

            playing = false;
            paused = false;
        }
    }

    public class VLCSharp : MonoBehaviour, IVideoPlayer
    {
        public long currentTime
        {
            get
            {
                long currentTime = player.currentTime;
                if (lastPlayTime == currentTime && lastPlayTime != 0)
                {
                    currentTime += (long)stopwatch.Elapsed.TotalMilliseconds - lastPlayTimeGlobal;
                }
                else
                {
                    lastPlayTime = currentTime;
                    lastPlayTimeGlobal = (long)stopwatch.Elapsed.TotalMilliseconds;
                }
                return currentTime + (offsetVideoSec * 1000);
            }
        }
        public long time
        {
            get
            {
                return (long)(currentTime * ((float)eegSampFreq / 1000));
            }
        }
        public long totalVideoTime
        {
            get
            {
                return player.totalVideoTime;
            }
        }
        public bool isPlaying
        {
            get
            {
                return player.IsPlaying;
            }
        }
        public bool isPaused
        {
            get
            {
                return player.IsPaused;
            }
        }
        public bool isStopped
        {
            get
            {
                return player.IsStopped;
            }
        }
        public byte[] textureBytes
        {
            get
            {
                return textureByteArray;
            }
        }
        //===
        private string videoPath = "";
        private int eegSampFreq = 0;
        private long eegFileDurationInSec = 0;
        //===
        VlcMediaPlayer player = null;
        VlcInstance instance = null;
        RawImage Tex2Draw = null;
        optionsHub hub = null;
        Bitmap picCopy = null;
        byte[] textureByteArray;
        bool newPic = false;
        //===
        Stopwatch stopwatch;
        long lastPlayTime = 0;
        long lastPlayTimeGlobal = 0;
        int offsetVideoSec = 0;
        //===
        private object objectLock = new object();

        public void init(string videoPath, int eegSampFreq, int eegFileDurationInSec)
        {
            this.videoPath = videoPath;
            this.eegSampFreq = eegSampFreq;
            this.eegFileDurationInSec = eegFileDurationInSec;
            stopwatch = new Stopwatch();
            instance = new VlcInstance(new string[] { "" });
            stopwatch = new Stopwatch();
            stopwatch.Start();

            using (VlcMedia media = new VlcMedia(instance, "file:///" + videoPath))
            {
                if (player == null)
                {
                    player = new VlcMediaPlayer(media);

                    IMemoryRenderer memRender = player.CustomRenderer;
                    memRender.SetCallback(delegate (Bitmap frame)
                    {
                        picCopy = frame.Clone(new RectangleF(0, 0, frame.Width, frame.Height), PixelFormat.Format32bppArgb);
                        //===
                        //Memory stream to store the bitmap data.
                        MemoryStream ms = new MemoryStream();
                        //Save to that memory stream.
                        picCopy.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        //Go to the beginning of the memory stream.
                        ms.Seek(0, SeekOrigin.Begin);
                        textureByteArray = ms.ToArray();
                        //Close the stream.
                        ms.Close();
                        ms = null;
                        //===
                        picCopy.Dispose();
                        newPic = true;
                    });

                    //the size of the bitmap format need to be the same as 
                    //the texture on unity Otherwise performance issue
                    memRender.SetFormat(new BitmapFormat(512, 512, ChromaType.RV32));
                }
                else
                {
                    player.Media = media;
                }
            }

            hub.videoRemote.offsetVideoHasChanged += new offsetVideoChangedEventHandler(
                delegate (int newVal)
                {
                    offsetVideoSec = newVal;
                });

            setVolume(0.5f);
        }

        public void getVideoReference(RawImage tex, optionsHub hubOpt)
        {
            Tex2Draw = tex;
            hub = hubOpt;
        }

        public void cleanup()
        {
            //remove offset video event
            hub.videoRemote.offsetVideoHasChanged -= new offsetVideoChangedEventHandler(
                delegate (int newVal)
                {
                    offsetVideoSec = newVal;
                });

            //to release resources
            if (player != null)
            {
                player.Stop();
                player.Dispose();
                player = null;
            }

            instance = new VlcInstance(new string[] { "" });
        }

        public void update()
        {
            if (eegSampFreq != 0 && newPic && player.IsPlaying)
            {
               ((Texture2D)Tex2Draw.texture).LoadImage(textureByteArray);
                newPic = false;
                //sendTimeEvent((int)time + offsetVideoSec);
            }
        }

        public void play()
        {
            if (player.IsPaused || player.IsStopped)
            {
                player.Play();
                stopwatch.Start();
            }
        }

        public void pause()
        {
            if (player.IsPlaying)
            {
                player.Pause();
                stopwatch.Stop();
            }
        }

        public void stop()
        {
            player.Stop();
            stopwatch.Stop();
        }

        public void moveTime(long secondsToAdd)
        {
            player.setTime(currentTime + (secondsToAdd * 1000));
        }

        public void setTime(long timeMilliSec)
        {
            player.setTime(timeMilliSec);
        }

        public void setVolume(float volume)
        {
            int maxPercentVideo = 200;
            player.SetVolume((int)(volume * maxPercentVideo));
        }

        static void Image2Texture(System.Drawing.Image im, Texture2D myTex)
        {
            //Memory stream to store the bitmap data.
            MemoryStream ms = new MemoryStream();
            //Save to that memory stream.
            im.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            //Go to the beginning of the memory stream.
            ms.Seek(0, SeekOrigin.Begin);
            //load in tex
            myTex.LoadImage(ms.ToArray());
            //Close the stream.
            ms.Close();
            ms = null;
        }
    }

}