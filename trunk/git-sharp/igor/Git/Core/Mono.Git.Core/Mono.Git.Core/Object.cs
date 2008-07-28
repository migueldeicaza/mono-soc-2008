// Object.cs
//
// Authors:
//   Hector E. Gomez <hectoregm@gmail.com>
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
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace Mono.Git.Core
{
	/// <summary>
	/// Contains the object types of Git
	/// </summary>
	public enum Type
	{
		Blob,
		Tree,
		Commit,
		Tag
	}
	
	/// <summary>
	/// Class that holds the basic object information
	/// </summary>
	public abstract class Object
	{
		protected SHA1 id;
		// it represent the object uncompressed content
		protected byte[] content;
		
		public abstract Type Type { get; }
		
		public SHA1 Id
		{
			get {
				return id;
			}
		}
		
		public Object ()
		{
		}
		
		public Object (Type type, byte[] objContent)
		{
			content = objContent;
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
		private static byte[] CreateObjectHeader (Type objType, int dataSize)
		{
			return Encoding.ASCII.GetBytes (String.Format ("{0} {1}\0",
			                                                 Object.TypeToString (objType),
			                                                 dataSize.ToString ()));
		}
		
//		/// <summary>
//		/// TODO: 
//		/// </summary>
//		/// <param name="filePath">
//		/// A <see cref="System.String"/>
//		/// </param>
//		public static void Write (string filePath)
//		{
//			Blob b = new Blob (filePath);
//			b.AddContent (filePath);
//			
//			//FileStream f = new FileStream (b.ToHexString (), FileMode.Create);
//			//FileStream f = new FileStream ("hello.object", FileMode.Create);
//			
//			byte[] bytesHeader;
//			//bytesHeader = Object.CreateHashHeader (Mono.Git.Core.Type.Blob, b.Content.Length);
//			
//			//f.Write (bytesHeader, 0, bytesHeader.Length);
//			
//			// Reading the file content
//			FileStream st = new FileStream (filePath, FileMode.Open);
//			FileInfo fileInfo = new FileInfo (filePath);
//			BinaryReader br = new BinaryReader (st);
//			
//			byte[] data = new byte[bytesHeader.Length + fileInfo.Length];
//			bytesHeader.CopyTo (data, 0); 
//			br.ReadBytes (Convert.ToInt32 (fileInfo.Length.ToString ())).CopyTo (data, bytesHeader.Length);
//			
//			//f.Write (data, 0, data.Length);
//			
//			MemoryStream ms = new MemoryStream ();
//			DeflateStream ds = new DeflateStream (ms, CompressionMode.Compress, true);
//			ds.Write (data, 0, data.Length);
//			ds.Close ();
//			
//			Console.WriteLine ("Original size {0}, Compressed size {1}", data.Length, ms.Length);
//			
//			f.Write (ms.GetBuffer (), 0, ms.GetBuffer ().Length);
//			
//			f.Close ();
//		}
//		
//		/// <summary>
//		/// TODO: 
//		/// </summary>
//		/// <param name="filePath">
//		/// A <see cref="System.String"/>
//		/// </param>
//		public static void Read (string filePath)
//		{
//			FileStream fs = new FileStream (filePath, FileMode.Open);
//			BinaryReader br = new BinaryReader (fs);
//			
//			//byte[] data = br.ReadBytes (Convert.ToInt32 (fs.Length));
//			
//			//Console.WriteLine (Object.BytesToString (data));
//		}
		
		// These methods are to parse different objects from a input byte array
		
		protected static bool ParseNewLine (byte[] input, ref int pos)
		{
			if (input.Length == pos)
				return false;
			
			if ((char)input[pos++] == '\n')
				return true;
			else {
				pos--;
				return false;
			}
		}
		
		protected static bool ParseZero (byte[] input, ref int pos)
		{
			if (input.Length == pos)
				return false;
			
			if ((char)input[pos++] == '\0')
				return true;
			else {
				pos--;
				return false;
			}
		}
		
		protected static bool ParseSpace (byte[] input, ref int pos)
		{
			if (input.Length == pos)
				return false;
			
			if ((char)input[pos++] == ' ')
				return true;
			else {
				pos--;
				return false;
			}
		}
		
		protected static bool ParseString (byte[] input, ref int pos, out string str)
		{
			StringBuilder sb = new StringBuilder ();
			str = null;
			
			if (input.Length == pos)
				return false;
			
			pos++;
			
			while (!ParseNewLine (input, ref pos) || !ParseZero (input, ref pos) || pos > input.Length)
				sb.Append ((char)input[pos++]);
			
			str = sb.ToString ();
			
			return true;
		}
		
		protected static bool ParseNoSpaceString (byte[] input, ref int pos, out string str)
		{
			StringBuilder sb = new StringBuilder ();
			str = null;
			
			if (input.Length == pos)
				return false;
			
			pos++;
			
			while (!ParseNewLine (input, ref pos) || !ParseSpace (input, ref pos) || !ParseZero (input, ref pos) || pos > input.Length)
				sb.Append ((char)input[pos++]);
			
			str = sb.ToString ();
			
			return true;
		}
		
		protected bool ParseHeader (byte[] input, ref int pos, out byte[] header)
		{
			header = null;
			
			if (pos != 0)
				return false;
			
			for (int i = 0; input[i] != '\0'; i++) {
				header[i] = input[i];
				pos++;
			}
			
			return true;
		}
		
		
		protected bool ParseType (byte[] input, ref int pos, out string type)
		{
			type = null;
			
			if (pos != 0)
				return false;
			
			if (!ParseNoSpaceString (input, ref pos, out type))
				return false;
			
			foreach (string s in new string[] {"blob", "tree", "tag", "commit"}) {
				if (type == s)
					return true;
			}
			
			return false;
		}
		
		protected static bool ParseInteger (byte[] input, ref int pos, out int integer)
		{
			integer = 0;
			
			if (pos >= (input.Length - 4))
				return false;
			
			integer += ((int)input[pos++]) | (((int)input[pos++]) << 8) | (((int)input[pos++]) << 16) | (((int)input[pos++]) << 24);
			//integer = BitConverter.ToInt32 (input, pos);
			// Convert 4 bytes to int... the mathematical way
			//integer += input[pos++] * (int) System.Math.Pow (256, 0);
			//integer += input[pos++] * (int) System.Math.Pow (256, 1);
			//integer += input[pos++] * (int) System.Math.Pow (256, 2);
			//integer += input[pos++] * (int) System.Math.Pow (256, 3);
			
			return true;
		}
		
		protected static void EncodeString (ref MemoryStream ms, string str)
		{
			BinaryWriter bw = new BinaryWriter (ms);
			bw.Write (str);
			
			bw.Close ();
		}
		
		protected static void EncodeHeader (ref MemoryStream ms, Type type, int len)
		{
			EncodeHeader (ref ms, CreateObjectHeader (type, len));
		}
		
		protected static void EncodeHeader (ref MemoryStream ms, byte[] header)
		{
			BinaryWriter bw = new BinaryWriter (ms);
			bw.Write (header);
			
			bw.Close ();
		}
		
		protected static void EncodeInteger (ref MemoryStream ms, int integer)
		{
			BinaryWriter bw = new BinaryWriter (ms);
			bw.Write (integer);
			
			bw.Close ();
		}
		
		protected static void EncodeZero (ref MemoryStream ms)
		{
			BinaryWriter bw = new BinaryWriter (ms);
			bw.Write ('\0');
			
			bw.Close ();
		}
		
		protected static void EncodeNewLine (ref MemoryStream ms)
		{
			BinaryWriter bw = new BinaryWriter (ms);
			bw.Write ('\n');
			
			bw.Close ();
		}
		
		protected static void EncodeSpace (ref MemoryStream ms)
		{
			BinaryWriter bw = new BinaryWriter (ms);
			bw.Write (' ');
			
			bw.Close ();
		}
		
		/// <summary>
		/// Convert a type to its string representation
		/// </summary>
		/// <param name="t">
		/// A <see cref="Type"/>
		/// </param>
		public static string TypeToString (Type t)
		{
			return t.ToString ().ToLower ();
		}
	}
}
