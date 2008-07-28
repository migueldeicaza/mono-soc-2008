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
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace System.Threading.Collections
{
	public class BlockingCollection<T>: IEnumerable<T>, ICollection, IEnumerable, IDisposable
	{
		readonly IConcurrentCollection<T> underlyingColl;
		readonly int upperBound;
		readonly Func<bool> isFull;
		
		//const int blockingTime = 100;
		readonly SpinWait sw = new SpinWait();
		
		volatile bool isComplete;
		
		#region ctors
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
				isFull = delegate { return false; };
			else
				isFull = delegate { return underlyingColl.Count >= upperBound; };
		}
		#endregion
		
		#region Add & Remove (+ Try)
		public void Add(T item)
		{
			while (isFull()) {
				if (isComplete)
					throw new InvalidOperationException("The BlockingCollection<T> has been marked as complete with regards to additions.");
				Block();
			}
			// Extra check. The status might have changed after Block() or if isFull() is always false
			if (isComplete)
				throw new InvalidOperationException("The BlockingCollection<T> has been marked as complete with regards to additions.");

			underlyingColl.Add(item);
		}
		
		public T Remove()
		{
			while (underlyingColl.Count == 0) {
				if (isComplete)
					throw new OperationCanceledException("The BlockingCollection<T> is empty and has been marked as complete with regards to additions.");
				Block();
			}
			
			T item;
			underlyingColl.Remove(out item);
			
			return item;
		}
		
		public bool TryAdd(T item)
		{
			if (isComplete || isFull()) {
				return false;
			}
			return underlyingColl.Add(item);
		}
		
		public bool TryAdd(T item, TimeSpan ts)
		{
			return TryAdd(item, (int)ts.TotalMilliseconds);
		}
		
		public bool TryAdd(T item, int millisecondsTimeout)
		{
			Stopwatch sw = Stopwatch.StartNew();
			while (isFull()) {
				if (isComplete || sw.ElapsedMilliseconds > millisecondsTimeout) {
					sw.Stop();
					return false;
				}
				Block();
			}
			return underlyingColl.Add(item);
		}
		
		public bool TryRemove(out T item)
		{
			return underlyingColl.Remove(out item);
		}
		
		public bool TryRemove(out T item, TimeSpan ts)
		{
			return TryRemove(out item, (int)ts.TotalMilliseconds);
		}
		
		public bool TryRemove(out T item, int millisecondsTimeout)
		{
			Stopwatch sw = Stopwatch.StartNew();
			while (underlyingColl.Count == 0) {
				if (isComplete || sw.ElapsedMilliseconds > millisecondsTimeout) {
					item = default(T);
					return false;
				}
					
				Block();
			}
			return underlyingColl.Remove(out item);
		}
		#endregion
		
		public void CompleteAdding()
		{
			isComplete = true;
		}
		
		public void CopyTo(Array array, int index)
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
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return ((IEnumerable)underlyingColl).GetEnumerator();
		}
		
		IEnumerator<T> IEnumerable<T>.GetEnumerator ()
		{
			return ((IEnumerable<T>)underlyingColl).GetEnumerator();
		}
		
		public void Dispose()
		{
			
		}
		
		public T[] ToArray()
		{
			return underlyingColl.ToArray();
		}
		
		// Method used to stall the thread for a limited period of time before retrying an operation
		void Block()
		{
			//Thread.Sleep(blockingTime);
			sw.SpinOnce();
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
		
		public object SyncRoot {
			get {
				return underlyingColl.SyncRoot;
			}
		}
		
		public bool IsSynchronized {
			get {
				return underlyingColl.IsSynchronized;
			}
		}
	}
}