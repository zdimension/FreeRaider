using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using LibSndFile;
using OpenTK;
using OpenTK.Audio.OpenAL;
using UniRaider.Loader;

namespace UniRaider
{
    public partial class Constants
    {
        public const int AL_FILTER_NULL = 0;

        public const int AL_EFFECTSLOT_NULL = 0;

        public const int AL_EFFECT_NULL = 0;

        public const int SF_FORMAT_SUBMASK = 0x0000FFFF;

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
    public enum TR_AUDIO_EMITTER : uint
    {
        Unknown = uint.MaxValue,

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
    public enum TR_AUDIO_SEND : int
    {
        NoSample = -2,
        NoChannel = -1,
        Ignored = 0,
        Processed = 1
    }

    /// <summary>
    /// Define some common samples across ALL TR versions.
    /// </summary>
    public enum TR_AUDIO_SOUND : int
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
        Unknown = -1,
        Track = 0, // Separate tracks. Used in TR 1, 2, 4, 5.
        Wad = 1, // WAD file.  Used in TR3.
        LastIndex = 2
    }

    /// <summary>
    /// Audio stream type defines stream behaviour. While background track
    /// loops forever until interrupted by other background track, one-shot
    /// and chat tracks doesn't interrupt them, playing in parallel instead.
    /// However, all stream types could be interrupted by next pending track
    /// with same type.
    /// </summary>
    public enum TR_AUDIO_STREAM_TYPE : int
    {
        Unknown = -1,
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

        public bool ListenerIsPlayer { get; set; } // RESERVED FOR FUTURE USE

        public int StreamBufferSize { get; set; }
    }

    /// <summary>
    /// FX manager structure.<br/>
    /// It contains all necessary info to process sample FX (reverb and echo).
    /// </summary>
    public struct AudioFxManager
    {
        public uint ALFilter;

        public uint[] ALEffect { get; set; }

        public uint[] ALSlot { get; set; }

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
        public AudioSource()
        {
            IsActive = false;
            EmitterID = -1;
            EmitterType = TR_AUDIO_EMITTER.Entity;
            EffectIndex = 0;
            SampleIndex = 0;
            SampleCount = 0;
            isWater = false;
            AL.GenSource(out sourceIndex);

            if(AL.IsSource(sourceIndex))
            {
                AL.Source(sourceIndex, ALSourcef.MinGain, 0.0f);
                AL.Source(sourceIndex, ALSourcef.MaxGain, 1.0f);

                if(audio_settings.UseEffects)
                {
                    AL.Source(sourceIndex, ALSourcef.EfxRoomRolloffFactor, 1.0f);
                    AL.Source(sourceIndex, ALSourceb.EfxAuxiliarySendFilterGainAuto, true);
                    AL.Source(sourceIndex, ALSourceb.EfxAuxiliarySendFilterGainHighFrequencyAuto, true);
                    AL.Source(sourceIndex, ALSourcef.EfxAirAbsorptionFactor, 0.1f);
                }
                else
                {
                    AL.Source(sourceIndex, ALSourcef.EfxAirAbsorptionFactor, 0.0f);
                }
            }
        }

        ~AudioSource()
        {
            if(AL.IsSource(sourceIndex))
            {
                AL.SourceStop(sourceIndex);
                AL.DeleteSource(ref sourceIndex);
            }
        }

        /// <summary>
        /// Make source active and play it
        /// </summary>
        public void Play()
        {
            if(AL.IsSource(sourceIndex))
            {
                if(EmitterType == TR_AUDIO_EMITTER.Global)
                {
                    AL.Source(sourceIndex, ALSourceb.SourceRelative, true);
                    AL.Source(sourceIndex, ALSource3f.Position, 0.0f, 0.0f, 0.0f);
                    AL.Source(sourceIndex, ALSource3f.Velocity, 0.0f, 0.0f, 0.0f);

                    if(audio_settings.UseEffects)
                    {
                        UnsetFX();
                    }
                }
                else
                {
                    AL.Source(sourceIndex, ALSourceb.SourceRelative, false);
                    linkEmitter();

                    if (audio_settings.UseEffects)
                    {
                        SetFX();
                        SetUnderwater();
                    }
                }

                AL.SourcePlay(sourceIndex);
                IsActive = true;
            }
        }

        /// <summary>
        /// Pause source (leaving it active)
        /// </summary>
        public void Pause()
        {
            if(AL.IsSource(sourceIndex))
            {
                AL.SourcePause(sourceIndex);
            }
        }

        /// <summary>
        /// Stop and destroy source
        /// </summary>
        public void Stop()
        {
            if (AL.IsSource(sourceIndex))
            {
                AL.SourceStop(sourceIndex);
            }
        }

        /// <summary>
        /// Update source parameters
        /// </summary>
        public void Update()
        {
            // Bypass any non-active source.
            if (!IsActive) return;

            // Disable and bypass source, if it is stopped.
            if (!IsPlaying)
            {
                IsActive = false;
                return;
            }

            if (EmitterType == TR_AUDIO_EMITTER.Global) return;

            float range, gain;

            AL.GetSource(sourceIndex, ALSourcef.Gain, out gain);
            AL.GetSource(sourceIndex, ALSourcef.MaxDistance, out range);

            // Check if source is in listener's range, and if so, update position, else stop and disable it.
            if (Audio.IsInRange(EmitterType, EmitterID, range, gain))
            {
                linkEmitter();

                if(audio_settings.UseEffects && isWater != (Audio.FXManager.WaterState != 0))
                {
                    SetUnderwater();
                }
            }
            else
            {
                // Immediately stop source only if track is looped. It allows sounds which
                // were activated for already destroyed entities to finish (e.g. grenade
                // explosions, ricochets, and so on).

                if (IsLooping) Stop();
            }
        }

        /// <summary>
        /// Assign buffer to source
        /// </summary>
        /// <param name="buffer">Buffer</param>
        public void SetBuffer(int buffer)
        {
            uint bufferID = engine_world.AudioBuffers[buffer];

            if(AL.IsSource(sourceIndex) && AL.IsBuffer(bufferID))
            {
                AL.Source(sourceIndex, ALSourcei.Buffer, (int)bufferID);

                // For some reason, OpenAL sometimes produces "Invalid Operation" error here,
                // so there's extra debug info - maybe it'll help some day.

                /*
                if(Audio.LogALError(1))
                {
                    int channels, bits, freq;

                    AL.GetBuffer(bufferID, ALGetBufferi.Channels, out channels);
                    AL.GetBuffer(bufferID, ALGetBufferi.Bits, out bits);
                    AL.GetBuffer(bufferID, ALGetBufferi.Frequency, out freq);

                    Debug.WriteLine($"Faulty buffer {bufferID} info: CH{channels}, B{bits}, F{freq}");
                }
                */
            }
        }

        /// <summary>
        /// Pitch shift
        /// </summary>
        public float Pitch
        {
            get
            {
                var pitch = 1.0f;
                if (AL.IsSource(sourceIndex))
                {
                    AL.GetSource(sourceIndex, ALSourcef.Pitch, out pitch);
                }
                return pitch;
            }
            set
            {
                if (AL.IsSource(sourceIndex))
                {
                    AL.Source(sourceIndex, ALSourcef.Pitch, value.Clamp(0.1f, 2.0f));
                }
            }
        }

