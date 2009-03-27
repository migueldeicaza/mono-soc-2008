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
			int[] sync  = Enumerable.ToArray(syncEnumerable);
			int[] async = Enumerable.ToArray(asyncEnumerable);
			
			// This is not AreEquals because IParallelEnumerable is non-deterministic (IParallelOrderedEnumerable is)
			// thus the order of the initial Enumerable might not be preserved
			CollectionAssert.AreEquivalent(sync, async, "#" + count);
		}
		
		static void AssertAreSame<T> (IEnumerable<T> expected, IEnumerable<T> actual)
		{
			if (expected == null) {
				Assert.IsNull (actual);
				return;
			}

			Assert.IsNotNull (actual);

			IEnumerator<T> ee = expected.GetEnumerator ();
			IEnumerator<T> ea = actual.GetEnumerator ();

			while (ee.MoveNext ()) {
				Assert.IsTrue (ea.MoveNext (), "'" + ee.Current + "' expected.");
				Assert.AreEqual (ee.Current, ea.Current);
			}

			if (ea.MoveNext ())
				Assert.Fail ("Unexpected element: " + ea.Current);
		}

		[TestAttribute]
		public void SelectTestCase()
		{
			ParallelTestHelper.Repeat (() => {
				IEnumerable<int> sync  = baseEnumerable.Select(i => i * i);
				IEnumerable<int> async = baseEnumerable.AsParallel().Select(i => i * i);
				
				AreEquivalent(sync, async, 1);
			});
		}
		
		[TestAttribute]
		public void WhereTestCase()
		{
			ParallelTestHelper.Repeat (() => {
				IEnumerable<int> sync  = baseEnumerable.Where(i => i % 2 == 0);
				IEnumerable<int> async = baseEnumerable.AsParallel().Where(i => i % 2 == 0);
				
				AreEquivalent(sync, async, 1);
			});
		}
		
		[TestAttribute]
		public void CountTestCase()
		{
			ParallelTestHelper.Repeat (() => {
				int sync  = baseEnumerable.Count();
				int async = baseEnumerable.AsParallel().Count();
				
				Assert.AreEqual(sync, async, "#1");
			});
		}
		
		[TestAttribute]
		public void AggregateTestCase()
		{
			ParallelTestHelper.Repeat (() => {
				IParallelEnumerable<int> range = ParallelEnumerable.Repeat (5, 1000);
				double average = range.Aggregate(() => new double[2],
				                                 (acc, elem) => { acc[0] += elem; acc[1]++; return acc; },
				(acc1, acc2) => { acc1[0] += acc2[0]; acc1[1] += acc2[1]; return acc1; },
				acc => acc[0] / acc[1]);
				
				Assert.AreEqual(5.0, average, "#1");
			});
		}
		
		[TestAttribute]
		public void ElementAtTestCase()
		{
			ParallelTestHelper.Repeat (() => {
				Assert.AreEqual(1, baseEnumerable.ElementAt(0), "#1");
				Assert.AreEqual(51, baseEnumerable.ElementAt(50), "#2");
				Assert.AreEqual(489, baseEnumerable.ElementAt(488), "#3");
			});
		}
		
		[TestAttribute]
		public void TakeTestCase()
		{
			ParallelTestHelper.Repeat (() => {
				IParallelEnumerable<int> async = baseEnumerable.AsParallel().Take(200);
				IEnumerable<int> sync = baseEnumerable.Take(200);
				
				AreEquivalent(sync, async, 1);
				
				async = baseEnumerable.AsParallel().Take(100);
				sync = baseEnumerable.Take(100);
			
				AreEquivalent(sync, async, 2);
			});
		}
		
		[TestAttribute]
		public void SkipTestCase()
		{
			ParallelTestHelper.Repeat (() => {
				IParallelEnumerable<int> async = baseEnumerable.AsParallel().AsOrdered().Skip(200);
				IEnumerable<int> sync = baseEnumerable.Skip(200);
				
				AreEquivalent(sync, async, 1);
				
				async = baseEnumerable.AsParallel().Skip(100);
				sync = baseEnumerable.Skip(100);
				
				Assert.AreEqual(sync.Count(), async.Count(), "#2");
			});
		}

		[TestAttribute]
		public void ZipTestCase()
		{
			ParallelTestHelper.Repeat (() => {
				IParallelEnumerable<int> async1 = ParallelEnumerable.Range(0, 10);
				IParallelEnumerable<int> async2 = ParallelEnumerable.Repeat(1, 10).Zip(async1, (e1, e2) => e1 + e2);
				
				int[] expected = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
				CollectionAssert.AreEquivalent(expected, async2.ToArray(), "#1");
			});
		}
		
		[TestAttribute]
		public void RangeTestCase()
		{
			ParallelTestHelper.Repeat (() => {
				IEnumerable<int> sync  = Enumerable.Range(1, 10);
				IEnumerable<int> async = ParallelEnumerable.Range(1, 10);
				
				AssertAreSame(sync, async);
			});
		}
		
		[TestAttribute]
		public void RepeatTestCase()
		{
			ParallelTestHelper.Repeat (() => {
				IEnumerable<int> sync  = Enumerable.Repeat(1, 10);
				IEnumerable<int> async = ParallelEnumerable.Repeat(1, 10);
				
				AssertAreSame(sync, async);
			});
		}
	}
}
