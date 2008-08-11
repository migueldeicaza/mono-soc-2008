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
	[Flags]
	public enum IndividualFileType
	{
		Zero = 0,
		File = 1,
		SymLink = 2,
		Directory = 4
	}
	
	[Flags]
	public enum IndividualFileMode
	{
		None = 0,
		Read = 4,
		Write = 2,
		Execute = 1,
		ReadExecute = 5,
		ReadWrite = 6,
		All = 7
	}
	
	public struct GitFileMode
	{
		private IndividualFileType file_type;
		private IndividualFileType sym_link;
		private IndividualFileType zero;
		private IndividualFileMode user;
		private IndividualFileMode group;
		private IndividualFileMode other;
		
		public GitFileMode (byte[] mode)
		{
			if ((char) mode[0] != '4') {
				file_type = (IndividualFileType) ((char) mode[0]);
				sym_link = (IndividualFileType) ((char) mode[1]);
				zero = (IndividualFileType) ((char) mode[2]);
				user = (IndividualFileMode) ((char) mode[3]);
				group = (IndividualFileMode) ((char) mode[4]);
				other = (IndividualFileMode) ((char) mode[5]);
			} else {
				file_type = (IndividualFileType) ((char) mode[0]);
				sym_link = (IndividualFileType) ((char) mode[1]);
				zero = IndividualFileType.Zero;
				user = (IndividualFileMode) ((char) mode[2]);
				group = (IndividualFileMode) ((char) mode[3]);
				other = (IndividualFileMode) ((char) mode[4]);
			}
		}
		
		public byte[] ModeBits {
			get {
				if (file_type == IndividualFileType.Directory) {
					return new byte[] {(byte) file_type, (byte) sym_link, (byte) user, (byte) group, (byte) other};
				}
				return new byte[] {(byte) file_type, (byte) sym_link, (byte) zero, (byte) user, (byte) group, (byte) other};
			}
		}
		
		public override string ToString ()
		{
			if (file_type == IndividualFileType.Directory) {
				Console.WriteLine ("we go here?");
				return String.Format ("0{0}{1}{2}{3}{4}", (char) file_type, (char) sym_link, (char) user, (char) group, (char) other);
			}
			
			return String.Format ("{0}{1}{2}{3}{4}{5}", (char) file_type, (char) sym_link, (char) zero, (char) user, (char) group, (char) other);
		}
	}
//	
//	public class GitFileMode
//	{
//		private byte[] bytes;
//		private int mode_bits; // mode, in _human_ (e.g. 755 == excecutable file)
//		
//		// Git Constants(mostly the meaning of this class)
//		public static GitFileMode Tree = new GitFileMode (040000);
//		public static GitFileMode Symlink = new GitFileMode(0120000);
//		public static GitFileMode RegularFile = new GitFileMode(0100644);
//		public static GitFileMode ExecutableFile = new GitFileMode(0100755);
//		
//		/// <summary>
//		/// Compares a _human_ filemode in a type to see if its equal
//		/// </summary>
//		/// <param name="modeBits">
//		/// A mode<see cref="System.Int32"/>
//		/// </param>
//		/// <param name="type">
//		/// A type of file<see cref="GitFileModeTypes"/>
//		/// </param>
//		/// <returns>
//		/// A true if are equal false if aren't<see cref="System.Boolean"/>
//		/// </returns>
////		public bool Equals (GitFileModeTypes type)
////		{
////			switch (type) {
////			case GitFileModeTypes.Tree:
////				return (mode_bits & 040000) == 040000;
////			case GitFileModeTypes.Symlink:
////				return (mode_bits & 020000) == 020000;
////			case GitFileModeTypes.RegularFile:
////				return (mode_bits & 0100000) == 0100000 && (mode_bits & 0111) == 0;
////			case GitFileModeTypes.ExecutableFile:
////				return (mode_bits & 0100000) == 0100000 && (mode_bits & 0111) != 0;
////			}
////			
////			// FIXME: Using this to avoid: not all code paths return a value(CS0161)
////			return false; 
////		}
//		
//		public byte[] Bytes { get { return bytes; } }
//		
//		public GitFileMode (byte[] mode)
//		{
//			
//		}
//		
//		/// <summary>
//		/// Set the filemode from an int representation to an octal representation 
//		/// </summary>
//		/// <param name="mode">
//		/// A filemode<see cref="System.Int32"/>
//		/// </param>
//		public GitFileMode (int mode)
//		{
//			mode_bits = mode;
//			
//			if (mode == 0) {
//				bytes = new byte[] { (byte) '0' };
//				return;
//			}
//			
//			byte[] tmp = new byte[10];
//			int size = tmp.Length;
//			
//			while (mode != 0) {
//				tmp[size--] = (byte) ('0' + (mode & 07));
//				mode >>= 3;
//			}
//			
//			bytes = new byte[tmp.Length - size];
//			
//			for (int i = 0; i < bytes.Length; i++) {
//				bytes[i] = tmp[size + i];
//			}
//		}
//	}
}
