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


namespace LibSndFile {


    [SerializableAttribute]
    [ComVisibleAttribute(true)]
    ///<summary>
    ///Exception that is thrown when a libsndfile native function returned with error
    ///</summary>
    public ref class SndFileException: ApplicationException
    {
    public:
        ///<summary>Error code returned by the failing libsndfile native function</summary>
        const int ErrorCode;

        SndFileException( int sndfileErrorCode )
            : ApplicationException( gcnew String(sf_error_number(sndfileErrorCode)) ),
            ErrorCode( sndfileErrorCode ){}
    };
}
