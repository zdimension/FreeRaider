using System;
using NLibsndfile.Native.Types;

namespace NLibsndfile.Native
{
    /// <summary>
    /// Interface to public Libsndfile API.
    /// </summary>
    public interface ILibsndfileApi
    {
        /// <summary>
        /// Interface to Libsndfile command methods.
        /// </summary>
        ILibsndfileCommandApi Commands { get; }

        /// <summary>
        /// Attempts to open an audio file at the <paramref name="path"/> location 
        /// with <paramref name="mode"/> based file access.
        /// </summary>
        /// <param name="path">Fully qualified path to location of audio file.</param>
        /// <param name="mode">File access to use when opening this file. ReadItems/Write/ReadWrite.</param>
        /// <param name="info"><see cref="LibsndfileInfo"/> structure contains information about the file we are opening.</param>
        /// <returns>Returns pointer to an internal object used by libsndfile that we can interact with.</returns>
        IntPtr Open(string path, LibsndfileMode mode, ref LibsndfileInfo info);

        /// <summary>
        /// Attempts to open an audio file with the <paramref name="handle"/> file descriptor 
        /// using <paramref name="mode"/> based file access.
        /// </summary>
        /// <param name="handle">File descriptor handle</param>
        /// <param name="mode">File access to use when opening this file. ReadItems/Write/ReadWrite</param>
        /// <param name="info"><see cref="LibsndfileInfo"/> structure contains information about the file we are opening.</param>
        /// <param name="closeHandle">Decide if we want libsndfile to close the file descriptor for us.</param>
        /// <returns>Returns pointer to an internal object used by libsndfile that we can interact with.</returns>
        IntPtr OpenFileDescriptor(int handle, LibsndfileMode mode, ref LibsndfileInfo info, int closeHandle);

        unsafe IntPtr OpenVirtual(ref SF_VIRTUAL_IO sfvirtual, LibsndfileMode mode, ref LibsndfileInfo info, void* userData);

        /// <summary>
        /// Check to see if the parameters in the <paramref name="info"/> struct are
        /// valid and supported by libsndfile.
        /// </summary>
        /// <param name="info"><see cref="LibsndfileInfo"/> struct contains information about a target file.</param>
        /// <returns>Returns TRUE if the parameters are valid, FALSE otherwise.</returns>
        bool FormatCheck(ref LibsndfileInfo info);

        /// <summary>
        /// Attempts to move the read/write data pointers to a specific location
        /// specified by the <paramref name="whence"/> and <paramref name="count"/> values
        /// in the <paramref name="sndfile"/> audio file.
        /// 
        /// Whence values can be the following:
        ///     0 - SEEK_SET  - The offset is set to the start of the audio data plus offset (multichannel) frames.
        ///     1 - SEEK_CUR  - The offset is set to its current location plus offset (multichannel) frames.
        ///     2 - SEEK_END  - The offset is set to the end of the data plus offset (multichannel) frames.
        ///     
        /// If the <paramref name="sndfile"/> audio file was opened in ReadWrite mode, the whence parameter
        /// can be bit-wise OR'd with <see cref="LibsndfileMode"/> SFM_READ or SFM_WRITE values to modify each pointer
        /// separately.
        /// </summary>
        /// <param name="sndfile">Audio file we wish to seek in.</param>
        /// <param name="count">Number of multichannel frames to offset from our <paramref name="whence"/> position.</param>
        /// <param name="whence">The position where our seek offset begins.</param>
        /// <returns>Returns offset in multichannel frames from the beginning of the audio file.</returns>
        long Seek(IntPtr sndfile, long count, SEEK whence);

        /// <summary>
        /// Attempts to execute the <paramref name="command"/> against the <paramref name="sndfile"/> audio 
        /// file while either passing or receiving data through the <paramref name="data"/> buffer.
        /// </summary>
        /// <param name="sndfile">Audio file we run this command against.
        /// Use NULL if you wish to run a static command against the library.</param>
        /// <param name="command"><see cref="NLibsndfile.Native.LibsndfileCommand"/> to run against the given audio file.</param>
        /// <param name="data">IntPtr(Void*) buffer for passing or receiving data based on the <paramref name="command"/> specifications.</param>
        /// <param name="size">Length in bytes of the data being passed through the <paramref name="data"/> buffer.</param>
        /// <returns>Returns a specific value based on the <paramref name="command"/>.</returns>
        int Command(IntPtr sndfile, LibsndfileCommand command, IntPtr data, int size);

