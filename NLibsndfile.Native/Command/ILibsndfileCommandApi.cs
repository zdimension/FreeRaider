using System;

namespace NLibsndfile.Native
{
    /// <summary>
    /// Interface to public Libsndfile Command API.
    /// </summary>
    public interface ILibsndfileCommandApi
    {
        /// <summary>
        /// Returns the version of the Libsndfile library.
        /// </summary>
        /// <returns>Libsndfile library version.</returns>
        string GetLibVersion();

        /// <summary>
        /// Returns the internal Libsndfile log generated when loading a file.
        /// </summary>
        /// <param name="sndfile">Audio file we want the log for.</param>
        /// <returns>Libsndfile log info.</returns>
        string GetLogInfo(IntPtr sndfile);

        /// <summary>
        /// Scan <paramref name="sndfile"/> file and return maximum calculated signal value. 
        /// </summary>
        /// <param name="sndfile">Audio file we want to scan.</param>
        /// <returns>Maximum signal value.</returns>
        double CalcSignalMax(IntPtr sndfile);

        /// <summary>
        /// Scan <paramref name="sndfile"/> file and return normalized maximum calculated signal value.
        /// </summary>
        /// <param name="sndfile">Audio file we want to scan.</param>
        /// <returns>Normalized maximum signal value.</returns>
        double CalcNormSignalMax(IntPtr sndfile);

        /// <summary>
        /// Scan <paramref name="sndfile"/> file and return single peak value for each channel.
        /// </summary>
        /// <param name="sndfile">Audio file we want to scan.</param>
        /// <param name="channels">Number of audio channels in the audio file.</param>
        /// <returns>Peak values for each channel.</returns>
        double[] CalcMaxAllChannels(IntPtr sndfile, int channels);

        /// <summary>
        /// Scan <paramref name="sndfile"/> file and return normalized peak value for each channel. 
        /// </summary>
        /// <param name="sndfile">Audio file we want to scan.</param>
        /// <param name="channels">Number of audio channels in the audio file.</param>
        /// <returns>Normalized peak values for each channel.</returns>
        double[] CalcNormMaxAllChannels(IntPtr sndfile, int channels);

        /// <summary>
        /// Retrieve the peak value for the file as stored in the file header.
        /// </summary>
        /// <param name="sndfile">Audio file we want to examine.</param>
        /// <returns>Peak value from file header.</returns>
        double GetSignalMax(IntPtr sndfile);

        /// <summary>
        /// Retrieve the peak value for each channel for the file as stored in the file header.
        /// </summary>
        /// <param name="sndfile">Audio file we want to examine.</param>
        /// <param name="channels">Number of audio channels in the audio file.</param>
        /// <returns>Peak values for each channel from file header.</returns>
        double[] GetMaxAllChannels(IntPtr sndfile, int channels);

        /// <summary>
        /// Sets float normalization for read and write functions on the <paramref name="sndfile"/> file.
        /// </summary>
        /// <param name="sndfile">Audio file to set float normalization on.</param>
        /// <param name="normalize">Flag to enable or disable float normalization.</param>
        /// <returns>Previous float normalization state.</returns>
        bool SetNormFloat(IntPtr sndfile, bool normalize);

        /// <summary>
        /// Sets double normalization for read and write functions on the <paramref name="sndfile"/> file.
        /// </summary>
        /// <param name="sndfile">Audio file to set double normalization on.</param>
        /// <param name="normalize">Flag to enable or disable double normalization.</param>
        /// <returns>Previous double normalization state.</returns>
        bool SetNormDouble(IntPtr sndfile, bool normalize);

        /// <summary>
        /// Gets float normalization for read and write functions on the <paramref name="sndfile"/> file.
        /// </summary>
        /// <param name="sndfile">Audio file to retrieve float normalization for.</param>
        /// <returns>Current float normalization state.</returns>
        bool GetNormFloat(IntPtr sndfile);

        /// <summary>
        /// Gets double normalization for read and write functions on the <paramref name="sndfile"/> file.
        /// </summary>
        /// <param name="sndfile">Audio file to retrieve double normalization for.</param>
        /// <returns>Current double normalization state.</returns>
        bool GetNormDouble(IntPtr sndfile);

        /// <summary>
        /// Sets the scale factor for when integer data is read from the <paramref name="sndfile"/>
        /// which contains floating point data.
        /// </summary>
        /// <param name="sndfile">Audio file to set int to float scaling.</param>
        /// <param name="enable">Flag to enable or disable int to float scaling.</param>
        /// <returns>Previous scaling state.</returns>
        bool SetScaleFloatIntRead(IntPtr sndfile, bool enable);

