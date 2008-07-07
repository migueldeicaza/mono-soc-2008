// IndexEntry.cs
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

namespace Mono.Git.Core
{
	public class IndexEntry
	{
		DateTime ctime; // TODO: A conversion method to handle this
		DateTime mtime; // TODO: A conversion method to handle this
		uint dev;
		uint ino; // Inode
		uint mode; // Create mode(umask)
		uint uid; // we don't really need this(jgit has as trick for this)
		uint gid; // apply jgit trick here too
		uint size;
		bool modified; // merged or not?
		SHA1 id; // ID
		string name; // whole path
		
		public IndexEntry ()
		{
		}
	}
}
