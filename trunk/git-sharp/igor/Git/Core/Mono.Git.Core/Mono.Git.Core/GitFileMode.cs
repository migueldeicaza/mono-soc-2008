// FileMode.cs
//
// Author:
//   Igor Guerrero Fonseca <igfgt1@gmail.com>
//
// Copyright (c) 2008
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using System.IO;

namespace Mono.Git.Core
{
	/// <summary>
	/// Enumeration with all the file modes that git handles
	/// </summary>
//	public enum GitFileModeTypes
//	{
//		Tree,
//		Symlink,
//		RegularFile,
//		ExecutableFile,
//	}
	
	public class GitFileMode
	{
		private byte[] bytes;
		private int mode_bits; // mode, in _human_ (e.g. 755 == excecutable file)
		
		// Git Constants(mostly the meaning of this class)
		public static GitFileMode Tree = new GitFileMode (040000);
		public static GitFileMode Symlink = new GitFileMode(0120000);
		public static GitFileMode RegularFile = new GitFileMode(0100644);
		public static GitFileMode ExecutableFile = new GitFileMode(0100755);
		
		/// <summary>
		/// Compares a _human_ filemode in a type to see if its equal
		/// </summary>
		/// <param name="modeBits">
		/// A mode<see cref="System.Int32"/>
		/// </param>
		/// <param name="type">
		/// A type of file<see cref="GitFileModeTypes"/>
		/// </param>
		/// <returns>
		/// A true if are equal false if aren't<see cref="System.Boolean"/>
		/// </returns>
//		public bool Equals (GitFileModeTypes type)
//		{
//			switch (type) {
//			case GitFileModeTypes.Tree:
//				return (mode_bits & 040000) == 040000;
//			case GitFileModeTypes.Symlink:
//				return (mode_bits & 020000) == 020000;
//			case GitFileModeTypes.RegularFile:
//				return (mode_bits & 0100000) == 0100000 && (mode_bits & 0111) == 0;
//			case GitFileModeTypes.ExecutableFile:
//				return (mode_bits & 0100000) == 0100000 && (mode_bits & 0111) != 0;
//			}
//			
//			// FIXME: Using this to avoid: not all code paths return a value(CS0161)
//			return false; 
//		}
		
		public byte[] Bytes { get { return bytes; } }
		
		public GitFileMode (byte[] mode)
		{
			
		}
		
		/// <summary>
		/// Set the filemode from an int representation to an octal representation 
		/// </summary>
		/// <param name="mode">
		/// A filemode<see cref="System.Int32"/>
		/// </param>
		public GitFileMode (int mode)
		{
			mode_bits = mode;
			
			if (mode == 0) {
				bytes = new byte[] { (byte) '0' };
				return;
			}
			
			byte[] tmp = new byte[10];
			int size = tmp.Length;
			
			while (mode != 0) {
				tmp[size--] = (byte) ('0' + (mode & 07));
				mode >>= 3;
			}
			
			bytes = new byte[tmp.Length - size];
			
			for (int i = 0; i < bytes.Length; i++) {
				bytes[i] = tmp[size + i];
			}
		}
	}
}
