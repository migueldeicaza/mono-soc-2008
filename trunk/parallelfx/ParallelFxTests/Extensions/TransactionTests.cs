
using System;
using Mono.Threading.Transactions;

using NUnit.Framework;

namespace ParallelFxTests
{
	[TestFixture]
	public class TransactionTests
	{
		internal class BusinessClass : ICloneable
		{
			int    value1;
			string myString;

			public string MyString {
				get {
					return myString;
				}
				set {
					myString = value;
				}
			}
			
			public int Value1 {
				get {
					return value1;
				}
				set {
					value1 = value;
				}
			}
			
			public BusinessClass (int value1, string myString)
			{
				this.value1 = value1;
				this.myString = myString;
			}
			
			public object Clone ()
			{
				return new BusinessClass(value1, myString);
			}
		}
		
		StmObject<BusinessClass> b1;
		StmObject<BusinessClass> b2;
		
		[SetUp]
		public void Setup ()
		{
			b1 = new StmObject<BusinessClass>(new BusinessClass(1, "foo"));
			b2 = new StmObject<BusinessClass>(new BusinessClass(2, "bar"));
		}
		
		[Test]
		public void UncontentionedRead()
		{
			int sum = 0;
			
			bool result = Transaction.Create(OpeningMode.Read, b1, b2, (bu1, bu2) => {
				sum += bu1.Value1;
				sum += bu2.Value1;
			}).Execute();
			
			Assert.IsTrue(result, "#1");
			Assert.AreEqual(3, sum, "#2");
		}
		
		[Test]
		public void ContentionedRepeatedRead()
		{
			int sum = 0;
			bool once = true;
			
			bool result2 = false;			
			bool result = Transaction.Create(OpeningMode.Read, b1, b2, (bu1, bu2) => {
				sum += bu1.Value1;
				if (once) {
					result2 = Transaction.Create(OpeningMode.Write, b1, (b) => b.Value1 = 20).Execute();
					once = false;
				}
				sum += bu2.Value1;
			}).Execute();
			
			Assert.IsTrue(result, "#1");
			Assert.AreEqual(25, sum, "#2");
			Assert.IsTrue(result2);
		}
	}
}
