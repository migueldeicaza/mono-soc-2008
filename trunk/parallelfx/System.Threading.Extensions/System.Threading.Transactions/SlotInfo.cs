
using System;

namespace System.Threading.Transactions
{
	
	public struct SlotInfo<T>
	{
		public readonly IsolatedOpenMode Mode;
		public readonly Isolated<T> Isolated;
		
		public SlotInfo (Isolated<T> isolated, IsolatedOpenMode mode)
		{
			this.Isolated = isolated;
			this.Mode = mode;
		}
		
		public static implicit operator SlotInfo<T>(Isolated<T> isolated)
		{
			return new SlotInfo<T>(isolated, IsolatedOpenMode.Default);
		}
	}
}
