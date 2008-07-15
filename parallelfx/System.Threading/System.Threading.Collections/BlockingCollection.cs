// BlockingCollection.cs
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

namespace System.Threading.Collections
{
	public class BlockingCollection<T>/*: IEnumerable<T>, ICollection, IEnumerable, IDisposable*/
	{
		readonly IConcurrentCollection<T> underlyingColl;
		readonly int upperBound;
		readonly Func<bool> isFull;
		
		bool isComplete;
		
		public BlockingCollection():
			this(new ConcurrentQueue<T>(), -1)
		{
		}
		
		public BlockingCollection(int upperBound):
			this(new ConcurrentQueue<T>(), upperBound)
		{
		}
		
		public BlockingCollection(IConcurrentCollection<T> underlyingColl):
			this(underlyingColl, -1)
		{
		}
		
		public BlockingCollection(IConcurrentCollection<T> underlyingColl, int upperBound)
		{
			this.underlyingColl = underlyingColl;
			this.upperBound     = upperBound;
			if (upperBound == -1)
				isFull = () => false;
			else
				isFull = () => underlyingColl.Count >= upperBound;
		}
		
		public void Add(T item)
		{
			while (isFull()) {
				if (isComplete)
					throw new InvalidOperationException("The BlockingCollection<(Of <(T>)>) has been marked as complete with regards to additions.");
				Thread.Sleep(100);
			}
			underlyingColl.Add(item);
		}
		
		public bool Remove(out T item)
		{
			return underlyingColl.Remove(out item);
		}
		
		public void CompleteAdding()
		{
			isComplete = true;
		}
		
		public void CopyTo(T[] array, int index)
		{
			underlyingColl.CopyTo(array, index);
		}
		
		public IEnumerable<T> GetConsumingEnumerable()
		{
			T item;
			while (underlyingColl.Remove(out item)) {
				yield return item;
			}
		}
		
		public T[] ToArray()
		{
			return underlyingColl.ToArray();
		}
		
		public int BoundedCapacity {
			get {
				return upperBound;
			}
		}
		
		public int Count {
			get {
				return underlyingColl.Count;
			}
		}
		
		public bool IsAddingComplete {
			get {
				return isComplete;
			}
		}
		
		public bool IsCompleted {
			get {
				return isComplete && underlyingColl.Count == 0;
			}
		}
	}
}
