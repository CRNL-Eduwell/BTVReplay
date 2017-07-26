using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

using VLCSharp.Tools;

namespace VLCSharp.Interface
{
    /// <summary>
    /// Base type for custom rendering types
    /// </summary>
    public interface IRenderer : IDisposable
    {
        /// <summary>
        /// Sets exception handler for exceptions thrown by background threads.
        /// </summary>
        /// <param name="handler"></param>
        void SetExceptionHandler(Action<Exception> handler);

        /// <summary>
        /// Gets the actual frame rate of the rendering.
        /// </summary>
        int ActualFrameRate { get; }
    }

    /// <summary>
    /// Enables custom processing of video frames.
    /// </summary>
    public interface IMemoryRenderer : IRenderer
    {
        /// <summary>
        /// Sets the callback which invoked when new frame should be displayed
        /// </summary>
        /// <param name="callback">Callback method</param>
        /// <remarks>The frame will be auto-disposed after callback invokation.</remarks>
        void SetCallback(NewFrameEventHandler callback);

        /// <summary>
        /// Gets the latest video frame that was displayed.
        /// </summary>
        Bitmap CurrentFrame { get; }

        /// <summary>
        /// Sets the bitmap format for the callback.
        /// </summary>
        /// <param name="format">Bitmap format of the video frame</param>
        void SetFormat(BitmapFormat format);
    }

    /// <summary>
    /// Contains methods for setting custom processing of video frames.
    /// </summary>
    public interface IMemoryRendererEx : IRenderer
    {
        /// <summary>
        /// Sets the callback which invoked when new frame should be displayed
        /// </summary>
        /// <param name="callback">Callback method</param>
        void SetCallback(NewFrameDataEventHandler callback);

        /// <summary>
        /// Gets the latest video frame that was displayed.
        /// </summary>
        PlanarFrame CurrentFrame { get; }

        /// <summary>
        /// Sets the callback invoked before the media playback starts to set the desired frame format.
        /// </summary>
        /// <param name="setupCallback"></param>
        /// <remarks>If not set, original media format will be used</remarks>
        void SetFormatSetupCallback(Func<BitmapFormat, BitmapFormat> setupCallback);
    }

    /// <summary>
    /// Enables custom processing of video frames.
    /// </summary>
    public interface IMemRenderer : IDisposable
    {
        /// <summary>
        /// Sets the callback which invoked when new frame should be displayed
        /// </summary>
        /// <param name="callback">Callback method</param>
        /// <remarks>The frame will be auto-disposed after callback invokation.</remarks>
        void SetCallback(NewFrameEH callback);

        /// <summary>
        /// Gets the latest video frame that was displayed.
        /// </summary>
        Bitmap CurrentFrame { get; }

        /// <summary>
        /// Sets the bitmap format for the callback.
        /// </summary>
        /// <param name="format">Bitmap format of the video frame</param>
        void SetFormat(BitmapFormat format);


        /// <summary>
        /// Sets exception handler for exceptions thrown by background threads.
        /// </summary>
        /// <param name="handler"></param>
        void SetExceptionHandler(Action<Exception> handler);

        /// <summary>
        /// Gets the actual frame rate of the rendering.
        /// </summary>
        int ActualFrameRate { get; }

    }

    //==== STUFF

    /// <summary>
    /// Structure incapsulation for audio samples
    /// </summary>
    [Serializable]
    public struct Sound
    {
        /// <summary>
        /// Pointer to the first audio sample
        /// </summary>
        public IntPtr SamplesData { get; set; }

        /// <summary>
        /// Size in bytes of SamplesData buffer
        /// </summary>
        public uint SamplesSize { get; set; }

        /// <summary>
        /// Playback time stamp in microseconds
        /// </summary>
        public long Pts { get; set; }
    }

    /// <summary>
    /// Data structure containing media statistics' parameters.
    /// </summary>
    [Serializable]
    public struct MediaStatistics
    {
        /* Input */
        public int ReadBytes;
        public float InputBitrate;

        /* Demux */
        public int DemuxReadBytes;
        public float DemuxBitrate;
        public int DemuxCorrupted;
        public int DemuxDiscontinuity;

        /* Decoders */
        public int DecodedVideo;
        public int DecodedAudio;

        /* Video Output */
        public int DisplayedPictures;
        public int LostPictures;

        /* Audio output */
        public int PlayedAbuffers;
        public int LostAbuffers;

        /* Stream output */
        public int SentPackets;
        public int SentBytes;
        public float SendBitrate;
    }

    /// <summary>
    /// Data structure containing parameters of elementary media stream
    /// </summary>
    [Serializable]
    public class MediaTrackInfo
    {
        public UInt32 Codec;

        public int Id;

