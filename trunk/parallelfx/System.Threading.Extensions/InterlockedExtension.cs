// InterlockedExtension.cs
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

namespace System.Threading
{
	#region Wrapper definition
	public class Swappable<T>
	{
		internal T InternalValue;
		// Used by MCas algorithm
		internal MCasDesc<T> Description;
		// Use it with parcimony
		internal SpinLock Lock = new SpinLock(false);

		public Swappable(T value)
		{
			InternalValue = value;
		}

		public T Value {
			get {
				return InternalValue;
			} set {
				InternalValue = value;
			}
		}
	}
	#endregion
	
	#region Helper objects
	internal enum Status {
		Undefined = 0,
		Successful,
		Failed
	}
	
	internal class MCasDesc<T>
	{
		public Swappable<T>[] Locations;
		public T[] Expecteds;
		public T[] NewValues;
		
		public int Stat;
		
		public MCasDesc(Swappable<T>[] locations, T[] expecteds, T[] newValues)
		{
			Locations = locations;
			Expecteds = expecteds;
			NewValues = newValues;
			Stat = (int)Status.Undefined;
		}
	}
	
	/*internal class CCasDesc<T>
	{
		public T e, a, n;
		public Status status;
		
		public CCasDesc(T e, T a, T n, Status status)
		{
			this.e = e;
			this.a = a;
			this.n = n;
			
			this.status = status;
		}
	}*/
	#endregion
	
	public static class InterlockedExtension
	{
		/*
		 * To work correctly MCAS'ed locations must *always* be passed in the same order
		 * e.g. : Thread1 { ... MCAS(new T[] { a1, a2, a3 }, .) ... } / Thread2 { ... MCAS(new T[] { a1, a2, a3 }, .) ... }
		 * and not : Thread1 { ... MCAS(new T[] { a1, a2, a3 }, .) ... } / Thread2 { ... MCAS(new T[] { a3, a1, a2 }, .) ... }
		*/

		public static bool MultiCompareAndSwap<T>(Swappable<T>[] locations, T[] expecteds, T[] newValues) where T : class
		{
			if (expecteds.Length != newValues.Length)
				throw new ArgumentOutOfRangeException("expected/newValues");
			
			MCasDesc<T> d = new MCasDesc<T>(locations, expecteds, newValues);
			
			return MultiCompareAndSwapInternal(d);
		}

		static bool MultiCompareAndSwapInternal<T>(MCasDesc<T> d) where T : class
		{
			Status desired = Status.Failed;
			bool success = true;
			
			for (int i = 0; i < d.Locations.Length && success; i++) {
				Swappable<T> a = d.Locations[i];
				T e = d.Expecteds[i];
				while (true) {
					CCas(a, d, e, ref d.Stat);
					// CCas failed, continue
					if (a.InternalValue == e && d.Stat == (int)Status.Undefined)
						continue;
					// CCas succeeded, move to the next element
					if (a.Description == d)
						break;
					
					MCasDesc<T> desc;
					if (!IsMCasDesc(a, out desc)) {
						success = false;
						break;
					}
				}
			}

			if (success)
				desired = Status.Successful;

			Interlocked.CompareExchange(ref d.Stat, (int)desired, (int)Status.Undefined);
			success = d.Stat == (int)Status.Successful;

			for (int i = 0; i < d.Locations.Length && success; i++)
				Interlocked.CompareExchange(ref d.Locations[i].InternalValue,
				                            success ? d.NewValues[i] : d.Expecteds[i], d.Expecteds[i]);

			return success;
		}
		
		static void CCas<T>(Swappable<T> location, MCasDesc<T> d, T expected, ref int cond) where T : class
		{
			// Here we use a lock for the sake of simplicity and because it's just an overhead of a boolean operation
			// Alternatively we would use a similar scheme than Swappable<T>
			try {
				location.Lock.Enter();
				if (cond == 0 && location.InternalValue == expected)
					location.Description = d;
			} finally {
				location.Lock.Exit();
			}
		}
		
		static bool IsMCasDesc<T>(Swappable<T> location, out MCasDesc<T> desc)
		{
			return (desc = location.Description) != null;
		}
	}
}
