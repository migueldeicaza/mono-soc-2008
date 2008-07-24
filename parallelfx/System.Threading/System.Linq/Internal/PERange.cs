// PERange.cs
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
	internal class PERange: ParallelEnumerableBase<int>
	{
		int start, count;
		
		public PERange(int start, int count, int dop): base(dop)
		{
			this.start = start;
			this.count = count;
		}
		
		class PERangeEnumerator: IParallelEnumerator<int>
		{
			static int current;
			readonly int start, count;
			int counter;
			
			public PERangeEnumerator(int start, int count)
			{
				this.start = this.counter = start;
				this.count = count;
			}
			
			int IEnumerator<int>.Current {
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
				current = Interlocked.Increment(ref counter) - 1;
				return current < (start + count);
			}
			
			public bool MoveNext(out int item, out int index)
			{
				item = Interlocked.Increment(ref counter) - 1;
				index = item - start;
				current = item;
				return item < (start + count);
			}
			
			public void Reset()
			{
				this.counter = start;
			}
			
			public void Dispose()
			{
				
			}
		}
		
		public override IParallelEnumerator<int> GetParallelEnumerator()
		{
			return new PERangeEnumerator(start, count);
		}
	}
}
