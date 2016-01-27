using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using LibSndFile;
using OpenTK;
using OpenTK.Audio.OpenAL;

namespace UniRaider
{
    public partial class Constants
    {
        /// <summary>
        /// AL_UNITS constant is used to translate native TR coordinates into
        /// OpenAL coordinates. By default, it's the same as geometry grid
        /// resolution (1024).
        /// </summary>
        public const float TR_AUDIO_AL_UNITS = 1024.0f;

        /// <summary>
        /// MAX_CHANNELS defines maximum amount of sound sources (channels)
        /// that can play at the same time. Contemporary devices can play
        /// up to 256 channels, but we set it to 32 for compatibility
        /// reasons.
        /// </summary>
        public const uint TR_AUDIO_MAX_CHANNELS = 32;

        /// <summary>
        /// MAX_SLOTS specifies amount of FX slots used to apply environmental
        /// effects to sounds. We need at least two of them to prevent glitches
        /// at environment transition (slots are cyclically changed, leaving
        /// previously played samples at old slot). Maximum amount is 4, but
        /// it's not recommended to set it more than 2.
        /// </summary>
        public const byte TR_AUDIO_MAX_SLOTS = 2;

        /// <summary>
        /// Sound flags are found at offset 7 of SoundDetail unit and specify
        /// certain sound modifications.
        /// </summary>
        public const byte TR_AUDIO_FLAG_UNKNOWN = 0x10;

        /// <summary>
        /// Sample number mask is a mask value used in bitwise operation with
        /// "num_samples_and_flags_1" field to extract amount of samples per
        /// effect.
        /// </summary>
        public const byte TR_AUDIO_SAMPLE_NUMBER_MASK = 0x0F;

        /// <summary>
        /// <see cref="TR_AUDIO_STREAM_NUMBUFFERS"/> is a number of buffers cyclically used for each stream.
        /// Double is enough, but we use quad for further stability.
        /// </summary>
        public const byte TR_AUDIO_STREAM_NUMBUFFERS = 4;

        /// <summary>
        /// <see cref="TR_AUDIO_STREAM_NUMSOURCES"/> tells the engine how many sources we should reserve for
        /// in-game music and BGMs, considering crossfades. By default, it's 6,
        /// as it's more than enough for typical TR audio setup (one BGM track
        /// plus one one-shot track or chat track in TR5).
        /// </summary>
        public const byte TR_AUDIO_STREAM_NUMSOURCES = 6;

        /// <summary>
        /// MAP_SIZE is similar to sound map size, but it is used to mark
        /// already played audiotracks. Note that audiotracks CAN play several
        /// times, if they were consequently called with increasing activation
        /// flags (e.g., at first we call it with 00001 flag, then with 00101,
        /// and so on). If all activation flags were set, including only once
        /// flag, audiotrack won't play anymore.
        /// </summary>
        public const int TR_AUDIO_STREAM_MAP_SIZE = 256;

        public const int TR_AUDIO_STREAM_WAD_STRIDE = 268;

        public const int TR_AUDIO_STREAM_WAD_NAMELENGTH = 260;

        public const int TR_AUDIO_STREAM_WAD_COUNT = 130;

        // Crossfades for different track types are also different,
        // since background ones tend to blend in smoothly, while one-shot
        // tracks should be switched fastly.

        public const float TR_AUDIO_STREAM_CROSSFADE_ONESHOT = GAME_LOGIC_REFRESH_INTERVAL / 0.3f;

        public const float TR_AUDIO_STREAM_CROSSFADE_BACKGROUND = GAME_LOGIC_REFRESH_INTERVAL / 1.0f;

        public const float TR_AUDIO_STREAM_CROSSFADE_CHAT = GAME_LOGIC_REFRESH_INTERVAL / 0.1f;

