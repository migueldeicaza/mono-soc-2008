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

namespace System.Threading
{
	public class Task: TaskCoordinator, IDisposable, IAsyncResult
	{
		static Task current;
		
		object asyncState;
		WaitHandle asyncWaitHandle;
		Exception exception;
		bool isCanceled;
		bool isCompleted;
		string name;
		Task parent;
		TaskCreationOptions taskCreationOptions;
		
		public event EventHandler Completed;
			
		public Task()
		{
		}
		
		public static Task Create(Action<object> action)
		{
			throw new NotImplementedException();
		}
		
		public static Task Create(Action<object> action, object state)
		{
			throw new NotImplementedException();
		}
		
		public static Task Create(Action<object> action, TaskManager tm)
		{
			throw new NotImplementedException();
		}
		
		public static Task Create(Action<object> action, TaskCreationOptions options)
		{
			throw new NotImplementedException();
		}
		
		public static Task Create(Action<object> action, string name)
		{
			throw new NotImplementedException();
		}
		
		public static Task Create(Action<object> action, TaskManager tm, TaskCreationOptions options)
		{
			throw new NotImplementedException();
		}
		
		public static Task Create(Action<object> action, TaskManager tm, TaskCreationOptions options,
		                          string name)
		{
			throw new NotImplementedException();
		}
		
		public static Task Create(Action<object> action, object state , TaskManager tm,
		                          TaskCreationOptions options, string name)
		{
			throw new NotImplementedException();
		}
		
		public static Task Current {
			get {
				return current;
			}
		}
		
		public override void Cancel()
		{
			throw new NotImplementedException();
		}
		
		public override void CancelAndWait()
		{
			throw new NotImplementedException();
		}
		
		public override bool CancelAndWait(TimeSpan ts)
		{
			throw new NotImplementedException();
		}
		
		public override bool CancelAndWait(int millisecondsTimeout)
		{
			throw new NotImplementedException();
		}
		
		public override void Wait()
		{
			throw new NotImplementedException();
		}
		
		public override bool Wait(TimeSpan ts)
		{
			throw new NotImplementedException();
		}
		
		public override bool Wait(int millisecondsTimeout)
		{
			throw new NotImplementedException();
		}
		
		public void Dispose()
		{
			throw new NotImplementedException();
		}
		
		public Exception Exception {
			get {
				return exception;	
			}
		}
		
		public override bool IsCanceled {
			get {
				return isCanceled;
			}
		}

		public bool IsCompleted {
			get {
				return isCompleted;
			}
		}

		public string Name {
			get {
				return name;
			}
			set {
				name = value;
			}
		}

		public Task Parent {
			get {
				return parent;
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
		
		protected void Finish()
		{
			throw new NotImplementedException();
		}
		
		protected virtual void Invoke()
		{
			throw new NotImplementedException();
		}
	}
}
