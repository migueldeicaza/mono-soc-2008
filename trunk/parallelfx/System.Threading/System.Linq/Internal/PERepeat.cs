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
	internal class PERepeat<T>: ParallelEnumerableBase<T>, ICollection<T>, IList<T>
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
		
		public override IParallelEnumerator<T> GetParallelEnumerator()
		{
			return new PERepeatEnumerator(element, count);
		}
		
		
		#region IList`1[System.Int32] implementation 
		
		public int IndexOf (T item)
		{
			// No real index, we may just be interested if the value is different from -1
			return Contains(item) ? 1 : -1;
		}
		
		public void Insert (int index, T item)
		{
			throw new NotImplementedException();
		}
		
		public void RemoveAt (int index)
		{
			throw new NotImplementedException();
		}
		
		public T this[int index] {
			get {
				return index < count ? element : default(T);	
			}
			set {
				throw new NotImplementedException();
			}
		}
		
		#endregion 
		
		#region ICollection`1[System.Int32] implementation 
		
		public void Add (T item)
		{
			throw new NotImplementedException();
		}
		
		public void Clear ()
		{
			throw new NotImplementedException();
		}
		
		public bool Contains (T item)
		{
			return item.Equals(element);
		}
		
		public void CopyTo (T[] array, int arrayIndex)
		{
			for (int i = arrayIndex; i < array.Length && i < (i - arrayIndex) + count; i++)
				array[i] = element;
		}
		
		public bool Remove (T item)
		{
			throw new NotImplementedException();
		}
		
		public int Count {
			get {
				return count;
			}
		}
		
		public bool IsReadOnly {
			get {
				return true;
			}
		}
		
		#endregion 
	}
}
