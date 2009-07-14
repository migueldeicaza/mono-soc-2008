// 
// EnumerablePartitioner.cs
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
using System.Linq;
using System.Collections.Generic;

namespace System.Collections.Concurrent
{
	public class EnumerablePartitioner<T> : OrderablePartitioner<T>
	{
		IEnumerable<T> source;
//#if USE_MONITOR_LOCK
		object syncLock = new object ();
//#endif
		const int InitialPartitionSize = 1;
		const int PartitionMultiplier = 2;
		
		public EnumerablePartitioner (IEnumerable<T> source) : base (true, false, false)
		{
			this.source = source;
		}
		
		public override IEnumerable<T> GetDynamicPartitions ()
		{
			throw new NotSupportedException ();
		}
		
		public override IList<IEnumerator<T>> GetPartitions (int partitionCount)
		{
			if (partitionCount <= 0)
				throw new ArgumentOutOfRangeException ("partitionCount");
			
			IEnumerator<T>[] enumerators = new IEnumerator<T>[partitionCount];
			
			IEnumerator<T> src = source.GetEnumerator ();
			
			for (int i = 0; i < enumerators.Length; i++) {
				enumerators[i] = GetPartitionEnumerator (src);
			}
			
			return enumerators;
		}
		
		IEnumerator<T> GetPartitionEnumerator (IEnumerator<T> src)
		{
//#if USE_MONITOR_LOCK
			int count = InitialPartitionSize;
			List<T> list = new List<T> ();
			
			while (true) {
				list.Clear ();
				
				lock (syncLock) {
					for (int i = 0; i < count; i++) {
						if (!src.MoveNext ()) {
							if (list.Count == 0)
								yield break;
							else
								break;
						}
						
						list.Add (src.Current);
					}					
				}
				
				foreach (T element in list)
					yield return element;
				
				count *= PartitionMultiplier;
			}
		}                                  
	}
}
