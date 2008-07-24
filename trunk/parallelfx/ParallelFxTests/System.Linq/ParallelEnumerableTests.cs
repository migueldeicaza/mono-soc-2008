// ParallelEnumerableTests.cs
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
using System.Linq;

using System.Collections.Generic;

using NUnit.Framework;

namespace ParallelFxTests
{
	
	[TestFixtureAttribute]
	public class ParallelEnumerableTests
	{
		IEnumerable<int> baseEnumerable;
		
		[SetUpAttribute]
		public void Setup()
		{
			baseEnumerable = Enumerable.Range(1, 500);
		}
		
		void AreEquivalent(IEnumerable<int> syncEnumerable, IEnumerable<int> asyncEnumerable, int count)
		{
			int[] sync  = syncEnumerable.ToArray();
			int[] async = asyncEnumerable.ToArray();
			
			// This is not AreEquals because IParallelEnumerable is not non-deterministic (IParallelOrderedEnumerable is)
			// thus the order of the initial Enumerable might not be preserved
			CollectionAssert.AreEquivalent(sync, async, "#" + count);
		}

		[TestAttribute]
		public void SelectTestCase()
		{
			IEnumerable<int> sync  = baseEnumerable.Select(i => i * i).ToArray();
			IEnumerable<int> async = baseEnumerable.AsParallel().Select(i => i * i).ToArray();
			
			AreEquivalent(sync, async, 1);
		}
		
		[TestAttribute]
		public void WhereTestCase()
		{
			IEnumerable<int> sync  = baseEnumerable.Where(i => i % 2 == 0).ToArray();
			IEnumerable<int> async = baseEnumerable.AsParallel().Where(i => i % 2 == 0).ToArray();
			
			AreEquivalent(sync, async, 1);
		}
		
		[TestAttribute]
		public void CountTestCase()
		{
			int sync  = baseEnumerable.Count();
			int async = baseEnumerable.AsParallel().Count();
			
			Assert.AreEqual(sync, async, "#1");
		}
		
		[TestAttribute]
		public void AggregateTestCase()
		{
			IParallelEnumerable<int> range = ParallelEnumerable.Range(1, 10);
			double average = range.Aggregate(
			                              () => new double[2],
			                              (acc, elem) => { acc[0] += elem; acc[1]++; return acc; },
			                              (acc1, acc2) => { acc1[0] += acc2[0]; acc1[1] += acc2[1]; return acc1; },
			                              acc => acc[0] / acc[1]);
			
			Assert.AreEqual(5.5, average, "#1");
		}
		
		[TestAttribute]
		public void ElementAtTestCase()
		{
			Assert.AreEqual(1, baseEnumerable.ElementAt(0), "#1");
			Assert.AreEqual(51, baseEnumerable.ElementAt(50), "#2");
			Assert.AreEqual(489, baseEnumerable.ElementAt(488), "#3");
		}
		
		[TestAttribute]
		public void TakeTestCase()
		{
			IParallelEnumerable<int> async = baseEnumerable.AsParallel().Take(200);
			IEnumerable<int> sync = baseEnumerable.Take(200);
			
			AreEquivalent(sync, async, 1);
			
			async = baseEnumerable.AsParallel().Take(100);
			sync = baseEnumerable.Take(100);
			
			AreEquivalent(sync, async, 2);
		}
		
		[TestAttribute]
		public void SkipTestCase()
		{
			IParallelEnumerable<int> async = baseEnumerable.AsParallel().AsOrdered().Skip(200);
			IEnumerable<int> sync = baseEnumerable.Skip(200);
			
			AreEquivalent(sync, async, 1);
			
			async = baseEnumerable.AsParallel().Skip(100);
			sync = baseEnumerable.Skip(100);
			
			Assert.AreEqual(sync.Count(), async.Count(), "#2");
		}
		
		[TestAttribute]
		public void RangeTestCase()
		{
			int[] sync  = Enumerable.Range(1, 10).ToArray();
			int[] async = ParallelEnumerable.Range(1, 10).ToArray();

			CollectionAssert.AreEqual(sync, async, "#1");
		}
		
		[TestAttribute]
		public void RepeatTestCase()
		{
			int[] sync  = Enumerable.Repeat(1, 10).ToArray();
			int[] async = ParallelEnumerable.Repeat(1, 10).ToArray();

			CollectionAssert.AreEqual(sync, async, "#1");
		}
	}
}
