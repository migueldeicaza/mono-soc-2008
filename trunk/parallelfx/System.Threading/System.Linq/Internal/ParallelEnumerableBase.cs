// ParallelEnumerableBase.cs
//
// Copyright (c) 2008 Jérémie "Garuma" Laval
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
//

using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Linq
{
	internal abstract class ParallelEnumerableBase<T>: IParallelEnumerable<T>
	{
		protected bool isLast = true;
		protected bool isOrdered = false;
		protected int dop;
		
		protected ParallelEnumerableBase(int dop)
		{
			this.dop = dop;
		}
		
		public IEnumerator<T> GetEnumerator(bool enablePipelining)
		{
			// Don't care about Pipelining for the moment
			// Just a matter of calling Task.WaitAll in the correct place
			return GetParallelEnumerator();
		}
		
		protected abstract IParallelEnumerator<T> GetParallelEnumerator();
		
		
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			this.IsLast();
			return (IEnumerator<T>)GetParallelEnumerator();
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			this.IsLast();
			return (IEnumerator)GetParallelEnumerator();
		}
		
		public bool IsLast {
			get {
				return isLast;
			}
			set {
				isLast = value;
			}
		}
		
		public bool IsOrdered {
			get {
				return isOrdered;
			}
			set {
				isOrdered = value;
			}
		}

		public int Dop {
			get {
				return dop;
			}
		}
		
	}
}