        /// <summary>
        /// Gain (volume)
        /// </summary>
        public float Gain
        {
            get
            {
                var gain = 1.0f;
                if (AL.IsSource(sourceIndex))
                {
                    AL.GetSource(sourceIndex, ALSourcef.Gain, out gain);
                }
                return gain;
            }
            set
            {
                if (AL.IsSource(sourceIndex))
                {
                    AL.Source(sourceIndex, ALSourcef.Gain, value.Clamp(0.0f, 1.0f) * audio_settings.SoundVolume);
                }
            }
        }

        /// <summary>
        /// Set maximum audible distante
        /// </summary>
        /// <param name="range">Maximum audible distance</param>
        public void SetRange(float range)
        {
            if(AL.IsSource(sourceIndex))
            {
                // Source will become fully audible on 1/6 of overall position.
                AL.Source(sourceIndex, ALSourcef.ReferenceDistance, range / 6.0f);
                AL.Source(sourceIndex, ALSourcef.MaxDistance, range);
            }
        }

        /// <summary>
        /// Set reverb FX, according to room flag
        /// </summary>
        public void SetFX()
        {
            uint effect, slot;

            // Reverb FX is applied globally through audio send. Since player can
            // jump between adjacent rooms with different reverb info, we assign
            // several (2 by default) interchangeable audio sends, which are switched
            // every time current room reverb is changed.

            if (Audio.FXManager.CurrentRoomType != Audio.FXManager.LastRoomType) // Switch audio send
            {
                Audio.FXManager.LastRoomType = Audio.FXManager.CurrentRoomType;
                Audio.FXManager.CurrentSlot = 
                    ++Audio.FXManager.CurrentSlot > Constants.TR_AUDIO_MAX_SLOTS - 1
                    ? 0
                    : Audio.FXManager.CurrentSlot;

                effect = Audio.FXManager.ALEffect[Audio.FXManager.CurrentRoomType];
                slot = Audio.FXManager.ALSlot[Audio.FXManager.CurrentSlot];

                if(Audio.EffectsExtension.IsAuxiliaryEffectSlot(slot) && Audio.EffectsExtension.IsEffect(effect))
                {
                    Audio.EffectsExtension.AuxiliaryEffectSlot(slot, EfxAuxiliaryi.EffectslotEffect, (int)effect);
                }
            }
            else // Do not switch audio send.
            {
                slot = Audio.FXManager.ALSlot[Audio.FXManager.CurrentSlot];
            }

            // Assign global reverb FX to channel.

            AL.Source(sourceIndex, ALSource3i.EfxAuxiliarySendFilter, (int)slot, 0, Constants.AL_FILTER_NULL);
        }

        /// <summary>
        /// Remove any reverb FX from source
        /// </summary>
        public void UnsetFX()
        {
            // Remove any audio sends and direct filters from channel.

            AL.Source(sourceIndex, ALSourcei.EfxDirectFilter, Constants.AL_FILTER_NULL);
            AL.Source(sourceIndex, ALSource3i.EfxAuxiliarySendFilter, Constants.AL_EFFECTSLOT_NULL, 0, Constants.AL_FILTER_NULL);
        }

        /// <summary>
        /// Apply low-pass underwater filter
        /// </summary>
        public void SetUnderwater()
        {
            // Water low-pass filter is applied when source's is_water flag is set.
            // Note that it is applied directly to channel, i. e. all sources that
            // are underwater will damp, despite of global reverb setting.

            if(Audio.FXManager.WaterState != 0)
            {
                AL.Source(sourceIndex, ALSourcei.EfxDirectFilter, (int)Audio.FXManager.ALFilter);
                isWater = true;
            }
            else
            {
                AL.Source(sourceIndex, ALSourcei.EfxDirectFilter, Constants.AL_FILTER_NULL);
                isWater = false;
            }
        }

        /// <summary>
        /// Check if source is looping
        /// </summary>
        public bool IsLooping
        {
            get
            {
                var looping = false;
                if(AL.IsSource(sourceIndex))
                {
                    AL.GetSource(sourceIndex, ALSourceb.Looping, out looping);
                }
                return looping;
            }
            set
            {
                if(AL.IsSource(sourceIndex))
                {
                    AL.Source(sourceIndex, ALSourceb.Looping, value);
                }
            }
        }

        /// <summary>
        /// Check if source is currently playing
        /// </summary>
        public bool IsPlaying
        {
            get
            {
                if (AL.IsSource(sourceIndex))
                {
                    var state = (int)ALSourceState.Stopped;
                    AL.GetSource(sourceIndex, ALGetSourcei.SourceState, out state);
                    var state2 = (ALSourceState) state;
                    // Paused state and existing file pointers also counts as playing.
                    return state2 == ALSourceState.Playing || state2 == ALSourceState.Paused;
                }
                return false;
            }
        }

        /// <summary>
        /// Check if source is active
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Entity of origin. -1 means no entity (hence - empty source)
        /// </summary>
        public int EmitterID { get; set; }

        /// <summary>
        /// 0 - ordinary entity, 1 - sound source, 2 - global sound
        /// </summary>
        public TR_AUDIO_EMITTER EmitterType { get; set; }

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
        private void linkEmitter()
        {
            switch (EmitterType)
            {
                case TR_AUDIO_EMITTER.Entity:
                    Entity ent = null;
                    if ((ent = engine_world.GetEntityByID(EmitterID)) != null)
                    {
                        Position = ent.Transform.Origin;
                        Velocity = ent.Speed;
                    }
                    return;
                case TR_AUDIO_EMITTER.SoundSource:
                    Position = engine_world.AudioEmitters[EmitterID].Position;
                    return;
            }
        }

        /// <summary>
        /// Source position
        /// </summary>
        public Vector3 Position
        {
            get
            {
                float x, y, z;
                x = y = z = 0.0f;
                if (AL.IsSource(sourceIndex))
                {
                    AL.GetSource(sourceIndex, ALSource3f.Position, out x, out y, out z);
                }
                return new Vector3(x, y, z);
            }
            set
            {
                if (AL.IsSource(sourceIndex))
                {
                    AL.Source(sourceIndex, ALSource3f.Position, value.X, value.Y, value.Z);
                }
            }
        }

        /// <summary>
        /// Source velocity (speed)
        /// </summary>
        public Vector3 Velocity
        {
            get
            {
                float x, y, z;
                x = y = z = 0.0f;
                if (AL.IsSource(sourceIndex))
                {
                    AL.GetSource(sourceIndex, ALSource3f.Velocity, out x, out y, out z);
                }
                return new Vector3(x, y, z);
            }
            set
            {
                if (AL.IsSource(sourceIndex))
                {
                    AL.Source(sourceIndex, ALSource3f.Velocity, value.X, value.Y, value.Z);
                }
            }
        }
    }

