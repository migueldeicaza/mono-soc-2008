// Index.cs
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
using Mono.Git.Core.Repository;

namespace Mono.Git.Core
{
	/// <summary>
	/// Merge stage enumeration
	/// 
	/// Normal: It Doesn't need any merge (0)
	/// Ancestor: Something is not ok(needs merge) (1)
	/// Our: Our version (2)
	/// Their: Their version (3)
	/// 
	/// A fully merged stage is always 0(Normal)
	/// </summary>
	public enum IndexStage
	{
		Normal,
		Ancestor,
		Our,
		Their
	}
	
	public class Index
	{
		private IndexEntry[] entries; // Index Entries
		private uint stage; // Merged in the constructor
		private FileStream index_file;
		private bool changed;
		
		public IndexEntry[] Entries
		{
			set {
				entries = value;
			}
			get {
				return entries;
			}
		}
		
		public uint Stage
		{
			set {
				stage = value;
			}
			get {
				return stage;
			}
		}
		
		public FileStream IndexFile
		{
			set {
				index_file = value;
			}
			get {
				return index_file;
			}
		}
		
		public bool Changed
		{
			set {
				changed = value;
			}
			get {
				return changed;
			}
		}
		
		public Index ()
		{
		}
		
		public Index (string indexFilePath)
		{
			// Recreate the index file if it exist
			if (File.Exists (indexFilePath))
				File.Create (indexFilePath);
			
			stage = (uint) IndexStage.Normal;
		}
		
		public void AddEntry (IndexEntry indexEntry)
		{
			ArrayList array = new ArrayList (entries);
			
			array.Add (indexEntry);
			
			entries = (IndexEntry[]) array.ToArray ();
		}
		
		public void RemoveEntry (IndexEntry indexEntry)
		{
			ArrayList array = new ArrayList (entries);
			
			array.Remove (indexEntry);
			
			entries = (IndexEntry[]) array.ToArray ();
		}
	}
}
