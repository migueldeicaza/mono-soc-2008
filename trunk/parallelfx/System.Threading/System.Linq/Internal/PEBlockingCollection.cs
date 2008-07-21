// PEBlockingCollection.cs
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
using System.Threading.Collections;

namespace System.Linq
{	
	internal class PEBlockingCollection<TSource, T>: ParallelEnumerableBase<T>
	{
		/*class BlockingCollectionOrderedEnumerator: BlockingCollectionEnumerator
		{
			SpinLock sl = new SpinLock(false);
			
			public BlockingCollectionEnumerator(BlockingCollection<T> bColl, bool isLast, 
			                                    Func<Action<T>, Action<int>, bool> action, Func<int> indexGetter): 
				base(bColl, isLast, action, indexGetter)
			{
			}
			
		}*/
		
		class BlockingCollectionEnumerator: IParallelEnumerator<T>
		{
			T current;
			readonly BlockingCollection<T> bColl;
			readonly bool isLast;
			readonly Func<IParallelEnumerator<TSource>, Action<T, bool>, Action<int>, bool> action;
			readonly Func<int> indexGetter;
			readonly IParallelEnumerator<TSource> enumerator;
			
			public BlockingCollectionEnumerator(BlockingCollection<T> bColl, bool isLast, 
			                                    Func<IParallelEnumerator<TSource>, Action<T, bool>, Action<int>, bool> action,
			                                    IParallelEnumerator<TSource> enumerator,
			                                    Func<int> indexGetter)
			{
				this.bColl =  bColl;
				this.isLast = isLast;
				this.action = action;
				this.indexGetter = indexGetter;
				this.enumerator = enumerator;
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
			
			void CurrentAdder(T element, bool isValid)
			{
				if (isValid)
					current = element;
			}
			
			void DummyIndexSetter(int index)
			{
			}
			
			public bool MoveNext()
			{
				if (bColl.IsCompleted)
					return false;
				
				if (!isLast) {
					//Console.WriteLine("Attempting child action from : " + Thread.CurrentThread.ManagedThreadId);
					return action(enumerator, CurrentAdder, DummyIndexSetter);
				} else {
					//Console.WriteLine("Main operator yielding");
					if (bColl.IsCompleted)
						return false;
					try {
						current = bColl.Remove();
					} catch {
						return false;
					}
				}
				
				return true;
			}
			
			public virtual bool MoveNext(out T item, out int index)
			{
				index = -1;
				item = default(T);
				
				if (bColl.IsCompleted)
					return false;
				
				if (!isLast) {
					//Console.WriteLine("Attempting child action from : " + Thread.CurrentThread.ManagedThreadId);
					T privElement = default(T);
					int i = -1;
					bool isValid = false;
					bool result  = false;
					Action<T, bool> adder = (T e, bool v) => { isValid = v; privElement = e; };
					Action<int> indexCallback = (int ind) => i = ind;
					
					do {
						result = action(enumerator, adder, indexCallback);
					} while(!isValid && result);
					
					item  = privElement;
					index = i;
					
					current = privElement;
					
					return result;
				} else {
					//Console.WriteLine("Main operator yielding");
					if (bColl.IsCompleted)
						return false;
					
					try {
						item = bColl.Remove();
					} catch {
						item = default(T);
						return false;
					}
					index = indexGetter();
					current = item;
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
		
		BlockingCollection<T> bColl;
		Func<IParallelEnumerator<TSource>, Action<T, bool>, Action<int>, bool> action;
		IParallelEnumerable<TSource> source;
		int index;
		
		
		public PEBlockingCollection(BlockingCollection<T> bColl,
		                            Func<IParallelEnumerator<TSource>, Action<T, bool>, Action<int>, bool> action,
		                            IParallelEnumerable<TSource> source, int dop): base(dop)
		{
			this.bColl = bColl;
			this.action = action;
			this.source = source;
		}
		
		void BlockingCollectionAdder(T element, bool isValid)
		{
			if (isValid)
				bColl.TryAdd(element);
		}
		
		void IndexSetter(int index)
		{
			this.index = index;
		}
		
		protected override IParallelEnumerator<T> GetParallelEnumerator()
		{
			IParallelEnumerator<TSource> enumerator = source.GetParallelEnumerator();
			if (isLast) {
				if (!isOrdered) {
					//Console.WriteLine("Starting main parallel looping " + Thread.CurrentThread.ManagedThreadId);
					Parallel.SpawnBestNumber(delegate {
						while (!bColl.IsAddingComplete) {
							//Console.WriteLine("Attempting action from : " + Thread.CurrentThread.ManagedThreadId);
							if (!action(enumerator, BlockingCollectionAdder, IndexSetter))
								break;
						}
					}, dop, () => bColl.CompleteAdding());
				
				}/* else {
					//Console.WriteLine("Starting main parallel looping " + Thread.CurrentThread.ManagedThreadId);
					SpinLock sl = new SpinLock(false);
					SpinWait sw = new SpinWait();
					int indexToBeAdded = -1;
					
					Parallel.SpawnBestNumber(delegate {
						int index;
						while (!bColl.IsAddingComplete) {
							if (!action(delegate (T element, bool isValid) {
								if (!isValid) {
									Interlocked.Increment(ref indexToBeAdded);
									continue;
								}
								
								bool added = false;
								while (!added) {
									try {
										sl.Enter();
										if (index == indexToBeAdded + 1) {
											blocking
										}	
									}
								}
							}, (i) => index = i)) 
								break;
						}
					}, dop, () => bColl.CompleteAdding());
				}*/
			}
			
			return new BlockingCollectionEnumerator(bColl, isLast, action, enumerator, () => index);
		}
	}
}