        /// <summary>
        /// Sets the scale factor for when integer data is written from the <paramref name="sndfile"/>
        /// as floating point data.
        /// </summary>
        /// <param name="sndfile">Audio file to set float to int scaling.</param>
        /// <param name="enable">Flag to enable or disable float to int scaling.</param>
        /// <returns>Previous scaling state.</returns>
        bool SetScaleIntFloatWrite(IntPtr sndfile, bool enable);

        /// <summary>
        /// Returns the number of simple formats supported by Libsndfile.
        /// </summary>
        /// <returns>Number of simple formats supported.</returns>
        int GetSimpleFormatCount();

        /// <summary>
        /// Retrieves information about the given <paramref name="format"/>.
        /// </summary>
        /// <param name="format">Format to retrieve information about.</param>
        /// <returns><see cref="LibsndfileFormatInfo"/> object containing format information.</returns>
        LibsndfileFormatInfo GetSimpleFormat(LibsndfileFormat format);

        /// <summary>
        /// Retrieve information about the given <paramref name="format"/> major/subtype format.
        /// </summary>
        /// <param name="format">Major or subtype format to retrieve information about.</param>
        /// <returns><see cref="LibsndfileFormatInfo"/> object containing format information.</returns>
        LibsndfileFormatInfo GetFormatInfo(LibsndfileFormat format);

        /// <summary>
        /// Returns the number of major formats supported by Libsndfile.
        /// </summary>
        /// <returns>Number of major formats supported.</returns>
        int GetFormatMajorCount();

        /// <summary>
        /// Retrieves information about the given <paramref name="format"/> major type.
        /// </summary>
        /// <param name="format">Major format to retrieve information about.</param>
        /// <returns><see cref="LibsndfileFormatInfo"/> object containing format information.</returns>
        LibsndfileFormatInfo GetFormatMajor(LibsndfileFormat format);

        /// <summary>
        /// Returns the number of available subformats.
        /// </summary>
        /// <returns>Number of subtype formats supported.</returns>
        int GetFormatSubtypeCount();

        /// <summary>
        /// Retrieves information about the given <paramref name="format"/> subtype.
        /// </summary>
        /// <param name="format">Subtype format to retrieve information about.</param>
        /// <returns><see cref="LibsndfileFormatInfo"/> object containing format information.</returns>
        LibsndfileFormatInfo GetFormatSubtype(LibsndfileFormat format);

        /// <summary>
        /// Sets PEAK chunk in <paramref name="sndfile"/> file which contains floating point data.
        /// </summary>
        /// <param name="sndfile">Audio file to set PEAK chunk.</param>
        /// <param name="enable">Flag to enable or disable PEAK chunk.</param>
        /// <returns>True/False on whether the PEAK chunk will be written on the next write call.</returns>
        /// <remarks>
        /// This call must be made before any data is written to the file.
        /// </remarks>
        bool SetAddPeakChunk(IntPtr sndfile, bool enable);

        /// <summary>
        /// Updates file header to reflect updated file information.
        /// </summary>
        /// <param name="sndfile">Audio file to update file header of.</param>
        void UpdateHeaderNow(IntPtr sndfile);

        /// <summary>
        /// Sets file header auto update for <paramref name="sndfile"/> after every subsequent write call.
        /// </summary>
        /// <param name="sndfile">Audio file to enable automatic file header updating.</param>
        /// <param name="enable">Flag to enable or file header auto-update.</param>
        /// <returns>Current file header auto-update state.</returns>
        bool SetUpdateHeaderAuto(IntPtr sndfile, bool enable);

        /// <summary>
        /// Truncate a file opened for write or read/write.
        /// </summary>
        /// <param name="sndfile">Audio file to truncate.</param>
        /// <param name="length">Number of frames remaining after truncation.</param>
        /// <returns>Success of file truncation.</returns>
        bool FileTruncate(IntPtr sndfile, long length);

        /// <summary>
        /// Change the data start offset for RAW files.
        /// </summary>
        /// <param name="sndfile">Audio file to change start offset for.</param>
        /// <param name="offset">Number of bytes offset from the beginning of the file.</param>
        void SetRawStartOffset(IntPtr sndfile, long offset);

