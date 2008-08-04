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
		BlockingCollection<T> bColl;
		Func<IParallelEnumerator<TSource>, Action<T, bool, int>, bool> action;
		IParallelEnumerable<TSource> source;
		
		public PEBlockingCollection(BlockingCollection<T> bColl,
		                            Func<IParallelEnumerator<TSource>, Action<T, bool, int>, bool> action,
		                            IParallelEnumerable<TSource> source, int dop): base(dop)
		{
			this.bColl = bColl;
			this.action = action;
			this.source = source;
		}
		
		void BlockingCollectionAdder(T element, bool isValid, int index)
		{
			if (isValid) {
				bColl.TryAdd(element);
			}
		}
		
		void LaunchUnorderedLast(IParallelEnumerator<TSource> enumerator)
		{
			Parallel.SpawnBestNumber(delegate {
				while (!bColl.IsAddingCompleted) {
					if (!action(enumerator, BlockingCollectionAdder))
						break;
				}
			}, dop, () => bColl.CompleteAdding());
		}
		
		void LaunchOrderedLast(IParallelEnumerator<TSource> enumerator)
		{
			SpinLock sl = new SpinLock(false);
			SpinWait sw = new SpinWait();
			int indexToBeAdded = -1;
			
			Action<T, bool, int> adderFunc = delegate (T element, bool isValid, int index) {
				bool added = false;
				while (!added) {
					try {
						sl.Enter();
						if (index == indexToBeAdded + 1) {
							if (!isValid) {
								// Just increment the index to let the next possible
								// valid item going in
								indexToBeAdded++;
							} else {
								bColl.Add(element);
								indexToBeAdded++;
							}
							added = true;
						}	
					} finally {
						sl.Exit();
					}
					
					if (!added)
						sw.SpinOnce();
				}
			};
				
			Parallel.SpawnBestNumber(delegate {
				while (!bColl.IsAddingCompleted)
					if (!action(enumerator, adderFunc)) 
						break;
			}, dop, () => bColl.CompleteAdding());
		}
		
		public override IParallelEnumerator<T> GetParallelEnumerator()
		{
			IParallelEnumerator<TSource> enumerator = source.GetParallelEnumerator();
			
			if (isLast) {
				if (isOrdered)
					LaunchOrderedLast(enumerator);
				else
					LaunchUnorderedLast(enumerator);
			}
			
			BlockingCollectionEnumeratorBase<T> result;
			if (isLast)
				result = new BlockingCollectionIsLastEnumerator<T>(bColl);
			else if (isOrdered)
				result = new BlockingCollectionOrderedEnumerator<TSource, T>(action, enumerator);
			else
				result = new BlockingCollectionEnumerator<TSource, T>(action, enumerator);
			
			return result;
		}
	}
}
