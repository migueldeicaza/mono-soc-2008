// 
// OrderablePartitioner.cs
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
	public abstract class OrderablePartitioner<T> : Partitioner<T>
	{
		bool keysOrderedInEachPartition;
		bool keysOrderedAcrossPartitions;
		bool keysNormalized;
		
		protected OrderablePartitioner (bool keysOrderedInEachPartition,
		                                bool keysOrderedAcrossPartitions, 
		                                bool keysNormalized) : base ()
		{
			this.keysOrderedInEachPartition = keysOrderedInEachPartition;
			this.keysOrderedAcrossPartitions = keysOrderedAcrossPartitions;
			this.keysNormalized = keysNormalized;
		}
		
		public override IEnumerable<T> GetDynamicPartitions ()
		{
			return GetOrderableDynamicPartitions ().Select ((e) => e.Value);
		}
		
		public override IList<IEnumerator<T>> GetPartitions (int partitionCount)
		{
			IEnumerator<T>[] temp = new IEnumerator<T>[partitionCount];
			IList<IEnumerator<KeyValuePair<long, T>>> enumerators = GetOrderablePartitions (partitionCount);
			
			for (int i = 0; i < enumerators.Count; i++)
				temp[i] = GetProxyEnumerator (enumerators[i]);
			
			return temp;
		}
		
		IEnumerator<T> GetProxyEnumerator (IEnumerator<KeyValuePair<long, T>> enumerator)
		{
			while (!enumerator.MoveNext ())
				yield return enumerator.Current.Value;
		}
		
		public abstract IList<IEnumerator<KeyValuePair<long, T>>> GetOrderablePartitions(int partitionCount);
		
		public virtual IEnumerable<KeyValuePair<long, T>> GetOrderableDynamicPartitions()
		{
			if (!SupportsDynamicPartitions)
				throw new NotSupportedException ();
			
			return null;
		}

		
		public bool KeysOrderedInEachPartition {
			get {
				return keysOrderedInEachPartition;
			}
		}
		
		public bool KeysOrderedAcrossPartitions {
			get {
				return keysOrderedAcrossPartitions;
			}
		}
		
		public bool KeysNormalized {
			get {
				return keysNormalized;
			}
		}
	}
}
