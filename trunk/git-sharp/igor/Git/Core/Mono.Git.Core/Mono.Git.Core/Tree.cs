// Tree.cs
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
using Mono.Git.Core;

namespace Mono.Git.Core
{
	/// <summary>
	/// A class that stores a tree of a commit
	/// </summary>
	public class Tree : Object
	{
		private string name;
		private Tree parent;
		private TreeEntry[] entries;
		
		public Tree (Tree parent)
		{
			this.parent = parent;
		}
		
		public override Type Type
		{
			get {
				return Type.Tree;
			}
		}
		
		public Tree Parent {
			get {
				return parent;
			}
			set {
				parent = value;
			}
		}
		
		public TreeEntry[] Entries
		{
			set {
				entries = value;
			}
			get {
				return entries;
			}
		}
		
		/// <summary>
		/// Init the entries, one for the current directory and 1 for the blob
		/// blobs can be more than 1 but at least 1 
		/// </summary>
		public void InitEntries ()
		{
			entries = new TreeEntry[2];
		}
		
		public void AddEntry (TreeEntry entry)
		{
			if (entries == null) {
				InitEntries ();
			}
			
			int size = entries.Length + 1;
			ArrayList newEntries = new ArrayList (size);
			
			foreach (TreeEntry en in entries) {
				newEntries.Add (en);
			}
			
			newEntries.Add (entry);
			
			Entries = (TreeEntry[]) newEntries.ToArray ();
		}
		
		public void RemoveEntry (TreeEntry entry)
		{
			if (entries.Length == 0) {
				return;
			}
			
			ArrayList newEntries = new ArrayList (entries.Length);
			
			foreach (TreeEntry en in entries) {
				newEntries.Add (en);
			}
			
			newEntries.Remove (entry);
			
			Entries = (TreeEntry[]) newEntries.ToArray ();
		}
		
		public TreeEntry[] EntriesGitSorted ()
		{
			if (entries.Length == 0) {
				return entries;
			}
			
			TreeEntry[] newEntries = new TreeEntry[entries.Length];
			for (int i = entries.Length - 1; i >= 0; i--)
				newEntries[i] = entries[i];
			
			return newEntries;
		}
		
//		public bool Exists (TreeEntry entry)
//		{
//			if (entries.Length == 0) {
//				return false;
//			} else if (entries.Length == 1) {
//				return entries[0].Id.bytes == entry.Id.bytes ? true : false;
//			}
//			
//			bool exist = false;
//			
//			foreach (TreeEntry en in entries) {
//				if (en.Id.bytes == entry.Id.bytes)
//					exist = true;
//			}
//			
//			return exist;
//		}
	}
}
