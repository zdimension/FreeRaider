//
// Copyright (C) 2010-2011 Tarmin Rehve <tarmin.rehve@gmail.com>
//
//This file is part of the LibSndFile.NET software library.
//
//    LibSndFile.NET is free software: you can redistribute it and/or modify
//    it under the terms of the GNU Lesser Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    LibSndFile.NET is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Lesser License for more details.
//
//    You should have received a copy of the GNU Lesser Public License
//    along with LibSndFile.NET.  If not, see <http://www.gnu.org/licenses/>.


#pragma once


using namespace System;
using namespace System::Runtime::InteropServices;


namespace LibSndFile {


    [SerializableAttribute]
    [ComVisibleAttribute(true)]
    ///<summary>Major format types (in SF_FORMAT_...) as defined by the <a href="http://www.mega-nerd.com/libsndfile">libsndfile</a>.</summary>
    ///<remarks>A file type would consist of a major type (ie <see cref="SndFileMajorFormat"/>::WAV) bitwise
    /// ORed with a sub type (ie SndFileSubFormat::PCM_S8).</remarks>
    public enum class SndFileMajorFormat
    {
        ///<summary>Microsoft WAV format (little endian default)</summary>
        WAV = SF_FORMAT_WAV,
        ///<summary>Apple/SGI AIFF format (big endian)</summary>
        AIFF = SF_FORMAT_AIFF,
        ///<summary>Sun/NeXT AU format (big endian)</summary>
        AU = SF_FORMAT_AU,
        ///<summary>RAW PCM data</summary>
        RAW = SF_FORMAT_RAW,
        ///<summary>Ensoniq PARIS file format</summary>
        PAF = SF_FORMAT_PAF,
        ///<summary>Amiga IFF / SVX8 / SV16 format</summary>
        SVX = SF_FORMAT_SVX,
        ///<summary>Sphere NIST format</summary>
        NIST = SF_FORMAT_NIST,
        ///<summary>VOC files</summary>
        VOC = SF_FORMAT_VOC,
        ///<summary>Berkeley/IRCAM/CARL</summary>
        IRCAM = SF_FORMAT_IRCAM,
        ///<summary>Sonic Foundry's 64 bit RIFF/WAV</summary>
        W64 = SF_FORMAT_W64,
        ///<summary>Matlab (tm) V4.2 / GNU Octave 2.0</summary>
        MAT4 = SF_FORMAT_MAT4,
        ///<summary>Matlab (tm) V5.0 / GNU Octave 2.1</summary>
        MAT5 = SF_FORMAT_MAT5,
        ///<summary>Portable Voice Format</summary>
        PVF = SF_FORMAT_PVF,
        ///<summary>Fasttracker 2 Extended Instrument</summary>
        XI = SF_FORMAT_XI,
        ///<summary>HMM Tool Kit format</summary>
        HTK = SF_FORMAT_HTK,
        ///<summary>Midi Sample Dump Standard</summary>
        SDS = SF_FORMAT_SDS,
        ///<summary>Audio Visual Research</summary>
        AVR = SF_FORMAT_AVR,
        ///<summary>MS WAVE with WAVEFORMATEX</summary>
        WAVEX = SF_FORMAT_WAVEX,
        ///<summary>Sound Designer 2</summary>
        SD2 = SF_FORMAT_SD2,
        ///<summary>FLAC lossless file format</summary>
        FLAC = SF_FORMAT_FLAC,
        ///<summary>Core Audio File format</summary>
        CAF = SF_FORMAT_CAF,
        ///<summary>Psion WVE format</summary>
        WVE = SF_FORMAT_WVE,
        ///<summary>Xiph OGG container</summary>
        OGG = SF_FORMAT_OGG,
        ///<summary>Akai MPC 2000 sampler</summary>
        MPC2K = SF_FORMAT_MPC2K,
        ///<summary>RF64 WAV file</summary>
        RF64 = SF_FORMAT_RF64
    };


    [SerializableAttribute]
    [ComVisibleAttribute(true)]
    ///<summary>Format Sub-types (minor format types in SF_FORMAT_...) as defined by <a href="http://www.mega-nerd.com/libsndfile">libsndfile</a>.</summary>
    ///<remarks>A file type would consist of a major type (ie <see cref="SndFileMajorFormat"/>::WAV) bitwise
    /// ORed with a sub type (ie <see cref="SndFileSubFormat"/>::PCM_S8).
    ///</remarks>
    public enum class SndFileSubFormat
    {
        ///<summary>Signed 8 bit data</summary>
        PCM_S8 = SF_FORMAT_PCM_S8,
        ///<summary>Signed 16 bit data</summary>
        PCM_16 = SF_FORMAT_PCM_16,
        ///<summary>Signed 24 bit data</summary>
        PCM_24 = SF_FORMAT_PCM_24,
        ///<summary>Signed 32 bit data</summary>
        PCM_32 = SF_FORMAT_PCM_32,

        ///<summary>Unsigned 8 bit data (WAV and RAW only)</summary>
        PCM_U8 = SF_FORMAT_PCM_U8,

        ///<summary>32 bit float data</summary>
        FLOAT = SF_FORMAT_FLOAT,
        ///<summary>64 bit float data</summary>
        DOUBLE = SF_FORMAT_DOUBLE,

        ///<summary>U-Law encoded</summary>
        ULAW = SF_FORMAT_ULAW,
        ///<summary>A-Law encoded</summary>
        ALAW = SF_FORMAT_ALAW,
        ///<summary>IMA ADPCM</summary>
        IMA_ADPCM = SF_FORMAT_IMA_ADPCM,
        ///<summary>Microsoft ADPCM</summary>
        MS_ADPCM = SF_FORMAT_MS_ADPCM,

        ///<summary>GSM 6.10 encoding</summary>
        GSM610 = SF_FORMAT_GSM610,
        ///<summary>OKI / Dialogix ADPCM</summary>
        VOX_ADPCM = SF_FORMAT_VOX_ADPCM,

        ///<summary>32kbs G721 ADPCM encoding</summary>
        G721_32 = SF_FORMAT_G721_32,
        ///<summary>24kbs G723 ADPCM encoding</summary>
        G723_24 = SF_FORMAT_G723_24,
        ///<summary>40kbs G723 ADPCM encoding</summary>
        G723_40 = SF_FORMAT_G723_40,

        ///<summary>12 bit Delta Width Variable Word encoding</summary>
        DWVW_12 = SF_FORMAT_DWVW_12,
        ///<summary>16 bit Delta Width Variable Word encoding</summary>
        DWVW_16 = SF_FORMAT_DWVW_16,
        ///<summary>24 bit Delta Width Variable Word encoding</summary>
        DWVW_24 = SF_FORMAT_DWVW_24,
        ///<summary>N bit Delta Width Variable Word encoding</summary>
        DWVW_N = SF_FORMAT_DWVW_N,

        ///<summary>8 bit differential PCM (XI only)</summary>
        DPCM_8 = SF_FORMAT_DPCM_8,
        ///<summary>16 bit differential PCM (XI only)</summary>
        DPCM_16 = SF_FORMAT_DPCM_16,

        ///<summary>Xiph Vorbis encoding</summary>
        VORBIS = SF_FORMAT_VORBIS
    };
}