        /// <summary>
        /// Damp coefficient specifies target volume level on a tracks
        /// that are being silenced (background music). The larger it is, the bigger
        /// silencing is.
        /// </summary>
        public const float TR_AUDIO_STREAM_DAMP_LEVEL = 0.6f;

        /// <summary>
        /// Damp fade speed is used when dampable track is either being
        /// damped or un-damped.
        /// </summary>
        public const float TR_AUDIO_STREAM_DAMP_SPEED = GAME_LOGIC_REFRESH_INTERVAL / 1.0f;

        public const double TR_AUDIO_DEINIT_DELAY = 2.0;
    }

    /// <summary>
    /// In TR3-5, there were 5 reverb / echo effect flags for each
    /// room, but they were never used in PC versions - however, level
    /// files still contain this info, so we now can re-use these flags
    /// to assign reverb/echo presets to each room.
    /// Also, underwater environment can be considered as additional
    /// reverb flag, so overall amount is 6.
    /// </summary>
    public enum TR_AUDIO_FX
    {
        Outside, // EFX_REVERB_PRESET_CITY
        SmallRoom, // EFX_REVERB_PRESET_LIVINGROOM
        MediumRoom, // EFX_REVERB_PRESET_WOODEN_LONGPASSAGE
        LargeRoom, // EFX_REVERB_PRESET_DOME_TOMB
        Pipe, // EFX_REVERB_PRESET_PIPE_LARGE
        Water, // EFX_REVERB_PRESET_UNDERWATER
        LastIndex
    }

    /// <summary>
    /// Entity types are used to identify different sound emitter types. Since
    /// sounds in TR games could be emitted either by entities, sound sources
    /// or global events, we have defined these three types of emitters.
    /// </summary>
    public enum TR_AUDIO_EMITTER
    {
        /// <summary>
        /// Entity (movable)
        /// </summary>
        Entity = 0,

        /// <summary>
        /// Sound source (static)
        /// </summary>
        SoundSource = 1,

        /// <summary>
        /// Global sound (menus, secret, etc.)
        /// </summary>
        Global = 2
    }

    /// <summary>
    /// Possible types of errors returned by Audio_Send / Audio_Kill functions.
    /// </summary>
    public enum TR_AUDIO_SEND
    {
        NoSample = -2,
        NoChannel = -1,
        Ignored = 0,
        Processed = 1
    }

    /// <summary>
    /// Define some common samples across ALL TR versions.
    /// </summary>
    public enum TR_AUDIO_SOUND
    {
        No = 2,
        Sliding = 3,
        Landing = 4,
        HolsterOut = 6,
        HolsterIn = 7,
        ShotPistols = 8,
        Reload = 9,
        Ricochet = 10,
        LaraScream = 30,
        LaraInjury = 31,
        Splash = 33,
        FromWater = 34,
        Swim = 35,
        LaraBreath = 36,
        Bubble = 37,
        UseKey = 39,
        ShotUzi = 43,
        ShotShotgun = 45,
        Underwater = 60,
        Pushable = 63,
        MenuRotate = 108,
        MenuSelect = 109,
        MenuOpen = 111,
        MenuClose = 112, // Only used in TR1-3.
        MenuClang = 114,
        MenuPage = 115,
        Medipack = 116
    }

    /// <summary>
    /// Certain sound effect indexes were changed across different TR
    /// versions, despite remaining the same - mostly, it happened with
    /// menu sounds and some general sounds. For such effects, we specify
    /// additional remap enumeration list, which is fed into Lua script
    /// to get actual effect ID for current game version.
    /// </summary>
    public enum TR_AUDIO_SOUND_GLOBALID
    {
        MenuOpen,
        MenuClose,
        MenuRotate,
        MenuPage,
        MenuSelect,
        MenuWeapon,
        MenuClang,
        MenuAudioTest,
        LastIndex
    }