    /// <summary>
    /// Main stream track class is used to create multi-channel soundtrack player,
    /// which differs from classic TR scheme, where each new soundtrack interrupted
    /// previous one. With flexible class handling, we now can implement multitrack
    /// player with automatic channel and crossfade management.
    /// </summary>
    public class StreamTrack
    {
        public StreamTrack()
        {
            buffers = new uint[Constants.TR_AUDIO_STREAM_NUMBUFFERS];
            AL.GenBuffers(buffers.Length, out buffers[0]); // Generate all buffers at once.
            

            AL.GenSource(out source);

            format = 0;
            rate = 0;
            IsDampable = false;

            wadFile = null;
            sndFile = null;

            if(AL.IsSource(source))
            {
                AL.Source(source, ALSource3f.Position, 0.0f, 0.0f, -1.0f); // OpenAL tut says this.
                AL.Source(source, ALSource3f.Velocity, 0.0f, 0.0f, 0.0f);
                AL.Source(source, ALSource3f.Direction, 0.0f, 0.0f, 0.0f);
                AL.Source(source, ALSourcef.RolloffFactor, 0.0f);
                AL.Source(source, ALSourceb.SourceRelative, true);
                AL.Source(source, ALSourceb.Looping, false); // No effect, but just in case...

                currentTrack = -1;
                currentVolume = 0.0f;
                dampedVolumes = 0.0f;
                IsActive = false;
                ending = false;
                streamType = TR_AUDIO_STREAM_TYPE.OneShot;

                // Setting method to -1 at init is required to prevent accidental
                // ov_clear call, which results in crash, if no vorbis file was
                // associated with given vorbis file structure.

                method = TR_AUDIO_STREAM_METHOD.Unknown;
            }
        }

        ~StreamTrack()
        {
            Stop(); // In case we haven't stopped yet.

            AL.DeleteSource(ref source);
            AL.DeleteBuffers(buffers);
        }

        /// <summary>
        /// Load routine prepares track for playing. Arguments are track index,
        /// stream type (background, one-shot or chat) and load method, which
        /// differs for TR1-2, TR3 and TR4-5.
        /// </summary>
        public bool Load(string path, int index, TR_AUDIO_STREAM_TYPE type, TR_AUDIO_STREAM_METHOD loadMethod)
        {
            if (path == null || loadMethod >= TR_AUDIO_STREAM_METHOD.LastIndex || type >= TR_AUDIO_STREAM_TYPE.LastIndex)
                return false; // Do not load, if path, type or method are incorrect.

            currentTrack = index;
            streamType = type;
            method = loadMethod;
            IsDampable = streamType == TR_AUDIO_STREAM_TYPE.Background; // Damp only looped (BGM) tracks.

            // Select corresponding stream loading method.
            return 
                method == TR_AUDIO_STREAM_METHOD.Track 
                ? loadTrack(path) 
                : loadWad((byte) index, path);
        }

        public bool Unload()
        {
            var res = false;

            if (AL.IsSource(source)) // Stop and unlink all associated buffers.
            {
                int queued;
                AL.GetSource(source, ALGetSourcei.BuffersQueued, out queued);

                while(queued-- > 0)
                {
                    AL.SourceUnqueueBuffer((int)source);
                }
            }

            if(sndFile != null)
            {
                sndFile.Dispose();
                sndFile = null;
                sndFile_FileStream.Dispose();
                res = true;
            }

            if(wadFile != null)
            {
                wadFile.Dispose();
                wadFile = null;
                res = true;
            }

            return res;
        }

        /// <summary>
        /// Begin to play track
        /// </summary>
        public bool Play(bool fadeIn = false)
        {
            uint buffersToPlay;

            // At start-up, we fill all available buffers.
            // TR soundtracks contain a lot of short tracks, like Lara speech etc., and
            // there is high chance that such short tracks won't fill all defined buffers.
            // For this reason, we count amount of filled buffers, and immediately stop
            // allocating them as long as Stream() routine returns false. Later, we use
            // this number for queuing buffers to source.

            for(buffersToPlay = 0; buffersToPlay < Constants.TR_AUDIO_STREAM_NUMBUFFERS; buffersToPlay++)
            {
                if (!stream(buffersToPlay))
                {
                    if (buffersToPlay != 1)
                    {
                        // TODO: Error preparing buffers
                        return false;
                    }
                    else break;
                }
            }

            currentVolume = fadeIn ? 0.0f : 1.0f;

            if(audio_settings.UseEffects)
            {
                if(streamType == TR_AUDIO_STREAM_TYPE.Chat)
                {
                    SetFX();
                }
                else
                {
                    UnsetFX();
                }
            }

            AL.Source(source, ALSourcef.Gain, currentVolume * audio_settings.MusicVolume);
            AL.SourceQueueBuffers(source, (int)buffersToPlay, buffers);
            AL.SourcePlay(source);

            ending = false;
            IsActive = true;
            return true;
        }

        /// <summary>
        /// Pause track, preserving position
        /// </summary>
        public void Pause()
        {
            if(AL.IsSource(source))
            {
                AL.SourcePause(source);
            }
        }

        /// <summary>
        /// End track with fade-out
        /// </summary>
        public void End()
        {
            ending = true;
        }

        /// <summary>
        /// Immediately stop track
        /// </summary>
        public void Stop()
        {
            if(AL.IsSource(source)) // Stop and unlink all associated buffers.
            {
                if(IsPlaying) AL.SourceStop(source);
            }
        }

        /// <summary>
        /// Update track and manage streaming
        /// </summary>
        public bool Update()
        {
            var processed = 0;
            var buffered = true;
            var changeGain = false;

            if (!IsActive) return true; // Nothing to do here

            if(!IsPlaying)
            {
                Unload();
                IsActive = false;
                return true;
            }

            // Update damping, if track supports it.

            if(IsDampable)
            {
                // We check if damp condition is active, and if so, is it already at low-level or not.

                if(DampActive && dampedVolume < Constants.TR_AUDIO_STREAM_DAMP_LEVEL)
                {
                    dampedVolume += Constants.TR_AUDIO_STREAM_DAMP_SPEED;

                    // Clamp volume
                    dampedVolume = dampedVolume.Clamp(0.0f, Constants.TR_AUDIO_STREAM_DAMP_LEVEL);
                    changeGain = true;
                }
                else if(!DampActive && dampedVolume > 0) // If damp is not active, but it's still at low, restore it.
                {
                    dampedVolume -= Constants.TR_AUDIO_STREAM_DAMP_SPEED;

                    // Clamp volume
                    dampedVolume = dampedVolume.Clamp(0.0f, Constants.TR_AUDIO_STREAM_DAMP_LEVEL);
                    changeGain = true;
                }
            }

            if(ending) // If track is ending, crossfade it.
            {
                switch(streamType)
                {
                    case TR_AUDIO_STREAM_TYPE.Background:
                        currentVolume -= Constants.TR_AUDIO_STREAM_CROSSFADE_BACKGROUND;
                        break;
                    case TR_AUDIO_STREAM_TYPE.OneShot:
                        currentVolume -= Constants.TR_AUDIO_STREAM_CROSSFADE_ONESHOT;
                        break;
                    case TR_AUDIO_STREAM_TYPE.Chat:
                        currentVolume -= Constants.TR_AUDIO_STREAM_CROSSFADE_CHAT;
                        break;
                }

                // Crossfade has ended, we can now kill the stream.
                if (currentVolume <= 0.0)
                {
                    Stop();
                    return true; // Stop track, although return success, as everything is normal.
                }
                else
                {
                    changeGain = true;
                }
            }
            else
            {
                // If track is not ending and playing, restore it from crossfade.
                if (currentVolume < 1.0)
                {
                    switch (streamType)
                    {
                        case TR_AUDIO_STREAM_TYPE.Background:
                            currentVolume += Constants.TR_AUDIO_STREAM_CROSSFADE_BACKGROUND;
                            break;
                        case TR_AUDIO_STREAM_TYPE.OneShot:
                            currentVolume += Constants.TR_AUDIO_STREAM_CROSSFADE_ONESHOT;
                            break;
                        case TR_AUDIO_STREAM_TYPE.Chat:
                            currentVolume += Constants.TR_AUDIO_STREAM_CROSSFADE_CHAT;
                            break;
                    }

                    // Clamp volume.
                    currentVolume = currentVolume.Clamp(0.0f, 1.0f);
                    changeGain = true;
                }
            }

            if(changeGain) // If any condition which modify track gain was met, call AL gain change.
            {
                AL.Source(source, ALSourcef.Gain, currentVolume * (1.0f - dampedVolume) * audio_settings.MusicVolume);
            }

            // Check if any track buffers were already processed.
            AL.GetSource(source, ALGetSourcei.BuffersProcessed, out processed);

            while (processed-- > 0) // Managed processed buffers.
            {
                var buffer = AL.SourceUnqueueBuffer((int)source); // Unlink processed buffer.
                buffered = stream((uint) buffer); // Refill processed buffer.
                if (buffered)
                    AL.SourceQueueBuffer((int)source, buffer); // Relink processed buffer.
            }

            return buffered;
        }

