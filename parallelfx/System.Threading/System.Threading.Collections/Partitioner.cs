// 
// Partitioner.cs
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
using System.Collections.Generic;

namespace System.Collections.Concurrent
{
	public static class Partitioner
	{
		public static OrderablePartitioner<T> Create<T> (IEnumerable<T> source)
		{
			IList<T> tempIList = source as IList<T>;
			if (tempIList != null)
				return Create (tempIList);
			
			return new EnumerablePartitioner<T> (source);
		}
		
		public static OrderablePartitioner<T> Create<T> (T[] source)
		{
			return Create ((IList<T>)source);
		}
		
		public static OrderablePartitioner<T> Create<T> (IList<T> source)
		{
			return new ListPartitioner<T> (source);
		}
	}
	
	public abstract class Partitioner<T>
	{
		protected Partitioner ()
		{
			
		}
		
		public virtual IEnumerable<T> GetDynamicPartitions ()
		{
			if (!SupportsDynamicPartitions)
				throw new NotSupportedException ();
			
			return null;
		}
		
		public abstract IList<IEnumerator<T>> GetPartitions (int partitionCount);
		
		public virtual bool SupportsDynamicPartitions {
			get {
				return false;
			}
		}
	}
}
