using System;

namespace NLibsndfile.Native
{
    [Flags]
    public enum LibsndfileFormat : uint
    {
        /* Major formats. */
        Wav = 0x010000,		/* Microsoft WAV format (little endian default). */
        Aiff = 0x020000,		/* Apple/SGI AIFF format (big endian). */
        Au = 0x030000,		/* Sun/NeXT AU format (big endian). */
        Raw = 0x040000,		/* RAW PCM data. */
        Paf = 0x050000,		/* Ensoniq PARIS file format. */
        Svx = 0x060000,		/* Amiga IFF / SVX8 / SV16 format. */
        Nist = 0x070000,		/* Sphere NIST format. */
        Voc = 0x080000,		/* VOC files. */
        Ircam = 0x0A0000,		/* Berkeley/IRCAM/CARL */
        W64 = 0x0B0000,		/* Sonic Foundry's 64 bit RIFF/WAV */
        Mat4 = 0x0C0000,		/* Matlab (tm) V4.2 / GNU Octave 2.0 */
        Mat5 = 0x0D0000,		/* Matlab (tm) V5.0 / GNU Octave 2.1 */
        Pvf = 0x0E0000,		/* Portable Voice Format */
        Xi = 0x0F0000,		/* Fasttracker 2 Extended Instrument */
        Htk = 0x100000,		/* HMM Tool Kit format */
        Sds = 0x110000,		/* Midi Sample Dump Standard */
        Avr = 0x120000,		/* Audio Visual Research */
        Wavex = 0x130000,		/* MS WAVE with WAVEFORMATEX */
        Sd2 = 0x160000,		/* Sound Designer 2 */
        Flac = 0x170000,		/* FLAC lossless file format */
        Caf = 0x180000,		/* Core Audio File format */
        Wve = 0x190000,		/* Psion WVE format */
        Ogg = 0x200000,		/* Xiph OGG container */
        Mpc2K = 0x210000,		/* Akai MPC 2000 sampler */
        Rf64 = 0x220000,		/* RF64 WAV file */

        /* Subtypes from here on. */

        PcmS8 = 0x0001,		/* Signed 8 bit data */
        Pcm16 = 0x0002,		/* Signed 16 bit data */
        Pcm24 = 0x0003,		/* Signed 24 bit data */
        Pcm32 = 0x0004,		/* Signed 32 bit data */

        PcmU8 = 0x0005,		/* Unsigned 8 bit data (WAV and RAW only) */

        Float = 0x0006,		/* 32 bit float data */
        Double = 0x0007,		/* 64 bit float data */

        Ulaw = 0x0010,		/* U-Law encoded. */
        Alaw = 0x0011,		/* A-Law encoded. */
        ImaAdpcm = 0x0012,		/* IMA ADPCM. */
        MsAdpcm = 0x0013,		/* Microsoft ADPCM. */

        Gsm610 = 0x0020,		/* GSM 6.10 encoding. */
        VoxAdpcm = 0x0021,		/* OKI / Dialogix ADPCM */

        G72132 = 0x0030,		/* 32kbs G721 ADPCM encoding. */
        G72324 = 0x0031,		/* 24kbs G723 ADPCM encoding. */
        G72340 = 0x0032,		/* 40kbs G723 ADPCM encoding. */

        Dwvw12 = 0x0040, 		/* 12 bit Delta Width Variable Word encoding. */
        Dwvw16 = 0x0041, 		/* 16 bit Delta Width Variable Word encoding. */
        Dwvw24 = 0x0042, 		/* 24 bit Delta Width Variable Word encoding. */
        DwvwN = 0x0043, 		/* N bit Delta Width Variable Word encoding. */

        Dpcm8 = 0x0050,		/* 8 bit differential PCM (XI only) */
        Dpcm16 = 0x0051,		/* 16 bit differential PCM (XI only) */

        Vorbis = 0x0060,		/* Xiph Vorbis encoding. */

        Submask = 0x0000FFFF,
        Typemask = 0x0FFF0000,
        Endmask = 0x30000000
    }
}