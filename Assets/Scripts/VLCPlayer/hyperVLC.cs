//    hyperVLC
//    
//    Mike Blakemore
//    Hyperplane Interactive
//    http://hyperplaneinteractive.com
//
//    
//
//    This project contains code borrowed from nNVLC:
//    http://www.codeproject.com/Articles/109639/nVLC
//
//    Many thanks to the VideoLAN team and Roman Ginzburg, author of nVLC.
//
//    hyperVLC is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    hyperVLC is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU General Public License for more details.
//     
// ========================================================================


using System;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Timers;

namespace hyperVLC
{

    internal unsafe class MemoryHeap
    {
        static int ph = GetProcessHeap();

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
        static extern int GetProcessHeap();

        [DllImport("kernel32")]
        static extern void* HeapAlloc(int hHeap, int flags, int size);

        [DllImport("kernel32")]
        static extern bool HeapFree(int hHeap, int flags, void* block);

        [DllImport("kernel32")]
        static extern void* HeapReAlloc(int hHeap, int flags, void* block, int size);

        [DllImport("kernel32")]
        static extern int HeapSize(int hHeap, int flags, void* block);

        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = true)]
        public static unsafe extern void CopyMemory(void* dest, void* src, int size);
    }
    public delegate void NewFrameEventHandler(Bitmap frame);
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
            if (pd == null)
            {
                return false;
            }

            return this == pd;
        }
    }
    public enum libvlc_event_e
    {
        libvlc_MediaMetaChanged = 0,
        libvlc_MediaSubItemAdded,
        libvlc_MediaDurationChanged,
        libvlc_MediaParsedChanged,
        libvlc_MediaFreed,
        libvlc_MediaStateChanged,

        libvlc_MediaPlayerMediaChanged = 0x100,
        libvlc_MediaPlayerNothingSpecial,
        libvlc_MediaPlayerOpening,
        libvlc_MediaPlayerBuffering,
        libvlc_MediaPlayerPlaying,
        libvlc_MediaPlayerPaused,
        libvlc_MediaPlayerStopped,
        libvlc_MediaPlayerForward,
        libvlc_MediaPlayerBackward,
        libvlc_MediaPlayerEndReached,
        libvlc_MediaPlayerEncounteredError,
        libvlc_MediaPlayerTimeChanged,
        libvlc_MediaPlayerPositionChanged,
        libvlc_MediaPlayerSeekableChanged,
        libvlc_MediaPlayerPausableChanged,
        libvlc_MediaPlayerTitleChanged,
        libvlc_MediaPlayerSnapshotTaken,
        libvlc_MediaPlayerLengthChanged,

        libvlc_MediaListItemAdded = 0x200,
        libvlc_MediaListWillAddItem,
        libvlc_MediaListItemDeleted,
        libvlc_MediaListWillDeleteItem,

        libvlc_MediaListViewItemAdded = 0x300,
        libvlc_MediaListViewWillAddItem,
        libvlc_MediaListViewItemDeleted,
        libvlc_MediaListViewWillDeleteItem,

        libvlc_MediaListPlayerPlayed = 0x400,
        libvlc_MediaListPlayerNextItemSet,
        libvlc_MediaListPlayerStopped,

        libvlc_MediaDiscovererStarted = 0x500,
        libvlc_MediaDiscovererEnded,

        libvlc_VlmMediaAdded = 0x600,
        libvlc_VlmMediaRemoved,
        libvlc_VlmMediaChanged,
        libvlc_VlmMediaInstanceStarted,
        libvlc_VlmMediaInstanceStopped,
        libvlc_VlmMediaInstanceStatusInit,
        libvlc_VlmMediaInstanceStatusOpening,
        libvlc_VlmMediaInstanceStatusPlaying,
        libvlc_VlmMediaInstanceStatusPause,
        libvlc_VlmMediaInstanceStatusEnd,
        libvlc_VlmMediaInstanceStatusError,
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct libvlc_log_message_t
    {
        public UInt32 sizeof_msg;
        public Int32 i_severity;
        public IntPtr psz_type;
        public IntPtr psz_name;
        public IntPtr psz_header;
        public IntPtr psz_message;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct libvlc_media_stats_t
    {
        /* Input */
        public int i_read_bytes;
        public float f_input_bitrate;

        /* Demux */
        public int i_demux_read_bytes;
        public float f_demux_bitrate;
        public int i_demux_corrupted;
        public int i_demux_discontinuity;

        /* Decoders */
        public int i_decoded_video;
        public int i_decoded_audio;

        /* Video Output */
        public int i_displayed_pictures;
        public int i_lost_pictures;

        /* Audio output */
        public int i_played_abuffers;
        public int i_lost_abuffers;

        /* Stream output */
        public int i_sent_packets;
        public int i_sent_bytes;
        public float f_send_bitrate;
    }

    public enum libvlc_meta_t
    {
        libvlc_meta_Title,
        libvlc_meta_Artist,
        libvlc_meta_Genre,
        libvlc_meta_Copyright,
        libvlc_meta_Album,
        libvlc_meta_TrackNumber,
        libvlc_meta_Description,
        libvlc_meta_Rating,
        libvlc_meta_Date,
        libvlc_meta_Setting,
        libvlc_meta_URL,
        libvlc_meta_Language,
        libvlc_meta_NowPlaying,
        libvlc_meta_Publisher,
        libvlc_meta_EncodedBy,
        libvlc_meta_ArtworkURL,
        libvlc_meta_TrackID
    }

    public enum libvlc_track_type_t
    {
        libvlc_track_unknown = -1,
        libvlc_track_audio = 0,
        libvlc_track_video = 1,
        libvlc_track_text = 2,
    }
    /* media descriptor */
    [StructLayout(LayoutKind.Sequential)]
    public struct media_meta_changed
    {
        public libvlc_meta_t meta_type;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct libvlc_media_track_info_type
    {
        [FieldOffset(0)]
        public audio audio;

        [FieldOffset(0)]
        public video video;
    }
    ///// <summary>
    ///// Represents a callback method that will handle each frame in video sequence as System.Drawing.Bitmap object.
    ///// </summary>
    ///// <param name="frame">New frame to display</param>
    //public delegate void NewFrameEventHandler(Bitmap frame);

    /// <summary>
    /// Represents the method that will handle each frame in video sequence as array of raw pixel planes.
    /// </summary>
    /// <param name="frame"></param>
    public delegate void NewFrameDataEventHandler(PlanarFrame frame);

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
    [StructLayout(LayoutKind.Sequential)]
    public struct audio
    {
        public int i_channels;
        public int i_rate;
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
    [StructLayout(LayoutKind.Sequential)]
    public struct video
    {
        public int i_height;
        public int i_width;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct libvlc_event_t
    {
        public libvlc_event_e type;
        public IntPtr p_obj;
        public MediaDescriptorUnion MediaDescriptor;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct MediaDescriptorUnion
    {
        [FieldOffset(0)]
        public media_meta_changed media_meta_changed;

        [FieldOffset(0)]
        public media_subitem_added media_subitem_added;

        [FieldOffset(0)]
        public media_duration_changed media_duration_changed;

        [FieldOffset(0)]
        public media_parsed_changed media_parsed_changed;

        [FieldOffset(0)]
        public media_freed media_freed;

        [FieldOffset(0)]
        public media_state_changed media_state_changed;

        [FieldOffset(0)]
        public media_player_position_changed media_player_position_changed;

        [FieldOffset(0)]
        public media_player_time_changed media_player_time_changed;

        [FieldOffset(0)]
        public media_player_title_changed media_player_title_changed;

        [FieldOffset(0)]
        public media_player_seekable_changed media_player_seekable_changed;

        [FieldOffset(0)]
        public media_player_pausable_changed media_player_pausable_changed;

        [FieldOffset(0)]
        public media_list_item_added media_list_item_added;

        [FieldOffset(0)]
        public media_list_will_add_item media_list_will_add_item;

        [FieldOffset(0)]
        public media_list_item_deleted media_list_item_deleted;

        [FieldOffset(0)]
        public media_list_will_delete_item media_list_will_delete_item;

        [FieldOffset(0)]
        public media_list_player_next_item_set media_list_player_next_item_set;

        [FieldOffset(0)]
        public media_player_snapshot_taken media_player_snapshot_taken;

        [FieldOffset(0)]
        public media_player_length_changed media_player_length_changed;

        [FieldOffset(0)]
        public vlm_media_event vlm_media_event;

        [FieldOffset(0)]
        public media_player_media_changed media_player_media_changed;
    }



    [StructLayout(LayoutKind.Sequential)]
    public struct media_subitem_added
    {
        public IntPtr new_child;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct media_duration_changed
    {
        public long new_duration;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct media_parsed_changed
    {
        public int new_status;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct media_freed
    {
        public IntPtr md;
    }
    public enum libvlc_state_t
    {
        libvlc_NothingSpecial = 0,
        libvlc_Opening,
        libvlc_Buffering,
        libvlc_Playing,
        libvlc_Paused,
        libvlc_Stopped,
        libvlc_Ended,
        libvlc_Error
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct media_state_changed
    {
        public libvlc_state_t new_state;
    }

    /* media instance */
    [StructLayout(LayoutKind.Sequential)]
    public struct media_player_position_changed
    {
        public float new_position;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct media_player_time_changed
    {
        public long new_time;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct media_player_title_changed
    {
        public int new_title;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct media_player_seekable_changed
    {
        public int new_seekable;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct media_player_pausable_changed
    {
        public int new_pausable;
    }

    /* media list */
    [StructLayout(LayoutKind.Sequential)]
    public struct media_list_item_added
    {
        public IntPtr item;
        public int index;
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
    [StructLayout(LayoutKind.Sequential)]
    public struct libvlc_media_track_info_t
    {
        public UInt32 i_codec;
        public int i_id;
        public libvlc_track_type_t i_type;
        public int i_profile;
        public int i_level;

        public libvlc_media_track_info_type audio_video;
    }
    /// <summary>
    /// Represents media track type
    /// </summary>
    public enum TrackType
    {
        Unknown = -1,
        Audio = 0,
        Video = 1,
        Text = 2,
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
    [StructLayout(LayoutKind.Sequential)]
    public struct media_list_will_add_item
    {
        public IntPtr item;
        public int index;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct media_list_item_deleted
    {
        public IntPtr item;
        public int index;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct media_list_will_delete_item
    {
        public IntPtr item;
        public int index;
    }

    /* media list player */
    [StructLayout(LayoutKind.Sequential)]
    public struct media_list_player_next_item_set
    {
        public IntPtr item;
    }

    /* snapshot taken */
    [StructLayout(LayoutKind.Sequential)]
    public struct media_player_snapshot_taken
    {
        public IntPtr psz_filename;
    }

    /* Length changed */
    [StructLayout(LayoutKind.Sequential)]
    public struct media_player_length_changed
    {
        public long new_length;
    }

    /* VLM media */
    [StructLayout(LayoutKind.Sequential)]
    public struct vlm_media_event
    {
        public IntPtr psz_media_name;
        public IntPtr psz_instance_name;
    }

    /* Extra MediaPlayer */
    [StructLayout(LayoutKind.Sequential)]
    public struct media_player_media_changed
    {
        public IntPtr new_media;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct libvlc_module_description_t
    {
        public IntPtr psz_name;
        public IntPtr psz_shortname;
        public IntPtr psz_longname;
        public IntPtr psz_help;
        public IntPtr p_next;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct libvlc_audio_output_t
    {
        public IntPtr psz_name;
        public IntPtr psz_description;
        public IntPtr p_next;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct libvlc_log_subscriber
    {
        public IntPtr prev;
        public IntPtr next;
        public IntPtr func;
        public IntPtr opaque;
    }
    //[StructLayout(LayoutKind.Sequential)]
    //public struct libvlc_event_t
    //{
    //    public libvlc_event_e type;
    //    public IntPtr p_obj;
    //    public MediaDescriptorUnion MediaDescriptor;
    //}


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

    public enum libvlc_log_level
    {
        LIBVLC_DEBUG = 0,   /**< Debug message */
        LIBVLC_NOTICE = 2,  /**< Important informational message */
        LIBVLC_WARNING = 3, /**< Warning (potential error) message */
        LIBVLC_ERROR = 4    /**< Error message */
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
            if (pd == null)
            {
                return false;
            }

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
            IntPtr zero = IntPtr.Zero;
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
            if(TryParseEnum<ChromaType>(chromaStr, true, out type))
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











    // http://www.videolan.org/developers/vlc/doc/doxygen/html/group__libvlc.html

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
    public delegate void NewFrameEH(Bitmap frame);

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
            GC.SuppressFinalize(this);
        }
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
    /// VLC pixel formats
    /// </summary>
    public enum ChromaType
    {
        /// <summary>
        /// 5 bit for each RGB channel
        /// </summary>
        RV15,

        /// <summary>
        /// 5 bit Red, 6 bit Green and 5 bit Blue
        /// </summary>
        RV16,

        /// <summary>
        /// 8 bit per channel
        /// </summary>
        RV24,

        /// <summary>
        /// 8 bit per RGB channel and 8 bit unused
        /// </summary>
        RV32,

        /// <summary>
        /// 8 bit per each RGBA channel
        /// </summary>
        RGBA,

        /// <summary>
        /// 12 bits per pixel planar format with Y plane followed by V and U planes
        /// </summary>
        YV12,

        /// <summary>
        /// Same as YV12 but V and U are swapped
        /// </summary>
        I420,

        /// <summary>
        /// 12 bits per pixel planar format with Y plane and interleaved UV plane
        /// </summary>
        NV12,

        /// <summary>
        /// 16 bits per pixel packed YUYV array
        /// </summary>
        YUY2,

        /// <summary>
        /// 16 bits per pixel packed UYVY array 
        /// </summary>
        UYVY,

        /// <summary>
        /// Same as I420, mainly used with MJPG codecs
        /// </summary>
        J420
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
            GC.SuppressFinalize(this);
        }
    }

    public class VlcMediaPlayer : IDisposable
    {
        internal IntPtr Handle;
        private IntPtr drawable;
        private bool playing, paused;
        //Action<Exception> m_excHandler = null;
        //BitmapFormat m_format;
        MemoryRenderer m_memRender = null;
        MemoryRendererEx m_memRenderEx = null;
        //Func<BitmapFormat, BitmapFormat> m_formatSetupCB = null;
        //  PlanarPixelData m_pixelData = default(PlanarPixelData);
        // void* m_pBuffer = null;

        public VlcMediaPlayer(VlcMedia media)
        {
            Handle = LibVlc.libvlc_media_player_new_from_media(media.Handle);
            if (Handle == IntPtr.Zero) throw new VlcException();

            // LibVlc.libvlc_video_set_callbacks(Handle,);
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


        //public void SetFormat(BitmapFormat format)
        //{
        //    m_format = format;

        //    LibVlc.libvlc_video_set_format(m_hMediaPlayer, m_format.Chroma.ToUtf8(), m_format.Width, m_format.Height, m_format.Pitch);
        //    m_pBuffer = MemoryHeap.Alloc(m_format.ImageSize);

        //    m_pixelData = new PixelData(m_format.ImageSize);
        //    m_pixelDataPtr = GCHandle.Alloc(m_pixelData, GCHandleType.Pinned);
        //    LibVlcMethods.libvlc_video_set_callbacks(m_hMediaPlayer, pLockCallback, pUnlockCallback, pDisplayCallback, m_pixelDataPtr.AddrOfPinnedObject());
        //}
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

        public bool SetVolume(int volume)
        {
            int volumeSet = LibVlc.libvlc_audio_set_volume(Handle, volume);

            if (volumeSet == 0)
                return true;
            else
                return false;
        }

        public long getTime()
        {
            return LibVlc.libvlc_media_player_get_time(Handle);        
        }

        public long getTotalTime()
        {
            return LibVlc.libvlc_media_player_get_length(Handle);
        }

        public void setTime(long timeMS)
        {
            LibVlc.libvlc_media_player_set_time(Handle, timeMS);
        }
    }
}