        /// <summary>
        /// Set clipping for when performing floating point to int conversions.
        /// </summary>
        /// <param name="sndfile">Audio file to set clipping on.</param>
        /// <param name="enable">Flag to enable or disable clipping.</param>
        /// <returns>Current clipping state.</returns>
        bool SetClipping(IntPtr sndfile, bool enable);

        /// <summary>
        /// Retrieves floating point to int conversion clipping state for the given <paramref name="sndfile"/> file.
        /// </summary>
        /// <param name="sndfile">Audio file to get clipping state of.</param>
        /// <returns>Current clipping state.</returns>
        bool GetClipping(IntPtr sndfile);

        /// <summary>
        /// Get the file offset and file length of a file enbedded within another larger file.
        /// </summary>
        /// <param name="sndfile">Audio file to scan for embedded files.</param>
        /// <returns></returns>
        LibsndfileEmbedFileInfo GetEmbedFileInfo(IntPtr sndfile);

        /// <summary>
        /// Test if the <paramref name="sndfile"/> has the GUID of a WAVEX file
        /// for any of the ambisonic formats.
        /// </summary>
        /// <param name="sndfile">Audio file to examine.</param>
        /// <returns>Returns true or false based on whether the file is ambisonic format.</returns>
        bool GetAmbisonic(IntPtr sndfile);

        /// <summary>
        /// Set the GUID of a the <paramref name="sndfile"/> WAVEX file to indicate an Ambisonic format.
        /// </summary>
        /// <param name="sndfile">Audio file to set ambisonic format of.</param>
        /// <param name="mode">Ambisonic format to use.</param>
        /// <returns>Success of setting ambisonic format for the given file.</returns>
        bool SetAmbisonic(IntPtr sndfile, LibsndfileMode mode);

        /// <summary>
        /// Set the Variable Bit Rate encoding quality for the given <paramref name="sndfile"/> file.
        /// </summary>
        /// <param name="sndfile">Audio file to set vbr quality for.</param>
        /// <param name="value">Vbr encoding quality.</param>
        /// <remarks>
        /// The command must be sent before any audio data is written to the file.
        /// </remarks>
        void SetVbrEncodingQuality(IntPtr sndfile, double value);

        /// <summary>
        /// Determine if RAW data read from the given <paramref name="sndfile"/> file needs to be endian swapped.
        /// </summary>
        /// <param name="sndfile">Audio file to check for endian swapping.</param>
        /// <returns>True if bytes should be endian swapped.</returns>
        bool RawNeedsEndianSwap(IntPtr sndfile);

        /// <summary>
        /// Retrieve the Broadcast Extension chunk from the given <paramref name="sndfile"/> file.
        /// </summary>
        /// <param name="sndfile">Audio file to examine for broadcast info chunk.</param>
        /// <returns><see cref="LibsndfileBroadcastInfo"/> structure containing broadcast info.</returns>
        LibsndfileBroadcastInfo? GetBroadcastInfo(IntPtr sndfile);

        /// <summary>
        /// Set the Broadcast Extension chuck on the given <paramref name="sndfile"/> file.
        /// </summary>
        /// <param name="sndfile">Audio file to set broadcast info chunk for.</param>
        /// <param name="broadcastInfo">Broadcast info chunk.</param>
        /// <returns>True if broadcast info was successfully set.</returns>
        bool SetBroadcastInfo(IntPtr sndfile, LibsndfileBroadcastInfo broadcastInfo);

        /// <summary>
        /// Retrieves loop information for the given <paramref name="sndfile"/> file.
        /// </summary>
        /// <param name="sndfile">Audio file to examine for loop information.</param>
        /// <returns><see cref="LibsndfileLoopInfo"/> structure containing info about the given file.</returns>
        LibsndfileLoopInfo? GetLoopInfo(IntPtr sndfile);

        /// <summary>
        /// Retrieves instrument information for the given <paramref name="sndfile"/> file.
        /// </summary>
        /// <param name="sndfile">Audio file to example for instrument information.</param>
        /// <returns><see cref="LibsndfileInstrumentInfo"/> structure containing info about the given file.</returns>
        LibsndfileInstrumentInfo? GetInstrument(IntPtr sndfile);

        /// <summary>
        /// Sets the instrument info for the given <paramref name="sndfile"/> file.
        /// </summary>
        /// <param name="sndfile">Audio file to set instrument info on.</param>
        /// <param name="instrumentInfo">Instrument info to set.</param>
        /// <returns>True if instrument info was set, false otherwise.</returns>
        bool SetInstrument(IntPtr sndfile, LibsndfileInstrumentInfo instrumentInfo);
    }
}