        public TrackType TrackType;

        public int Profile;

        public int Level;

        public int Channels;
        public int Rate;

        public int Height;
        public int Width;
    }


    /// <summary>
    /// Specifies the parameters of the bitmap.
    /// </summary>
    [Serializable]
    public class BitmapFormat
    {
        /// <summary>
        /// Initializes new instance of BitmapFormat class
        /// </summary>
        /// <param name="width">The width of the bitmap in pixels</param>
        /// <param name="height">The height of the bitmap in pixels</param>
        /// <param name="chroma">Chroma type of the bitmap</param>
        public BitmapFormat(int width, int height, ChromaType chroma)
        {
            Width = width;
            Height = height;
            ChromaType = chroma;
            Planes = 1;
            PlaneSizes = new int[3];
            Init();

            Chroma = ChromaType.ToString();
            if (IsRGB)
            {
                Pitch = Width * BitsPerPixel / 8;
                PlaneSizes[0] = ImageSize = Pitch * Height;
                Pitches = new int[1] { Pitch };
                Lines = new int[1] { Height };
            }
        }

        private void Init()
        {
            switch (ChromaType)
            {
                case ChromaType.RV15:
                    PixelFormat = PixelFormat.Format16bppRgb555;
                    BitsPerPixel = 16;
                    break;

                case ChromaType.RV16:
                    PixelFormat = PixelFormat.Format16bppRgb565;
                    BitsPerPixel = 16;
                    break;

                case ChromaType.RV24:
                    PixelFormat = PixelFormat.Format24bppRgb;
                    BitsPerPixel = 24;
                    break;

                case ChromaType.RV32:
                    PixelFormat = PixelFormat.Format32bppRgb;
                    BitsPerPixel = 32;
                    break;

                case ChromaType.RGBA:
                    PixelFormat = PixelFormat.Format32bppArgb;
                    BitsPerPixel = 32;
                    break;

                case ChromaType.NV12:
                    BitsPerPixel = 12;
                    Planes = 2;
                    PlaneSizes[0] = Width * Height;
                    PlaneSizes[1] = Width * Height / 2;
                    Pitches = new int[2] { Width, Width };
                    Lines = new int[2] { Height, Height / 2 };
                    ImageSize = PlaneSizes[0] + PlaneSizes[1];
                    break;

                case ChromaType.I420:
                case ChromaType.YV12:
                case ChromaType.J420:
                    BitsPerPixel = 12;
                    Planes = 3;
                    PlaneSizes[0] = Width * Height;
                    PlaneSizes[1] = PlaneSizes[2] = Width * Height / 4;
                    Pitches = new int[3] { Width, Width / 2, Width / 2 };
                    Lines = new int[3] { Height, Height / 2, Height / 2 };
                    ImageSize = PlaneSizes[0] + PlaneSizes[1] + PlaneSizes[2];
                    break;

                case ChromaType.YUY2:
                case ChromaType.UYVY:
                    BitsPerPixel = 16;
                    PlaneSizes[0] = Width * Height * 2;
                    Pitches = new int[1] { Width * 2 };
                    Lines = new int[1] { Height };
                    ImageSize = PlaneSizes[0];
                    break;

                default:
                    throw new ArgumentException("Unsupported chroma type " + ChromaType);
            }
        }

        /// <summary>
        /// Gets the size in bytes of the scan line 
        /// </summary>
        public int Pitch { get; private set; }

        /// <summary>
        /// Gets the size of the image in bytes
        /// </summary>
        public int ImageSize { get; private set; }

        /// <summary>
        /// Gets the chroma type string
        /// </summary>
        public string Chroma { get; private set; }

        /// <summary>
        /// Gets the pixel format of the bitmap. Valid only for RGB formats.
        /// </summary>
        public PixelFormat PixelFormat { get; private set; }

        /// <summary>
        /// Gets the width of the bitmap
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Gets the height of the bitmap
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Gets number of bits used for a pixel according to ChromaType
        /// </summary>
        public int BitsPerPixel { get; private set; }

        /// <summary>
        /// Gets value indication whether the format contains more than one pixel plane
        /// </summary>
        public bool IsPlanarFormat
        {
            get
            {
                return ChromaType == ChromaType.I420 ||
                       ChromaType == ChromaType.NV12 ||
                       ChromaType == ChromaType.YV12 ||
                       ChromaType == ChromaType.J420;
            }
        }

        /// <summary>
        /// Gets value indicating whether the format is packed RGB
        /// </summary>
        public bool IsRGB
        {
            get
            {
                return ChromaType == ChromaType.RV15 ||
                       ChromaType == ChromaType.RV16 ||
                       ChromaType == ChromaType.RV24 ||
                       ChromaType == ChromaType.RV32 ||
                       ChromaType == ChromaType.RGBA;
            }
        }

