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
	internal class PEBlockingCollection<T>: ParallelEnumerableBase<T>
	{
		class BlockingCollectionEnumerator: IParallelEnumerator<T>
		{
			T current;
			readonly BlockingCollection<T> bColl;
			readonly bool isLast;
			readonly Func<Action<T>, bool> action;
			
			public BlockingCollectionEnumerator(BlockingCollection<T> bColl, bool isLast, Func<Action<T>, bool> action)
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
			
			void CurrentAdder(T element)
			{
				current = element;
			}
			
			public bool MoveNext()
			{
				if (bColl.IsCompleted)
					return false;
				
				if (!isLast) {
					//Console.WriteLine("Attempting child action from : " + Thread.CurrentThread.ManagedThreadId);
					return action(CurrentAdder);
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
			
			public bool MoveNext(out T item)
			{
				if (bColl.IsCompleted)
					return false;
				
				if (!isLast) {
					//Console.WriteLine("Attempting child action from : " + Thread.CurrentThread.ManagedThreadId);
					T privElement = default(T);
					bool result = action(delegate(T element) { privElement = element; current = element; });
					item = privElement;
					
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
		Func<Action<T>, bool> action;
		
		public PEBlockingCollection(BlockingCollection<T> bColl, Func<Action<T>, bool> action, int dop): base(dop)
		{
			this.bColl = bColl;
			this.action = action;
		}
		
		void BlockingCollectionAdder(T element)
		{
			bColl.TryAdd(element);
		}
		
		protected override IParallelEnumerator<T> GetParallelEnumerator()
		{
			if (isLast) {
				//Console.WriteLine("Starting main parallel looping " + Thread.CurrentThread.ManagedThreadId);
				Parallel.SpawnBestNumber(delegate {
					while (!bColl.IsAddingComplete) {
						//Console.WriteLine("Attempting action from : " + Thread.CurrentThread.ManagedThreadId);
						if (!action(BlockingCollectionAdder))
							break;
					}
				}, dop, () => bColl.CompleteAdding());
			}
			
			return new BlockingCollectionEnumerator(bColl, isLast, action);
		}
	}
}
