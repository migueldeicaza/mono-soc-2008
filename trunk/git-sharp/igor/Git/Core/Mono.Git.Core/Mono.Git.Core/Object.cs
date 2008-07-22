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
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
		
		/// <summary>
		/// Compress a byte array
		/// </summary>
		/// <param name="data">
		/// A data byte array<see cref="System.Byte"/>
		/// </param>
		/// <returns>
		/// A compressed byte array<see cref="System.Byte"/>
		/// </returns>
		public static byte[] Compress (byte[] data)
		{
			MemoryStream ms = new MemoryStream ();
			DeflateStream ds = new DeflateStream (ms, CompressionMode.Compress);
			
			ds.Write (data, 0, data.Length);
			ds.Flush ();
			ds.Close ();
			
			return ms.ToArray ();
		}
		
		/// <summary>
		/// Decompress a byte array
		/// </summary>
		/// <param name="data">
		/// A data byte array<see cref="System.Byte"/>
		/// </param>
		/// <returns>
		/// A decompressed byte array<see cref="System.Byte"/>
		/// </returns>
		public static byte[] Decompress(byte[] data)
		{
			const int BUFFER_SIZE = 256;
			byte[] tempArray = new byte[BUFFER_SIZE];
			List<byte[]> tempList = new List<byte[]>();
			int count = 0, length = 0;
			
			MemoryStream ms = new MemoryStream (data);
			DeflateStream ds = new DeflateStream (ms, CompressionMode.Decompress);
			
			while ((count = ds.Read (tempArray, 0, BUFFER_SIZE)) > 0) {
				if (count == BUFFER_SIZE) {
					tempList.Add (tempArray);
					tempArray = new byte[BUFFER_SIZE];
				} else {
					byte[] temp = new byte[count];
					Array.Copy (tempArray, 0, temp, 0, count);
					tempList.Add (temp);
				}
				length += count;
			}
			
			byte[] retVal = new byte[length];
			
			count = 0;
			foreach (byte[] temp in tempList) {
				Array.Copy (temp, 0, retVal, count, temp.Length);
				count += temp.Length;
			}
			
			return retVal;
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
		
		/// <summary>
		/// Convert a type to its string representation
		/// </summary>
		/// <param name="t">
		/// A <see cref="Type"/>
		/// </param>
		public static string TypeToString(Type t)
		{
			return t.ToString ().ToLower ();
		}
	}
}