        /// <summary>
        /// Gets number of pixel planes
        /// </summary>
        public int Planes { get; private set; }

        /// <summary>
        /// Gets array of pixel plane's sizes
        /// </summary>
        public int[] PlaneSizes { get; private set; }

        /// <summary>
        /// Gets array of pitch size per pixel plane
        /// </summary>
        public int[] Pitches { get; private set; }

        /// <summary>
        /// Gets array of scan lines (height) per pixel plane
        /// </summary>
        public int[] Lines { get; private set; }

        /// <summary>
        /// Gets the pixel format of the video frame
        /// </summary>
        public ChromaType ChromaType { get; private set; }
    }

    /// <summary>
    /// Structure for pixel planes and their sizes
    /// </summary>
    public struct PlanarFrame
    {
        /// <summary>
        /// Initializes new instance
        /// </summary>
        /// <param name="planes"></param>
        /// <param name="lenghts"></param>
        public PlanarFrame(IntPtr[] planes, int[] lenghts)
            : this()
        {
            if (planes.Length != lenghts.Length)
            {
                throw new ArgumentException("Number of planes must be equal to lenghts array");
            }

            this.Planes = planes;
            this.Lenghts = lenghts;
        }

        /// <summary>
        /// Gets pointer array to the pixel planes on the native heap 
        /// </summary>
        public IntPtr[] Planes { get; set; }

        /// <summary>
        /// Gets length of each pixel plane
        /// </summary>
        public int[] Lenghts { get; set; }
    }

    /// <summary>
    /// Container for custom audio processing callbacks
    /// </summary>
    public class AudioCallbacks
    {
        /// <summary>
        /// Callback method for handling voulume and mute proerties change
        /// </summary>
        public VolumeChangedEventHandler VolumeCallback;

        /// <summary>
        /// Callback method for handling PCM samples
        /// </summary>
        public NewSoundEventHandler SoundCallback;

        /// <summary>
        /// Callback method called when media player switches to Pause state
        /// </summary>
        public Action<long> PauseCallback;

        /// <summary>
        /// Callback method called when media player switches to Playback state
        /// </summary>
        public Action<long> ResumeCallback;

        /// <summary>
        /// Callback method called when all pending buffers should be discarded
        /// </summary>
        public Action<long> FlushCallback;

        /// <summary>
        /// Callback method called when all pending buffers must be played
        /// </summary>
        public Action DrainCallback;
    }

    //==== DELEGATE 
    public delegate void NewFrameEH(Bitmap frame);

    public delegate void NewFrameEventHandler(Bitmap frame);

    /// <summary>
    /// Represents the method that will handle each frame in video sequence as array of raw pixel planes.
    /// </summary>
    /// <param name="frame"></param>
    public delegate void NewFrameDataEventHandler(PlanarFrame frame);

    /// <summary>
    /// Represents a callback method which handles audio samples
    /// </summary>
    /// <param name="newSound"></param>
    public delegate void NewSoundEventHandler(Sound newSound);

    /// <summary>
    /// Represents a callback method which handles change in volume or mute values
    /// </summary>
    /// <param name="volume"></param>
    /// <param name="mute"></param>
    public delegate void VolumeChangedEventHandler(float volume, bool mute);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate void VlcEventHandlerDelegate(ref libvlc_event_t libvlc_event, IntPtr userData);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    unsafe delegate void* LockEventHandler(void* opaque, void** plane);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    unsafe delegate void UnlockEventHandler(void* opaque, void* picture, void** plane);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    unsafe delegate void DisplayEventHandler(void* opaque, void* picture);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    unsafe delegate void* CallbackEventHandler(void* data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    unsafe delegate int VideoFormatCallback(void** opaque, char* chroma, int* width, int* height, int* pitches, int* lines);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    unsafe delegate void CleanupCallback(void* opaque);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    unsafe delegate void PlayCallbackEventHandler(void* data, void* samples, uint count, long pts);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    unsafe delegate void VolumeCallbackEventHandler(void* data, float volume, bool mute);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    unsafe delegate int SetupCallbackEventHandler(void** data, char* format, int* rate, int* channels);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    unsafe delegate void AudioCallbackEventHandler(void* data, long pts);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    unsafe delegate void AudioDrainCallbackEventHandler(void* data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    unsafe delegate int ImemGet(void* data, char* cookie, long* dts, long* pts, int* flags, uint* dataSize, void** ppData);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    unsafe delegate void ImemRelease(void* data, char* cookie, uint dataSize, void* pData);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    unsafe delegate void LogCallback(void* data, libvlc_log_level level, char* fmt, char* args);

}