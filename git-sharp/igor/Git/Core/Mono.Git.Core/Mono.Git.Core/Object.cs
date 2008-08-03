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
		private SHA1 id;
		// it represent the object uncompressed content
		private byte[] content;
		
		public abstract Type Type { get; }
		
		public SHA1 Id { get { return id; } }
		public byte[] Content { get { return content; } }
		
		public Object (Type type, byte[] content)
		{
			byte[] header = CreateObjectHeader (type, (content.Length - 1).ToString ());
			this.content = new byte[header.Length + (content.Length - 1)];
			
			// filling the content
			Array.Copy (header, 0, this.content, 0, header.Length); // Copying the header first,
			Array.Copy (content, 0, this.content, header.Length - 1, content.Length); // then the data, on the final data
			
			// initializing the id with the content
			id = new SHA1 (this.content, true);
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
		private static byte[] CreateObjectHeader (Type objType, string dataSize)
		{
			return Encoding.UTF8.GetBytes (String.Format ("{0} {1}\0",
			                                                 Object.TypeToString (objType),
			                                                 dataSize));
		}
		
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
			int index = pos;
			str = null;
			
			if (input.Length == pos)
				return false;
			
			while ((input [index] != (byte)'\n') && (input [index] != (byte)0))
			{
				if (index < input.Length) {
					index++;
				} else {
					return false;
				}
			}
			
			int length = index - pos;
			if (length == 0)
			{
				return false;
			}
			
			str = Encoding.UTF8.GetString (input, pos, length);
			pos = index;
			
 			return true;
		}
		
		protected static bool ParseNoSpaceString (byte[] input, ref int pos, out string str)
		{
			int index = pos;
			str = null;
			
			if (input.Length == pos)
				return false;
			
			while ((input [index] != (byte)'\n') && (input [index] != (byte)0) && (input [index] != (byte)' '))
			{
				if (index < input.Length) {
					index++;
				} else {
					return false;
				}
			}
			
			int length = index - pos;
			if (length == 0)
			{
				return false;
			}
			
			str = Encoding.UTF8.GetString (input, pos, length);
			pos = index;
			
 			return true;
		}
		protected static bool ParseHeader (byte[] input, ref int pos, out Type type, out string length)
		{
			// FIXME: I'm getting an error if I don't asign these parameters, this is because
			// I get out the method before asigned anything to those parameters
			length = null;
			//type = Type.Blob;
			
			// Here I get out of the method
			if (!ParseType (input, ref pos, out type))
				return false;
			
			if (!ParseNoSpaceString (input, ref pos, out length))
				return false;
			
			return true;
		}
		
		protected static bool ParseType (byte[] input, ref int pos, out Type type)
		{
			string decodedType;
			type = Type.Blob; // we need a default
			
			if (pos != 0)
				return false;
			
			if (!ParseNoSpaceString (input, ref pos, out decodedType))
				return false;
			
			pos ++;
			
			switch (decodedType) {
			case "blob":
				type = Type.Blob;
				return true;
			case "tree":
				type = Type.Tree;
				return true;
			case "commit":
				type = Type.Commit;
				return true;
				break;
			case "tag":
				type = Type.Tag;
				return true;
				break;
			}
			
			return true;
		}
		
		protected static bool ParseInteger (byte[] input, ref int pos, out int integer)
		{
			integer = 0;
			
			if (pos >= (input.Length - 4))
				return false;
			
			integer += ((int)input[pos++]) | (((int)input[pos++]) << 8) | (((int)input[pos++]) << 16) | (((int)input[pos++]) << 24);
			
			// Here you added pos += 4, why? I don't see the point on doing that
			
			return true;
		}
		
		protected static void EncodeString (ref MemoryStream ms, string str)
		{
			BinaryWriter bw = new BinaryWriter (ms);
			bw.Write (Encoding.UTF8.GetBytes (str));
			
			bw.Close ();
		}
		
		protected static void EncodeHeader (ref MemoryStream ms, Type type, string length)
		{
			EncodeHeader (ref ms, CreateObjectHeader (type, length));
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
			ms.WriteByte ((byte)'\0');
		}
		
		protected static void EncodeNewLine (ref MemoryStream ms)
		{
			ms.WriteByte ((byte)'\n');
		}
		
		protected static void EncodeSpace (ref MemoryStream ms)
		{
			ms.WriteByte ((byte)' ');
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
		
		public static Type StringToType (string type)
		{
			switch (type) {
			case "blob":
				return Mono.Git.Core.Type.Blob;
				break;
			case "tree":
				return Mono.Git.Core.Type.Tree;
				break;
			case "commit":
				return Mono.Git.Core.Type.Commit;
				break;
			case "tag":
				return Mono.Git.Core.Type.Tag;
				break;
			}
			
			return Mono.Git.Core.Type.Blob;
		}
		
		public static Object DecodeObject (byte[] content)
		{
			Type type;
			string length;
			int pos = 0;
			
			ParseHeader (content, ref pos, out type, out length);
			
			switch (type) {
			case Type.Blob:
				byte[] blobContent = new byte[(content.Length) - pos];
				Array.Copy (content, pos, blobContent, 0, blobContent.Length);
				return new Blob (blobContent);
			case Type.Tree:
				//TODO: return new Tree (contents);
				break;
			case Type.Tag:
				//TOOD: return new Blob (contents);
				break;
			case Type.Commit:
				//TODO: return new Blob (contents);
				break;
			}
			
			// This is to ensure that all the code paths return a value
			byte[] blobBytes = new byte[content.Length - pos];
			content.CopyTo (blobBytes, pos);
			return new Blob (blobBytes);
		}
		
		protected abstract byte[] Decode ();
		protected abstract void Encode (byte[] data);
	}
}
