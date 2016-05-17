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
	public enum class SndFileError 
	{
		NoError = 0,
		UnrecognisedFormat = 1,
		System = 2,
		MalformedFile = 3,
		UnsupportedEncoding = 4
	};

	[SerializableAttribute]
	[ComVisibleAttribute(true)]
	public enum class SndFileSeek
	{
		Set = 0,
		Current = 1,
		End = 2
	};
}