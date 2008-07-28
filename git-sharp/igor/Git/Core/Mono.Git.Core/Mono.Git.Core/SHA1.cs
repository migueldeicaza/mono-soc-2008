// SHA1.cs
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
using System.Text;

namespace Mono.Git.Core
{
	
	/// <summary>
	/// Struct that represent a SHA1 hash
	/// </summary>
	public struct SHA1
	{
		private byte[] bytes;
		
		public byte[] Bytes { get { return bytes; } }
		
		public SHA1 (byte[] obj, Type type)
		{
			//bytes = HashObj (obj, type);
			bytes = obj;
		}
		
		public override int GetHashCode ()
		{
			return ((int)bytes[0]) | (((int)bytes[1]) << 8) | (((int)bytes[2]) << 16) | (((int)bytes[3]) << 24);
		}
		
		/// <summary>
		/// Compute the byte array to a SHA1 hash
		/// </summary>
		/// <param name="bytes">
		/// A byte array to convert<see cref="System.Byte"/>
		/// </param>
		/// <returns>
		/// A SHA1 byte array<see cref="System.Byte"/>
		/// </returns>
		private static byte[] ComputeSHA1Hash (byte[] bytes)
		{
			return System.Security.Cryptography.SHA1.Create ().ComputeHash (bytes);
		}
		
		/// <summary>
		/// Convert a Byte array to Hexadecimal format
		/// </summary>
		/// <returns>
		/// A String of a byte array converted to Hexadecimial format<see cref="System.String"/>
		/// </returns>
		public string ToHexString (int start, int size)
		{
			// Final length of the string will have 2 chars for every byte
			StringBuilder hexString = new StringBuilder (size * 2);
			
			for (int i = start; i < (start + size); i++)
				hexString.Append (bytes[i].ToString ("x2"));
			
			return hexString.ToString ();
		}
		
		public string ToHexString ()
		{
			// FIXME: I don't know about calculating the Length... isn't it always 20?
			return ToHexString (0, bytes.Length);
		}
		
		public string GetGitFileName ()
		{
			// FIXME: Again... is it always 20? I believe it should
			return ToHexString (0, 2) + "/" + ToHexString (2, bytes.Length);
		}
		
		public bool Equals (SHA1 o)
		{
			if (o.Bytes == null)
				return false;
			
			if (o.Bytes.Length == 0)
				return false;
			
			if (bytes == o.Bytes)
				return true;
			
			for (int i = 0; i < o.Bytes.Length; i++) {
				if (bytes[i] != o.Bytes[i]) {
					return false;
				}
			}
			
			return true;
		}
		
	}
}
