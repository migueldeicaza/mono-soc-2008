
using System;
using System.Linq;
using System.Threading;
//using System.Collections.Generic;

namespace System.Threading.Transactions
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
