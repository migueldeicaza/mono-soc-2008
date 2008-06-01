// TaskCoordinator.cs
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
	
	
	public abstract class TaskCoordinator
	{
		public abstract void Cancel();
		public abstract void CancelAndWait();
		public abstract bool CancelAndWait(TimeSpan ts);
		public abstract bool CancelAndWait(int millisecondsTimeout);
		public abstract bool IsCanceled { get; }
		
		public abstract void Wait();
		public abstract bool Wait(TimeSpan ts);
		public abstract bool Wait(int millisecondsTimeout);
		
		// TODO: for each [] style method provide an equivalent IEnumerable<T> 
		public static void WaitAll(params TaskCoordinator[] tasks)
		{
			foreach (var t in tasks)
				t.Wait();
		}
		
		public static bool WaitAll(TaskCoordinator[] tasks, TimeSpan ts)
		{
			bool result = true;
			foreach (var t in tasks)
				result &= t.Wait(ts);
			return result;
		}
		
		public static bool WaitAll(TaskCoordinator[] tasks, int millisecondsTimeout)
		{
			bool result = true;
			foreach (var t in tasks)
				result &= t.Wait(millisecondsTimeout);
			return result;
		}
	}
}
