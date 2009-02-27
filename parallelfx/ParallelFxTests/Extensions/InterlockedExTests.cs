
using System;
using System.Threading;
using Mono.Threading;

using NUnit.Framework;

namespace ParallelFxTests
{
	
	[TestFixture]
	public class InterlockedExTests
	{
		class Wrapper
		{
			public int Value;
		}
		
		Swappable<Wrapper> val1;
		Swappable<Wrapper> val2;
		Swappable<Wrapper> val3;
		
		Swappable<Wrapper>[] locations;
		
		[SetUp]
		public void Setup()
		{
			val1 = new Swappable<Wrapper>(null);
			val2 = new Swappable<Wrapper>(null);
			val3 = new Swappable<Wrapper>(null);
			locations = new Swappable<Wrapper>[] { val1, val2, val3 };
		}
		
		[Test]
		public void UncontentionedSucceedingMCas()
		{
			Wrapper one = new Wrapper();
			one.Value = 1;
			Wrapper[] expected = new Wrapper[] { null, null, null };
			Wrapper[] newVals = new Wrapper[] { one, null, one };
			
			Assert.IsTrue(InterlockedEx.MultiCompareAndSwap(locations, expected, newVals), "#1");
			Assert.AreEqual(one, val3.Value, "#2");
			Assert.AreEqual(null, val2.Value, "#3");
			Assert.AreEqual(one, val1.Value, "#4");
		}
		
		[Test]
		public void UncontentionedFailingMCas()
		{
			Wrapper one = new Wrapper();
			one.Value = 1;
			Wrapper[] expected = new Wrapper[] { null, one, null };
			Wrapper[] newVals = new Wrapper[] { null, null, one };
			
			Assert.IsFalse(InterlockedEx.MultiCompareAndSwap(locations, expected, newVals), "#1");
			Assert.AreEqual(null, val3.Value, "#2");
			Assert.AreEqual(null, val2.Value, "#3");
			Assert.AreEqual(null, val1.Value, "#4");
		}
	}
}
