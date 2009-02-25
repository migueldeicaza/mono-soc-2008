
using System;

namespace System.Threading.Transactions
{
	public interface ITransactionManager
	{
		bool ExecuteRead(ExecutionType type, Transaction transaction);
		bool ExecuteWrite(ExecutionType type, Transaction transaction);
	}
}
