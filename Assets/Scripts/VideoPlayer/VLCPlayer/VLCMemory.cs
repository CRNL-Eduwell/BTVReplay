using System;
using System.Text;
using System.Timers;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using VLCSharp.Tools;
using VLCSharp.Interface;

namespace VLCSharp.VLCMemory
{
    internal unsafe class MemoryHeap
    {
        static IntPtr ph = GetProcessHeap();

        private MemoryHeap() { }

        /// <summary>
        /// Allocates a memory block of the given size. The allocated memory is
        /// automatically initialized to zero.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static void* Alloc(int size)
        {
            void* result = HeapAlloc(ph, HEAP_ZERO_MEMORY, size);
            if (result == null)
            {
                throw new OutOfMemoryException();
            }

            return result;
        }

        /// <summary>
        /// Frees a memory block.
        /// </summary>
        /// <param name="block"></param>
        public static void Free(void* block)
        {
            if (!HeapFree(ph, 0, block))
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Re-allocates a memory block. If the reallocation request is for a
        /// larger size, the additional region of memory is automatically
        /// initialized to zero.
        /// </summary>
        /// <param name="block"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static void* ReAlloc(void* block, int size)
        {
            void* result = HeapReAlloc(ph, HEAP_ZERO_MEMORY, block, size);
            if (result == null)
            {
                throw new OutOfMemoryException();
            }

            return result;
        }

        /// <summary>
        /// Returns the size of a memory block.
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public static int SizeOf(void* block)
        {
            int result = HeapSize(ph, 0, block);
            if (result == -1)
            {
                throw new InvalidOperationException();
            }

            return result;
        }

        // Heap API flags
        const int HEAP_ZERO_MEMORY = 0x00000008;

        // Heap API functions
        [DllImport("kernel32")]
        static extern IntPtr GetProcessHeap();

        [DllImport("kernel32")]
        static extern void* HeapAlloc(IntPtr hHeap, int flags, int size);

        [DllImport("kernel32")]
        static extern bool HeapFree(IntPtr hHeap, int flags, void* block);

        [DllImport("kernel32")]
        static extern void* HeapReAlloc(IntPtr hHeap, int flags, void* block, int size);

        [DllImport("kernel32")]
        static extern int HeapSize(IntPtr hHeap, int flags, void* block);

        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = true)]
        public static unsafe extern void CopyMemory(void* dest, void* src, int size);
    }

    /// <summary>
    /// Base class for managing native resources.
    /// </summary>
    public abstract class DisposableBase : IDisposable
    {
        private bool m_isDisposed;

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (!m_isDisposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);

                m_isDisposed = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected abstract void Dispose(bool disposing);
        //      if (disposing)
        //      {
        //         // get rid of managed resources 
        //      }
        //      // get rid of unmanaged resources 

        /// <summary>
        /// 
        /// </summary>
        ~DisposableBase()
        {
            if (!m_isDisposed)
            {
                Dispose(false);
                m_isDisposed = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected void VerifyObjectNotDisposed()
        {
            if (m_isDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }
    }

    internal unsafe struct PixelData : IDisposable
    {
        public byte* pPixelData;
        public int size;

        public PixelData(int size)
        {
            this.size = size;
            this.pPixelData = (byte*)MemoryHeap.Alloc(size);
        }

        #region IDisposable Members

        public void Dispose()
        {
            MemoryHeap.Free(this.pPixelData);
        }

        #endregion

        public static bool operator ==(PixelData pd1, PixelData pd2)
        {
            return (pd1.size == pd2.size && pd1.pPixelData == pd2.pPixelData);
        }

        public static bool operator !=(PixelData pd1, PixelData pd2)
        {
            return !(pd1 == pd2);
        }

        public override int GetHashCode()
        {
            return size.GetHashCode() ^ pPixelData->GetHashCode();
        }

        public override bool Equals(object obj)
        {
            PixelData pd = (PixelData)obj;
            //if (pd == null)
            //{
            //    return false;
            //}

            return this == pd;
        }
    }

    internal static class Extensions
    {
        public static byte[] ToUtf8(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            return Encoding.UTF8.GetBytes(str);
        }

        /// <summary>
        /// String format : [width]x[height]+[left offset]+[top offset]
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static string ToCropFilterString(this Rectangle rect)
        {
            return string.Format("{0}x{1}+{2}+{3}", rect.Width, rect.Height, rect.X, rect.Y);
        }

        public static Rectangle ToRectangle(this string str)
        {
            string[] items = str.Split(new char[] { 'x', '+' }, 4);

            return new Rectangle(int.Parse(items[3]), int.Parse(items[2]), int.Parse(items[1]), int.Parse(items[0]));
        }

        public static MediaStatistics ToMediaStatistics(this libvlc_media_stats_t stats)
        {
            MediaStatistics ms = new MediaStatistics();
            ms.DecodedAudio = stats.i_decoded_audio;
            ms.DecodedVideo = stats.i_decoded_video;
            ms.DemuxBitrate = stats.f_demux_bitrate;
            ms.DemuxCorrupted = stats.i_demux_corrupted;
            ms.DemuxDiscontinuity = stats.i_demux_discontinuity;
            ms.DemuxReadBytes = stats.i_demux_read_bytes;
            ms.DisplayedPictures = stats.i_displayed_pictures;
            ms.InputBitrate = stats.f_input_bitrate;
            ms.LostAbuffers = stats.i_lost_abuffers;
            ms.LostPictures = stats.i_lost_pictures;
            ms.PlayedAbuffers = stats.i_played_abuffers;
            ms.ReadBytes = stats.i_read_bytes;
            ms.SendBitrate = stats.f_send_bitrate;
            ms.SentBytes = stats.i_sent_bytes;
            ms.SentPackets = stats.i_sent_packets;

            return ms;
        }

        public static MediaTrackInfo ToMediaInfo(this libvlc_media_track_info_t tInfo)
        {
            MediaTrackInfo mti = new MediaTrackInfo();
            mti.Channels = tInfo.audio_video.audio.i_channels;
            mti.Codec = tInfo.i_codec;
            mti.Height = tInfo.audio_video.video.i_width;
            mti.Id = tInfo.i_id;
            mti.TrackType = (TrackType)(int)tInfo.i_type;
            mti.Level = tInfo.i_level;
            mti.Profile = tInfo.i_profile;
            mti.Rate = tInfo.audio_video.audio.i_rate;
            mti.Width = tInfo.audio_video.video.i_width;

            return mti;
        }
    }

    internal unsafe struct PlanarPixelData : IDisposable
    {
        public int[] Sizes;
        public byte** Data;

        public PlanarPixelData(int[] lineSizes)
        {
            Sizes = lineSizes;

            Data = (byte**)MemoryHeap.Alloc(sizeof(byte*) * Sizes.Length);

            for (int i = 0; i < Sizes.Length; i++)
            {
                Data[i] = (byte*)MemoryHeap.Alloc(sizeof(byte) * Sizes[i]);
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < Sizes.Length; i++)
            {
                MemoryHeap.Free(Data[i]);
            }

            MemoryHeap.Free(Data);
        }

        public static bool operator ==(PlanarPixelData pd1, PlanarPixelData pd2)
        {
            return (pd1.Data == pd2.Data && pd1.Sizes == pd2.Sizes);
        }

        public static bool operator !=(PlanarPixelData pd1, PlanarPixelData pd2)
        {
            return !(pd1 == pd2);
        }

        public override int GetHashCode()
        {
            return Sizes.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            PlanarPixelData pd = (PlanarPixelData)obj;
            //if (pd == null)
            //{
            //    return false;
            //}

            return this == pd;
        }
    }

    internal sealed unsafe class MemoryRenderer : DisposableBase, IMemoryRenderer
    {
        IntPtr m_hMediaPlayer;
        NewFrameEventHandler m_callback = null;
        BitmapFormat m_format;
        Timer m_timer = new Timer();
        volatile int m_frameRate = 0;
        int m_latestFps;
        object m_lock = new object();
        List<Delegate> m_callbacks = new List<Delegate>();

        IntPtr pLockCallback;
        IntPtr pUnlockCallback;
        IntPtr pDisplayCallback;
        Action<Exception> m_excHandler = null;
        GCHandle m_pixelDataPtr = default(GCHandle);
        PixelData m_pixelData;
        void* m_pBuffer = null;

        public MemoryRenderer(IntPtr hMediaPlayer)
        {
            m_hMediaPlayer = hMediaPlayer;

            LockEventHandler leh = OnpLock;
            UnlockEventHandler ueh = OnpUnlock;
            DisplayEventHandler deh = OnpDisplay;

            pLockCallback = Marshal.GetFunctionPointerForDelegate(leh);
            pUnlockCallback = Marshal.GetFunctionPointerForDelegate(ueh);
            pDisplayCallback = Marshal.GetFunctionPointerForDelegate(deh);

            m_callbacks.Add(leh);
            m_callbacks.Add(deh);
            m_callbacks.Add(ueh);

            m_timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            m_timer.Interval = 1000;
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            m_latestFps = m_frameRate;
            m_frameRate = 0;
        }

        unsafe void* OnpLock(void* opaque, void** plane)
        {
            PixelData* px = (PixelData*)opaque;
            *plane = px->pPixelData;
            return null;
        }

        unsafe void OnpUnlock(void* opaque, void* picture, void** plane)
        {

        }

        unsafe void OnpDisplay(void* opaque, void* picture)
        {
            lock (m_lock)
            {
                try
                {
                    PixelData* px = (PixelData*)opaque;
                    MemoryHeap.CopyMemory(m_pBuffer, px->pPixelData, px->size);

                    m_frameRate++;
                    if (m_callback != null)
                    {
                        using (Bitmap frame = GetBitmap())
                        {
                            m_callback(frame);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (m_excHandler != null)
                    {
                        m_excHandler(ex);
                    }
                    else
                    {
                        throw ex;
                    }
                }
            }
        }

        private Bitmap GetBitmap()
        {
            return new Bitmap(m_format.Width, m_format.Height, m_format.Pitch, m_format.PixelFormat, new IntPtr(m_pBuffer));
        }

        #region IMemoryRenderer Members

        public void SetCallback(NewFrameEventHandler callback)
        {
            m_callback = callback;
        }

        public void SetFormat(BitmapFormat format)
        {
            m_format = format;
            LibVlc.libvlc_video_set_format(m_hMediaPlayer, m_format.Chroma.ToUtf8(), m_format.Width, m_format.Height, m_format.Pitch);
            m_pBuffer = MemoryHeap.Alloc(m_format.ImageSize);

            m_pixelData = new PixelData(m_format.ImageSize);
            m_pixelDataPtr = GCHandle.Alloc(m_pixelData, GCHandleType.Pinned);
            LibVlc.libvlc_video_set_callbacks(m_hMediaPlayer, pLockCallback, pUnlockCallback, pDisplayCallback, m_pixelDataPtr.AddrOfPinnedObject());
        }

        internal void StartTimer()
        {
            m_timer.Start();
        }

        public int ActualFrameRate
        {
            get
            {
                return m_latestFps;
            }
        }

        public Bitmap CurrentFrame
        {
            get
            {
                lock (m_lock)
                {
                    return GetBitmap();
                }
            }
        }

        public void SetExceptionHandler(Action<Exception> handler)
        {
            m_excHandler = handler;
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            //IntPtr zero = IntPtr.Zero;
            // LibVlc.libvlc_video_set_callbacks(m_hMediaPlayer, zero, zero, zero, zero);

            m_pixelDataPtr.Free();
            m_pixelData.Dispose();

            MemoryHeap.Free(m_pBuffer);

            if (disposing)
            {
                m_timer.Dispose();
                m_callback = null;
                m_callbacks.Clear();
            }
        }
    }

    internal sealed unsafe class MemoryRendererEx : DisposableBase, IMemoryRendererEx
    {
        IntPtr m_hMediaPlayer;
        NewFrameDataEventHandler m_callback = null;
        Timer m_timer = new Timer();
        volatile int m_frameRate = 0;
        int m_latestFps;
        object m_lock = new object();
        List<Delegate> m_callbacks = new List<Delegate>();
        Func<BitmapFormat, BitmapFormat> m_formatSetupCB = null;
        IntPtr[] m_planes = new IntPtr[3];
        BitmapFormat m_format;
        Action<Exception> m_excHandler = null;
        IntPtr pLockCallback;
        IntPtr pDisplayCallback;
        IntPtr pFormatCallback;

        PlanarPixelData m_pixelData = default(PlanarPixelData);

        public MemoryRendererEx(IntPtr hMediaPlayer)
        {
            m_hMediaPlayer = hMediaPlayer;

            LockEventHandler leh = OnpLock;
            DisplayEventHandler deh = OnpDisplay;
            VideoFormatCallback formatCallback = OnFormatCallback;

            pFormatCallback = Marshal.GetFunctionPointerForDelegate(formatCallback);
            pLockCallback = Marshal.GetFunctionPointerForDelegate(leh);
            pDisplayCallback = Marshal.GetFunctionPointerForDelegate(deh);

            m_callbacks.Add(leh);
            m_callbacks.Add(deh);
            m_callbacks.Add(formatCallback);

            m_timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            m_timer.Interval = 1000;

            LibVlc.libvlc_video_set_format_callbacks(m_hMediaPlayer, pFormatCallback, IntPtr.Zero);
            LibVlc.libvlc_video_set_callbacks(m_hMediaPlayer, pLockCallback, IntPtr.Zero, pDisplayCallback, IntPtr.Zero);
        }

        //tryparse enum flo : pas présent mono 
        public bool TryParseEnum<T>(string str, bool caseSensitive, out T value) where T : struct
        {
            // Can't make this a type constraint...
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("Type parameter must be an enum");
            }
            var names = Enum.GetNames(typeof(T));
            value = (Enum.GetValues(typeof(T)) as T[])[0];  // For want of a better default
            foreach (var name in names)
            {
                if (String.Equals(name, str, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase))
                {
                    value = (T)Enum.Parse(typeof(T), name);
                    return true;
                }
            }
            return false;
        }

        private unsafe int OnFormatCallback(void** opaque, char* chroma, int* width, int* height, int* pitches, int* lines)
        {
            IntPtr pChroma = new IntPtr(chroma);
            string chromaStr = Marshal.PtrToStringAnsi(pChroma);

            ChromaType type;
            //if (!Enum.TryParse<ChromaType>(chromaStr, out type))
            if (TryParseEnum<ChromaType>(chromaStr, true, out type))
            {
                ArgumentException exc = new ArgumentException("Unsupported chroma type " + chromaStr);
                if (m_excHandler != null)
                {
                    m_excHandler(exc);
                    return 0;
                }
                else
                {
                    throw exc;
                }
            }

            m_format = new BitmapFormat(*width, *height, type);
            if (m_formatSetupCB != null)
            {
                m_format = m_formatSetupCB(m_format);
            }

            Marshal.Copy(m_format.Chroma.ToUtf8(), 0, pChroma, 4);
            *width = m_format.Width;
            *height = m_format.Height;

            for (int i = 0; i < m_format.Planes; i++)
            {
                pitches[i] = m_format.Pitches[i];
                lines[i] = m_format.Lines[i];
            }

            m_pixelData = new PlanarPixelData(m_format.PlaneSizes);

            return m_format.Planes;
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            m_latestFps = m_frameRate;
            m_frameRate = 0;
        }

        unsafe void* OnpLock(void* opaque, void** plane)
        {
            for (int i = 0; i < m_pixelData.Sizes.Length; i++)
            {
                plane[i] = m_pixelData.Data[i];
            }

            return null;
        }

        unsafe void OnpDisplay(void* opaque, void* picture)
        {
            lock (m_lock)
            {
                try
                {
                    m_frameRate++;
                    for (int i = 0; i < m_pixelData.Sizes.Length; i++)
                    {
                        m_planes[i] = new IntPtr(m_pixelData.Data[i]);
                    }

                    if (m_callback != null)
                    {
                        PlanarFrame pf = GetFrame();
                        m_callback(pf);
                    }
                }
                catch (Exception ex)
                {
                    if (m_excHandler != null)
                    {
                        m_excHandler(ex);
                    }
                    else
                    {
                        throw ex;
                    }
                }
            }
        }

        internal void StartTimer()
        {
            m_timer.Start();
        }

        private PlanarFrame GetFrame()
        {
            return new PlanarFrame(m_planes, m_format.PlaneSizes);
        }

        #region IMemoryRendererEx Members

        public void SetCallback(NewFrameDataEventHandler callback)
        {
            m_callback = callback;
        }

        public PlanarFrame CurrentFrame
        {
            get
            {
                lock (m_lock)
                {
                    return GetFrame();
                }
            }
        }

        public void SetFormatSetupCallback(Func<BitmapFormat, BitmapFormat> setupCallback)
        {
            m_formatSetupCB = setupCallback;
        }

        public int ActualFrameRate
        {
            get
            {
                return m_latestFps;
            }
        }

        public void SetExceptionHandler(Action<Exception> handler)
        {
            m_excHandler = handler;
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            IntPtr zero = IntPtr.Zero;
            LibVlc.libvlc_video_set_callbacks(m_hMediaPlayer, zero, zero, zero, zero);

            if (m_pixelData != default(PlanarPixelData))
            {
                m_pixelData.Dispose();
            }

            if (disposing)
            {
                m_timer.Dispose();
                m_formatSetupCB = null;
                m_excHandler = null;
                m_callback = null;
                m_callbacks.Clear();
            }
        }
    }
}
