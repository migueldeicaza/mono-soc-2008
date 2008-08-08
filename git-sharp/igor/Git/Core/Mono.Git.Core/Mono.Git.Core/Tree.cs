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
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Mono.Git.Core;

namespace Mono.Git.Core
{
	/// <summary>
	/// A class that stores a tree of a commit
	/// </summary>
	public class Tree : Object
	{
		private TreeEntry[] entries;
		
		public override Type Type { get { return Type.Tree; } }
		public TreeEntry[] Entries { get { return entries; } }
		
		public Tree (byte[] content) : base (Type.Tree, content) // TODO: add a real encoding
		{
		}
		
		private void AddEntry (byte[] data)
		{
			if (data.Length <= 27)
				throw new ArgumentException ("The data is not a tree entry, the size is to small");
			
			int pos = 0;
			byte[] mode;
			byte[] id;
			string name;
			
			ParseTreeEntry (data, ref pos, out mode, out name, out id);
			
			AddEntry (mode, name, new SHA1 (id, false));
		}
		
		private void AddEntry (byte[] mode, string name, SHA1 id)
		{
			List<TreeEntry> entriesList = new List<TreeEntry> (entries);
			
			entriesList.Add (new TreeEntry (mode, name, id));
			
			entries = entriesList.ToArray ();
		}
		
		private TreeEntry GetTreeEntry (string name)
		{
			foreach (TreeEntry te in entries) {
				if (te.Name.Equals (name)) {
					return te;
				}
			}
			
			throw new ArgumentException ("entry: {0} was not found", name);
		}
		
		private bool RemoveEntry (string name)
		{
			List<TreeEntry> entriesList = new List<TreeEntry> (entries);
			
			TreeEntry entry = GetTreeEntry (name);
			
			if (entriesList.Remove (entry)){
				entries = entriesList.ToArray ();
				return true;
			} else {
				return false;
			}
		}
		
		public void SortEnties ()
		{
			throw new NotImplementedException ("Sort tree entries is not implemented yet");
		}
//		
//		private void AddEntry (SHA1 id, TreeEntry entry)
//		{
//			if (entries.ContainsKey (id))
//				return;
//			
//			entries.Add (id, entry);
//		}
		
//		public TreeEntry[] EntriesGitSorted ()
//		{
//			if (entries.Length == 0) {
//				return entries;
//			}
//			
//			TreeEntry[] newEntries = new TreeEntry[entries.Length];
//			for (int i = entries.Length - 1; i >= 0; i--)
//				newEntries[i] = entries[i];
//			
//			return newEntries;
//		}
		
		protected override byte[] Decode ()
		{
			throw new NotImplementedException ();
		}
		
		protected override void Encode (byte[] content)
		{
			throw new NotImplementedException ();
		}
	}
}
