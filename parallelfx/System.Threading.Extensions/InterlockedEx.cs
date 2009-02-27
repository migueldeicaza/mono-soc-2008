// InterlockedEx.cs
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
using System.Threading;

namespace Mono.Threading
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
	
	internal class MCasDesc
	{
		/*internal readonly T[] Expecteds;
		internal readonly T[] NewValues;
		
		public MCasDesc(T[] expecteds, T[] newValues)
		{
			this.Expecteds = expecteds;
			this.NewValues = newValues;
		}*/
		
		internal volatile MCasState State = MCasState.Undefined;
	}
	
	internal class MCasSlot<T> where T : class
	{
		internal readonly MCasDesc Desc;
		internal readonly T Expected;
		internal readonly T NewValue;
	
		public MCasSlot(MCasDesc desc, T expected, T newValue)
		{
			this.Desc = desc;
			this.Expected = expected;
			this.NewValue = newValue;
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
			MCasDesc desc = new MCasDesc();
			desc.State = MCasState.Implementing;
			for (int i = 0; i < locations.Length && desc.State != MCasState.Failed; i++) {
				Swappable<T> isolated = locations[i];
				MCasSlot<T> slot = new MCasSlot<T>(desc, expected[i], newValues[i]);
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
						if (s.Desc.State == MCasState.Succeed) {
							T ret = Interlocked.CompareExchange(ref isolated.Object,
							                                    s.NewValue,
							                                    s.Expected);
							
							
							// 1/ Failed CAS or 2/ someone already used CAS successfully
							if (ret != s.Expected && ret != s.NewValue)
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
