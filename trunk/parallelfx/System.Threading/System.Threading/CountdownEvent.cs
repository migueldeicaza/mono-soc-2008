// CountdownEvent.cs
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
using System.Diagnostics;

namespace System.Threading
{	
	public class CountdownEvent: ISupportsCancellation, IDisposable
	{
		int count;
		readonly int initial;
		
		public CountdownEvent(int count)
		{
			this.initial = this.count = count;
		}
		
		public void Decrement()
		{
			Decrement(1);
		}
		
		public void Decrement(int num)
		{
			if (num < 0)
				throw new ArgumentOutOfRangeException("num");
			if (count == 0 || num > count)
				throw new InvalidOperationException();
			Interlocked.Add(ref count, -num);
		}
		
		public void Increment()
		{
			Increment(1);
		}
		
		public void Increment(int num)
		{
			if (num < 0)
				throw new ArgumentOutOfRangeException("num");
			if (count == 0 || num > int.MaxValue - count)
				throw new InvalidOperationException();
			Interlocked.Add(ref count, num);
		}
		
		public bool TryIncrement()
		{
			return TryIncrement(1);
		}
		
		public bool TryIncrement(int num)
		{
			if (count == 0)
				return false;
			if (num < 0)
				throw new ArgumentOutOfRangeException("num");
			if (num > int.MaxValue - count)
				throw new InvalidOperationException();
			
			Interlocked.Add(ref count, num);
			return true;
		}
		
		public void Wait()
		{
			SpinWait wait = new SpinWait();
			while (!IsSet) {
				wait.SpinOnce();
			}
		}
		
		public bool Wait(int timeoutMilli)
		{
			SpinWait wait = new SpinWait();
			Stopwatch sw = Stopwatch.StartNew();
			
			while (!IsSet) {
				if (sw.ElapsedMilliseconds > (long)timeoutMilli) {
					sw.Stop();
					return false;
				}
				wait.SpinOnce();
			}
			return true;
		}
		
		public bool Wait(TimeSpan span)
		{
			return Wait((int)span.TotalMilliseconds);
		}
		
		public int CurrentCount {
			get {
				return count;
			}
		}
		
		public int InitialCount {
			get {
				return initial;
			}
		}
			
		public bool IsSet {
			get {
				return count == 0;
			}
		}
		
		

		#region IDisposable implementation 
		
		public void Dispose ()
		{
			throw new NotImplementedException();
		}
		
		#endregion 
		
		
		#region ISupportsCancellation implementation 
		
		
		public void Cancel()
		{
			
		}
		public bool IsCanceled {
			get {
				return false;
			}
		}
		
		#endregion 
		
	}
}