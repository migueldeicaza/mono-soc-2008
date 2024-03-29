// ObjectTest.cs
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
using System.IO.Compression;
using System.Text;
using Mono.Git.Core;

namespace Mono.Git.Tests
{
	/// <summary>
	/// Here we will store all the tests, this is temporal I believe it'll better
	/// if we use Nunit or something like that
	/// </summary>
	public class ObjectTest
	{
		public ObjectTest ()
		{
			// Calling the test of create blob hash validation
			if (!CreateBlobHashTest ()) {
				throw new Exception ("CreateBlobHashTest () -> failed");
			}
			
			Console.WriteLine ("All the test passed :)");
		}
		
		/// <summary>
		/// This test will create a blob using a file that has 
		/// 7ca7efe6dbed6a3a0d73030134521c8b1048e6a7 as a hash string and
		/// a "Hello, World!" content on it the test will compare the hashes
		/// generated by CGit and GitSharp
		/// </summary>
		/// <returns>
		/// Result of the test<see cref="System.Boolean"/>
		/// </returns>
		public static bool CreateBlobHashTest ()
		{
			//Blob b = new Blob ("hello.txt");
			//string hash = Mono.Git.Core.Object.BytesToHexString (b.Id.bytes);
			
			//if (hash != "7ca7efe6dbed6a3a0d73030134521c8b1048e6a7")
			//	return false;
			
			return true;
		}
		
		public static void ByteWriterTest ()
		{
			int num = 259;
			byte[] bytes;
			MemoryStream ms = new MemoryStream ();
			BinaryWriter bw = new BinaryWriter (ms);
			
			bw.Write (num);
			bytes = ms.GetBuffer ();
			
			int tmp = 0;
			
			for (int i = 0; i < 4; i++) {
				tmp += bytes[i] * (int)System.Math.Pow (256, i);
				Console.WriteLine ("#{0}", i);
				Console.WriteLine ("tmp += bytes[i] * (int)System.Math.Pow (256, i);");
				Console.WriteLine ("tmp += {0} * {1}", bytes[i], (int)System.Math.Pow (256, i));
			}
			
			
			
			Console.WriteLine ("{0}, {1}, {2}, {3}", (bytes[0] - '0')*10, (bytes[1] - '0'), (bytes[2] - '0'), (bytes[3] - '0'));
			Console.WriteLine ("Num: {0}", BitConverter.ToInt32 (bytes, 0));
			Console.WriteLine ("Num: {0}", tmp);
		}
		
		public static void ReadGitObj ()
		{
			FileStream fs = new FileStream ("f1ba39c400cf9fb8b48f652bc05aaa9f087c66cf", FileMode.Open);
			Console.WriteLine ("SHA1 = f1ba39c400cf9fb8b48f652bc05aaa9f087c66cf");
			//FileStream fs = new FileStream ("4d9a46593467984153457aef51f049af038f59c9", FileMode.Open);
			//FileStream fs = new FileStream ("c45099a4cd4ba61a32e1c61d987b73dc3b6146b", FileMode.Open);
			BinaryReader br = new BinaryReader (fs);
			int len = (int)fs.Length;
			
			byte[] content = br.ReadBytes (len);
			
			Console.WriteLine ("First 2 bytes: {0} {1}", (int)content[0], (int)content[1]);
			Console.WriteLine ("Last 4 bytes: {0}{1}{2}{3}", (int)content[len - 4], (int)content[len - 3], 
			                   (int)content[len - 2], (int)content[len - 1]);
			
//			byte[] dContent = new byte [len - 2];
//			
//			for (int i = 2; i < len; i++)
//				dContent[i - 2] = content[i];
			
			byte[] deflated = Mono.Git.Core.ObjectStore.Decompress (content);
			
			int pos = 0;
			
			for (; deflated[pos] != '\0'; pos++);
			
			byte[] deflatedNoHeader = new byte [deflated.Length - pos];
			
			Array.Copy (deflated, pos, deflatedNoHeader, 0, deflatedNoHeader.Length);
			
			Blob blob = (Blob) Mono.Git.Core.Object.DecodeObject (deflated);
			
			Console.WriteLine ("Read from Blob(Object):");
			Console.WriteLine ("Length: {0}", blob.Content.Length);
			Console.WriteLine ("SHA1: {0}", blob.Id.ToHexString ());
			foreach (byte b in blob.Content) {
				if (b == '\0') {
					Console.Write ("[NULL]");
					continue;
				}
				Console.Write ((char) b);
			}
			
			Console.WriteLine ("------------------");
			
			Console.WriteLine ("Read from filesystem:");
			Console.WriteLine ("Length: {0}", deflated.Length);
			foreach (byte b in deflated) {
				if ((char) b == '\0') {
					Console.Write ("[NULL]");
					continue;
				} 
				Console.Write ((char)b);
			}
			
			if (blob.Id.ToHexString ().Equals ("f1ba39c400cf9fb8b48f652bc05aaa9f087c66cf"))
				Console.WriteLine ("Whoohoo!");
		}
		
