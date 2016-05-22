using System;

namespace NLibsndfile.Native
{
    /// <summary>
    /// Static utility class for handling common Libsndfile Command methods.
    /// </summary>
    internal static class LibsndfileCommandUtilities
    {
        /// <summary>
        /// Returns true if the <paramref name="command"/> doesn't require a handle to a soundfile. 
        /// </summary>
        /// <param name="command">Command to check soundfile parameter requirements.</param>
        /// <returns>True/False based on whether this command type requires a open soundfile.</returns>
        internal static bool IsStaticCommand(LibsndfileCommand command)
        {
            switch (command)
            {
                case LibsndfileCommand.GetLibVersion:
                case LibsndfileCommand.GetSimpleFormatCount:
                case LibsndfileCommand.GetSimpleFormat:
                case LibsndfileCommand.GetFormatInfo:
                case LibsndfileCommand.GetFormatMajorCount:
                case LibsndfileCommand.GetFormatMajor:
                case LibsndfileCommand.GetFormatSubtypeCount:
                case LibsndfileCommand.GetFormatSubtype:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the <paramref name="result"/> value is valid for the <paramref name="command"/>.
        /// </summary>
        /// <param name="sndfile">Pointer to the soundfile that was used when executing the command.</param>
        /// <param name="command">LibsndfileCommand type used.</param>
        /// <param name="result">Result returned from sf_command.</param>
        /// <returns>True/False based on the success of the call for the given result value.</returns>
        internal static bool IsValidResult(IntPtr sndfile, LibsndfileCommand command, int result)
        {
            switch (command)
            {
                case LibsndfileCommand.GetLibVersion:
                    return result > 0;

                case LibsndfileCommand.GetLogInfo:
                    return sndfile != IntPtr.Zero ? result > 0 : result == 0;

                case LibsndfileCommand.CalcSignalMax:
                case LibsndfileCommand.CalcNormSignalMax:
                case LibsndfileCommand.CalcMaxAllChannels:
                case LibsndfileCommand.CalcNormMaxAllChannels:
                case LibsndfileCommand.GetSimpleFormatCount:
                case LibsndfileCommand.GetSimpleFormat:
                case LibsndfileCommand.GetFormatInfo:
                case LibsndfileCommand.GetFormatMajorCount:
                case LibsndfileCommand.GetFormatMajor:
                case LibsndfileCommand.GetFormatSubtypeCount:
                case LibsndfileCommand.GetFormatSubtype:
                case LibsndfileCommand.UpdateHeaderNow:
                case LibsndfileCommand.FileTruncate:
                case LibsndfileCommand.SetRawStartOffset:
                case LibsndfileCommand.GetEmbedFileInfo:
                case LibsndfileCommand.SetVbrEncodingQuality:
                    return result == 0;

                case LibsndfileCommand.GetSignalMax:
                case LibsndfileCommand.GetMaxAllChannels:
                case LibsndfileCommand.SetNormFloat:
                case LibsndfileCommand.SetNormDouble:
                case LibsndfileCommand.GetNormFloat:
                case LibsndfileCommand.GetNormDouble:
                case LibsndfileCommand.SetScaleFloatIntRead:
                case LibsndfileCommand.SetScaleIntFloatWrite:
                case LibsndfileCommand.SetAddPeakChunk:
                case LibsndfileCommand.SetUpdateHeaderAuto:
                case LibsndfileCommand.SetClipping:
                case LibsndfileCommand.GetClipping:
                case LibsndfileCommand.RawDataNeedsEndswap:
                case LibsndfileCommand.GetBroadcastInfo:
                case LibsndfileCommand.SetBroadcastInfo:
                case LibsndfileCommand.GetLoopInfo:
                case LibsndfileCommand.GetInstrument:
                case LibsndfileCommand.SetInstrument:
                    return result == 0 || result == 1;

                case LibsndfileCommand.WavexGetAmbisonic:
                case LibsndfileCommand.WavexSetAmbisonic:
                    return ((LibsndfileMode)result == LibsndfileMode.AmbisonicNone ||
                            (LibsndfileMode)result == LibsndfileMode.AmbisonicBFormat ||
                            (LibsndfileMode)result == LibsndfileMode.False);
            }
            return false;
        }
    }
}