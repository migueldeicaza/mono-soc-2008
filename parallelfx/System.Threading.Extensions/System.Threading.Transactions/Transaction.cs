// Transaction.cs
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
using System.Linq;
using System.Collections.Generic;

namespace System.Threading.Transactions
{

	public class Transaction
	{	
		readonly Action<object[]> transaction;
		readonly Isolated[] isolateds;
		readonly OpeningMode mode;
		
		readonly ITransactionManager manager = new TransactionManager();

		internal Isolated[] Isolateds {
			get {
				return isolateds;
			}
		}
		
		internal Action<object[]> Action {
			get {
				return transaction;
			}
		}
		
		
		private Transaction (OpeningMode mode, Isolated[] isolateds, Action<object[]> transaction)
		{
			this.isolateds = isolateds;
			this.transaction = transaction;
			this.mode = mode;
		}
		
		public static Transaction Create<T> (OpeningMode mode, Isolated<T> val1, Action<T> tr)
			where T : class
		{
			return new Transaction(mode, new Isolated[] { val1 }, (isolateds) => tr((T)isolateds[0]));
		}
		
		public static Transaction Create<T, U> (OpeningMode mode, Isolated<T> val1,
		                                        Isolated<U> val2, Action<T, U> tr)
			where T : class where U : class
		{
			return new Transaction(mode, new Isolated[] { val1, val2 },
			(isolateds) => tr((T)isolateds[0], (U)isolateds[1]));
		}
		
		public bool Execute()
		{
			return Execute(ExecutionType.UntilSucceed);
		}
		
		public bool Execute(ExecutionType type)
		{
			return mode == OpeningMode.Read ? manager.ExecuteRead(type, this)
				: manager.ExecuteWrite(type, this);
		}
	}
}