    /// <summary>
    /// Stream loading method describes the way audiotracks are loaded.
    /// There are either seperate track files or single CDAUDIO.WAD file.
    /// </summary>
    public enum TR_AUDIO_STREAM_METHOD
    {
        Track, // Separate tracks. Used in TR 1, 2, 4, 5.
        Wad, // WAD file.  Used in TR3.
        LastIndex
    }

    /// <summary>
    /// Audio stream type defines stream behaviour. While background track
    /// loops forever until interrupted by other background track, one-shot
    /// and chat tracks doesn't interrupt them, playing in parallel instead.
    /// However, all stream types could be interrupted by next pending track
    /// with same type.
    /// </summary>
    public enum TR_AUDIO_STREAM_TYPE
    {
        Background, // BGM tracks.
        OneShot, // One-shot music pieces.
        Chat, // Chat tracks.
        LastIndex
    }

    /// <summary>
    /// Possible errors produced by Audio_StreamPlay / Audio_StreamStop functions.
    /// </summary>
    public enum TR_AUDIO_STREAMPLAY
    {
        PlayError = -4,
        LoadError = -3,
        WrongTrack = -2,
        NoFreeStream = -1,
        Ignored = 0,
        Processed = 1
    }

    /// <summary>
    /// Audio settings structure.
    /// </summary>
    public struct AudioSettings
    {
        public float MusicVolume { get; set; }

        public float SoundVolume { get; set; }

        public bool UseEffects { get; set; }

        public bool EffectsInitialized { get; set; }

        public bool ListenerIsPlayed { get; set; } // RESERVED FOR FUTURE USE

        public int StreamBufferSize { get; set; }
    }

    /// <summary>
    /// FX manager structure.<br/>
    /// It contains all necessary info to process sample FX (reverb and echo).
    /// </summary>
    public struct AudioFxManager
    {
        public uint ALFilter { get; set; }

        public uint ALEffect { get; set; }

        public uint ALSlot { get; set; }

        public uint CurrentSlot { get; set; }

        public uint CurrentRoomType { get; set; }

        public uint LastRoomType { get; set; }

        /// <summary>
        /// If listener is underwater, all samples will damp.
        /// </summary>
        public sbyte WaterState { get; set; }
    }

    /// <summary>
    /// Effect structure.<br/>
    /// Contains all global effect parameters.
    /// </summary>
    public struct AudioEffect
    {
        #region General sound source parameters

        /// <summary>
        /// [PIT in TR] Global pitch shift
        /// </summary>
        public float Pitch { get; set; }

        /// <summary>
        /// [VOL in TR] Global gain (volume)
        /// </summary>
        public float Gain { get; set; }

        /// <summary>
        /// [RAD in TR] Range (radius)
        /// </summary>
        public float Range { get; set; }

        /// <summary>
        /// [CH in TR] Chance to play
        /// </summary>
        public float Chance { get; set; }

        public Loader.LoopType Loop { get; set; }

        /// <summary>
        /// Similar to flag 0x200000 (P) in native TRs.
        /// </summary>
        public bool RandomizePitch { get; set; }

        /// <summary>
        /// Similar to flag 0x400000 (V) in native TRs.
        /// </summary>
        public bool RandomizeGain { get; set; }

        #endregion

        #region Additional sound source parameters

        /// <summary>
        /// Slightly randomize frequency
        /// </summary>
        public bool RandomizeFreq { get; set; }

        /// <summary>
        /// Pitch randomizer bounds
        /// </summary>
        public uint RandomizePitchVar { get; set; }

        /// <summary>
        /// Gain randomizer bounds
        /// </summary>
        public uint RandomizeGainVar { get; set; }

        /// <summary>
        /// Frequency randomizer bounds
        /// </summary>
        public uint RandomizeFreqVar { get; set; }

        #endregion

        #region Sample reference parameters

        /// <summary>
        /// First (or only) sample (buffer) index
        /// </summary>
        public uint SampleIndex { get; set; }

        /// <summary>
        /// Sample amount to randomly select from
        /// </summary>
        public uint SampleCount { get; set; }

