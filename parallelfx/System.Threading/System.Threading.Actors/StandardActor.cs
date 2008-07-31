// StandardActor.cs
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
using System.Threading.Tasks;
using System.Threading.Collections;

namespace System.Threading.Actors
{
	// This actors is similar to SimpleActor except that it enforces execution of the action
	// by only one thread at a time. Additionnal actions are stored to a mailbox (queue) waiting for execution.
	public class StandardActor<T>: IActor<T>
	{
		ConcurrentQueue<T> mbox = new ConcurrentQueue<T>();
		Action<T> action;
		
		// Flag telling if there is processing occuring
		int       isRunning;
		const int running = 1;
		const int idle    = 0;
		
		public StandardActor(Action<T> action)
		{
			this.action = action;
		}
		
		public void Act(T data)
		{
			int result = Interlocked.Exchange(ref isRunning, running);
			// If idle then we process the data and create a continuation to test if there has been
			// further work added while we were processing
			if (result == idle) {
				Task.Create(delegate {
					action(data);
					T other;
					while (mbox.Remove(out other)) {
						action(other);
					}
					Thread.VolatileWrite(ref isRunning, idle);
				});
			} else {
				mbox.Add(data);
			}
		}
	}
}