        /// <summary>
        /// Check desired track's index
        /// </summary>
        public bool IsTrack(int trackIndex)
        {
            return currentTrack == trackIndex;
        }

        /// <summary>
        /// Check desired track's type
        /// </summary>
        public bool IsType(TR_AUDIO_STREAM_TYPE trackType)
        {
            return trackType == streamType;
        }

        /// <summary>
        /// Check if track is playing
        /// </summary>
        public bool IsPlaying
        {
            get
            {
                if (AL.IsSource(source))
                {
                    var state = (int)ALSourceState.Stopped;
                    AL.GetSource(source, ALGetSourcei.SourceState, out state);
                    var state2 = (ALSourceState)state;
                    // Paused state and existing file pointers also counts as playing.
                    return state2 == ALSourceState.Playing || state2 == ALSourceState.Paused;
                }
                return false;
            }
        }

        /// <summary>
        /// If track is active or not
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Specifies if track can be damped by others
        /// </summary>
        public bool IsDampable { get; private set; }

        /// <summary>
        /// Set reverb FX, according to room flag
        /// </summary>
        public void SetFX()
        {
            uint effect, slot;

            // Reverb FX is applied globally through audio send. Since player can
            // jump between adjacent rooms with different reverb info, we assign
            // several (2 by default) interchangeable audio sends, which are switched
            // every time current room reverb is changed.

            if (Audio.FXManager.CurrentRoomType != Audio.FXManager.LastRoomType) // Switch audio send
            {
                Audio.FXManager.LastRoomType = Audio.FXManager.CurrentRoomType;
                Audio.FXManager.CurrentSlot =
                    ++Audio.FXManager.CurrentSlot > Constants.TR_AUDIO_MAX_SLOTS - 1
                    ? 0
                    : Audio.FXManager.CurrentSlot;

                effect = Audio.FXManager.ALEffect[Audio.FXManager.CurrentRoomType];
                slot = Audio.FXManager.ALSlot[Audio.FXManager.CurrentSlot];

                if (Audio.EffectsExtension.IsAuxiliaryEffectSlot(slot) && Audio.EffectsExtension.IsEffect(effect))
                {
                    Audio.EffectsExtension.AuxiliaryEffectSlot(slot, EfxAuxiliaryi.EffectslotEffect, (int)effect);
                }
            }
            else // Do not switch audio send.
            {
                slot = Audio.FXManager.ALSlot[Audio.FXManager.CurrentSlot];
            }

            // Assign global reverb FX to channel.

            AL.Source(source, ALSource3i.EfxAuxiliarySendFilter, (int)slot, 0, Constants.AL_FILTER_NULL);
        }

        /// <summary>
        /// Remove any reverb FX from source
        /// </summary>
        public void UnsetFX()
        {
            // Remove any audio sends and direct filters from channel.

            AL.Source(source, ALSourcei.EfxDirectFilter, Constants.AL_FILTER_NULL);
            AL.Source(source, ALSource3i.EfxAuxiliarySendFilter, Constants.AL_EFFECTSLOT_NULL, 0, Constants.AL_FILTER_NULL);
        }

        /// <summary>
        /// Global flag for damping BGM tracks
        /// </summary>
        public static bool DampActive { get; set; } = false;

        /// <summary>
        /// Track loading
        /// </summary>
        private bool loadTrack(string path)
        {
            sndFile_FileStream = File.OpenRead(path);
            try
            {
                sndFile = new SndFile(sndFile_FileStream, new SndFileInfo());
            }
            catch
            {
                method = TR_AUDIO_STREAM_METHOD.Unknown;
                return false;
            }

            sfInfo = sndFile.GetSndFileInfo();

            /* TODO:
# ifdef AUDIO_OPENAL_FLOAT

            if (sf_info.channels == 1)
                format = AL_FORMAT_MONO_FLOAT32;
            else
                format = AL_FORMAT_STEREO_FLOAT32;
#else
            if (sf_info.channels == 1)
                format = AL_FORMAT_MONO16;
            else
                format = AL_FORMAT_STEREO16;
#endif

    */

            format = sfInfo.Channels == 1 ? ALFormat.MonoFloat32Ext : ALFormat.StereoFloat32Ext;

            rate = sfInfo.SamplesPerSecond;

            return true; // Success!
        }

        /// <summary>
        /// Wad loading
        /// </summary>
        private bool loadWad(byte index, string filename)
        {
            if (index >= Constants.TR_AUDIO_STREAM_WAD_COUNT)
            {
                // TODO: Warning: WAD out of bounds
                return false;
            }

            try
            {
                wadFile = File.OpenRead(filename);
            }
            catch
            {
                // TODO: handle exception
                return false;
            }

            using (var br = new BinaryReader(wadFile))
            {
                br.BaseStream.Position = index * Constants.TR_AUDIO_STREAM_WAD_NAMELENGTH;
                var trackName = br.ParseString(Constants.TR_AUDIO_STREAM_WAD_NAMELENGTH);
                var length = br.ReadUInt32();
                var offset = br.ReadUInt32();
                br.BaseStream.Position = offset;
            }

            try
            {
                sndFile = new SndFile(wadFile, new SndFileInfo());
            }
            catch
            {
                method = TR_AUDIO_STREAM_METHOD.Unknown;
                return false;
            }

            sfInfo = sndFile.GetSndFileInfo();


            /* TODO:
# ifdef AUDIO_OPENAL_FLOAT

if (sf_info.channels == 1)
    format = AL_FORMAT_MONO_FLOAT32;
else
    format = AL_FORMAT_STEREO_FLOAT32;
#else
if (sf_info.channels == 1)
    format = AL_FORMAT_MONO16;
else
    format = AL_FORMAT_STEREO16;
#endif

*/

            format = sfInfo.Channels == 1 ? ALFormat.MonoFloat32Ext : ALFormat.StereoFloat32Ext;

            rate = sfInfo.SamplesPerSecond;

            return true; // Success!
        }

