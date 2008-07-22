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
			bytes = HashObj (obj, type);
		}
		
		public override int GetHashCode ()
		{
			byte[] tmp = new byte [4];
			
			for (int i = 0; i < 4; i++)
				tmp[i] = bytes[i];
			
			return BitConverter.ToInt32 (tmp, 0);
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
		/// Create a header for a Git hash
		/// </summary>
		/// <param name="objType">
		/// Type of the object to hash<see cref="Type"/>
		/// </param>
		/// <param name="dataSize">
		/// Size of the object to hash<see cref="System.Int32"/>
		/// </param>
		/// <returns>
		/// Header<see cref="System.Byte"/>
		/// </returns>
		private static byte[] CreateHashHeader (Type objType, int dataSize)
		{
			return Encoding.Default.GetBytes (String.Format ("{0} {1}\0",
			                                                 Object.TypeToString (objType),
			                                                 dataSize.ToString ()));
		}
		
		/// <summary>
		/// Convert a Byte array to Hexadecimal format
		/// </summary>
		/// <param name="bytes">
		/// A byte array<see cref="System.Byte"/>
		/// </param>
		/// <returns>
		/// A String of a byte array converted to Hexadecimial format<see cref="System.String"/>
		/// </returns>
		public static string BytesToHexString (byte[] data)
		{
			StringBuilder hexString = new StringBuilder (data.Length);
			
			foreach (byte b in data) {
				hexString.Append (b.ToString ("x2"));
			}
			
			return hexString.ToString ();
		}
		
		/// <summary>
		/// Convert a Byte array to string
		/// </summary>
		/// <param name="bytes">
		/// A byte array<see cref="System.Byte"/>
		/// </param>
		/// <returns>
		/// A String of a byte array converted to String format<see cref="System.String"/>
		/// </returns>
		public static string BytesToString (byte[] data)
		{
			StringBuilder str = new StringBuilder (data.Length);
			
			foreach (byte b in data) {
				str.Append (b.ToString ());
			}
			
			return str.ToString ();
		}
		
		/// <summary>
		/// Hash a single file by a given Filename
		/// </summary>
		/// <param name="filename">
		/// A file path<see cref="System.String"/>
		/// </param>
		/// <returns>
		/// A sha1 hash<see cref="System.Byte"/>
		/// </returns>
		private static byte[] HashObj (byte[] objContent, Type objType)
		{
			byte[] header;
			byte[] data;
			
			header = CreateHashHeader (objType, objContent.Length);
			data = new byte[header.Length + objContent.Length];
			
			header.CopyTo (data, 0);
			objContent.CopyTo (data, header.Length);
			
			return ComputeSHA1Hash (data);
		}
	}
}
