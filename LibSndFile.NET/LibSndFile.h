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


#include <windows.h>
#include <io.h>
#include <Fcntl.h>
#include "SndFileFormat.h"
#include "SndFileException.h"
#include "SndFileEnums.h"
#include <iostream>
#include <vector>


using namespace System;
using namespace System::Runtime::Remoting;
using namespace System::Runtime::InteropServices;
using namespace System::IO;

using namespace std;


namespace LibSndFile {

	

	public struct MemBufferFileIo : public SF_VIRTUAL_IO
	{
		

		MemBufferFileIo(const unsigned char* data, sf_count_t dataSize)
			: SF_VIRTUAL_IO()
			, m_data(data)
			, m_dataSize(dataSize)
		{
			//assert(data != nullptr);

			get_filelen = &MemBufferFileIo::getFileLength;
			seek = &MemBufferFileIo::doSeek;
			read = &MemBufferFileIo::doRead;
			write = &MemBufferFileIo::doWrite;
			tell = &MemBufferFileIo::doTell;
		}

		MemBufferFileIo(std::vector<byte> data, sf_count_t dataSize)
			: SF_VIRTUAL_IO()
			, m_datavec(data)
			, m_dataSize(dataSize)
			, m_data(data.data())
		{
			//assert(data != nullptr);

			get_filelen = &MemBufferFileIo::getFileLength;
			seek = &MemBufferFileIo::doSeek;
			read = &MemBufferFileIo::doRead;
			write = &MemBufferFileIo::doWrite;
			tell = &MemBufferFileIo::doTell;
		}

		static sf_count_t getFileLength(void *user_data)
		{
			auto self = static_cast<MemBufferFileIo*>(user_data);
			return self->m_dataSize;
		}

		static sf_count_t doSeek(sf_count_t offset, int whence, void *user_data)
		{
			auto self = static_cast<MemBufferFileIo*>(user_data);
			switch (whence)
			{
			case SEEK_SET:
				//assert(offset >= 0 && offset <= self->m_dataSize);
				self->m_where = offset;
				break;
			case SEEK_CUR:
				//assert(self->m_where + offset <= self->m_dataSize && self->m_where + offset >= 0);
				self->m_where += offset;
				break;
			case SEEK_END:
				//assert(offset >= 0 && offset <= self->m_dataSize);
				self->m_where = self->m_dataSize - offset;
				break;
			default:
				break;
				//assert(false);
			};
			return self->m_where;
		}

		static sf_count_t doRead(void *ptr, sf_count_t count, void *user_data)
		{
			using namespace std;
			auto self = static_cast<MemBufferFileIo*>(user_data);
			if (self->m_where + count > self->m_dataSize)
				count = self->m_dataSize - self->m_where;

			//assert(self->m_where + count <= self->m_dataSize);

			byte* buf = static_cast<byte*>(ptr);
			std::copy(self->m_data + self->m_where, self->m_data + self->m_where + count, buf);
			self->m_where += count;
			return count;
		}

		static sf_count_t doWrite(const void* /*ptr*/, sf_count_t /*count*/, void* /*user_data*/)
		{
			return 0; // read-only
		}

		static sf_count_t doTell(void *user_data)
		{
			auto self = static_cast<MemBufferFileIo*>(user_data);
			return self->m_where;
		}

	private:
		const byte* const m_data;
		const sf_count_t m_dataSize;
		sf_count_t m_where = 0;
		std::vector<byte> m_datavec;
	};

    [SerializableAttribute]
    [ComVisibleAttribute(true)]
    ///<summary>A SndFileInfo reference is passed at construction of any SndFile instance.</summary>
    ///<remarks>
    ///<para>For Writing access, the SndFileInfo instance shall be priorly filled in by user to describe the sound file characteristics.</para>
    ///<para>For Reading access, the SndFileInfo instance will reflect characteritics of the sound file.</para>
    ///</remarks>
    public ref struct SndFileInfo
    {
        ///<summary>Count of samples per second.</summary>
        int SamplesPerSecond;
        ///<summary>Count of audio channels.</summary>
        int Channels;
        ///<summary> Indicates the sound file format (SF_FORMAT_...) as defined by the <a href="http://www.mega-nerd.com/libsndfile">libsndfile</a> library.</summary>
        ///<remarks>
        ///Possible value is a bitwise ORed of one <see cref="SndFileMajorFormat"/> value and one compatible <see cref="SndFileSubFormat"/> value.
        ///</remarks>
        int Format;
		sf_count_t Frames;
		int Sections;
		int Seekable;
    };

