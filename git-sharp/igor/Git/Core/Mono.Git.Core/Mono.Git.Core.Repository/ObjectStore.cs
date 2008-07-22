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
using System.IO;
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
		
		public Object[] Objects
		{
			set {
				objects = value;
			}
			get {
				return objects;
			}
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
