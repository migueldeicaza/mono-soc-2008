// SpinLock.cs
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

	public struct SpinLock
	{
		const int isFree = 0;
		const int isOwned = 1;
		int lockState;
		SpinWait sw;
		int threadWhoTookLock;
		bool isThreadOwnerTrackingEnabled;
		
		public bool IsThreadOwnerTrackingEnabled {
			get {
				return isThreadOwnerTrackingEnabled;
			}
		}
		
		public bool IsHeldByCurrentThread {
			get {
				if (isThreadOwnerTrackingEnabled)
					return lockState == isOwned && Thread.CurrentThread.ManagedThreadId == threadWhoTookLock;
				else
					return lockState == isOwned;
			}
		}

		public SpinLock(bool trackId)
		{
			this.isThreadOwnerTrackingEnabled = trackId;
			this.threadWhoTookLock = 0;
			this.lockState = isFree;
			this.sw = new SpinWait();
		}
		
		public void ReliableEnter(ref bool lockTaken)
		{
			try {
				Enter();
				lockTaken = lockState == isOwned && Thread.CurrentThread.ManagedThreadId == threadWhoTookLock;;
			} catch {
				lockTaken = false;
			}
		}
		
		public void Enter() 
		{
			Thread.BeginCriticalRegion();
			while (true)  {
				// If resource available, set it to in-use and return
				if (Interlocked.Exchange(ref lockState, isOwned) == isFree) {
					threadWhoTookLock = Thread.CurrentThread.ManagedThreadId;
					return;
				}

				// Efficiently spin, until the resource looks like it might 
				// be free. NOTE: Just reading here (as compared to repeatedly 
				// calling Exchange) improves performance because writing 
				// forces all CPUs to update this value
				while (Thread.VolatileRead(ref lockState) == isOwned) {
					sw.SpinOnce();
				}
			}
		}
		
		public bool TryEnter()
		{
			Thread.BeginCriticalRegion();

			// If resource available, set it to in-use and return
			if (Interlocked.Exchange(ref lockState, isOwned) == isFree) {
				threadWhoTookLock = Thread.CurrentThread.ManagedThreadId;
				return true;
			}
			return false;
		}
		
		public bool TryEnter(TimeSpan timeout)
		{
			return TryEnter((int)timeout.TotalMilliseconds);
		}
		
		public bool TryEnter(int milliSeconds)
		{
			Thread.BeginCriticalRegion();
			
			Stopwatch sw = Stopwatch.StartNew();
			bool result = false;
			
			while (sw.ElapsedMilliseconds < milliSeconds) {
				if (Interlocked.Exchange(ref lockState, isOwned) == isFree) {
					threadWhoTookLock = Thread.CurrentThread.ManagedThreadId;
					result = true;
				}
			}
			sw.Stop();
			return result;
		}
		
		public void TryReliableEnter(TimeSpan timeout, ref bool lockTaken)
		{
			TryReliableEnter((int)timeout.TotalMilliseconds, ref lockTaken);
		}
		
		public void TryReliableEnter(int milliSeconds, ref bool lockTaken)
		{
			Thread.BeginCriticalRegion();
			
			Stopwatch sw = Stopwatch.StartNew();
			
			while (sw.ElapsedMilliseconds < milliSeconds) {
				ReliableEnter(ref lockTaken);
			}
			sw.Stop();
		}

		public void Exit() 
		{ 
			Exit(false);
		}

		public void Exit(bool flushReleaseWrites) 
		{ 
			// Mark the resource as available
			if (flushReleaseWrites) {
				//Interlocked.Exchange(ref lockState, isFree);
				lockState = isFree;
			} else {
				Thread.VolatileWrite(ref lockState, isFree);
			}
			Thread.EndCriticalRegion();
		}
	}
}