        #endregion
    }

    /// <summary>
    /// Audio emitter (aka SoundSource) structure.
    /// </summary>
    public struct AudioEmitter
    {
        /// <summary>
        /// Unique emitter index
        /// </summary>
        public uint EmitterIndex { get; set; }

        /// <summary>
        /// Sound index
        /// </summary>
        public uint SoundIndex { get; set; }

        /// <summary>
        /// Vector coordinate
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// Flags - MEANING UNKNOWN
        /// </summary>
        public ushort Flags { get; set; }
    }

    /// <summary>
    /// Main audio source class.
    /// Sound source is a complex class, each member of which is linked with
    /// certain in-game entity or sound source, but also a kind of entity by itself.
    /// Number of simultaneously existing sound sources is fixed, and can't be more than
    /// MAX_CHANNELS global constant.
    /// </summary>
    public class AudioSource
    {
        /// <summary>
        /// Make source active and play it
        /// </summary>
        public void Play();

        /// <summary>
        /// Pause source (leaving it active)
        /// </summary>
        public void Pause();

        /// <summary>
        /// Stop and destroy source
        /// </summary>
        public void Stop();

        /// <summary>
        /// Update source parameters
        /// </summary>
        public void Update();

        /// <summary>
        /// Assign buffer to source
        /// </summary>
        /// <param name="buffer">Buffer</param>
        public void SetBuffer(int buffer);

        /// <summary>
        /// Set looping flag
        /// </summary>
        /// <param name="isLooping">Looping flag</param>
        public void SetLooping(bool isLooping);

        /// <summary>
        /// Set pitch shift
        /// </summary>
        /// <param name="pitch">Pitch shift</param>
        public void SetPitch(float pitch);

        /// <summary>
        /// Set gain (volume)
        /// </summary>
        /// <param name="gain">Gain (volume)</param>
        public void SetGain(float gain);

        /// <summary>
        /// Set maximum audible distante
        /// </summary>
        /// <param name="range">Maximum audible distance</param>
        public void SetRange(float range);

        /// <summary>
        /// Set reverb FX, according to room flag
        /// </summary>
        public void SetFX();

        /// <summary>
        /// Remove any reverb FX from source
        /// </summary>
        public void UnsetFX();

        /// <summary>
        /// Apply low-pass underwater filter
        /// </summary>
        public void SetUnderwater();

        /// <summary>
        /// Check if source is looping
        /// </summary>
        public bool IsLooping();

        /// <summary>
        /// Check if source is currently playing
        /// </summary>
        public bool IsPlaying();

        /// <summary>
        /// Check if source is active
        /// </summary>
        public bool IsActive();

        /// <summary>
        /// Entity of origin. -1 means no entity (hence - empty source)
        /// </summary>
        public int EmitterID { get; set; }

        /// <summary>
        /// 0 - ordinary entity, 1 - sound source, 2 - global sound
        /// </summary>
        public uint EmitterType { get; set; }

        /// <summary>
        /// Effect index. Used to associate effect with entity for R/W flags
        /// </summary>
        public uint EffectIndex { get; set; }

        /// <summary>
        /// OpenAL sample (buffer) index. May be the same for different sources
        /// </summary>
        public uint SampleIndex { get; set; }

        /// <summary>
        /// How many buffers to use, beginning with <see cref="SampleIndex"/>
        /// </summary>
        public uint SampleCount { get; set; }

        public static int IsEffectPlaying(int effectID, int entityType, int entityID);

        /// <summary>
        /// Source gets autostopped and destroyed on next frame, if it's not set.
        /// </summary>
        private bool active;

        /// <summary>
        /// Marker to define if sample is in underwater state or not.
        /// </summary>
        private bool isWater;

        /// <summary>
        /// Source index. Should be unique for each source.
        /// </summary>
        private uint sourceIndex = 0;

