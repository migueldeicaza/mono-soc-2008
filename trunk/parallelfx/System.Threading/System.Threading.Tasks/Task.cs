// Task.cs
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

namespace System.Threading.Tasks
{
	//FIXME: Task normally implements IAsyncResult but it seems strange in the context
	public class Task: TaskBase, IDisposable
	{
		static Task current;
		
		object asyncState;
		WaitHandle asyncWaitHandle;
		Exception exception;
		bool  isCanceled;
		bool isCompleted;
		Task parent = current;
		Task creator = current;
		TaskCreationOptions taskCreationOptions;
		
		protected readonly TaskManager tm;
		Action<object> action;
		object state;
		
		internal event EventHandler Completed;
			
		internal Task(TaskManager tm, Action<object> action, object state, TaskCreationOptions taskCreationOptions)
		{
			this.taskCreationOptions = taskCreationOptions;
			this.tm = TaskManager.Current = tm;
			this.action = action;
			this.state = state;
			
			tm.AddWork(delegate {
				if (isCanceled)
					return;
				current = this;
				InnerInvoke();
				isCompleted = true;
				// Call the event in the correct style
				EventHandler tempCompleted = Completed;
				if (tempCompleted != null)
					tempCompleted(this, EventArgs.Empty);
			});
		}
		
		// TODO : addition : make a generic Create so that the state isn't necessarily object (which is completly stupid)
		public static Task Create(Action<object> action)
		{
			return Create(action, null, TaskManager.Default, TaskCreationOptions.None);
		}
		
		public static Task Create(Action<object> action, object state)
		{
			return Create(action, state, TaskManager.Current, TaskCreationOptions.None);
		}
		
		public static Task Create(Action<object> action, TaskManager tm)
		{
			return Create(action, null, tm, TaskCreationOptions.None);
		}
		
		public static Task Create(Action<object> action, TaskCreationOptions options)
		{
			return Create(action, null, TaskManager.Current, options);
		}
		
		
		public static Task Create(Action<object> action, TaskManager tm, TaskCreationOptions options)
		{
			return Create(action, null, tm, options);
		}
		
		public static Task Create(Action<object> action, object state, TaskManager tm, TaskCreationOptions options)
		{
			Task result = new Task(tm, action, state, options);
						
			return result;
		}
		
		public static Task Current {
			get {
				return current;
			}
		}

		protected virtual void InnerInvoke()
		{
			action(state);
		}
		
		#region Cancel and Wait related methods
		public void Cancel()
		{
			// Mark the Task as canceled so that the worker function will return immediately
			isCanceled = true;
		}
		
		public void CancelAndWait()
		{
			Cancel();
			Wait();
		}
		
		public bool CancelAndWait(TimeSpan ts)
		{
			Cancel();
			return Wait(ts);
		}
		
		public bool CancelAndWait(int millisecondsTimeout)
		{
			Cancel();
			return Wait(millisecondsTimeout);
		}
		
		public void Wait()
		{
			if (this.IsCompleted)
				return;
			tm.WaitForTask(this);
		}
		
		public bool Wait(TimeSpan ts)
		{
			return Wait((int)ts.TotalMilliseconds);
		}
		
		public bool Wait(int millisecondsTimeout)
		{
			if (this.IsCompleted)
				return true;
			
			System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
			bool result = tm.WaitForTaskWithPredicate(this, delegate { return sw.ElapsedMilliseconds >= millisecondsTimeout; });
			sw.Stop();
			return !result;
		}
		
		public static void WaitAll(params Task[] tasks)
		{
			foreach (var t in tasks)
				t.Wait();
		}
		
		public static bool WaitAll(Task[] tasks, TimeSpan ts)
		{
			bool result = true;
			foreach (var t in tasks)
				result &= t.Wait(ts);
			return result;
		}
		
		public static bool WaitAll(Task[] tasks, int millisecondsTimeout)
		{
			bool result = true;
			foreach (var t in tasks)
				result &= t.Wait(millisecondsTimeout);
			return result;
		}
		
		// predicate for WaitAny would be numFinished == 1 and for WaitAll numFinished == count
		public static int WaitAny(params Task[] tasks)
		{
			int numFinished = 0;
			int indexFirstFinished = -1;
			
			foreach (Task t in tasks) {
				t.Completed += delegate (object sender, EventArgs e) { 
					int result = Interlocked.Increment(ref numFinished);
					if (result == 0) {
						Task target = (Task)sender;
						indexFirstFinished = Array.FindIndex(tasks, (elem) => elem == target);
					}
				};	
			}
			
			TaskManager.Current.WaitForTasksUntil(delegate {
				return numFinished >= 1;
			});
			
			return indexFirstFinished;
		}
		
		public static int WaitAny(Task[] tasks, TimeSpan ts)
		{
			return WaitAny(tasks, (int)ts.TotalMilliseconds);
		}
		
		public static int WaitAny(Task[] tasks, int millisecondsTimeout)
		{
			int numFinished = 0;
			int indexFirstFinished = -1;
			
			foreach (Task t in tasks) {
				t.Completed += delegate (object sender, EventArgs e) { 
					int result = Interlocked.Increment(ref numFinished);
					if (result == 0) {
						Task target = (Task)sender;
						indexFirstFinished = Array.FindIndex(tasks, (elem) => elem == target);
					}
				};	
			}
			
			System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
			TaskManager.Current.WaitForTasksUntil(delegate {
				if (sw.ElapsedMilliseconds > millisecondsTimeout)
					return true;
				return numFinished >= 1;
			});
			sw.Stop();
			
			return indexFirstFinished;
		}
		
		protected void Finish()
		{
			
		}
		#endregion
		
		public void Dispose()
		{
			throw new NotImplementedException();
		}
		
		public Exception Exception {
			get {
				return exception;	
			}
		}
		
		public bool IsCanceled {
			get {
				return isCanceled;
			}
		}

		public bool IsCompleted {
			get {
				return isCompleted;
			}
		}

		public Task Parent {
			get {
				return parent;
			}
		}
		
		public Task Creator {
			get {
				return creator;	
			}
		}

		public TaskCreationOptions TaskCreationOptions {
			get {
				return taskCreationOptions;
			}
		}

		public object AsyncState {
			get {
				return asyncState;
			}
		}

		public WaitHandle AsyncWaitHandle {
			get {
				return asyncWaitHandle;
			}
		}
	}
}