        /// <summary>
        /// Attempts to execute the <paramref name="command"/> against the <paramref name="sndfile"/> audio 
        /// file while either passing or receiving data through the <paramref name="data"/> buffer.
        /// </summary>
        /// <param name="sndfile">Audio file we run this command against.
        /// Use NULL if you wish to run a static command against the library.</param>
        /// <param name="command"><see cref="NLibsndfile.Native.LibsndfileCommand"/> to run against the given audio file.</param>
        /// <param name="data">Ref int(int*) buffer for passing or receiving data based on <paramref name="command"/> specifications.</param>
        /// <param name="size">Size, in bytes, of a int.</param>
        /// <returns>Returns a specific value based on the <paramref name="command"/>.</returns>
        int Command(IntPtr sndfile, LibsndfileCommand command, ref int data, int size);

        /// <summary>
        /// Attempts to execute the <paramref name="command"/> against the <paramref name="sndfile"/> audio 
        /// file while either passing or receiving data through the <paramref name="data"/> buffer.
        /// </summary>
        /// <param name="sndfile">Audio file we run this command against.
        /// Use NULL if you wish to run a static command against the library.</param>
        /// <param name="command"><see cref="NLibsndfile.Native.LibsndfileCommand"/> to run against the given audio file.</param>
        /// <param name="data">Ref double(double*) buffer for passing or receiving data based on <paramref name="command"/> specifications.</param>
        /// <param name="size">Size, in bytes, of a double.</param>
        /// <returns>Returns a specific value based on the <paramref name="command"/>.</returns>
        int Command(IntPtr sndfile, LibsndfileCommand command, ref double data, int size);

        /// <summary>
        /// Attempts to execute the <paramref name="command"/> against the <paramref name="sndfile"/> audio 
        /// file while either passing or receiving data through the <paramref name="data"/> buffer.
        /// </summary>
        /// <param name="sndfile">Audio file we run this command against.
        /// Use NULL if you wish to run a static command against the library.</param>
        /// <param name="command"><see cref="NLibsndfile.Native.LibsndfileCommand"/> to run against the given audio file.</param>
        /// <param name="data">Ref long(long*) buffer passing or receiving data based on <paramref name="command"/> specifications.</param>
        /// <param name="size">Size, in bytes, of a long.</param>
        /// <returns>Returns a specific value based on the <paramref name="command"/>.</returns>
        int Command(IntPtr sndfile, LibsndfileCommand command, ref long data, int size);

        /// <summary>
        /// Attempts to execute the <paramref name="command"/> against the <paramref name="sndfile"/> audio 
        /// file while either passing or receiving data through the <paramref name="data"/> buffer.
        /// </summary>
        /// <param name="sndfile">Audio file we run this command against.
        /// Use NULL if you wish to run a static command against the library.</param>
        /// <param name="command"><see cref="NLibsndfile.Native.LibsndfileCommand"/> to execute</param>
        /// <param name="data">Double[](double*) buffer passing or receiving data based on <paramref name="command"/> specifications.</param>
        /// <param name="size">Size, in bytes, of (double * buffer length)</param>
        /// <returns>Returns a specific value based on the <paramref name="command"/>.</returns>
        int Command(IntPtr sndfile, LibsndfileCommand command, double[] data, int size);

        /// <summary>
        /// Returns the current error number for the given <paramref name="sndfile"/> audio file.
        /// </summary>
        /// <param name="sndfile">Audio file to check for errors.</param>
        /// <returns><see cref="LibsndfileError"/>error code.</returns>
        LibsndfileError Error(IntPtr sndfile);

        /// <summary>
        /// Returns the string representation of the current error for the <paramref name="sndfile"/> audio file.
        /// </summary>
        /// <param name="sndfile">Audio file we want to check for errors.</param>
        /// <returns>String containing the description of the current error.</returns>
        string ErrorString(IntPtr sndfile);

        /// <summary>
        /// Returns the string representation of the int value backing <see cref="LibsndfileError"/>.
        /// </summary>
        /// <param name="error"><see cref="LibsndfileError"/> error code.</param>
        /// <returns>Description of the given error code.</returns>
        string ErrorNumber(int error);

