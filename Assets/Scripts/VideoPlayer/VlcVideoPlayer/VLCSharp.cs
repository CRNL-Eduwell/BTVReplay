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
        public static extern IntPtr libvlc_new(int argc,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] argv);

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
            out uint px,
            out uint py);
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
                VlcException exept = new VlcException();
                UnityEngine.Debug.LogError(exept.Message);
                throw exept;
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
            //GC.SuppressFinalize(this);
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

        public int TrackId
        {
            get
            {
                return LibVlc.libvlc_video_get_track(Handle) - 1;
            }
        }

        public Size getSize()
        {
            uint a = 0;
            uint b = 0;
            LibVlc.libvlc_video_get_size(Handle, TrackId, out a, out b);
            return new Size((int)a, (int)b);
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

    /// <summary>
    /// Represents an instance of a Video Reader using VLC lib
    /// </summary>
    public class VLCSharp : MonoBehaviour, IVideoPlayer
    {
        public bool IsPrepared { get { return false; } } //vlc sharp not used at the moment, just to prevent compile error

        /// <summary>
        /// Exact Time of the video without a possible offset, there is a possible offset due to user input
        /// This is an extrapolation of the time returned by VLC API since we need a greater precision
        /// In MilliSeconds
        /// </summary>
        public long CurrentTime
        {
            get
            {
                long currentTime = m_VideoPlayer.currentTime;
                if (m_lastPlayTime == currentTime && m_lastPlayTime != 0)
                {
                    currentTime += (long)m_stopwatch.Elapsed.TotalMilliseconds - m_lastPlayTimeGlobal;
                }
                else
                {
                    m_lastPlayTime = currentTime;
                    m_lastPlayTimeGlobal = (long)m_stopwatch.Elapsed.TotalMilliseconds;
                }
                return currentTime + m_offsetVideoMilliSec;
            }
        }

        /// <summary>
        /// Time of the video, there is a possible offset due to user input
        /// In MilliSeconds
        /// </summary>
        public long Time
        {
            get
            {
                return (long)(CurrentTime);
            }
        }

        /// <summary>
        /// Exact Time of the video without a possible offset
        /// In MilliSeconds
        /// </summary>
        public long VideoTime
        {
            get
            {
                return (long)((CurrentTime - m_offsetVideoMilliSec));
            }
        }

        /// <summary>
        /// Total Duration of the Video
        /// In MilliSeconds
        /// </summary>
        public long TotalVideoTime
        {
            get
            {
                if (m_VideoPlayer.totalVideoTime != -1)
                    return m_VideoPlayer.totalVideoTime;
                else
                    return m_eegFileDurationInSec * 1000;
            }
        }
        public bool IsPlaying
        {
            get
            {
                return m_VideoPlayer.IsPlaying;
            }
        }
        public bool IsPaused
        {
            get
            {
                return m_VideoPlayer.IsPaused;
            }
        }
        public bool IsStopped
        {
            get
            {
                return m_VideoPlayer.IsStopped;
            }
        }
        public byte[] TextureBytes
        {
            get
            {
                return m_TextureByteArray;
            }
        }

        #region private members
        private string m_videoPath = "";
        private long m_eegFileDurationInSec = 0;
        //===
        VlcMediaPlayer m_VideoPlayer = null;
        VlcInstance m_VlcInstance = null;
        RawImage m_TextureForVideo = null;
        NewFrameEventHandler m_VideoCallback = null;
        Bitmap m_BitmapCopy = null;
        byte[] m_TextureByteArray;
        bool m_newPic = false;
        //===
        Stopwatch m_stopwatch;
        long m_lastPlayTime = 0, m_lastPlayTimeGlobal = 0;
        int m_offsetVideoMilliSec = 0;
        //===
        private RectTransform m_parentRectTransform = null;
        private float WidthToHeightRatio = 0.0f, HeightToWidthRatio = 0.0f;
        #endregion


        public void Init(string videoPath, int eegFileDurationInSec, RawImage tex)
        {
            m_TextureForVideo = tex;
            m_parentRectTransform = transform.parent.GetComponent<RectTransform>();

            m_videoPath = videoPath;
            m_eegFileDurationInSec = eegFileDurationInSec;
            m_stopwatch = new Stopwatch();
            m_VlcInstance = new VlcInstance(new string[] { "" });
            m_stopwatch = new Stopwatch();
            m_stopwatch.Start();

            using (VlcMedia media = new VlcMedia(m_VlcInstance, "file:///" + videoPath))
            {
                if (m_VideoPlayer == null)
                {
                    m_VideoPlayer = new VlcMediaPlayer(media);
                    IMemoryRenderer memRender = m_VideoPlayer.CustomRenderer;

                    //Define callback to process video frame from libvlc
                    m_VideoCallback = new NewFrameEventHandler(ProcessFrameCallback);
                    memRender.SetCallback(m_VideoCallback);

                    //the size of the bitmap format need to be the same as 
                    //the texture on unity Otherwise performance issue
                    memRender.SetFormat(new BitmapFormat(512, 512, ChromaType.RV32));
                }
                else
                {
                    m_VideoPlayer.Media = media;
                }
            }

            //m_hub.videoRemote.offsetVideoHasChanged += new offsetVideoChangedEventHandler(
            //    delegate (float newVal)
            //    {
            //        m_offsetVideoMilliSec = (int)newVal;
            //    });

            SetVolume(0.5f);
        }

        public void SetVideoOffset(float newOffset)
        {
            m_offsetVideoMilliSec = (int)newOffset;
        }

        public void Cleanup()
        {
            //remove offset video event
            //m_hub.videoRemote.offsetVideoHasChanged -= new offsetVideoChangedEventHandler(
            //    delegate (float newVal)
            //    {
            //        m_offsetVideoMilliSec = (int)newVal;
            //    });

            //to release resources
            if (m_VideoPlayer != null)
            {
                m_VideoPlayer.Stop();
                m_VideoPlayer.Dispose();
                m_VideoPlayer = null;
            }

            m_VlcInstance = new VlcInstance(new string[] { "" });
        }

        public void Update()
        {
            if (m_newPic && m_VideoPlayer.IsPlaying)
            {
                //UnityEngine.Debug.Log("update called");
                ((Texture2D)m_TextureForVideo.texture).LoadImage(m_TextureByteArray);
                m_newPic = false;
            }
        }

        public void Play()
        {
            if (m_VideoPlayer.IsStopped)
            {
                WidthToHeightRatio = (float)m_TextureForVideo.texture.width / m_TextureForVideo.texture.height;
                HeightToWidthRatio = (float)m_TextureForVideo.texture.height / m_TextureForVideo.texture.width;
                ResizeTexture();
            }

            if (m_VideoPlayer.IsPaused || m_VideoPlayer.IsStopped)
            {
                m_VideoPlayer.Play();
                m_stopwatch.Start();
            }
        }

        public void Pause()
        {
            if (m_VideoPlayer.IsPlaying)
            {
                m_VideoPlayer.Pause();
                m_stopwatch.Stop();
            }
        }

        public void Stop()
        {
            m_VideoPlayer.Stop();
            m_stopwatch.Stop();
        }

        public void MoveTime(long secondsToAdd)
        {
            m_VideoPlayer.setTime(CurrentTime + (secondsToAdd * 1000));
        }

        public void SetTime(long timeMilliSec)
        {
            m_VideoPlayer.setTime(timeMilliSec);
        }

        public void SetVolume(float volume)
        {
            int maxPercentVideo = 200;
            m_VideoPlayer.SetVolume((int)(volume * maxPercentVideo));
        }

        /// <summary>
        /// Callback to process the frame extracted from the video
        /// 
        /// We make a copy of the frame, load it in a MemoryStream
        /// in order to be abble to load the data in a byte array
        /// that will be used for rendering
        /// </summary>
        /// <param name="frame">Frame sent from the dll</param>
        private void ProcessFrameCallback(Bitmap frame)
        {
            m_BitmapCopy = frame.Clone(new RectangleF(0, 0, frame.Width, frame.Height), PixelFormat.Format32bppArgb);
            //Memory stream to store the bitmap data.
            MemoryStream ms = new MemoryStream();
            //Save to that memory stream.
            m_BitmapCopy.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            //Go to the beginning of the memory stream.
            ms.Seek(0, SeekOrigin.Begin);
            m_TextureByteArray = ms.ToArray();
            //Close the stream.
            ms.Close();
            //===
            m_BitmapCopy.Dispose();
            m_newPic = true;
        }

        private void ResizeTexture()
        {
            if (m_parentRectTransform == null) return;

            float resizingWidth = m_parentRectTransform.rect.height * WidthToHeightRatio;
            float resizingHeight = m_parentRectTransform.rect.width * HeightToWidthRatio;

            float width = (resizingHeight >= m_parentRectTransform.rect.height) ? resizingWidth : m_parentRectTransform.rect.width;
            float height = (resizingHeight >= m_parentRectTransform.rect.height) ? m_parentRectTransform.rect.height : resizingHeight;
            m_TextureForVideo.rectTransform.sizeDelta = new Vector2(width, height);
        }
    }
}