using System;
using System.IO;
using System.Drawing;
using System.Diagnostics; //Requiered for Stopwatch
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

using VLCSharp.Tools;
using VLCSharp.Interface;
using VLCSharp.VLCMemory;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;//Requiered for Event data.

// VLC API Documentation
// http://www.videolan.org/developers/vlc/doc/doxygen/html/group__libvlc.html

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

    public delegate void timeVideo(int currentTime);

    public class VLCSharp : MonoBehaviour
    {
        public event timeVideo sendTime;

        [SerializeField] optionsHub hub = null;
        [SerializeField] BTVMedia media = null;

        public RawImage TextureToDraw = null;
        public Text currentTimetext = null;
        public Text totalTimeText = null;
        public Button playPause = null;
        public Button backTime10 = null;
        public Button backTime1 = null;
        public Button frontTime10 = null;
        public Scrollbar scrollBar = null;
        public Scrollbar volumeScrollBar = null;
        public VlcMediaPlayer player = null;
        public long time
        {
            get
            {
                if (scrollBarNotClicked)
                    return (long)(currentTime * 0.064);
                else
                    return (long)(scrollBar.value * player.totalVideoTime * 0.064);
            }
        }

        bool videoInit = false;

        //===Video Data
        Texture2D texPlay = null, texPause = null, texLogo = null;
        VlcInstance instance = null;
        Bitmap picCopy = null;
        string pathToVideo = "";
        byte[] textureByteArray;
        bool newPic = false;

        //===Timer Part
        bool scrollBarNotClicked = true;
        long lastPlayTime = 0;
        long lastPlayTimeGlobal = 0;
        Stopwatch stopwatch;
        EventTrigger trigger = null;
        int offsetVideoSec = 0;

        public int size = 0;

        void Awake() //OnEnable
        {
            media.loadVideo += new initVideo(loadVideoInit);

            backTime10.onClick.AddListener(() => MoveTime(-10));
            backTime1.onClick.AddListener(() => MoveTime(-1));
            frontTime10.onClick.AddListener(() => MoveTime(10));

            trigger = scrollBar.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerDown;
            entry.callback.AddListener((eventData) => { scrollBarNotClicked = false; });
            trigger.triggers.Add(entry);

            EventTrigger.Entry entry2 = new EventTrigger.Entry();
            entry2.eventID = EventTriggerType.PointerUp;
            entry2.callback.AddListener((eventData) => { setTimeScrollBar(); });
            trigger.triggers.Add(entry2);
        }

        void OnDestroy() //OnDisable
        {
            media.loadVideo -= new initVideo(loadVideoInit);

            if (videoInit)
            {
                hub.videoRemote.offsetVideoHasChanged -= new offsetVideoChangedEventHandler(
                    delegate (int newVal)
                    {
                        offsetVideoSec = newVal;
                    });
            }
            backTime10.onClick.RemoveAllListeners();
            backTime1.onClick.RemoveAllListeners();
            frontTime10.onClick.RemoveAllListeners();

            for (int i = 0; i < trigger.triggers.Count; i++)
                trigger.triggers[i].callback.RemoveAllListeners();

            //to release resources
            if (player != null)
            {
                player.Stop();
                player.Dispose();
                player = null;
            }

            instance = new VlcInstance(new string[] { "" });
        }

        void Start()
        {
            texPause = Resources.Load("Pictures/playIcone", typeof(Texture2D)) as Texture2D;
            texPlay = Resources.Load("Pictures/pauseIcone", typeof(Texture2D)) as Texture2D;
            texLogo = Resources.Load("Pictures/BTVLogo", typeof(Texture2D)) as Texture2D;

            TextureToDraw.texture = (Texture2D)Instantiate(texLogo);

            stopwatch = new Stopwatch();
            instance = new VlcInstance(new string[] { "" });
        }

        void Update()
        {
            if (newPic)
            {
                if (player.IsPlaying)
                {
                    ((Texture2D)TextureToDraw.texture).LoadImage(textureByteArray);
                    updateScBarPosAndTimeText();
                    newPic = false;

                    //sendTime((int)time - 640);
                    sendTime((int)time + offsetVideoSec);
                }
            }
        }

        #region userInterface

        public void Play()
        {
            if (videoInit)
            {
                if (player.IsPaused || player.IsStopped)
                {
                    player.Play();
                    stopwatch.Start();
                    playPause.GetComponent<RawImage>().texture = texPlay;
                }
                else
                {
                    player.Pause();
                    stopwatch.Stop();
                    playPause.GetComponent<RawImage>().texture = texPause;
                }
            }
        }

        public void Stop()
        {
            if (videoInit)
            {
                player.Stop();
                TextureToDraw.texture = Instantiate(texLogo);
                playPause.GetComponent<RawImage>().texture = texPause;
                stopwatch.Stop();
            }
        }

        public void MoveTime(long secToAdd)
        {
            if (videoInit)
            {
                player.setTime(currentTime + (secToAdd * 1000));
            }
        }

        public void setTimeScrollBar()
        {
            if (videoInit)
            {
                long newTimeValue = (long)(scrollBar.value * player.totalVideoTime);
                player.setTime(newTimeValue);
                scrollBarNotClicked = true;
            }
        }

        public void setVolume()
        {
            if (videoInit)
            {
                int maxPercentVideo = 200;
                player.SetVolume((int)(volumeScrollBar.value * maxPercentVideo));
            }
        }

        #endregion

        #region videoStuff

        public void loadVideoInit(string path2Video)
        {
            pathToVideo = path2Video;
            stopwatch = new Stopwatch();
            stopwatch.Start();

            using (VlcMedia media = new VlcMedia(instance, "file:///" + pathToVideo))
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
                    memRender.SetFormat(new BitmapFormat(256, 256, ChromaType.RV32));
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

            setVolume();
            videoInit = true;
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

        #endregion

        #region timeInterface

        //in millisec
        //extrapolate the currenttime because vlc api is not accurate enough
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

        void updateScBarPosAndTimeText()
        {
            if (scrollBarNotClicked)
                scrollBar.value = (float)currentTime / player.totalVideoTime;

            long currentTimeScrollBar = (long)(scrollBar.value * player.totalVideoTime);
            long timeSec = Mathf.RoundToInt(currentTimeScrollBar * 0.001f);
            displayTimeGUI(currentTimetext, timeSec);

            long totalTimeSec = Mathf.RoundToInt(player.totalVideoTime * 0.001f);
            displayTimeGUI(totalTimeText, totalTimeSec);
        }

        void displayTimeGUI(Text textGUI, long timeInSec)
        {
            long h = timeInSec / 3600;
            long m = (timeInSec / 60) % 60;
            long s = timeInSec % 60;
            timeToString(textGUI, h, m, s);
        }

        void timeToString(Text textGUI, long h, long m, long s)
        {
            if (h > 0)
            {
                textGUI.text = returnTimeString(h) + ":" + returnTimeString(m) + ":" + returnTimeString(s);
            }
            else
            {
                textGUI.text = returnTimeString(m) + ":" + returnTimeString(s);
            }
        }

        string returnTimeString(long time)
        {
            if (time < 10)
            {
                return "0" + time;
            }
            else
            {
                return time.ToString();
            }
        }
        #endregion
    }
}
