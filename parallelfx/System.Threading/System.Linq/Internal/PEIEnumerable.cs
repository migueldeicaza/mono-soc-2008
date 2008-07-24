// PEIEnumerable.cs
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
	internal class PEIEnumerable<T>: ParallelEnumerableBase<T>
	{
		IEnumerable<T> enumerable;
		
		public PEIEnumerable(IEnumerable<T> enumerable, int dop): base(dop)
		{
			this.enumerable = enumerable;
		}
		
		class PEIEnumerableEnumerator: IParallelEnumerator<T>
		{
			const int chunkSize = 10;
			
			readonly IEnumerable<T> enumerable;
			readonly SpinLock sl = new SpinLock(false);
			
			IEnumerator<T> enumerator;
			T current;
			int currIndex = -1;
			
			/*T[] buffer = new T[chunkSize];
			int fillIndex;*/
			
			public PEIEnumerableEnumerator(IEnumerable<T> enumerable)
			{
				this.enumerable = enumerable;
				this.enumerator = enumerable.GetEnumerator();
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
				bool result = false;
				
				try {
					sl.Enter();
					if (result = enumerator.MoveNext()) {
						current = enumerator.Current;
					}
				} finally {
					sl.Exit();
				}
				
				return result;
			}
			
			//readonly object syncRoot = new object();
			
			public bool MoveNext(out T item, out int index)
			{
				bool result = false;
				index = -1;
				
				try {
					sl.Enter();
					if (result = enumerator.MoveNext()) {
						current = item = enumerator.Current;
						index = Interlocked.Increment(ref currIndex);
					}
				} finally {
					sl.Exit();
				}
				/*lock (syncRoot) {
					if (result = enumerator.MoveNext()) {
						item = enumerator.Current;
						index = currIndex++;
					}
				}*/
				
				return result;
			}
			
			public void Reset()
			{
				try {
					sl.Enter();
					enumerator = enumerable.GetEnumerator();
				} finally {
					sl.Exit();
				}
			}
			
			public void Dispose()
			{
				
			}
		}
		
		public override IParallelEnumerator<T> GetParallelEnumerator()
		{
			return new PEIEnumerableEnumerator(enumerable);
		}
	}
}