        /// <summary>
        /// Closes the <paramref name="sndfile"/> audio file.
        /// </summary>
        /// <param name="sndfile">Audio file we want to close.</param>
        /// <returns><see cref="LibsndfileError"/> error code.</returns>
        LibsndfileError Close(IntPtr sndfile);

        /// <summary>
        /// Forces operating system to write buffers to disk. Only works if <paramref name="sndfile"/> is
        /// opened in <see cref="LibsndfileMode"/> SFM_WRITE or SFM_RDWR.
        /// </summary>
        /// <param name="sndfile">Audio file you wish to flush buffers on.</param>
        void WriteSync(IntPtr sndfile);

        /// <summary>
        /// ReadItems <paramref name="items"/> from the <paramref name="sndfile"/> audio file into the audio
        /// <paramref name="buffer"/>. Items must be a product of the # of channels for
        /// the <paramref name="sndfile"/>. 
        /// </summary>
        /// <param name="sndfile">Audio file to read from.</param>
        /// <param name="buffer">Buffer to fill.</param>
        /// <param name="items">Number of items to put in the <paramref name="buffer"/>.</param>
        /// <returns>Returns the number of items read. Should be equal to <paramref name="items"/> unless
        /// you've reached EOF.</returns>
        long ReadItems(IntPtr sndfile, short[] buffer, long items);

        /// <summary>
        /// ReadItems <paramref name="items"/> from the <paramref name="sndfile"/> audio file into the audio
        /// <paramref name="buffer"/>. Items must be a product of the # of channels for
        /// the <paramref name="sndfile"/>. 
        /// </summary>
        /// <param name="sndfile">Audio file to read from.</param>
        /// <param name="buffer">Buffer to fill.</param>
        /// <param name="items">Number of items to put in the <paramref name="buffer"/>.</param>
        /// <returns>Returns the number of items read. Should be equal to <paramref name="items"/> unless
        /// you've reached EOF.</returns>
        long ReadItems(IntPtr sndfile, int[] buffer, long items);

        /// <summary>
        /// ReadItems <paramref name="items"/> from the <paramref name="sndfile"/> audio file into the audio
        /// <paramref name="buffer"/>. Items must be a product of the # of channels for
        /// the <paramref name="sndfile"/>. 
        /// </summary>
        /// <param name="sndfile">Audio file to read from.</param>
        /// <param name="buffer">Buffer to fill.</param>
        /// <param name="items">Number of items to put in the <paramref name="buffer"/>.</param>
        /// <returns>Returns the number of items read. Should be equal to <paramref name="items"/> unless
        /// you've reached EOF.</returns>
        long ReadItems(IntPtr sndfile, float[] buffer, long items);

        /// <summary>
        /// ReadItems <paramref name="items"/> from the <paramref name="sndfile"/> audio file into the audio
        /// <paramref name="buffer"/>. Items must be a product of the # of channels for
        /// the <paramref name="sndfile"/>. 
        /// </summary>
        /// <param name="sndfile">Audio file to read from.</param>
        /// <param name="buffer">Buffer to fill.</param>
        /// <param name="items">Number of items to put in the <paramref name="buffer"/>.</param>
        /// <returns>Returns the number of items read. Should be equal to <paramref name="items"/> unless
        /// you've reached EOF.</returns>
        long ReadItems(IntPtr sndfile, double[] buffer, long items);

        /// <summary>
        /// ReadItems <paramref name="frames"/> from the <paramref name="sndfile"/> audio file into the audio
        /// <paramref name="buffer"/>.
        /// </summary>
        /// <param name="sndfile">Audio file to read from.</param>
        /// <param name="buffer">Buffer to fill.</param>
        /// <param name="frames">Number of frames to put in the <paramref name="buffer"/>.</param>
        /// <returns>Returns the number of frames read. Should be equal to <paramref name="frames"/> unless
        /// you've reached EOF.</returns>
        long ReadFrames(IntPtr sndfile, short[] buffer, long frames);

