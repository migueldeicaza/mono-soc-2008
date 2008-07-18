// ParallelEnumerable.cs
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
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Collections;

namespace System.Linq
{
	internal class ParallelEnumerable<T>: IParallelEnumerable<T>
	{
		Func<IEnumerator<T>> getEnumerator;
		// Dop is Degree of Parallelism. It corresponds to the ideal number of threads
		// that should be used to compute the query.
		int  dop;
		bool isLast;
		
		// For ctor 2
		Action                action;
		BlockingCollection<T> bColl;
		// For ctor 3
		int from, count;
		// For ctor 1
		IEnumerable<T> uEnumerable;
		IEnumerator<T> uEnumerator;
		SpinLock sl = new SpinLock(false);
		
		// Will use chunk partitionning, used by AsParallel()
		public ParallelEnumerable(IEnumerable<T> uEnumerable, int dop): this(dop)
		{
			this.uEnumerable = uEnumerable;
			this.uEnumerator = uEnumerable.GetEnumerator();
			getEnumerator = GetEnumeratorFromStandardIEnumerable;
		}
		
		IEnumerator<T> GetEnumeratorFromStandardIEnumerable()
		{
			const int chunkSize = 10;
			bool cont = true;
			int i;
			T[] buffer = new T[chunkSize];
			Console.WriteLine("Getting Parallel enumerator from standard Enumerable");
			
			while (cont) {
				try {
					sl.Enter();
					for (i = 0; i < chunkSize; i++) {
						if (!uEnumerator.MoveNext()) {
							cont = false;
							break;
						}
						buffer[i] = uEnumerator.Current;
					}
				} finally {
					sl.Exit();
				}
				
				for (int j = 0; j < i; j++) {
					Console.WriteLine("Parallel enumerator from standard Enumerable yielding");
					yield return buffer[j];
				}
			}
		}
		
		// Comes from the return of ParallelEnumerable extension method
		public ParallelEnumerable(Action action, BlockingCollection<T> bColl, int dop): this(dop)
		{
			this.action = action;
			this.bColl = bColl;
			getEnumerator = GetEnumeratorFromBlockingCollection;
		}
				
		class BlockingCollectionEnumerator: IEnumerator<T>
		{
			T current;
			readonly BlockingCollection<T> bColl;
			readonly bool isLast;
			readonly Action action;
			
			public BlockingCollectionEnumerator(BlockingCollection<T> bColl, bool isLast, Action action)
			{
				this.bColl =  bColl;
				this.isLast = isLast;
				this.action = action;
			}
			
			T IEnumerator<T>.Current {
				get {
					return current;
				}
			}
			
			object IEnumerator.Current {
				get {
					return (object) current;
				}
			}
			
			public bool MoveNext()
			{
				if (bColl.IsCompleted)
					return false;
				
				if (!isLast) {
					Console.WriteLine("Attempting child action from : " + Thread.CurrentThread.ManagedThreadId);
					if (!bColl.IsAddingComplete) {
						do {
							action();
						} while (bColl.TryRemove(out current) && !bColl.IsCompleted);
					}
					if (bColl.IsCompleted) {
						return false;
					}
				} else {
					Console.WriteLine("Main operator yielding");
					if (bColl.IsCompleted)
						return false;
					current = bColl.Remove();
				}
				
				return true;
			}
			
			public void Reset()
			{
				
			}
			
			public void Dispose()
			{
			}
		}
		
		IEnumerator<T> GetEnumeratorFromBlockingCollection()
		{
			if (isLast) {
				Console.WriteLine("Starting main parallel looping " + Thread.CurrentThread.ManagedThreadId);
				Parallel.SpawnBestNumber(delegate {
						while (!bColl.IsAddingComplete) {
							Console.WriteLine("Attempting action from : " + Thread.CurrentThread.ManagedThreadId);
							action();
						}
				}, dop);
			}
			
			return new BlockingCollectionEnumerator(bColl, isLast, action);
		}
		
		public ParallelEnumerable(int dop)
		{
			this.dop = dop;
			this.isLast = true;
		}
		
		// Will use range partitionning
		public static ParallelEnumerable<int> GetRangeParallelEnumerable(int from, int count, int dop)
		{
			//HACK: rather ugly
			ParallelEnumerable<int> temp = new ParallelEnumerable<int>(dop);
			temp.from = from;
			temp.count = count;
			temp.getEnumerator = temp.GetEnumeratorFromRange;
			
			return temp;
		}
		
		protected IEnumerator<int> GetEnumeratorFromRange()
		{
			int counter = from;
			int value;
			do {
				value = Interlocked.Increment(ref counter) - 1;
				yield return value;
			} while (value < (from + count - 1));
		}
		
		public IEnumerator<T> GetEnumerator(bool enablePipelining)
		{
			// Don't care about Pipelining for the moment
			// Just a matter of calling Task.WaitAll in the correct place
			return getEnumerator();
		}
		
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return getEnumerator();
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator)getEnumerator();
		}
		
		public int Dop {
			get {
				return dop;
			}
		}
		
		public bool IsLast {
			get {
				return isLast;
			}
			set {
				isLast = value;
			}
		}
	}
}
