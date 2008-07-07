// Commit.cs
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
using Mono.Git.Core.Repository;

namespace Mono.Git.Core
{
	/// <summary>
	/// 
	/// </summary>
	public class Commit : Object
	{
		private SHA1 parent; // Represent the parent commit
		private SHA1 tree; // its the tree that you're commiting
		
		private User author; // initial author of the commit(inherited from parent)
		private User commiter; // the commiter name
		private string message; // message in the commit
		
		private Repo repository; // repository where it belongs
		
		public User Author
		{
			set {
				author = value;
			}
			get {
				return author;
			}
		}
		
		public User Commiter
		{
			set {
				commiter = value;
			}
			get {
				return commiter;
			}
		}
		
		public string Message
		{
			set {
				message = value;
			}
			get {
				return message;
			}
		}
		
		public SHA1 Tree
		{
			set {
				tree = value;
			}
			get {
				return tree;
			}
		}
		
		public SHA1 Parent
		{
			set {
				parent = value;
			}
			get {
				return parent;
			}
		}
		
		public Repo Repository
		{
			set {
				repository = value;
			}
			get {
				return repository;
			}
		}
		
		/// <summary>
		/// Creating an empty commit
		/// </summary>
		public Commit () : base (Type.Commit)
		{
			parent = new SHA1 ();
			tree = new SHA1 ();
		}
		
		public Commit (Repo repo) : base (Type.Commit)
		{
			repository = repo;
			parent.bytes = null;
		}
		
		public Commit (Repo repo, SHA1 treeId, SHA1 parentId, string authorName,
		               string commiterName, string authorEmail, string commiterEmail, 
		               string messageContent) : base (Type.Commit)
		{
			repository = repo;
			tree = treeId;
			parent = parentId;
			
			author.Name = authorName;
			author.Email = authorEmail;
			
			commiter.Name = commiterName;
			commiter.Name = commiterEmail;
			
			message = messageContent;
		}
	}
}
