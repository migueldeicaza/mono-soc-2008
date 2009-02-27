// TransactionManager.cs
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
using System.Threading;

namespace Mono.Threading.Transactions
{
	public class TransactionManager : ITransactionManager
	{

		#region ITransactionManager implementation
		public bool ExecuteRead (ExecutionType type, Transaction transaction)
		{
			if (type == ExecutionType.OnlyOnce) {
				return DoRead(transaction.Isolateds, transaction.Action);
			} else {
				while (!DoRead(transaction.Isolateds, transaction.Action))
				{
				}
			}
			
			return true;
		}
		
		bool DoRead(StmObject[] isolateds, Action<object[]> tr)
		{
			object[] args = isolateds.Select((i) => i.Object.Value).ToArray();
			tr(args);
			
			for (int i = 0; i < args.Length; i++)
				if (args[i] != isolateds[i].Object.Object)
					return false;
			return true;
		}
		
		public bool ExecuteWrite (ExecutionType type, Transaction transaction)
		{
			if (type == ExecutionType.OnlyOnce) {
				return DoWrite(transaction.Isolateds, transaction.Action);
			} else {
				while (!DoWrite(transaction.Isolateds, transaction.Action))
				{
				}
			}
			
			return true;
		}
		
		bool DoWrite(StmObject[] isolateds, Action<object[]> tr)
		{
			CloneContainer[] args = isolateds.Select((iso) => iso.GetClone()).ToArray();
			object[] clones = new object[args.Length];
			object[] initials = new object[args.Length];
			
			int i = 0;
			foreach (CloneContainer cont in args) {
				clones[i] = cont.Clone;
				initials[i++] = cont.Initial;
			}
			
			tr(clones);
			return InterlockedEx.MultiCompareAndSwap(isolateds.Select((iso) => iso.Object).ToArray(),
			                                         initials, clones);
		}
		#endregion
		
	}
}
