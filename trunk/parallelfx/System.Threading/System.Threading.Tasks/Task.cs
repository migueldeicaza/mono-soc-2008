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
	public class Task: TaskBase, IDisposable, IAsyncResult
	{
		// With this attribute each thread has its own value so that it's correct for our Schedule code
		// and for Parent and Creator properties. Though it may not be the value that Current should yield.
		[System.ThreadStatic]
		static Task current;
		
		[System.ThreadStatic]
		internal static Action<Task> childWorkAdder;
		
		static int id = 0;
		
		int taskId;
		WaitHandle asyncWaitHandle;
		Exception exception;
		bool  isCanceled;
		bool respectParentCancellation;
		bool isCompleted;
		Task parent  = current;
		Task creator = current;
		TaskCreationOptions taskCreationOptions;
		
		protected readonly TaskManager tm;
		Action<object> action;
		object state;
		
		EventHandler Completed;
		
		internal ThreadStart threadStart;
			
		internal Task(TaskManager tm, Action<object> action, object state, TaskCreationOptions taskCreationOptions)
		{
			this.taskCreationOptions = taskCreationOptions;
			this.tm = TaskManager.Current = tm;
			this.action = action == null ? delegate (object o) { } : action;
			this.state = state;
			this.taskId = Interlocked.Increment(ref id);

			// Process taskCreationOptions
			if (CheckTaskOptions(taskCreationOptions, TaskCreationOptions.Detached))
				parent = null;
			
			respectParentCancellation = CheckTaskOptions(taskCreationOptions, TaskCreationOptions.RespectCreatorCancellation);
			
			if (CheckTaskOptions(taskCreationOptions, TaskCreationOptions.SelfReplicating))
				Task.Create(action, state, tm, taskCreationOptions);
			
		}
		
		protected void Schedule()
		{
			threadStart = delegate {
				if (!isCanceled
				    && (!respectParentCancellation || (respectParentCancellation && parent != null && !parent.IsCanceled))) {
					current = this;
					try {
						InnerInvoke();
					} catch (Exception e) {
						exception = e;
					}
				} else {
					this.exception = new TaskCanceledException(this);
				}
				
				isCompleted = true;
				// Call the event in the correct style
				EventHandler tempCompleted = Completed;
				if (tempCompleted != null) tempCompleted(this, EventArgs.Empty);
				Finish();
			};
			
			// If worker is null it means it is a local one, revert to the old behavior
			if (current == null || childWorkAdder == null) {
				tm.AddWork(this);
			} else {
				/* Like the semantic of the ABP paper describe it, we add ourselves to the bottom 
				 * of our Parent Task's ThreadWorker deque. It's ok to do that since we are in
				 * the correct Thread during the creation
				 */
				childWorkAdder(this);
			}
		}

		bool CheckTaskOptions(TaskCreationOptions opt, TaskCreationOptions member)
		{
			return (opt & member) == member;
		}
		
		~Task() {
			Dispose(false);
		}
		
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
			result.Schedule();
			return result;
		}
		
		public Task ContinueWith(Action<Task> a)
		{
			return ContinueWith(a, TaskContinuationKind.OnAny, TaskCreationOptions.None);
		}
		
		public Task ContinueWith(Action<Task> a, TaskContinuationKind kind)
		{
			return ContinueWith(a, kind, TaskCreationOptions.None);
		}
		
		public Task ContinueWith(Action<Task> a, TaskContinuationKind kind, TaskCreationOptions option)
		{
			Task continuation = new Task(TaskManager.Current, delegate { a(this); }, null, option);
			ContinueWithCore(continuation, kind, false);
			return continuation;
		}
		
		protected void ContinueWithCore(Task continuation, TaskContinuationKind kind, bool executeSync)
		{
			if (IsCompleted) {
				continuation.Schedule();
				return;
			}
				
			this.Completed += delegate {
				switch (kind) {
					case TaskContinuationKind.OnAny:
						continuation.Schedule();
						break;
					case TaskContinuationKind.OnAborted:
						if (isCanceled)
							continuation.Schedule();
						break;
					case TaskContinuationKind.OnFailed:
						if (exception != null)
							continuation.Schedule();
						break;
					case TaskContinuationKind.OnCompletedSuccessfully:
						if (exception == null && !isCanceled)
							continuation.Schedule();
						break;
				}
			};
		}
		
		public static Task Current {
			get {
				return current;
			}
		}

		protected virtual void InnerInvoke()
		{
			action(state);
			// Set action to null so that the GC can collect the delegate and thus
			// any big object references that the user might have captured in a anonymous method
			action = null;
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
			int index = 0;
			
			foreach (Task t in tasks) {
				if (t.IsCompleted) {
					return index;
				}
				t.Completed += delegate (object sender, EventArgs e) {
					int result = Interlocked.Increment(ref numFinished);
					// Check if we are the first to have finished
					if (result == 1) {
						Task target = (Task)sender;
						indexFirstFinished = Array.FindIndex(tasks, (elem) => elem == target);
					}
				};	
				index++;
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
					if (result == 1) {
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
			Dispose();
		}
		#endregion
		
		public void Dispose()
		{
			Dispose(true);
		}
		
		protected virtual void Dispose(bool disposeManagedRes)
		{
			// Set action to null so that the GC can collect the delegate and thus
			// any big object references that the user might have captured in a anonymous method
			if (disposeManagedRes)
				action = null;
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
				return state;
			}
		}
		
		public bool CompletedSynchronously {
			get {
				return true;
			}
		}

		public WaitHandle AsyncWaitHandle {
			get {
				return asyncWaitHandle;
			}
		}
		
		public int Id {
			get {
				return taskId;
			}
		}
		
		public override string ToString ()
		{
			return Id.ToString();
		}

	}
}
