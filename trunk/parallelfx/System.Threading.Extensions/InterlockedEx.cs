
using System;

namespace System.Threading
{
	
	public class Swappable<T> where T : class
	{
		internal T Object;
		internal MCasSlot<T> Slot;
		
		public Swappable(T obj)
		{
			Object = obj;
		}
		
		public Swappable()
		{
		}
		
		public T Value {
			get {
				return Object;
			}
		}
	}
	
	internal enum MCasState
	{
		// No description
		Undefined,
		// First loop, description is being implemented
		Implementing,
		// Working on getting everything CAS-ified
		Succeed,
		// Failed state, you may recycle the slot
		Failed
	}
	
	internal class MCasDesc<T> where T : class
	{
		internal readonly T[] Expecteds;
		internal readonly T[] NewValues;
		
		public MCasDesc(T[] expecteds, T[] newValues)
		{
			this.Expecteds = expecteds;
			this.NewValues = newValues;
		}
		
		//internal volatile bool Succeed = true;
		internal volatile MCasState State = MCasState.Undefined;
	}
	
	internal class MCasSlot<T> where T : class
	{
		internal readonly MCasDesc<T> Desc;
		internal readonly int Index;
		
		public MCasSlot(MCasDesc<T> desc, int index)
		{
			this.Desc = desc;
			this.Index = index;
		}
	}
		
	public static class InterlockedEx
	{
		// A not wait-free version, though negligible overhead for small input
		public static bool MultiCompareAndSwap<T>(Swappable<T>[] locations,
		                                          T[] expected, T[] newValues) where T : class
		{
			if (locations.Length != expected.Length || locations.Length != newValues.Length)
				throw new ArgumentOutOfRangeException("parameters length mismatch");
			
			SpinWait wait = new SpinWait();
			MCasDesc<T> desc = new MCasDesc<T>(expected, newValues);
			desc.State = MCasState.Implementing;
			for (int i = 0; i < locations.Length && desc.State != MCasState.Failed; i++) {
				Swappable<T> isolated = locations[i];
				MCasSlot<T> slot = new MCasSlot<T>(desc, i);
				while (desc.State != MCasState.Failed) {
					MCasSlot<T> s = null;
					
					// Try to implement our slot
					if ((s = Interlocked.CompareExchange(ref isolated.Slot, slot, null)) != null) {
						// TODO: instead of waiting, help by implementing other descriptor
						// by exposing index i in desc and manipulate it via Interlocked.Increment
						if (s.Desc.State == MCasState.Implementing) {
							wait.SpinOnce();
							continue;
						}
						// Already a slot present. Helps the existing MCas process
						int j = s.Index;
						if (s.Desc.State == MCasState.Succeed) {
							T ret = Interlocked.CompareExchange(ref isolated.Object,
							                                    s.Desc.NewValues[j],
							                                    s.Desc.Expecteds[j]);
							
							
							// 1/ Failed CAS or 2/ someone already used CAS successfully
							if (ret != s.Desc.Expecteds[j] && ret != s.Desc.NewValues[j])
								s.Desc.State = MCasState.Failed;
						}
						Interlocked.CompareExchange(ref isolated.Slot, null, s);
					} else {
						if (locations[i].Object != expected[i])
							desc.State = MCasState.Failed;
						break;
					}
				}
			}
			
			
			if (desc.State != MCasState.Failed) {
				desc.State = MCasState.Succeed;
				// We acquired everything so update where it is necessary
				for (int i = 0; i < locations.Length; i++) {
					Swappable<T> isolated = locations[i];
					MCasSlot<T> s = isolated.Slot;
					if (s != null && s.Desc == desc)
						Interlocked.CompareExchange(ref isolated.Object,
						                            newValues[i], expected[i]);
				}
			}
			
			return desc.State == MCasState.Succeed;
		}
	}
}
