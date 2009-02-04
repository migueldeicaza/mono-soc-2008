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
	/* Todo : register all access/modification 
	 *
	 */
	/* Probably better to use a interface here */
	public class Transaction
	{
		// Register a list of ITrObject and uses MCas to try update each
		// ITrObject
		IList<TransactionContainer> objects = new List<TransactionContainer>();
		SpinWait sw = new SpinWait();
		
		public Transaction()
		{
			
		}

		internal void Register(TransactionContainer container)
		{
			objects.Add(container);
		}
		
		public bool Commit()
		{
			bool result = false;

			do {
				result = InterlockedExtension.MultiCompareAndSwap<ICloneable>(objects.Select((o) => o.TransactionObject.Object).ToArray(),
				                                                              objects.Select((o) => o.CurrentInstance).ToArray(),
				                                                              objects.Select((o) => o.BaseInstance).ToArray());
			} while (!result && GetDecision());
			
			if (result)
				objects.Clear();
			
			return result;
		}

		// TODO: Get the IConflictManager hooked here
		bool GetDecision()
		{
			sw.SpinOnce();
			return true;
		}
	}
}
