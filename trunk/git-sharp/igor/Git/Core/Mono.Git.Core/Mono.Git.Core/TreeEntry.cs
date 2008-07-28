// TreeEntry.cs
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
using Mono.Git.Core;
using Mono.Git.Core.Repository;

namespace Mono.Git.Core
{
	/// <summary>
	/// Every Tree has entries, which are more trees and blobs, this is a
	/// representation of a tree entry that can be another tree or a blob
	/// </summary>
	public class TreeEntry
	{
		private SHA1 id;
		private string name;
		private Tree parent;
		
		public TreeEntry ()
		{
		}
		
		public TreeEntry (Tree myParent, SHA1 objId, string objName)
		{
			id = objId;
			name = objName;
			parent = myParent;
		}
		
		public string Name
		{
			set {
				name = value;
			}
			get {
				return name;
			}
		}
		
//		public SHA1 Id
//		{
//			set {
//				if (parent != null && value.bytes != null) {
//					id = value;
//				} else {
//					id.bytes = null;
//				}
//			}
//			get {
//				return id;
//			}
//		}
		
		/// <summary>
		/// Helper to get the last char in a tree entry
		/// </summary>
		/// <param name="entry">
		/// The entry we wish to add<see cref="Object"/>
		/// </param>
		/// <returns>
		/// A string \0 if its a blob / if its a tree<see cref="System.String"/>
		/// </returns>
		public static string LastChar (Object entry)
		{
			return entry.Type == Type.Blob ? "\0" : "/";
		}
		
		/// <summary>
		/// Identify if an entry is a Tree
		/// </summary>
		/// <param name="repository">
		/// The repository where it belongs<see cref="Repo"/>
		/// </param>
		/// <returns>
		/// A returned boolean<see cref="System.Boolean"/>
		/// </returns>
//		public bool IsTree (Repo repository)
//		{
//			bool isTree = false;
//			
//			Object[] objects = repository.Objects.Objects;
//			
//			foreach (Object obj in objects) {
//				if (obj.Type == Type.Tree)
//					isTree = true;
//			}
//			
//			return isTree;
//		}
	}
}
