// SpinWait.cs
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
	
	public struct SpinWait
	{
		// The number of step until SpinOnce yield on multicore machine
		const           int  step = 10;
		static readonly bool isSingleCpu = (Environment.ProcessorCount == 1);
		
		int ntime;
		public void SpinOnce() 
		{
			// On a single-CPU system, spinning does no good
			if (isSingleCpu) {
				Yield();
			} else {
				if (Interlocked.Increment(ref ntime) % step == 0) {
					Yield();
				} else {
					// Multi-CPU system might be hyper-threaded, let other thread run
					Thread.SpinWait(10);
				}
			}
		}
		
		void Yield()
		{
			// Replace by Thread.Sleep(0) which does almost the same thing
			// (going back in kernel mode and yielding) but avoid the branching and unmanaged bridge
			Thread.Sleep(0);
			/*if (isWindows)
				SwitchToThread();
			else
				sched_yield();*/
		}
		
		public void Reset()
		{
			ntime = 0;
		}
		
		public bool NextSpinWillYield {
			get {
				return isSingleCpu ? true : ntime % step == 0;
			}
		}

		// scheld_yield() is a POSIX function so it should work on other system (Linux, BSD)
		// than Windows (it's the equivalent of SwitchToThread()), at least present in libc
		/*[DllImport("kernel32.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
		private static extern void SwitchToThread();

		[DllImport("libc.so.6"), SuppressUnmanagedCodeSecurity]
		private static extern void sched_yield();*/
	}
}