        /// <summary>
        /// General stream routine
        /// </summary>
        private unsafe bool stream(uint buffer)
        {
            // TODO: Assert audio_settings.StreamBufferSize >= sf_info.channels - 1
            /*
#ifdef AUDIO_OPENAL_FLOAT
    std::vector<ALfloat> pcm(audio_settings.stream_buffer_size);
#else
    std::vector<ALshort> pcm(audio_settings.stream_buffer_size);
#endif
*/
            var pcm = new float[audio_settings.StreamBufferSize];
            var size = 0;
            while (size < pcm.Length - sfInfo.Channels + 1)
            {
                var samplesToRead = (audio_settings.StreamBufferSize - size) / sfInfo.Channels * sfInfo.Channels;
                /*
#ifdef AUDIO_OPENAL_FLOAT
        const sf_count_t samplesRead = sf_read_float(snd_file, pcm.data() + size, samplesToRead);
#else
        const sf_count_t samplesRead = sf_read_short(snd_file, pcm.data() + size, samplesToRead);
#endif
                */
                var samplesRead = 0;
                fixed (float* ptr = pcm)
                    samplesRead = sndFile.Read(ptr + size, samplesToRead);

                if(samplesRead > 0)
                {
                    size += samplesRead;
                }
                else
                {
                    // TODO: Handle error (Audio.cpp L950)
                }
            }

            if (size == 0)
                return false;

            AL.BufferData((int)buffer, format, pcm, size * sizeof(float), rate);
            return true;
        }

        /// <summary>
        /// General handle for opened wad file
        /// </summary>
        private FileStream wadFile;

        /// <summary>
        /// Sndfile file reader needs its own handle
        /// </summary>
        private SndFile sndFile;

        private FileStream sndFile_FileStream;

        private SndFileInfo sfInfo;

        #region General OpenAL fields

        private uint source;
        private uint[] buffers;
        private ALFormat format;
        private int rate;

        /// <summary>
        /// Stream volume, considering fades
        /// </summary>
        private float currentVolume;

        /// <summary>
        /// Additional damp volume multiplier
        /// </summary>
        private float dampedVolume;

        #endregion

        /// <summary>
        /// Used when track is being faded by other one
        /// </summary>
        private bool ending;

        /// <summary>
        /// Either BACKGROUND, ONESHOT or CHAT
        /// </summary>
        private TR_AUDIO_STREAM_TYPE streamType;

        /// <summary>
        /// Needed to prevent same track sending
        /// </summary>
        private int currentTrack;

        /// <summary>
        /// TRACK (TR1-2/4-5) or WAD (TR3)
        /// </summary>
        private TR_AUDIO_STREAM_METHOD method;
    }

    public class Audio
    {
        #region General audio routines

        public static void InitGlobals()
        {
            audio_settings.MusicVolume = 0.7f;
            audio_settings.SoundVolume = 0.8f;
            audio_settings.UseEffects = true;
            audio_settings.ListenerIsPlayer = false;
            audio_settings.StreamBufferSize = 32;
        }

        public static void InitFX()
        {
            if (audio_settings.EffectsInitialized)
                return;

            FXManager = new AudioFxManager();

            // Set up effect slots, effects and filters.

            FXManager.ALSlot = new uint[Constants.TR_AUDIO_MAX_SLOTS];
            EffectsExtension.GenAuxiliaryEffectSlots(Constants.TR_AUDIO_MAX_SLOTS, out FXManager.ALSlot[0]);

            FXManager.ALEffect = new uint[(int) TR_AUDIO_FX.LastIndex];
            EffectsExtension.GenEffects((int) TR_AUDIO_FX.LastIndex, out FXManager.ALEffect[0]);

            uint alFilter = 0;
            EffectsExtension.GenFilter(out alFilter);
            FXManager.ALFilter = alFilter;

            EffectsExtension.Filter(alFilter, EfxFilteri.FilterType, (int)EfxFilterType.Lowpass);
            EffectsExtension.Filter(alFilter, EfxFilterf.LowpassGain, 0.7f); // Low frequencies gain.
            EffectsExtension.Filter(alFilter, EfxFilterf.LowpassGainHF, 0.0f); // High frequencies gain.

            // Fill up effects with reverb presets

            LoadReverbToFX(TR_AUDIO_FX.Outside, EffectsExtension.ReverbPresets.City.ToEfxEaxReverb());
            LoadReverbToFX(TR_AUDIO_FX.SmallRoom, EffectsExtension.ReverbPresets.Livingroom.ToEfxEaxReverb());
            LoadReverbToFX(TR_AUDIO_FX.MediumRoom, EffectsExtension.ReverbPresets.WoodenLongpassage.ToEfxEaxReverb());
            LoadReverbToFX(TR_AUDIO_FX.LargeRoom, EffectsExtension.ReverbPresets.DomeTomb.ToEfxEaxReverb());
            LoadReverbToFX(TR_AUDIO_FX.Pipe, EffectsExtension.ReverbPresets.PipeLarge.ToEfxEaxReverb());
            LoadReverbToFX(TR_AUDIO_FX.Water, EffectsExtension.ReverbPresets.Underwater.ToEfxEaxReverb());

            audio_settings.EffectsInitialized = true;
        }

        public static void Init(uint numSources = Constants.TR_AUDIO_MAX_CHANNELS)
        {
            // FX should be inited first, as source constructor checks for FX slot to be created.

            if(audio_settings.UseEffects) InitFX();

            // Generate new source array.

            numSources -= Constants.TR_AUDIO_STREAM_NUMSOURCES; // Subtract sources reserved for music.
            engine_world.AudioSources.Resize((int)numSources);

            // Generate stream tracks array.

            engine_world.StreamTracks.Resize(Constants.TR_AUDIO_STREAM_NUMSOURCES);

            // Reset last room type used for assigning reverb.

            FXManager.LastRoomType = (uint)TR_AUDIO_FX.LastIndex;
        }

        public static bool DeInit()
        {
            StopAllSources();
            StopStreams();

            DeInitDelay();

            var engine_world = new World();
            engine_world.AudioSources.Clear();
            engine_world.StreamTracks.Clear();
            engine_world.StreamTrackMap.Clear();

            // CRITICAL: You must delete all sources before deleting buffers!

            AL.DeleteBuffers(engine_world.AudioBuffers.ToArray());
            engine_world.AudioBuffers.Clear();

            engine_world.AudioEffects.Clear();
            engine_world.AudioMap.Clear();

            if(audio_settings.EffectsInitialized)
            {
                for (var i = 0; i < Constants.TR_AUDIO_MAX_SLOTS; i++)
                {
                    var cur = FXManager.ALSlot[i];
                    if (cur != 0)
                    {
                        EffectsExtension.AuxiliaryEffectSlot(cur, EfxAuxiliaryi.EffectslotEffect, Constants.AL_EFFECT_NULL);
                        EffectsExtension.DeleteAuxiliaryEffectSlot(ref FXManager.ALSlot[i]);
                    }
                }

                EffectsExtension.DeleteFilter(ref FXManager.ALFilter);
                EffectsExtension.DeleteEffects(FXManager.ALEffect);
                audio_settings.EffectsInitialized = false;
            }

            return true;
        }

        public static void Update()
        {
            UpdateSources();
            UpdateStreams();

            if(audio_settings.ListenerIsPlayer)
            {
                UpdateListenerByEntity(engine_world.Character);
            }
            else
            {
                UpdateListenerByCamera(renderer.camera());
            }
        }

        #endregion

        #region Audio source (samples) routines

