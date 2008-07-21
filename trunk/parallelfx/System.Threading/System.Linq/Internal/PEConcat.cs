// PEConcat.cs
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
	// Used by Concat and consort
	internal class PEConcat<T>: ParallelEnumerableBase<T>
	{
		IParallelEnumerable<T>[] enumerables;
		
		public PEConcat(IParallelEnumerable<T>[] enumerables, int dop): base(dop)
		{
			this.enumerables = enumerables;
		}
		
		class PEIConcatEnumerator: IParallelEnumerator<T>
		{
			int flag;
			int currIndex;
			IParallelEnumerable<T>[] enumerables;
			IParallelEnumerator<T> currentEnumerator;
			T current;
			SpinWait sw = new SpinWait();
			
			// Tell by how much index is shifted
			int stride;
			// Keep the last index returned for stride
			int backupIndex;
			
			bool finished;
			
			public PEIConcatEnumerator(IParallelEnumerable<T>[] enumerables)
			{
				this.enumerables = enumerables;
				currIndex = 0;
				currentEnumerator = enumerables[currIndex].GetParallelEnumerator();
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
				int i;
				return MoveNext(out current, out i);
			}
			
			public bool MoveNext(out T item, out int elementIndex)
			{
				int i = -1;
				
				while (flag != 0) {
					sw.SpinOnce();
				}
				
				while (!currentEnumerator.MoveNext(out item, out i) && !finished) {
					int result = Interlocked.Exchange(ref flag, 1);
					if (result == 0) {
						int index = currIndex++;
						stride += backupIndex;
						if (index >= enumerables.Length) {
							finished = true;
						} else {
							currentEnumerator = enumerables[index].GetParallelEnumerator();
						}
						flag = 0;
					}
					while (flag != 0) { 
						sw.SpinOnce();
					}
				}
				
				elementIndex = i + stride;
				backupIndex  = elementIndex;
				
				if (!finished)
					current = item;
				
				return !finished;
			}
			
			public void Reset()
			{
			}
			
			public void Dispose()
			{
			}
		}
		
		protected override IParallelEnumerator<T> GetParallelEnumerator()
		{
			return new PEIConcatEnumerator(enumerables);
		}
	}
}