    [ComVisibleAttribute(true)]
    ///<summary>
    ///A managed class wrapper for the functions of the libsndfile library
    ///</summary>
    public ref class SndFile: public MarshalByRefObject
    {
    private:
        IO::FileStream^ const _InnerFileStream;

    protected:
        SNDFILE*        sndfileHandle;
        SndFileInfo^    sndFileInfo;

    public:
        ///<summary>
        ///Gets the inner FileStream instance used for access to the file.
        ///</summary>
        property IO::FileStream^ InnerFileStream
        { IO::FileStream^ get() { return _InnerFileStream; } }

		property SndFileInfo^ FileInfo {SndFileInfo^ get() { return sndFileInfo; }}
        
        ///<summary>
        ///Initializes a new instance of the SndFile class.
        ///<para>According to the right access (Reading/Writing) of <paramref name="innerfileStream"/>,
        /// the sound file is opened for reading and/or writing.</para>
        ///</summary>
        ///<param name="innerfileStream">the FileStream instance used for access to the file.</param>
        ///<param name="info">the file info required for writing to a sound file or reflecting the sound file characteristics in reading access.</param>
        ///<exception cref="SndFileException">Thrown when libsndfile function sf_open_fd failed.</exception>
        ///<exception cref="IOException">Thrown when file opening failed.</exception>
        SndFile(
            [InAttribute] FileStream^ innerfileStream,
            [InAttribute] [OutAttribute] SndFileInfo^ info ):
        _InnerFileStream( innerfileStream )
        {
            SF_INFO sf_info;
            // native file handle
            IntPtr intPtr = innerfileStream->SafeFileHandle->DangerousGetHandle();
            int sf_mode;
            int _Flags;

            sndFileInfo = info;

            sf_info.samplerate = info->SamplesPerSecond;
            sf_info.channels = info->Channels;
            sf_info.format = info->Format;
			sf_info.frames = info->Frames;
			sf_info.sections = info->Sections;
			sf_info.seekable = info->Seekable;
            
            if( innerfileStream->CanRead )
            {
                if( innerfileStream->CanWrite )
                {
                    sf_mode = SFM_RDWR;
                    _Flags = _O_RDWR;
                }
                else
                {
                    sf_mode = SFM_READ;
                    _Flags = _O_RDONLY;
                }
            }
            else
            {
                sf_mode = SFM_WRITE;
                _Flags = _O_WRONLY;
            }

            // native file descriptor
            int fd = _open_osfhandle( (intptr_t) intPtr.ToPointer(), _Flags );
            if( fd < 0 )
                throw gcnew IOException( "_open_osfhandle() was not able to return a valid file descriptor");
            

            sndfileHandle = sf_open_fd( fd, sf_mode, &sf_info, FALSE );
            if( sndfileHandle == NULL )
            {                
                //TODO: return a sndfile error code
                throw gcnew SndFileException( NULL );
            }

            info->SamplesPerSecond = sf_info.samplerate;
            info->Channels = sf_info.channels;
            info->Format = sf_info.format;
			info->Frames = sf_info.frames;
			info->Sections = sf_info.sections;
			info->Seekable = sf_info.seekable;
        }


        ///<summary>
        ///Releases all resources used by the <see cref="SndFile"/>.
        ///</summary>
        !SndFile()
        {
            int sf_return;

            sf_write_sync( sndfileHandle );
            sf_return = sf_close( sndfileHandle );

            Diagnostics::Debug::Assert( sf_return == 0, "sf_close has returned a non null value:" + sf_return );
        }
        ///<summary>
        ///Releases all resources used by the <see cref="SndFile"/>.
        ///</summary>
        ~SndFile(){ this->!SndFile(); }


        ///<summary>
        ///Reads some PCM audio <paramref name="samples"/> from the sound file.
        ///</summary>
        ///<remarks>
        ///<para>If the sound file contains several channels items are interleaved in the output buffer.</para>
        ///<para>The count of items to be read must be an integer product of the number of channels.</para>
        ///<para>
        ///It is important to note that the data type used by the calling program and the data format of the file
        /// do not need to be the same.
        /// For instance, it is possible to open a 16 bit PCM encoded WAV file and
        ///read the data using Read<b>&lt;Double&gt;</b>().
        /// The library seamlessly converts between the two formats on-the-fly.
        ///</para>
        ///</remarks>
        ///<typeparam name="T">Type of the audio samples. This can be short, int, float or double</typeparam>
        ///<param name="samples">Reference on the 1st element of the managed array where to store items read from the file.</param>
        ///<param name="countOfSamples">Count of items to read.</param>
        ///<returns>Count of items actually read. Can be less than <paramref name="countOfSamples"/> if end of file has been reached.</returns>
        generic <typename T> where T: value class
            int Read( T% samples, int countOfSamples )
        {
            int countOfSamplesRead;
            pin_ptr<T> buffer = &samples;

            if      ( T::typeid == short::typeid )
                countOfSamplesRead = Read( (short*) buffer, countOfSamples );
            else if ( T::typeid == int::typeid )
                countOfSamplesRead = Read( (int*) buffer, countOfSamples );
            else if ( T::typeid == float::typeid )
                countOfSamplesRead = Read( (float*) buffer, countOfSamples );
            else if ( T::typeid == double::typeid )
                countOfSamplesRead = Read( (double*) buffer, countOfSamples );
            else
                throw gcnew InvalidDataException( T::typeid->Name + " is not a supported type for the parameter samples" );

            return countOfSamplesRead;
        }

        ///<summary>
        ///Reads some PCM audio <paramref name="samples"/> from the sound file.
        ///Wrapper for the <a href="http://www.mega-nerd.com/libsndfile/api.html#read">libsndfile sf_read_short</a> function
        ///</summary>
        ///<remarks>
        ///<para>This function fills the native array pointed to by <paramref name="samples"/> with the requested <paramref name="countOfSamples"/>.</para>
        ///<para>
        ///It is important to note that the data format of the file does not need necessary to be short.
        /// For instance, it is possible to open a 8 bit PCM encoded WAV file and read the data using this function.
        /// libsndfile seamlessly converts between the two formats on-the-fly.
        ///</para>
        ///<para>
        ///If the sound file contains several audio channels, PCM samples of the different channels will be interleaved in the output array.
        ///</para>
        ///</remarks>
        ///<param name="samples">Pointer to the native output array where to store items read from the file.</param>
        ///<param name="countOfSamples">
        ///Count of PCM audio samples to read.
        ///It must be an integer product of the number of channels or an error will occur.
        ///</param>
        ///<returns>Count of items actually read. Can be less than <paramref name="countOfSamples"/> if the end of file has been reached.</returns>
        ///<exception cref="SndFileException">Thrown if libsnfile function sf_read_short has returned an error</exception>
        int Read( short* samples, int countOfSamples)
        {
            int count = (int) sf_read_short( sndfileHandle, samples, countOfSamples);
            /*int error = sf_error( sndfileHandle );

            if( error != SF_ERR_NO_ERROR )
                throw gcnew SndFileException( error );*/
            return count;
       }

        ///<summary>
        ///Reads some PCM audio <paramref name="samples"/> from the sound file.
        ///Wrapper for the <a href="http://www.mega-nerd.com/libsndfile/api.html#read">libsndfile sf_read_int</a> function
        ///</summary>
        ///<remarks>
        ///<para>This function fills the native array pointed to by <paramref name="samples"/> with the requested <paramref name="countOfSamples"/>.</para>
        ///<para>
        ///It is important to note that the data format of the file does not need necessary to be int.
        /// For instance, it is possible to open a 8 bit PCM encoded WAV file and read the data using this function.
        /// libsndfile seamlessly converts between the two formats on-the-fly.
        ///</para>
        ///<para>
        ///If the sound file contains several audio channels, PCM samples of the different channels will be interleaved in the output array.
        ///</para>
        ///</remarks>
        ///<param name="samples">Pointer to the native output array where to store items read from the file.</param>
        ///<param name="countOfSamples">
        ///Count of PCM audio samples to read.
        ///It must be an integer product of the number of channels or an error will occur.
        ///</param>
        ///<returns>Count of items actually read. Can be less than <paramref name="countOfSamples"/> if the end of file has been reached.</returns>
        ///<exception cref="SndFileException">Thrown if libsnfile function sf_read_int has returned an error</exception>
        int Read( int* samples, int countOfSamples)
        {
            int count = (int) sf_read_int( sndfileHandle, samples, countOfSamples);
            /*int error = sf_error( sndfileHandle );

            if( error != SF_ERR_NO_ERROR )
                throw gcnew SndFileException( error );*/
            return count;
        }

        ///<summary>
        ///Reads some PCM audio <paramref name="samples"/> from the sound file.
        ///Wrapper for the <a href="http://www.mega-nerd.com/libsndfile/api.html#read">libsndfile sf_read_float</a> function
        ///</summary>
        ///<remarks>
        ///<para>This function fills the native array pointed to by <paramref name="samples"/> with the requested <paramref name="countOfSamples"/>.</para>
        ///<para>
        ///It is important to note that the data format of the file does not need necessary to be float.
        /// For instance, it is possible to open a 8 bit PCM encoded WAV file and read the data using this function.
        /// libsndfile seamlessly converts between the two formats on-the-fly.
        ///</para>
        ///<para>
        ///If the sound file contains several audio channels, PCM samples of the different channels will be interleaved in the output array.
        ///</para>
        ///</remarks>
        ///<param name="samples">Pointer to the native output array where to store items read from the file.</param>
        ///<param name="countOfSamples">
        ///Count of PCM audio samples to read.
        ///It must be an integer product of the number of channels or an error will occur.
        ///</param>
        ///<returns>Count of items actually read. Can be less than <paramref name="countOfSamples"/> if the end of file has been reached.</returns>
        ///<exception cref="SndFileException">Thrown if libsnfile function sf_read_float has returned an error</exception>
        int Read( float* samples, int countOfSamples)
        {
            int count = (int) sf_read_float( sndfileHandle, samples, countOfSamples);
            /*int error = sf_error( sndfileHandle );

            if( error != SF_ERR_NO_ERROR )
                throw gcnew SndFileException( error );*/
            return count;
        }

        ///<summary>
        ///Reads some PCM audio <paramref name="samples"/> from the sound file.
        ///Wrapper for the <a href="http://www.mega-nerd.com/libsndfile/api.html#read">libsndfile sf_read_double</a> function
        ///</summary>
        ///<remarks>
        ///<para>This function fills the native array pointed to by <paramref name="samples"/> with the requested <paramref name="countOfSamples"/>.</para>
        ///<para>
        ///It is important to note that the data format of the file does not need necessary to be double.
        /// For instance, it is possible to open a 8 bit PCM encoded WAV file and read the data using this function.
        /// libsndfile seamlessly converts between the two formats on-the-fly.
        ///</para>
        ///<para>
        ///If the sound file contains several audio channels, PCM samples of the different channels will be interleaved in the output array.
        ///</para>
        ///</remarks>
        ///<param name="samples">Pointer to the native output array where to store items read from the file.</param>
        ///<param name="countOfSamples">
        ///Count of PCM audio samples to read.
        ///It must be an integer product of the number of channels or an error will occur.
        ///</param>
        ///<returns>Count of items actually read. Can be less than <paramref name="countOfSamples"/> if the end of file has been reached.</returns>
        ///<exception cref="SndFileException">Thrown if libsnfile function sf_read_double has returned an error</exception>
        int Read( double* samples, int countOfSamples)
        {
            int count = (int) sf_read_double( sndfileHandle, samples, countOfSamples);
            /*int error = sf_error( sndfileHandle );

            if( error != SF_ERR_NO_ERROR )
                throw gcnew SndFileException( error );*/
            return count;
        }

        ///<summary>
        ///Reads some <paramref name="samples"/> from the sound file. And store them in a managed Array.
        ///</summary>
        ///<remarks>
        ///<para>
        ///It is important to note that the data format of the file does not need necessary to be of type <typeparamref name="T"/>.
        /// libsndfile seamlessly converts between the formats of the file and requested format <typeparamref name="T"/> on-the-fly.
        ///</para>
        ///<para>
        ///If the sound file contains several audio channels, PCM samples of the different channels will be interleaved in the output array.
        ///</para>
        ///</remarks>
        ///<param name="samples">Pointer to the managed output array where to store items read from the file.</param>
        ///<param name="countOfSamples">
        ///Count of PCM audio samples to read.
        ///It must be an integer product of the number of channels or an error will occur.
        ///</param>
        ///<returns>Count of items actually read. Can be less than <paramref name="countOfSamples"/> if the end of file has been reached.</returns>
        ///<typeparam name="T">Type of the audio samples. This can be short, int, float or double</typeparam>
        generic <typename T> where T: value class int Read( array<T>^ samples, int countOfSamples)
        {
            return Read( samples[0], countOfSamples );
        }

        ///<summary>
        ///Reads some <paramref name="samples"/> from the sound file. And store them in a managed Array.
        ///</summary>
        ///<remarks>
        ///<para>
        ///It is important to note that the data format of the file does not need necessary to be of type <typeparamref name="T"/>.
        /// libsndfile seamlessly converts between the formats of the file and requested format <typeparamref name="T"/> on-the-fly.
        ///</para>
        ///<para>
        ///If the sound file contains several audio channels, PCM samples of the different channels will be interleaved in the output array.
        ///</para>
        ///</remarks>
        ///<param name="samples">Pointer to the managed output array where to store items read from the file.</param>
        ///<param name="index">Position in the output array where to start to store read samples.</param>
        ///<param name="countOfSamples">
        ///Count of PCM audio samples to read.
        ///It must be an integer product of the number of channels or an error will occur.
        ///</param>
        ///<returns>Count of items actually read. Can be less than <paramref name="countOfSamples"/> if the end of file has been reached.</returns>
        ///<typeparam name="T">Type of the audio samples. This can be short, int, float or double</typeparam>
        generic <typename T> where T: value class int Read( array<T>^ samples, int index, int countOfSamples)
        {
            return Read( samples[index], countOfSamples );
        }

        ///<summary>
        ///Reads some <paramref name="samples"/> from the sound file. And store them in a managed Array.
        ///</summary>
        ///<remarks>
        ///<para>
        ///It is important to note that the data format of the file does not need necessary to be of type <paramref name="sampleType"/>.
        /// libsndfile seamlessly converts between the formats of the file and requested format <paramref name="sampleType"/> on-the-fly.
        ///</para>
        ///<para>
        ///If the sound file contains several audio channels, PCM samples of the different channels will be interleaved in the output array.
        ///</para>
        ///</remarks>
        ///<param name="samples">Pointer to the managed output array where to store items read from the file.</param>
        ///<param name="sampleType">Type of the audio samples. This can be TypeCode::Int16, TypeCode::Int32, TypeCode::Single or TypeCode::Double</param>
        ///<param name="index">Position in the output array where to start to store read samples.</param>
        ///<param name="countOfSamples">
        ///Count of PCM audio samples to read.
        ///It must be an integer product of the number of channels or an error will occur.
        ///</param>
        ///<returns>Count of items actually read. Can be less than <paramref name="countOfSamples"/> if the end of file has been reached.</returns>
        int Read( Array^ samples, Type^ sampleType, int index, int countOfSamples)
        {
            switch( Type::GetTypeCode(sampleType) )
            {
            case TypeCode::Int16:
                return Read( (array<short>^) samples, index, countOfSamples );
                break;
            case TypeCode::Int32:
                return Read( (array<int>^) samples, index, countOfSamples );
                break;
            case TypeCode::Single:
                return Read( (array<float>^) samples, index, countOfSamples );
                break;
            case TypeCode::Double:
                return Read( (array<double>^) samples, index, countOfSamples );
                break;
            default:
                throw gcnew ArgumentException( "type" + sampleType->Name + " is not valid for sampleType parameter" );
            }
        }

        ///<summary>
        ///Reads some <paramref name="samples"/> from the sound file. And store them in a managed Array.
        ///</summary>
        ///<remarks>
        ///<para>
        ///It is important to note that the data format of the file does not need necessary to be of type <paramref name="sampleType"/>.
        /// libsndfile seamlessly converts between the formats of the file and requested format <paramref name="sampleType"/> on-the-fly.
        ///</para>
        ///<para>
        ///If the sound file contains several audio channels, PCM samples of the different channels will be interleaved in the output array.
        ///</para>
        ///</remarks>
        ///<param name="samples">Pointer to the managed output array where to store items read from the file.</param>
        ///<param name="sampleType">Type of the audio samples. This can be TypeCode::Int16, TypeCode::Int32, TypeCode::Single or TypeCode::Double</param>
        ///<param name="countOfSamples">
        ///Count of PCM audio samples to read.
        ///It must be an integer product of the number of channels or an error will occur.
        ///</param>
        ///<returns>Count of items actually read. Can be less than <paramref name="countOfSamples"/> if the end of file has been reached.</returns>
        int Read( Array^ samples, Type^ sampleType, int countOfSamples)
        {
            return Read( samples, sampleType, 0, countOfSamples );
        }


        ///<summary>
        ///Writes some PCM audio <paramref name="samples"/> to the sound file.
        ///</summary>
        ///<remarks>
        ///<para>If the SndFile instance has been initialized to contain several channels, items are interleaved in the target file.</para>
        ///<para>The count of items to be written must be an integer product of the number of channels.</para>
        ///<para>
        ///It is important to note that the data type used by the calling program and the data format of the file
        /// do not need to be the same.
        /// For instance, it is possible to open a 16 bit PCM encoded WAV file and
        ///write samples of type Single.
        /// The library seamlessly converts between the two formats on-the-fly.
        ///</para>
        ///</remarks>
        ///<typeparam name="T">Type of the audio samples. This can be short, int, float or double</typeparam>
        ///<param name="samples">Reference on the 1st element of the managed array to be written to the file.</param>
        ///<param name="countOfSamples">Count of items to write.</param>
        ///<returns>Count of items written (which should be the same as the <paramref name="countOfSamples"/> parameter).</returns>
        generic <typename T> where T: value class int Write( T% samples, int countOfSamples )
        {
            int countOfSamplesWritten;
            pin_ptr<T> buffer = &samples;

            if( T::typeid == short::typeid )
                countOfSamplesWritten = Write( (short*) buffer, countOfSamples );
            else if(T::typeid == int::typeid)
                countOfSamplesWritten = Write( (int*) buffer, countOfSamples );
            else if(T::typeid == float::typeid)
                countOfSamplesWritten = Write( (float*) buffer, countOfSamples );
            else if(T::typeid == double::typeid)
                countOfSamplesWritten = Write( (double*) buffer, countOfSamples );
            else
                throw gcnew InvalidDataException( T::typeid->Name + " is not a supported type for the parameter samples" );

            return countOfSamplesWritten;
        }

        ///<summary>
        ///Writes some PCM audio <paramref name="samples"/> to the sound file.
        ///Wrapper for the <a href="http://www.mega-nerd.com/libsndfile/api.html#write">libsndfile sf_write_short</a> function
        ///</summary>
        ///<remarks>
        ///<para>If the SndFile instance has been initialized to contain several channels, items are interleaved in the target file.</para>
        ///<para>The count of items to be written must be an integer product of the number of channels.</para>
        ///<para>
        ///It is important to note that the data type used by the calling program and the data format of the file
        /// do not need to be the same.
        /// For instance, it is possible to open a 8 bit PCM encoded WAV file and
        ///write samples using this method.
        /// The library seamlessly converts between the two formats on-the-fly.
        ///</para>
        ///</remarks>
        ///<param name="samples">Reference on the 1st element of the managed array to be written to the file.</param>
        ///<param name="countOfSamples">Count of items to write.</param>
        ///<returns>Count of items written (which should be the same as the <paramref name="countOfSamples"/> parameter).</returns>
        ///<exception cref="SndFileException">Thrown if libsnfile function sf_write_short has returned an error</exception>
        int Write( short* samples, int countOfSamples)
        {
            int count = (int) sf_write_short( sndfileHandle, samples, countOfSamples);
            /*int error = sf_error( sndfileHandle );

            if( error != SF_ERR_NO_ERROR )
                throw gcnew SndFileException( error );*/
            return count;
        }

        ///<summary>
        ///Writes some PCM audio <paramref name="samples"/> to the sound file.
        ///Wrapper for the <a href="http://www.mega-nerd.com/libsndfile/api.html#write">libsndfile sf_write_int</a> function
        ///</summary>
        ///<remarks>
        ///<para>If the SndFile instance has been initialized to contain several channels, items are interleaved in the target file.</para>
        ///<para>The count of items to be written must be an integer product of the number of channels.</para>
        ///<para>
        ///It is important to note that the data type used by the calling program and the data format of the file
        /// do not need to be the same.
        /// For instance, it is possible to open a 8 bit PCM encoded WAV file and
        ///write samples using this method.
        /// The library seamlessly converts between the two formats on-the-fly.
        ///</para>
        ///</remarks>
        ///<param name="samples">Reference on the 1st element of the managed array to be written to the file.</param>
        ///<param name="countOfSamples">Count of items to write.</param>
        ///<returns>Count of items written (which should be the same as the <paramref name="countOfSamples"/> parameter).</returns>
        ///<exception cref="SndFileException">Thrown if libsnfile function sf_write_int has returned an error</exception>
        int Write( int* samples, int countOfSamples)
        {
            int count = (int) sf_write_int( sndfileHandle, samples, countOfSamples);
            /*int error = sf_error( sndfileHandle );

            if( error != SF_ERR_NO_ERROR )
                throw gcnew SndFileException( error );*/
            return count;
        }

        ///<summary>
        ///Writes some PCM audio <paramref name="samples"/> to the sound file.
        ///Wrapper for the <a href="http://www.mega-nerd.com/libsndfile/api.html#write">libsndfile sf_write_float</a> function
        ///</summary>
        ///<remarks>
        ///<para>If the SndFile instance has been initialized to contain several channels, items are interleaved in the target file.</para>
        ///<para>The count of items to be written must be an integer product of the number of channels.</para>
        ///<para>
        ///It is important to note that the data type used by the calling program and the data format of the file
        /// do not need to be the same.
        /// For instance, it is possible to open a 8 bit PCM encoded WAV file and
        ///write samples using this method.
        /// The library seamlessly converts between the two formats on-the-fly.
        ///</para>
        ///</remarks>
        ///<param name="samples">Reference on the 1st element of the managed array to be written to the file.</param>
        ///<param name="countOfSamples">Count of items to write.</param>
        ///<returns>Count of items written (which should be the same as the <paramref name="countOfSamples"/> parameter).</returns>
        ///<exception cref="SndFileException">Thrown if libsnfile function sf_write_float has returned an error</exception>
        int Write( float* samples, int countOfSamples)
        {
            int count = (int) sf_write_float( sndfileHandle, samples, countOfSamples);
            /*int error = sf_error( sndfileHandle );

            if( error != SF_ERR_NO_ERROR )
                throw gcnew SndFileException( error );*/
            return count;
        }

        ///<summary>
        ///Writes some PCM audio <paramref name="samples"/> to the sound file.
        ///Wrapper for the <a href="http://www.mega-nerd.com/libsndfile/api.html#write">libsndfile sf_write_double</a> function
        ///</summary>
        ///<remarks>
        ///<para>If the SndFile instance has been initialized to contain several channels, items are interleaved in the target file.</para>
        ///<para>The count of items to be written must be an integer product of the number of channels.</para>
        ///<para>
        ///It is important to note that the data type used by the calling program and the data format of the file
        /// do not need to be the same.
        /// For instance, it is possible to open a 8 bit PCM encoded WAV file and
        ///write samples using this method.
        /// The library seamlessly converts between the two formats on-the-fly.
        ///</para>
        ///</remarks>
        ///<param name="samples">Reference on the 1st element of the managed array to be written to the file.</param>
        ///<param name="countOfSamples">Count of items to write.</param>
        ///<returns>Count of items written (which should be the same as the <paramref name="countOfSamples"/> parameter).</returns>
        ///<exception cref="SndFileException">Thrown if libsnfile function sf_write_double has returned an error</exception>
        int Write( double* samples, int countOfSamples)
        {
            int count = (int) sf_write_double( sndfileHandle, samples, countOfSamples);
            int error = sf_error( sndfileHandle );

            /*if( error != SF_ERR_NO_ERROR )
                throw gcnew SndFileException( error );*/
            return count;
        }

        ///<summary>
        ///Writes some PCM audio <paramref name="samples"/> to the sound file.
        ///</summary>
        ///<remarks>
        ///<para>If the SndFile instance has been initialized to contain several channels, items are interleaved in the target file.</para>
        ///<para>The count of items to be written must be an integer product of the number of channels.</para>
        ///<para>
        ///It is important to note that the data type used by the calling program and the data format of the file
        /// do not need to be the same.
        /// For instance, it is possible to open a 16 bit PCM encoded WAV file and
        ///write samples of type Single.
        /// The library seamlessly converts between the two formats on-the-fly.
        ///</para>
        ///</remarks>
        ///<typeparam name="T">Type of the audio samples. This can be short, int, float or double</typeparam>
        ///<param name="samples">Managed array to be written to the file.</param>
        ///<param name="countOfSamples">Count of items to write.</param>
        ///<returns>Count of items written (which should be the same as the <paramref name="countOfSamples"/> parameter).</returns>
        generic <typename T> where T: value class int Write( array<T>^ samples, int countOfSamples)
        {
            return Write( samples[0], countOfSamples );
        }

        ///<summary>
        ///Writes some PCM audio <paramref name="samples"/> to the sound file.
        ///</summary>
        ///<remarks>
        ///<para>If the SndFile instance has been initialized to contain several channels, items are interleaved in the target file.</para>
        ///<para>The count of items to be written must be an integer product of the number of channels.</para>
        ///<para>
        ///It is important to note that the data type used by the calling program and the data format of the file
        /// do not need to be the same.
        /// For instance, it is possible to open a 16 bit PCM encoded WAV file and
        ///write samples of type Single.
        /// The library seamlessly converts between the two formats on-the-fly.
        ///</para>
        ///</remarks>
        ///<typeparam name="T">Type of the audio samples. This can be short, int, float or double</typeparam>
        ///<param name="samples">Managed array to be written to the file.</param>
        ///<param name="index">Index in the <paramref name="samples"/> where to start writing.</param>
        ///<param name="countOfSamples">Count of items to write.</param>
        ///<returns>Count of items written (which should be the same as the <paramref name="countOfSamples"/> parameter).</returns>
        generic <typename T> where T: value class int Write( array<T>^ samples, int index, int countOfSamples)
        {
            return Write( samples[index], countOfSamples );
        }

        ///<summary>
        ///Writes some PCM audio <paramref name="samples"/> to the sound file.
        ///</summary>
        ///<remarks>
        ///<para>If the SndFile instance has been initialized to contain several channels, items are interleaved in the target file.</para>
        ///<para>The count of items to be written must be an integer product of the number of channels.</para>
        ///<para>
        ///It is important to note that the data type used by the calling program and the data format of the file
        /// do not need to be the same.
        /// For instance, it is possible to open a 16 bit PCM encoded WAV file and
        ///write samples of type Single.
        /// The library seamlessly converts between the two formats on-the-fly.
        ///</para>
        ///</remarks>
        ///<param name="sampleType">Type of the audio samples. This can be short, int, float or double</param>
        ///<param name="samples">Managed array to be written to the file.</param>
        ///<param name="index">Index in the <paramref name="samples"/> where to start writing.</param>
        ///<param name="countOfSamples">Count of items to write.</param>
        ///<returns>Count of items written (which should be the same as the <paramref name="countOfSamples"/> parameter).</returns>
        int Write( Array^ samples, Type^ sampleType, int index, int countOfSamples)
        {
            switch( Type::GetTypeCode(sampleType) )
            {
            case TypeCode::Int16:
                return Write( (array<short>^) samples, index, countOfSamples );
                break;
            case TypeCode::Int32:
                return Write( (array<int>^) samples, index, countOfSamples );
                break;
            case TypeCode::Single:
                return Write( (array<float>^) samples, index, countOfSamples );
                break;
            case TypeCode::Double:
                return Write( (array<double>^) samples, index, countOfSamples );
                break;
            default:
                throw gcnew ArgumentException( "type" + sampleType->Name + " is not valid for sampleType parameter" );
            }
        }

        ///<summary>
        ///Writes some PCM audio <paramref name="samples"/> to the sound file.
        ///</summary>
        ///<remarks>
        ///<para>If the SndFile instance has been initialized to contain several channels, items are interleaved in the target file.</para>
        ///<para>The count of items to be written must be an integer product of the number of channels.</para>
        ///<para>
        ///It is important to note that the data type used by the calling program and the data format of the file
        /// do not need to be the same.
        /// For instance, it is possible to open a 16 bit PCM encoded WAV file and
        ///write samples of type Single.
        /// The library seamlessly converts between the two formats on-the-fly.
        ///</para>
        ///</remarks>
        ///<param name="sampleType">Type of the audio samples. This can be short, int, float or double</param>
        ///<param name="samples">Managed array to be written to the file.</param>
        ///<param name="countOfSamples">Count of items to write.</param>
        ///<returns>Count of items written (which should be the same as the <paramref name="countOfSamples"/> parameter).</returns>
        int Write( Array^ samples, Type^ sampleType, int countOfSamples)
        {
            return Write( samples, sampleType, 0, countOfSamples );
        }

        ///<summary>Gets total count of PCM sample frames in the sound file.</summary>
        ///<remarks>
        ///The total of PCM samples in the sound file is the product
        /// the channels count by the frames count.
        ///</remarks>
        ///<exception cref="SndFileException">Thrown if libsnfile function sf_command has returned an error</exception>
        property int FramesCount
        {
            int get()
            {
                SF_INFO sf_info;
                //int error;

                sf_command( sndfileHandle, SFC_GET_CURRENT_SF_INFO, &sf_info, sizeof(sf_info) );
                /*error = sf_error( sndfileHandle );

                if( error != SF_ERR_NO_ERROR )
                    throw gcnew SndFileException( error );*/

                return (int) sf_info.frames;
            }
        }

		SndFileError GetError()
		{
			return (SndFileError)sf_error(sndfileHandle);
		}

		sf_count_t Seek(sf_count_t frames, SndFileSeek whence)
		{
			return sf_seek(sndfileHandle, frames, (int)whence);
		}

		SndFile(byte* samplePointer, unsigned long sampleSize, int mode, SndFileInfo^ info) :
			_InnerFileStream()
		{
			SF_INFO sf_info;
			memset(&sf_info, 0, sizeof(sf_info));
			int _Flags;

			sndFileInfo = info;

			sf_info.samplerate = info->SamplesPerSecond;
			sf_info.channels = info->Channels;
			sf_info.format = info->Format;
			sf_info.frames = info->Frames;
			sf_info.sections = info->Sections;
			sf_info.seekable = info->Seekable;

			MemBufferFileIo wavMem(samplePointer, sampleSize);

			sndfileHandle = sf_open_virtual(&wavMem, mode, &sf_info, &wavMem);

			if (sndfileHandle == NULL)
			{
				//TODO: return a sndfile error code
				throw gcnew SndFileException(NULL);
			}

			if (mode == SFM_RDWR)
			{
				_Flags = _O_RDWR;
			}
			else if (mode == SFM_READ)
			{
				_Flags = _O_RDONLY;
			}
			else if (mode == SFM_WRITE)
			{
				_Flags = _O_WRONLY;
			}

			sndFileInfo->SamplesPerSecond = sf_info.samplerate;
			sndFileInfo->Channels = sf_info.channels;
			sndFileInfo->Format = sf_info.format;
			sndFileInfo->Frames = sf_info.frames;
			sndFileInfo->Sections = sf_info.sections;
			sndFileInfo->Seekable = sf_info.seekable;	
		}

		SndFile(
			[InAttribute][OutAttribute] MemBufferFileIo* sfvirtual,
			[InAttribute] int mode,
			[InAttribute][OutAttribute] SndFileInfo^ info,
			[InAttribute][OutAttribute] void* userdata) :
			_InnerFileStream()
		{
			SF_INFO sf_info;
			int _Flags;

			sndFileInfo = info;

			sf_info.samplerate = info->SamplesPerSecond;
			sf_info.channels = info->Channels;
			sf_info.format = info->Format;
			sf_info.frames = info->Frames;
			sf_info.sections = info->Sections;
			sf_info.seekable = info->Seekable;

			if (mode == SFM_RDWR)
			{
				_Flags = _O_RDWR;
			}
			else if (mode == SFM_READ)
			{
				_Flags = _O_RDONLY;
			}
			else if (mode == SFM_WRITE)
			{
				_Flags = _O_WRONLY;
			}

			sndfileHandle = sf_open_virtual(sfvirtual, mode, &sf_info, &userdata);

			if (sndfileHandle == NULL)
			{
				//TODO: return a sndfile error code
				throw gcnew SndFileException(NULL);
			}

			info->SamplesPerSecond = sf_info.samplerate;
			info->Channels = sf_info.channels;
			info->Format = sf_info.format;
			info->Frames = sf_info.frames;
			info->Sections = sf_info.sections;
			info->Seekable = sf_info.seekable;
		}

		static System::String^ SfErrorNumber(int code)
		{
			return gcnew String(string(sf_error_number(code)).c_str());
		}
    };

	
}