        public static int GetFreeSource()
        {
            for (var i = 0; i < engine_world.AudioSources.Count; i++)
            {
                if (!engine_world.AudioSources[i].IsActive)
                    return i;
            }

            return -1;
        }

        public static bool IsInRange(TR_AUDIO_EMITTER emitterType, int entityID, float range, float gain)
        {
            var vec = Vector3.Zero;

            switch (emitterType)
            {
                case TR_AUDIO_EMITTER.Entity:
                    var ent = engine_world.GetEntityByID((uint)entityID);
                    if (ent == null) return false;
                    vec = ent.Transform.GetOrigin();
                    break;
                case TR_AUDIO_EMITTER.SoundSource:
                    if ((uint) entityID + 1 > engine_world.AudioEmitters.Count) return false;
                    vec = engine_world.AudioEmitters[entityID].Position;
                    break;
                case TR_AUDIO_EMITTER.Global:
                    return true;
                default:
                    return false;
            }

            // We add 1/4 of overall distance to fix up some issues with
            // pseudo-looped sounds that are called at certain frames in animations.

            var dist = (ListenerPosition - vec).LengthSquared / (gain + 1.25f);

            return dist < range * range;
        }

        public static int IsEffectPlaying(int effectID = -1, TR_AUDIO_EMITTER emitterType = TR_AUDIO_EMITTER.Unknown, int emitterID = -1)
        {
            for (var i = 0; i < engine_world.AudioSources.Count; i++)
            {
                var c = engine_world.AudioSources[i];
                if((effectID == -1 || effectID == c.EffectIndex) &&
                    (emitterType == TR_AUDIO_EMITTER.Unknown || emitterType == c.EmitterType) &&
                    (emitterID == -1 || emitterID == c.EmitterID))
                {
                    if (c.IsPlaying) return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Send to play effect with given parameters.
        /// </summary>
        public static TR_AUDIO_SEND Send(uint effectID, TR_AUDIO_EMITTER emitterType = TR_AUDIO_EMITTER.Global,
            int emitterID = 0)
        {
            var sourceNumber = 0;
            AudioEffect effect;

            // If there are no audio buffers or effect index is wrong, don't process.

            if (engine_world.AudioBuffers.Count == 0 || effectID < 0) return TR_AUDIO_SEND.Ignored;

            // Remap global engine effect ID to local effect ID.

            if (effectID >= engine_world.AudioMap.Count)
            {
                return TR_AUDIO_SEND.NoSample; // Sound is out of bounds; stop.
            }

            var realID = (int) engine_world.AudioMap[(int) effectID];

            // Pre-step 1: if there is no effect associated with this ID, bypass audio send.

            if (realID == -1)
            {
                return TR_AUDIO_SEND.Ignored;
            }
            else
            {
                effect = engine_world.AudioEffects[realID];
            }

            // Pre-step 2: check if sound non-looped and chance to play isn't zero,
            // then randomly select if it should be played or not.

            if (effect.Loop != LoopType.Forward && effect.Chance > 0)
            {
                if (effect.Chance < Helper.CPPRand() % 0x7FFF)
                {
                    // Bypass audio send, if chance test is not passed.
                    return TR_AUDIO_SEND.Ignored;
                }
            }

            // Pre-step 3: Calculate if effect's hearing sphere intersect listener's hearing sphere.
            // If it's not, bypass audio send (cause we don't want it to occupy channel, if it's not
            // heard).

            if (!IsInRange(emitterType, emitterID, effect.Range, effect.Gain))
            {
                return TR_AUDIO_SEND.Ignored;
            }

            // Pre-step 4: check if R (Rewind) flag is set for this effect, if so,
            // find any effect with similar ID playing for this entity, and stop it.
            // Otherwise, if W (Wait) or L (Looped) flag is set, and same effect is
            // playing for current entity, don't send it and exit function.

            sourceNumber = IsEffectPlaying((int) effectID, emitterType, emitterID);

            if (sourceNumber != -1)
            {
                if (effect.Loop == LoopType.PingPong)
                {
                    engine_world.AudioSources[sourceNumber].Stop();
                }
                else if (effect.Loop != LoopType.None) // Any other looping case (Wait / Loop).
                {
                    return TR_AUDIO_SEND.Ignored;
                }
            }
            else
            {
                sourceNumber = GetFreeSource(); // Get free source
            }

            if (sourceNumber != -1) // Everything is OK, we're sending audio to channel.
            {
                var bufferIndex = 0;

                // Step 1. Assign buffer to source.

                if (effect.SampleCount > 1)
                {
                    // Select random buffer, if effect info contains more than 1 assigned samples.
                    bufferIndex = unchecked((int) (Helper.CPPRand() % effect.SampleCount + effect.SampleIndex));
                }
                else
                {
                    // Just assign buffer to source, if there is only one assigned sample.
                    bufferIndex = (int) effect.SampleIndex;
                }

                var source = engine_world.AudioSources[sourceNumber];

                source.SetBuffer(bufferIndex);

                // Step 2. Check looped flag, and if so, set source type to looped.

                source.IsLooping = effect.Loop == LoopType.Forward;

                // Step 3. Apply internal sound parameters.

                source.EmitterID = emitterID;
                source.EmitterType = emitterType;
                source.EffectIndex = effectID;

                // Step 4. Apply sound effect properties.

                source.Pitch = effect.Pitch;
                if (effect.RandomizePitch) // Vary pitch, if flag is set.
                {
                    source.Pitch += (Helper.CPPRand() % effect.RandomizePitchVar - 25.0f) / 200.0f;
                }

                source.Gain = effect.Gain;
                if (effect.RandomizeGain) // Vary gain, if flag is set.
                {
                    source.Gain += (Helper.CPPRand() % effect.RandomizeGainVar - 25.0f) / 200.0f;
                }

                source.SetRange(effect.Range); // Set audible range

                source.Play(); // Everything is OK, play sound now!

                return TR_AUDIO_SEND.Processed;
            }

            return TR_AUDIO_SEND.NoChannel;
        }

        /// <summary>
        /// If exist, immediately stop and destroy all effects with given parameters.
        /// </summary>
        public static TR_AUDIO_SEND Kill(int effectID, TR_AUDIO_EMITTER emitterType = TR_AUDIO_EMITTER.Global, int emitterID = 0)
        {
            var playingSound = IsEffectPlaying(effectID, emitterType, emitterID);

            if(playingSound != -1)
            {
                engine_world.AudioSources[playingSound].Stop();
                return TR_AUDIO_SEND.Processed;
            }

            return TR_AUDIO_SEND.Ignored;
        }

        /// <summary>
        /// Used to pause all effects currently playing.
        /// </summary>
        public static void PauseAllSources()
        {
            foreach (var audioSource in engine_world.AudioSources.Where(audioSource => audioSource.IsActive))
            {
                audioSource.Pause();
            }
        }

        /// <summary>
        /// Used in audio deinit.
        /// </summary>
        public static void StopAllSources()
        {
            foreach (var audioSource in engine_world.AudioSources)
            {
                audioSource.Stop();
            }
        }

        /// <summary>
        /// Used to resume all effects currently paused.
        /// </summary>
        public static void ResumeAllSources()
        {
            foreach (var audioSource in engine_world.AudioSources.Where(audioSource => audioSource.IsActive))
            {
                audioSource.Play();
            }
        }

        /// <summary>
        /// Main sound loop.
        /// </summary>
        public static void UpdateSources()
        {
            if (engine_world.AudioSources.Count == 0) return;

            var listenerPos = ListenerPosition;
            AL.GetListener(ALListener3f.Position, out listenerPos);
            ListenerPosition = listenerPos;

            for (int i = 0; i < engine_world.AudioEmitters.Count; i++)
            {
                Send(engine_world.AudioEmitters[i].SoundIndex, TR_AUDIO_EMITTER.SoundSource, i);
            }
            
            engine_world.AudioSources.ForEach(x => x.Update());
        }

        /// <summary>
        /// Updates listener parameters by camera structure. For correct speed calculation
        /// that function have to be called every game frame.
        /// </summary>
        /// <param name="cam">Pointer to the camera structure.</param>
        public static void UpdateListenerByCamera(Camera cam)
        {
            AL.Listener(ALListenerfv.Orientation, ref cam.ViewDirection, ref cam.UpDirection);
            AL.Listener(ALListener3f.Position, ref cam.Position);

            AL.Listener(ALListener3f.Velocity, ref (cam.Position - cam.previousPosition) / (float)engine_frame_time);
            cam.previousPosition = cam.Position;

            if(cam.currentRoom != null)
            {
                if(cam.currentRoom.Flags.HasFlagEx(RoomFlags.FilledWithWater))
                {
                    FXManager.CurrentRoomType = (uint)TR_AUDIO_FX.Water;
                }
                else
                {
                    FXManager.CurrentRoomType = cam.currentRoom.ReverbInfo;
                }

                if(FXManager.WaterState != (cam.currentRoom.Flags.HasFlagEx(RoomFlags.FilledWithWater) ? 1 : 0))
                {
                    if((FXManager.WaterState = (sbyte)(cam.currentRoom.Flags.HasFlagEx(RoomFlags.FilledWithWater) ? 1 : 0)) != 0)
                    {
                        Send((uint)TR_AUDIO_SOUND.Underwater);
                    }
                    else
                    {
                        Kill((int) TR_AUDIO_SOUND.Underwater);
                    }
                }
            }
        }

        public static void UpdateListenerByEntity(Entity ent)
        {
            // TODO: Add entity listener updater here
        }

        public static bool FillALBuffer(uint bufNumber, SndFile wavFile, uint bufSize, SndFileInfo sfInfo)
        {
            if (sfInfo.Channels > 1) // We can't use non-mono samples
            {
                // TODO: Warn "Error: sample %bufNumber% is not mono!"
                return false;
            }

            // TODO: See line 1845

            var frames = new float[bufSize / sizeof (float)];
            wavFile.Read(frames, frames.Length);
            AL.BufferData((int)bufNumber, ALFormat.MonoFloat32Ext, frames, (int)bufSize, sfInfo.SamplesPerSecond);

            LogALError();
            return true;
        }

        public static unsafe int LoadALBufferFromMem(uint bufNumber, byte* samplePointer, uint sampleSize,
            uint uncompSampleSize = 0);

        public static int LoadALBufferFromFile(uint bufNumber, string filename)
        {
            try
            {
                using (var fs = File.OpenRead(filename))
                {
                    using (var file = new SndFile(fs, new SndFileInfo()))
                    {
                        var sfInfo = file.GetSndFileInfo();
                        // TODO: See line 1826 of audio.cpp
                        return FillALBuffer(bufNumber, file, (uint) file.FramesCount * sizeof (float), sfInfo) ? 0 : -3; // Zero means success
                    }
                }
            }
            catch
            {
                // TODO: Handle warning "Can't open file"
                return -1;
            }
        }

        public static void LoadOverridedSamples(World world);

        public static bool LoadReverbToFX(TR_AUDIO_FX effectID, EffectsExtension.EfxEaxReverb reverb)
        {
            var effect = FXManager.ALEffect[(int) effectID];

            if(EffectsExtension.IsEffect(effect))
            {
                EffectsExtension.Effect(effect, EfxEffecti.EffectType, (int)EfxEffectType.Reverb);

                EffectsExtension.Effect(effect, EfxEffectf.ReverbDensity, reverb.Density);
                EffectsExtension.Effect(effect, EfxEffectf.ReverbDiffusion, reverb.Diffusion);
                EffectsExtension.Effect(effect, EfxEffectf.ReverbGain, reverb.Gain);
                EffectsExtension.Effect(effect, EfxEffectf.ReverbGainHF, reverb.GainHF);
                EffectsExtension.Effect(effect, EfxEffectf.ReverbDecayTime, reverb.DecayTime);
                EffectsExtension.Effect(effect, EfxEffectf.ReverbDecayHFRatio, reverb.DecayHFRatio);
                EffectsExtension.Effect(effect, EfxEffectf.ReverbReflectionsGain, reverb.ReflectionsGain);
                EffectsExtension.Effect(effect, EfxEffectf.ReverbReflectionsDelay, reverb.ReflectionsDelay);
                EffectsExtension.Effect(effect, EfxEffectf.ReverbLateReverbGain, reverb.LateReverbGain);
                EffectsExtension.Effect(effect, EfxEffectf.ReverbLateReverbDelay, reverb.LateReverbDelay);
                EffectsExtension.Effect(effect, EfxEffectf.ReverbAirAbsorptionGainHF, reverb.AirAbsorptionGainHF);
                EffectsExtension.Effect(effect, EfxEffectf.ReverbRoomRolloffFactor, reverb.RoomRolloffFactor);
                EffectsExtension.Effect(effect, EfxEffecti.ReverbDecayHFLimit, reverb.DecayHFLimit);
            }
            else
            {
                // TODO: Handle error: "OpenAL error: no effect %effect%"
                return false;
            }

            return true;
        }

        #endregion

        #region Stream tracks (music / BGM) routines

        /// <summary>
        /// Get free (stopped) stream
        /// </summary>
        public static int GetFreeStream()
        {
            for (var i = 0; i < engine_world.StreamTracks.Count; i++)
            {
                var c = engine_world.StreamTracks[i];

                if (!c.IsPlaying && !c.IsActive) return i;
            }

            return (int)TR_AUDIO_STREAMPLAY.NoFreeStream; // If no free source, return error.
        }

        /// <summary>
        /// See if track is already playing
        /// </summary>
        public static bool IsTrackPlaying(int trackID = -1)
        {
            return engine_world.StreamTracks.Any(x => x.IsPlaying && (trackID == -1 || x.IsTrack(trackID)));
        }

        /// <summary>
        /// Check if track played with given activation mask
        /// </summary>
        public static bool TrackAlreadyPlayed(uint trackID, sbyte mask = 0)
        {
            if (mask == 0) return false; // No mask, play in any case.
            if (trackID >= engine_world.StreamTrackMap.Count) return true; // No such track, hence "already" played.

            mask &= 0x3F; // Clamp mask just in case.

            if(engine_world.StreamTrackMap[(int)trackID] == mask)
            {
                return true; // Immediately return true, if flags are directly equal.
            }
            else
            {
                var played = engine_world.StreamTrackMap[(int) trackID] & mask;

                if(played == mask)
                {
                    return true; // Bits were set, hence already played.
                }
                else
                {
                    engine_world.StreamTrackMap[(int) trackID] |= (byte)mask;
                    return false; // Not yet played, set bits and return false.
                }
            }
        }

        /// <summary>
        /// Update all streams
        /// </summary>
        public static void UpdateStreams()
        {
            UpdateStreamsDamping();

            engine_world.StreamTracks.ForEach(x => x.Update());
        }

        /// <summary>
        /// See if there is any damping tracks playing
        /// </summary>
        public static void UpdateStreamsDamping()
        {
            // Scan for any tracks that can provoke damp. Usually it's any tracks that are
            // NOT background. So we simply check this condition and set damp activity flag
            // if condition is met.

            StreamTrack.DampActive = engine_world.StreamTracks.Any(x => x.IsPlaying && !x.IsType(TR_AUDIO_STREAM_TYPE.Background));
        }

        /// <summary>
        /// Pause all streams [of specified type]
        /// </summary>
        public static void PauseStreams(TR_AUDIO_STREAM_TYPE streamType = TR_AUDIO_STREAM_TYPE.Unknown);

        /// <summary>
        /// Resume all streams [of specified type]
        /// </summary>
        public static void ResumeStreams(TR_AUDIO_STREAM_TYPE streamType = TR_AUDIO_STREAM_TYPE.Unknown);

        /// <summary>
        /// End all streams (with crossfade) [of specified type]
        /// </summary>
        public static bool EndStreams(TR_AUDIO_STREAM_TYPE streamType = TR_AUDIO_STREAM_TYPE.Unknown)
        {
            var res = false;

            foreach (
                var c in
                    engine_world.StreamTracks.Where(
                        c => c.IsPlaying && (c.IsType(streamType) || streamType == TR_AUDIO_STREAM_TYPE.Unknown)))
            {
                res = true;
                c.End();
            }

            return res;
        }

        /// <summary>
        /// Immediately stop all streams [of specified type]
        /// </summary>
        public static bool StopStreams(TR_AUDIO_STREAM_TYPE streamType = TR_AUDIO_STREAM_TYPE.Unknown)
        {
            var res = false;

            foreach (
                var c in
                    engine_world.StreamTracks.Where(
                        c => c.IsPlaying && (c.IsType(streamType) || streamType == TR_AUDIO_STREAM_TYPE.Unknown)))
            {
                res = true;
                c.Stop();
            }

            return res;
        }

        // Generally, you need only this function to trigger any track
        // TODO: Inconsistent type: trackID is uint here and in TrackAlreadyPlayed, is int in IsTrackPlaying
        public static TR_AUDIO_STREAMPLAY StreamPlay(uint trackID, byte mask = 0)
        {
            var targetStream = -1;
            var doFadeIn = false;
            var loadMethod = TR_AUDIO_STREAM_METHOD.Track;
            var streamType = TR_AUDIO_STREAM_TYPE.Background;

            var filePath = "";

            // Don't even try to do anything with track, if its index is greater than overall amount of
            // soundtracks specified in a stream track map count (which is derived from script).

            if(trackID >= engine_world.StreamTrackMap.Count)
            {
                // TODO: Warning TrackID out of bounds
                return TR_AUDIO_STREAMPLAY.WrongTrack;
            }

            // Don't play track, if it is already playing.
            // This should become useless option, once proper one-shot trigger functionality is implemented.

            if(IsTrackPlaying((int)trackID))
            {
                // TODO: Warning Track already playing
                return TR_AUDIO_STREAMPLAY.Ignored;
            }

            // lua_GetSoundtrack returns stream type, file path and load method in last three
            // provided arguments. That is, after calling this function we receive stream type
            // in "stream_type" argument, file path into "file_path" argument and load method into
            // "load_method" argument. Function itself returns false, if script wasn't found or
            // request was broken; in this case, we quit.

            if(!engine_lua.GetSoundtrack(trackID, filePath, loadMethod, streamType))
            {
                // TODO: Warning Track wrong index
                return TR_AUDIO_STREAMPLAY.WrongTrack;
            }

            // Don't try to play track, if it was already played by specified bit mask.
            // Additionally, TrackAlreadyPlayed function applies specified bit mask to track map.
            // Also, bit mask is valid only for non-looped tracks, since looped tracks are played
            // in any way.

            if(streamType != TR_AUDIO_STREAM_TYPE.Background && TrackAlreadyPlayed(trackID, (sbyte)mask))
            {
                return TR_AUDIO_STREAMPLAY.Ignored;
            }

            // Entry found, now process to actual track loading.

            targetStream = GetFreeStream(); // At first, we need to get free stream.

            if(targetStream == -1)
            {
                doFadeIn = StopStreams(streamType); // If no free track found, hardly stop all tracks.
                targetStream = GetFreeStream(); // Try again to assign free stream.

                if(targetStream == -1)
                {
                    // TODO: Warning No free stream
                    return TR_AUDIO_STREAMPLAY.NoFreeStream; // No success, exit and don't play anything.
                }
            }
            else
            {
                doFadeIn = EndStreams(streamType); // End all streams of this type with fadeout.

                // Additionally check if track type is looped. If it is, force fade in in any case.
                // This is needed to smooth out possible pop with gapless looped track at a start-up.

                doFadeIn = streamType == TR_AUDIO_STREAM_TYPE.Background;
            }

            // Finally - load our track.

            if(!engine_world.StreamTracks[targetStream].Load(filePath, (int)trackID, streamType, loadMethod))
            {
                // TODO: Warning Stream load error
                return TR_AUDIO_STREAMPLAY.LoadError;
            }

            // Try to play newly assigned and loaded track.

            if (!engine_world.StreamTracks[targetStream].Play(doFadeIn))
            {
                // TODO: Warning Stream play error
                return TR_AUDIO_STREAMPLAY.PlayError;
            }

            return TR_AUDIO_STREAMPLAY.Processed; // Everything is OK!
        }

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

        public static float GetByteDepth(SndFileInfo sfInfo)
        {
            switch ((SndFileSubFormat) (sfInfo.Format & Constants.SF_FORMAT_SUBMASK))
            {
                case SndFileSubFormat.PCM_S8:
                case SndFileSubFormat.PCM_U8:
                    return 1;
                case SndFileSubFormat.PCM_16:
                    return 2;
                case SndFileSubFormat.PCM_24:
                    return 3;
                case SndFileSubFormat.PCM_32:
                case SndFileSubFormat.FLOAT:
                    return 4;
                case SndFileSubFormat.DOUBLE:
                    return 8;
                case SndFileSubFormat.MS_ADPCM:
                    return 0.5f;
                default:
                    return 1;
            }
        }

        public static void LoadALExtFunctions(IntPtr device)
        {
            // TODO #ifndef in audio.cpp
        }

        public static bool DeInitDelay()
        {
            var sw = new Stopwatch();
            sw.Start();

            while(IsTrackPlaying() && IsEffectPlaying() >= 0)
            {
                if(sw.Elapsed.Seconds > Constants.TR_AUDIO_DEINIT_DELAY)
                {
                    // TODO: Handle "Audio deinit timeout reached! Something is wrong with the audio driver!"
                    break;
                }
            }

            sw.Stop();

            return true;
        }

        #endregion

        public static Vector3 ListenerPosition { get; set; }

        public static AudioFxManager FXManager;

        public static EffectsExtension EffectsExtension { get; set; } = new EffectsExtension();
    }

    public enum SF_FORMAT
    {
        WAV = 1,
        PCM_16 = 2,
        PCM_U8 = 4,
        Float = 6,
        PCM_32 = 8,
        PCM_24 = 10,
        Submask = 14
    }
}
