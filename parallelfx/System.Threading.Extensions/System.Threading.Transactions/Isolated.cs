
using System;
using System.Threading;

namespace System.Threading.Transactions
{
	public struct CloneContainer
	{
		public readonly object Initial;
		public readonly object Clone;
		
		public CloneContainer(object initial, object clone)
		{
			Clone = clone;
			Initial = initial;
		}
	}
	
	public abstract class Isolated
	{
		internal readonly Swappable<object> Object;
		readonly Func<object, object> cloneFunc;
		
		protected Isolated (object value, Func<object, object> cloneFunc)
		{
			Object = new Swappable<object> (value);
			this.cloneFunc = cloneFunc;
		}
		
		public CloneContainer GetClone ()
		{
			object temp = Object.Value;
			return new CloneContainer(temp, cloneFunc (temp));
		}
	}
	
	public class Isolated<T> : Isolated where T : class
	{
		public Isolated (ICloneable value)
			: base (value, (v) => ((ICloneable)v).Clone())
		{
		}
		
		public Isolated (T value, Func<T, T> cloneFunc)
			: base (value, (v) => cloneFunc ((T)v))
		{
		}
		
		public T Value {
			get {
				object value = Object.Value;
				return (T)value;
			}
		}
	}
}
