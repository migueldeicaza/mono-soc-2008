//#if NET_4_0
// Future.cs
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

namespace System.Threading.Tasks
{
	
	public class Task<T>: Task
	{
		T value;
		
		Func<object, T> function;
		object state;
		
		public T Value {
			get {
				Wait ();
				return value;
			}
			private set {
				this.value = value;
			}
		}
		
		public Task (Func<T> function) : this (function, TaskCreationOptions.None)
		{
			
		}
		
		public Task (Func<T> function, TaskCreationOptions options) : this ((o) => function (), null, options)
		{
			
		}
		
		public Task (Func<object, T> function, object state) : this (function, state, TaskCreationOptions.None)
		{
			
		}
		
		public Task (Func<object, T> function, object state, TaskCreationOptions options)
			: base (null, state, options)
		{
			this.function = function;
			this.state = state;
		}
		
		protected override void InnerInvoke ()
		{
			value = function (state);
		}
		
		public Task ContinueWith (Action<Task<T>> a)
		{
			return ContinueWith (a, TaskContinuationOptions.None);
		}
		
		public Task ContinueWith (Action<Task<T>> a, TaskContinuationOptions options)
		{
			return ContinueWith (a, TaskScheduler.Current, options);
		}
		
		public Task ContinueWith (Action<Task<T>> a, TaskScheduler scheduler)
		{
			return ContinueWith (a, scheduler, TaskContinuationOptions.None);
		}
		
		public Task ContinueWith (Action<Task<T>> a, TaskScheduler scheduler, TaskContinuationOptions options)
		{
			Task t = new Task ((o) => a ((Task<T>)o), this, TaskCreationOptions.None);
			ContinueWithCore (t, options, scheduler);
			
			return t;
		}
		
		/*public Task ContinueWith (Action<Task<T>> a)
		{
			return ContinueWith (a, TaskContinuationKind.OnAny, TaskCreationOptions.None);
		}
		
		public Task ContinueWith (Action<Task<T>> a, TaskContinuationKind kind)
		{
			return ContinueWith (a, kind, TaskCreationOptions.None);
		}
		
		public Task ContinueWith (Action<Task<T>> a, TaskContinuationKind kind, TaskCreationOptions option)
		{
			return ContinueWith (a, kind, option, false);
		}
		
		public Task ContinueWith (Action<Task<T>> a, TaskContinuationKind kind, TaskCreationOptions option, bool exSync)
		{
			Task continuation = new Task (TaskManager.Current, delegate {
				a (this);
			}, null, option);
			ContinueWithCore (continuation, kind, exSync);
			
			return continuation;
		}
		
		public Task<U> ContinueWith<U> (Func<Task<T>, U> a)
		{
			return ContinueWith (a, TaskContinuationKind.OnAny, TaskCreationOptions.None);
		}
		
		public Task<U> ContinueWith<U> (Func<Task<T>, U> a, TaskContinuationKind kind)
		{
			return ContinueWith (a, kind, TaskCreationOptions.None);
		}
		
		public Task<U> ContinueWith<U> (Func<Task<T>, U> a, TaskContinuationKind kind, TaskCreationOptions option)
		{
			return ContinueWith<U> (a, kind, option, false);
		}
		
		public Task<U> ContinueWith<U> (Func<Task<T>, U> a, TaskContinuationKind kind, TaskCreationOptions option, bool exSync)
		{
			Task<U> continuation = new Task<U> (TaskManager.Current, delegate { 
				return a (this);
			}, option, false);
			ContinueWithCore (continuation, kind, exSync);
			
			return continuation;
		}*/
	}
}
//#endif
