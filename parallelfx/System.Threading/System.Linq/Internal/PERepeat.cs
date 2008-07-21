// PERepeat.cs
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
using System.Threading;
using System.Collections;
using System.Collections.Generic;

namespace System.Linq
{
	internal class PERepeat<T>: ParallelEnumerableBase<T>
	{
		int count;
		T element;
		
		public PERepeat(T element, int count, int dop): base(dop)
		{
			this.element = element;
			this.count = count;
		}
		
		class PERepeatEnumerator: IParallelEnumerator<T>
		{
			readonly int count;
			readonly T   element;
			int counter;
			T current;
			
			public PERepeatEnumerator(T element, int count)
			{
				this.element = element;
				this.count = count;
				this.current = element;
			}
			
			T IEnumerator<T>.Current {
				get {
					return current;
				}
			}
			
			object IEnumerator.Current {
				get {
					return current;
				}
			}
			
			public bool MoveNext()
			{
				int i = Interlocked.Increment(ref counter) - 1;
				return i < count;
			}
			
			public bool MoveNext(out T item, out int index)
			{
				int i = Interlocked.Increment(ref counter) - 1;
				item  = element;
				index = i;
				return i < count;
			}
			
			public void Reset()
			{
				this.counter = 0;
			}
			
			public void Dispose()
			{
				
			}
		}
		
		protected override IParallelEnumerator<T> GetParallelEnumerator()
		{
			return new PERepeatEnumerator(element, count);
		}
	}
}