        /// <summary>
        /// Read <paramref name="frames"/> from the <paramref name="sndfile"/> audio file into the audio
        /// <paramref name="buffer"/>.
        /// </summary>
        /// <param name="sndfile">Audio file to read from.</param>
        /// <param name="buffer">Buffer to fill.</param>
        /// <param name="frames">Number of frames to put in the <paramref name="buffer"/>.</param>
        /// <returns>Returns the number of frames read. Should be equal to <paramref name="frames"/> unless
        /// you've reached EOF.</returns>
        long ReadFrames(IntPtr sndfile, int[] buffer, long frames);

        /// <summary>
        /// Read <paramref name="frames"/> from the <paramref name="sndfile"/> audio file into the audio
        /// <paramref name="buffer"/>.
        /// </summary>
        /// <param name="sndfile">Audio file to read from.</param>
        /// <param name="buffer">Buffer to fill.</param>
        /// <param name="frames">Number of frames to put in the <paramref name="buffer"/>.</param>
        /// <returns>Returns the number of frames read. Should be equal to <paramref name="frames"/> unless
        /// you've reached EOF.</returns>
        long ReadFrames(IntPtr sndfile, float[] buffer, long frames);

        /// <summary>
        /// Read <paramref name="frames"/> from the <paramref name="sndfile"/> audio file into the audio
        /// <paramref name="buffer"/>.
        /// </summary>
        /// <param name="sndfile">Audio file to read from.</param>
        /// <param name="buffer">Buffer to fill.</param>
        /// <param name="frames">Number of frames to put in the <paramref name="buffer"/>.</param>
        /// <returns>Returns the number of frames read. Should be equal to <paramref name="frames"/> unless
        /// you've reached EOF.</returns>
        long ReadFrames(IntPtr sndfile, double[] buffer, long frames);

        /// <summary>
        /// Write <paramref name="items"/> from the <paramref name="buffer"/> into the audio <paramref name="sndfile"/>.
        /// </summary>
        /// <param name="sndfile">Audio file to write to.</param>
        /// <param name="buffer">Buffer to write from.</param>
        /// <param name="items">Number of items to read from the <paramref name="buffer"/>.</param>
        /// <returns>Returns the number of items written. Should be equal to <paramref name="items"/> unless
        /// you've reached EOF.</returns>
        long WriteItems(IntPtr sndfile, short[] buffer, long items);

        /// <summary>
        /// Write <paramref name="items"/> from the <paramref name="buffer"/> into the audio <paramref name="sndfile"/>.
        /// </summary>
        /// <param name="sndfile">Audio file to write to.</param>
        /// <param name="buffer">Buffer to write from.</param>
        /// <param name="items">Number of items to read from the <paramref name="buffer"/>.</param>
        /// <returns>Returns the number of items written. Should be equal to <paramref name="items"/> unless
        /// you've reached EOF.</returns>
        long WriteItems(IntPtr sndfile, int[] buffer, long items);

        /// <summary>
        /// Write <paramref name="items"/> from the <paramref name="buffer"/> into the audio <paramref name="sndfile"/>.
        /// </summary>
        /// <param name="sndfile">Audio file to write to.</param>
        /// <param name="buffer">Buffer to write from.</param>
        /// <param name="items">Number of items to read from the <paramref name="buffer"/>.</param>
        /// <returns>Returns the number of items written. Should be equal to <paramref name="items"/> unless
        /// you've reached EOF.</returns>
        long WriteItems(IntPtr sndfile, float[] buffer, long items);

        /// <summary>
        /// Write <paramref name="items"/> from the <paramref name="buffer"/> into the audio <paramref name="sndfile"/>.
        /// </summary>
        /// <param name="sndfile">Audio file to write to.</param>
        /// <param name="buffer">Buffer to write from.</param>
        /// <param name="items">Number of items to read from the <paramref name="buffer"/>.</param>
        /// <returns>Returns the number of items written. Should be equal to <paramref name="items"/> unless
        /// you've reached EOF.</returns>
        long WriteItems(IntPtr sndfile, double[] buffer, long items);

        /// <summary>
        /// Write <paramref name="frames"/> from the <paramref name="buffer"/> into the audio <paramref name="sndfile"/>.
        /// </summary>
        /// <param name="sndfile">Audio file to write to.</param>
        /// <param name="buffer">Buffer to write from.</param>
        /// <param name="frames">Number of frames to read from the <paramref name="buffer"/>.</param>
        /// <returns>Returns the number of frames written. Should be equal to <paramref name="frames"/> unless
        /// you've reached EOF.</returns>
        long WriteFrames(IntPtr sndfile, short[] buffer, long frames);

