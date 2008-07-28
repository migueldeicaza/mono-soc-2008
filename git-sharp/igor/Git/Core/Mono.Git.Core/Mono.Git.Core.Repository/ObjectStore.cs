// ObjectStore.cs
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Mono.Git.Core;

namespace Mono.Git.Core.Repository
{
	/// <summary>
	/// This class has all the information about the object store tipicaly
	/// .git/objects
	/// </summary>
	public class ObjectStore
	{
		Object[] objects;
		
		private Dictionary<SHA1, Object> cache;
		
		// Zlib first two bytes, these are the ones we are going to use writing
		// those bytes(78 and 1) mean that we are in a 32bit environment and using the
		// fastest deflate algorithm
		private static readonly byte ZlibFirstByte = 78;
		private static readonly byte ZlibSecondByte = 1;
		
		public string Path
		{
			get {
				return Directory.GetCurrentDirectory () + 
					System.IO.Path.AltDirectorySeparatorChar + ".git" +
					System.IO.Path.AltDirectorySeparatorChar + "objects";
			}
		}
		
		public Object Get (SHA1 key)
		{
			// TODO
			return cache[key];
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
		/// Problem: in order to compress with Zlib(C Git deflate library)
		/// we must add 2 bytes at the begining of this, those represent the type of
		/// algorithm used, it just simple extra data that just confused me for a while
		/// </summary>
		/// <param name="data">
		/// A <see cref="System.Byte"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Byte"/>
		/// </returns>
		private static byte[] AddZlibSignature (byte[] data)
		{
			List<byte> list = new List<byte> (data);
			
			list.Insert (0, ZlibFirstByte);
			list.Insert (1, ZlibSecondByte);
			
			return list.ToArray ();
		}
		
		/// <summary>
		/// Problem: in order to decompress with Zlib(C Git deflate library)
		/// we must remove 2 bytes at the begining of this, those represent the type of
		/// algorithm used, it just simple extra data that just confused me for a while	
		/// <param name="data">
		/// A <see cref="System.Byte"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Byte"/>
		/// </returns>
		private static byte[] RemoveZlibSignature (byte[] data)
		{
			List<byte> list = new List<byte> (data);
			
			list.RemoveRange (0, 2);
			
			return list.ToArray ();
		}
		
		public ObjectStore ()
		{
		}
		
		public static void Init ()
		{
			
		}
		
		public void Add (Object obj)
		{
			ArrayList array = new ArrayList (objects);
			
			switch (obj.Type) {
			case Type.Blob:
				array.Add ((Blob) obj);
				break;
			case Type.Commit:
				array.Add ((Commit) obj);
				break;
			case Type.Tag:
				array.Add ((Tag) obj);
				break;
			case Type.Tree:
				array.Add ((Tree) obj);
				break;
			}
			
			objects = (Object[]) array.ToArray ();
		}
		
		// FIXME: I don't know about this one
		public void Remove (Object obj)
		{
			ArrayList array = new ArrayList (objects);
			
			array.Remove (obj);
			
			objects = (Object[]) array.ToArray ();
		}
		
		public static byte[] GetFileContent (string path)
		{
			FileStream fs = File.Open (path, FileMode.Open);
			
			return new BinaryReader (fs).ReadBytes ((int) fs.Length);
		}
		
//		public Object GetObjectById (SHA1 objIdSHA1)
//		{
//			return GetObjectById (objIdSHA1.bytes);
//		}
//		
//		public Object GetObjectById (byte[] objIdBytes)
//		{
//			foreach (Object obj in objects) {
//				if (obj.Id.bytes == objIdBytes)
//					return obj;
//			}
//			
//			return null;
//		}
//		
//		public Object GetOjectById (string objIdSHA1)
//		{
//			foreach (Object obj in objects) {
//				if (Object.BytesToHexString (obj.Id.bytes) == objIdSHA1)
//					return obj;
//			}
//			
//			return null;
//		}
	}
}
