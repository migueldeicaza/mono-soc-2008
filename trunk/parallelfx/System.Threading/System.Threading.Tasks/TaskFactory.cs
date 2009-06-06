// 
// TaskFactory.cs
//  
// Author:
//       Jérémie "Garuma" Laval <jeremie.laval@gmail.com>
// 
// Copyright (c) 2009 Jérémie "Garuma" Laval
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

using System;
using System.Threading;

namespace System.Threading.Tasks
{
	
	public class TaskFactory
	{
		TaskScheduler scheduler;
		TaskCreationOptions options;
		TaskCreationOptions contOptions;		
		
		#region ctors
		public TaskFactory () : this (scheduler.Default, TaskCreationOptions.None, TaskCreationOptions.None)
		{	
		}
		
		public TaskFactory (TaskScheduler scheduler) : this (scheduler, TaskCreationOptions.None, TaskCreationOptions.None)
		{	
		}
		
		public TaskFactory (TaskCreationOptions options, TaskCreationOptions contOptions)
			: this (TaskScheduler.Default, options, contOptions)
		{	
		}
		
		public TaskFactory (TaskScheduler scheduler, TaskCreationOptions options, TaskCreationOptions contOptions)
		{
			this.scheduler = scheduler;
			this.options = options;
			this.contOptions = contOptions;
		}
		#endregion
		
		#region StartNew
		public Task StartNew (Action action)
		{
			return StartNew (action, TaskCreationOptions.None);
		}
		
		public Task StartNew (Action action, TaskCreationOptions options)
		{
			return StartNew (action, options, null);
		}
		
		public Task StartNew (Action action, TaskCreationOptions options, TaskScheduler scheduler)
		{
			return StartNew ((o) => action (), null, options, scheduler);
		}
		
		public Task StartNew (Action<object> action, object state)
		{
			return StartNew (action, state, TaskManager.Current, TaskCreationOptions.None);
		}
		
		public Task StartNew (Action<object> action, object state, TaskCreationOptions options)
		{
			return StartNew (action, null, options, null);
		}
		
		public Task StartNew (Action<object> action, object state, TaskCreationOptions options, TaskScheduler scheduler)
		{
			return null;
		}
		#endregion
		
		#region Continue
		public Task ContinueWhenAny(Task[] tasks, Action<Task> continuationAction)
		{
			return ContinueWhenAny (tasks, continuationAction, contOptions);
		}
		
		public Task ContinueWhenAny(Task[] tasks, Action<Task> continuationAction, TaskContinuationOptions continuationOptions)
		{
			return ContinueWhenAny (tasks, continuationAction, continuationOptions, scheduler);
		}

		public Task ContinueWhenAny(Task[] tasks, Action<Task> continuationAction, TaskContinuationOptions continuationOptions,
		                            TaskScheduler scheduler)
		{
			return null;
		}
		#endregion
		
		public TaskScheduler Scheduler {
			get {
				return scheduler;
			}
		}
		
		public TaskCreationOptions ContinuationOptions {
			get {
				return contOptions;
			}
		}
		
		public TaskCreationOptions CreationOptions {
			get {
				return options;
			}
		}
	}
}