        /// <summary>
        /// Write <paramref name="frames"/> from the <paramref name="buffer"/> into the audio <paramref name="sndfile"/>.
        /// </summary>
        /// <param name="sndfile">Audio file to write to.</param>
        /// <param name="buffer">Buffer to write from.</param>
        /// <param name="frames">Number of frames to read from the <paramref name="buffer"/>.</param>
        /// <returns>Returns the number of frames written. Should be equal to <paramref name="frames"/> unless
        /// you've reached EOF.</returns>
        long WriteFrames(IntPtr sndfile, int[] buffer, long frames);

        /// <summary>
        /// Write <paramref name="frames"/> from the <paramref name="buffer"/> into the audio <paramref name="sndfile"/>.
        /// </summary>
        /// <param name="sndfile">Audio file to write to.</param>
        /// <param name="buffer">Buffer to write from.</param>
        /// <param name="frames">Number of frames to read from the <paramref name="buffer"/>.</param>
        /// <returns>Returns the number of frames written. Should be equal to <paramref name="frames"/> unless
        /// you've reached EOF.</returns>
        long WriteFrames(IntPtr sndfile, float[] buffer, long frames);

        /// <summary>
        /// Write <paramref name="frames"/> from the <paramref name="buffer"/> into the audio <paramref name="sndfile"/>.
        /// </summary>
        /// <param name="sndfile">Audio file to write to.</param>
        /// <param name="buffer">Buffer to write from.</param>
        /// <param name="frames">Number of frames to read from the <paramref name="buffer"/>.</param>
        /// <returns>Returns the number of frames written. Should be equal to <paramref name="frames"/> unless
        /// you've reached EOF.</returns>
        long WriteFrames(IntPtr sndfile, double[] buffer, long frames);

        /// <summary>
        /// Reads <paramref name="bytes"/> amount of raw audio data from 
        /// <paramref name="sndfile"/> into <paramref name="buffer"/>.
        /// </summary>
        /// <param name="sndfile">Audio file to read from.</param>
        /// <param name="buffer">Buffer to fill.</param>
        /// <param name="bytes">Number of bytes to read from <paramref name="sndfile"/>.</param>
        /// <returns>Returns the number of frames written. Should be equal to <paramref name="bytes"/> unless
        /// you've reached EOF.</returns>
        long ReadRaw(IntPtr sndfile, byte[] buffer, long bytes);

        /// <summary>
        /// Writes <paramref name="bytes"/> amount of raw audio data from
        /// <paramref name="buffer"/> into <paramref name="sndfile"/>.
        /// </summary>
        /// <param name="sndfile">Audio file to write to.</param>
        /// <param name="buffer">Buffer to write from.</param>
        /// <param name="bytes">Number of bytes to read from <paramref name="sndfile"/>.</param>
        /// <returns>Returns the number of frames written. Should be equal to <paramref name="bytes"/> unless
        /// you've reached EOF.</returns>
        long WriteRaw(IntPtr sndfile, byte[] buffer, long bytes);

        /// <summary>
        /// Reads the <paramref name="type"/> tag from the <paramref name="sndfile"/> audio file.
        /// </summary>
        /// <param name="sndfile">Audio file to read tags from.</param>
        /// <param name="type"><see cref="LibsndfileStringType"/> tag to read.</param>
        /// <returns>Returns the value of the <paramref name="type"/> tag.</returns>
        string GetString(IntPtr sndfile, LibsndfileStringType type);

        /// <summary>
        /// Writes the <paramref name="value"/> to the ID3 tag of <paramref name="type"/> 
        /// in the <paramref name="sndfile"/> audio file.
        /// </summary>
        /// <param name="sndfile">Audio file to write tags to.</param>
        /// <param name="type"><see cref="LibsndfileStringType"/> tag to change.</param>
        /// <param name="value">New value of <see cref="LibsndfileStringType"/> tag.</param>
        /// <returns>Returns an <see cref="LibsndfileError"/> error code.</returns>
        LibsndfileError SetString(IntPtr sndfile, LibsndfileStringType type, string value);


    }
}