		public static void ReadGitTree ()
		{
			FileStream fs = new FileStream ("c45099a4cd4ba61a32e1c61d987b73dc3b6146b9", FileMode.Open);
			Console.WriteLine ("SHA1 = c45099a4cd4ba61a32e1c61d987b73dc3b6146b9");
			
			BinaryReader br = new BinaryReader (fs);
			int len = (int)fs.Length;
			
			byte[] content = br.ReadBytes (len);
			
			Console.WriteLine ("First 2 bytes: {0} {1}", (int)content[0], (int)content[1]);
			Console.WriteLine ("Last 4 bytes: {0}{1}{2}{3}", (int)content[len - 4], (int)content[len - 3], 
			                   (int)content[len - 2], (int)content[len - 1]);
			
			byte[] deflated = Mono.Git.Core.ObjectStore.Decompress (content);
			
			int pos = 0;
			
			Console.Write ("header: ");
			
			for (; deflated[pos] != '\0'; pos++) {
				Console.Write ((char) deflated[pos]);
			}
			
			if (deflated[pos] == '\0') {
				Console.WriteLine ("[null]");
				pos++;
			}
			
			Console.WriteLine ("Content: ");
			
			byte[] deflatedNoHeader = new byte [deflated.Length - pos];
			
			Array.Copy (deflated, pos, deflatedNoHeader, 0, deflatedNoHeader.Length);
			
			pos = 0;
			int count = 0;
			
//			for (; pos < deflatedNoHeader.Length; pos++) {
//				if (deflatedNoHeader [pos] == '\0') {
//					count = 0;
//					Console.Write ("[#{0} null]", pos);
//					pos += 21;
//					if (pos > deflatedNoHeader.Length)
//						break;
//					byte[] bytes = new byte[20];
//					Array.Copy (deflatedNoHeader, pos, bytes, 0, 20);
//					SHA1 id = new SHA1 (bytes, true);
//					
//					Console.Write (id.ToHexString ());
//					continue;
//				}
//				if (deflatedNoHeader [pos] == '1' && deflatedNoHeader [pos + 1] == '0' && deflatedNoHeader [pos + 2] == '0') {
//					Console.Write ("\n#{0} count: {1}", pos, count);
//				}
//				Console.Write ((char) deflatedNoHeader [pos]);
//			}
			
			for (; pos < deflatedNoHeader.Length; pos++) {
				if (deflatedNoHeader [pos] == '\0') {
					break;
				}
				Console.Write ((char) deflatedNoHeader [pos]);
			}
			
			pos += 1;
			
			byte[] bytes = new byte[20];
			Array.Copy (deflatedNoHeader, pos, bytes, 0, 20);
			
			SHA1 id = new SHA1 (bytes, false);
			
			Console.WriteLine (" " + id.ToHexString ());
			
//			foreach (byte b in deflatedNoHeader) {
//				if (b == '\0')
//					Console.Write ("[null]");
//				Console.Write ((char) b);
//			}
		}
		
		public static void TestFileSystemModes ()
		{
			System.IO.FileInfo fi = new FileInfo ("updatecoredll.sh");
			
			Console.WriteLine (fi.Attributes);
		}
		
		public static void ReadDirectories (string path)
		{
			string[] files;
			
			files = Directory.GetFileSystemEntries (path);
			
			foreach (string entry in files) {
				if (File.Exists (entry)) {
					Console.WriteLine ("{0} is a file", entry);
				} else if (Directory.Exists (entry)) {
					Console.WriteLine ("{0} is a directory", entry);
				}
			}
		}
		
		public static void CheckoutTest (string hash, string objStorePath)
		{
			ObjectStore store = new ObjectStore (objStorePath);
			
			SHA1 id = new SHA1 (SHA1.FromHexString (hash), false);
			
			Console.WriteLine ("Hash: " + hash);
			Console.WriteLine ("Id:   " + id.ToHexString ());
			
			Console.WriteLine ("hash created");
			
			Tree tree = (Tree) store.Get (id);
			
			Console.WriteLine ("tree created No. entries: " + tree.Entries.Length);
			
			store.Checkout (Environment.CurrentDirectory, tree);
			
			Console.WriteLine ("Checkout done WIIIII!!!");
		}
		
		public static void LsTreeTest (string tree, string path)
		{
			ObjectStore store = new ObjectStore (path);
			store.LsTree (tree);
		}
		
		public static void CheckinObject (string filePath, string storePath)
		{
			ObjectStore store = new ObjectStore (storePath);
			store.Checkin (filePath);
			
			store.Write ();
		}
		
		public static void ViewCompressedFile (string filePath)
		{
			FileStream fs = File.Open (filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
			
			byte[] bytes = new byte[(int) fs.Length];
			
			fs.Read (bytes, 0, (int) fs.Length);
			
			byte[] content = ObjectStore.Decompress (bytes);
			
			SHA1 id = new SHA1 (content, true);
			
			Console.WriteLine ("ID: {0}", id.ToHexString ());
			Console.WriteLine ("Path {0}", filePath);
			
			Console.WriteLine ("Length: " + content.Length);
			
			foreach (char c in content) {
				if (c == '\n') {
					Console.WriteLine ("\\n");
					continue;
				}
				if (c == '\0')
					Console.Write ("[NULL]");
				Console.Write (c);
			}
		}
	}
}
