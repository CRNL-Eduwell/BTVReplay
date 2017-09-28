using System;
using System.Runtime.InteropServices;

namespace VLCSharp.Tools
{
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

    public enum libvlc_log_level
    {
        LIBVLC_DEBUG = 0,   /**< Debug message */
        LIBVLC_NOTICE = 2,  /**< Important informational message */
        LIBVLC_WARNING = 3, /**< Warning (potential error) message */
        LIBVLC_ERROR = 4    /**< Error message */
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

    [StructLayout(LayoutKind.Sequential)]
    public struct audio
    {
        public int i_channels;
        public int i_rate;
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
}