        /// <summary>
        /// Link source to parent emitter
        /// </summary>
        private void linkEmitter();

        /// <summary>
        /// Set source position
        /// </summary>
        private void setPosition(float[] posVector);

        /// <summary>
        /// Set source velocity (speed)
        /// </summary>
        private void setVelocity(float[] velVector);
    }

    /// <summary>
    /// Main stream track class is used to create multi-channel soundtrack player,
    /// which differs from classic TR scheme, where each new soundtrack interrupted
    /// previous one. With flexible class handling, we now can implement multitrack
    /// player with automatic channel and crossfade management.
    /// </summary>
    public struct StreamTrack
    {
        /// <summary>
        /// Load routine prepares track for playing. Arguments are track index,
        /// stream type (background, one-shot or chat) and load method, which
        /// differs for TR1-2, TR3 and TR4-5.
        /// </summary>
        public bool Load(string path, int index, int type, int loadMethod);

        public bool Unload();

        /// <summary>
        /// Begin to play track
        /// </summary>
        public bool Play(bool fadeIn = false);

        /// <summary>
        /// Pause track, preserving position
        /// </summary>
        public void Pause();

        /// <summary>
        /// End track with fade-out
        /// </summary>
        public void End();

        /// <summary>
        /// Immediately stop track
        /// </summary>
        public void Stop();

        /// <summary>
        /// Update track and manage streaming
        /// </summary>
        public bool Update();

        /// <summary>
        /// Check desired track's index
        /// </summary>
        public bool IsTrack(int trackIndex);

        /// <summary>
        /// Check desired track's type
        /// </summary>
        public bool IsType(int trackType);

        /// <summary>
        /// Check if track is playing
        /// </summary>
        public bool IsPlaying();

        /// <summary>
        /// Check if track is active
        /// </summary>
        public bool IsActive();

        /// <summary>
        /// Check if track is dampable
        /// </summary>
        public bool IsDampable();

        /// <summary>
        /// Set reverb FX, according to room flag
        /// </summary>
        public void SetFX();

        /// <summary>
        /// Remove any reverb FX from source
        /// </summary>
        public void UnsetFX();

        /// <summary>
        /// Global flag for damping BGM tracks
        /// </summary>
        public static bool DampActive { get; set; }

        /// <summary>
        /// Track loading
        /// </summary>
        private bool loadTrack(string path);

        /// <summary>
        /// Wad loading
        /// </summary>
        private bool loadWad(byte index, string fileName);

        /// <summary>
        /// General stream routine
        /// </summary>
        private bool stream(uint buffer);

        /// <summary>
        /// General handle for opened wad file
        /// </summary>
        private FileStream wadFile;

        /// <summary>
        /// Sndfile file reader needs its own handle
        /// </summary>
        private SndFile sndFile;

        private SndFileInfo sfInfo;

        #region General OpenAL fields

        private uint source;
        private uint[] buffers;
        private uint format;
        private int rate;

        /// <summary>
        /// Stream volume, considering fades
        /// </summary>
        private float currentVolume;

        /// <summary>
        /// Additional damp volume multiplier
        /// </summary>
        private float dampedVolumes;

        #endregion

        /// <summary>
        /// If track is active or not
        /// </summary>
        private bool active;

        /// <summary>
        /// Used when track is being faded by other one
        /// </summary>
        private bool ending;

        /// <summary>
        /// Specifies if track can be damped by others
        /// </summary>
        private bool dampable;

        /// <summary>
        /// Either BACKGROUND, ONESHOT or CHAT
        /// </summary>
        private int streamType;

        /// <summary>
        /// Needed to prevent same track sending
        /// </summary>
        private int currentTrack;

        /// <summary>
        /// TRACK (TR1-2/4-5) or WAD (TR3)
        /// </summary>
        private int method;
    }

    public class Audio
    {
        #region General audio routines

        public static void InitGlobals();

        public static void InitFX();

