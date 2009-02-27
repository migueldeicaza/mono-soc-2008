// ThreadPoolScheduler.cs
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

namespace System.Threading.Actors
{
	internal class ThreadPoolScheduler : IScheduler
	{
		SpinWait sw = new SpinWait ();
		
		#region IDisposable implementation 
		public void Dispose ()
		{
		}
		#endregion

		#region IScheduler implementation 
		
		public void AddWork (Task t)
		{
			ThreadPool.QueueUserWorkItem (delegate {
				t.Execute (AddWork);
			});
		}
		
		public void Participate ()
		{
			throw new NotSupportedException();
		}
		
		public void ParticipateUntil (Task task)
		{
			while (!task.IsCompleted)
				sw.SpinOnce();
		}
		
		public bool ParticipateUntil (Task task, Func<bool> predicate)
		{
			while (!task.IsCompleted) {
				if (predicate())
					return false;
				sw.SpinOnce();
			}

			return true;
		}
		
		public void ParticipateUntil (Func<bool> predicate)
		{
			while (!predicate())
				sw.SpinOnce();
		}
		
		public void PulseAll ()
		{
			
		}
		
		#endregion 
		
	}
}
