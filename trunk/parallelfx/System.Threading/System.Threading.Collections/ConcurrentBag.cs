// 
// ConcurrentBag.cs
//  
// Author:
//       Jérémie "Garuma" Laval <jeremie.laval@gmail.com>
// 
// Copyright (c) 2009 Jérémie "Garuma" Laval
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

using System;
using System.Collections;
using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;

namespace System.Collections.Concurrent
{

	public class ConcurrentBag<T> : IProducerConsumerCollection<T>, IEnumerable<T>, IEnumerable
	{
		int size = 2;
		int baseSize = 1;
		int count;
		
		CyclicDeque<T>[] container;
		
		object syncLock = new object ();
		
		public ConcurrentBag ()
		{
			container = new CyclicDeque<T>[size];
			for (int i = 0; i < container.Length; i++)
				container[i] = new CyclicDeque<T> ();
		}
		
		public ConcurrentBag (IEnumerable<T> enumerable) : this ()
		{
			
		}
		
		public bool TryAdd (T item)
		{
			Add (item);
			
			return true;
		}
		
		public void Add (T item)
		{
			Interlocked.Increment (ref count);
			GrowIfNecessary ();
			
			CyclicDeque<T> bag = GetBag ();
			bag.PushBottom (item);
		}
		
		public bool TryTake (out T item)
		{
			item = default (T);
			CyclicDeque<T> bag = GetBag ();
			
			if (bag == null || bag.PopBottom (out item) != PopResult.Succeed) {
				for (int i = 0; i < size; i++) {
					if (container[i].PopTop (out item) == PopResult.Succeed) {
						Interlocked.Decrement (ref count);
						return true;
					}
				}
			} else {
				Interlocked.Decrement (ref count);
				return true;
			}
			
			return false;
		}
		
		public int Count {
			get {
				return count;
			}
		}
		
		public bool IsEmpty {
			get {
				return count == 0;
			}
		}
		
		public object SyncRoot  {
			get {
				return this;
			}		
		}
		
		public bool IsSynchronized  {
			get {
				return true;
			}
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return null;
		}
		
		IEnumerator<T> IEnumerable<T>.GetEnumerator ()
		{
			return null;
		}
		
		public void CopyTo (Array array, int index)
		{
			
		}
		
		public T[] ToArray ()
		{
			return null;
		}
			
		int GetIndex ()
		{
			return Thread.CurrentThread.ManagedThreadId - 1;
		}
		
		void GrowIfNecessary ()
		{
			int index = GetIndex ();
			int currentSize = size;
			
			while (index > currentSize - 1) {
				currentSize = size;
				Grow (currentSize);
			}
		}
		
		CyclicDeque<T> GetBag ()
		{			
			int i = GetIndex ();
			
			return i < container.Length ? container[i] : null;
		}
		
		void Grow (int referenceSize)
		{
			lock (syncLock) {
				if (referenceSize != size)
					return;
				
				CyclicDeque<T>[] slice = new CyclicDeque<T>[1 << (++baseSize)];
				int i = 0;
				for (i = 0; i < container.Length; i++)
					slice[i] = container[i];
				for (; i < slice.Length; i++)
					slice[i] = new CyclicDeque<T> ();
				
				container = slice;
				size = slice.Length;
			}
		}
	}
}