        public static void Init(uint numSources = Constants.TR_AUDIO_MAX_CHANNELS);

        public static int DeInit();

        public static void Update();

        #endregion

        #region Audio source (samples) routines

        public static int GetFreeSource();

        public static bool IsInRange(int entityType, int entityID, float range, float gain);

        public static int IsEffectPlaying(int effectID = -1, int entityType = -1, int entityID = -1);

        /// <summary>
        /// Send to play effect with given parameters.
        /// </summary>
        public static int Send(int effectID, int entityType = Constants.TR_AUDIO_EMITTER_GLOBAL, int entityID = 0);

        /// <summary>
        /// If exist, immediately stop and destroy all effects with given parameters.
        /// </summary>
        public static int Kill(int effectID, int entityType = Constants.TR_AUDIO_EMITTER_GLOBAL, int entityID = 0);

        /// <summary>
        /// Used to pause all effects currently playing.
        /// </summary>
        public static void PauseAllSources();

        /// <summary>
        /// Used in audio deinit.
        /// </summary>
        public static void StopAllSources();

        /// <summary>
        /// Used to resume all effects currently paused.
        /// </summary>
        public static void ResumeAllSources();

        /// <summary>
        /// Main sound loop.
        /// </summary>
        public static void UpdateSources();

        public static void UpdateListenerByCamera(Camera cam);

        public static void UpdateListenerByEntity(Entity ent);

        public static bool FillALBuffer(uint bufNumber, SndFile wavFile, uint bufSize, SndFileInfo sfInfo);

        public static unsafe int LoadALBufferFromMem(uint bufNumber, byte* samplePointer, uint sampleSize,
            uint uncompSampleSize = 0);

        public static int LoadALBufferFromFile(uint bufNumber, string filename);

        public static void LoadOverridedSamples(World world);

        public static int LoadReverbToFX(int effectID, EffectsExtension.EfxEaxReverb reverb);

        #endregion

        #region Stream tracks (music / BGM) routines

        /// <summary>
        /// Get free (stopped) stream
        /// </summary>
        public static int GetFreeStream();

        /// <summary>
        /// See if track is already playing
        /// </summary>
        public static bool IsTrackPlaying(int trackID = -1);

        /// <summary>
        /// Check if track played with given activation mask
        /// </summary>
        public static bool TrackAlreadyPlayed(uint trackID, sbyte mask = 0);

        /// <summary>
        /// Update all streams
        /// </summary>
        public static void UpdateStreams();

        /// <summary>
        /// See if there is any damping tracks playing
        /// </summary>
        public static void UpdateStreamsDamping();

        /// <summary>
        /// Pause all streams [of specified type]
        /// </summary>
        public static void PauseStreams(int streamType = -1);

        /// <summary>
        /// Resume all streams [of specified type]
        /// </summary>
        public static void ResumeStreams(int streamType = -1);

        /// <summary>
        /// End all streams (with crossfade) [of specified type]
        /// </summary>
        public static void EndStreams(int streamType = -1);

        /// <summary>
        /// Immediately stop all streams [of specified type]
        /// </summary>
        public static void StopStreams(int streamType = -1);

        // Generally, you need only this function to trigger any track
        // TODO: Inconsistent type: trackID is uint here and in TrackAlreadyPlayed, is int in IsTrackPlaying
        public static int StreamPlay(uint trackID, byte mask = 0);

        #endregion

        #region Error handling routines

        /// <summary>
        /// AL-specific error handler
        /// </summary>
        public static bool LogALError(int errorMarker = 0);

        /// <summary>
        /// <see cref="SndFile"/>-specified error handler
        /// </summary>
        public static void LogSndFileError(int code);

        #endregion

        #region Helper functions

        public static float GetByteDepth(SndFileInfo sfInfo);

        public static void LoadALExtFunctions(IntPtr device);

        public static bool DeInitDelay();

        #endregion
    }
}
