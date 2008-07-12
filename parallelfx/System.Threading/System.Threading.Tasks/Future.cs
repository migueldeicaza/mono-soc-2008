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
	public static class Future
	{
		public static Future<T> Create<T>()
		{
			return Create<T>(null, TaskManager.Current, TaskCreationOptions.None);
		}
		
		public static Future<T> Create<T>(Func<T> function)
		{
			return Create<T>(function, TaskManager.Current, TaskCreationOptions.None);
		}
		
		public static Future<T> Create<T>(Func<T> function, TaskCreationOptions options)
		{
			return Create<T>(function, TaskManager.Current, options);
		}
		
		public static Future<T> Create<T>(Func<T> function, TaskManager tm)
		{
			return Create<T>(function, tm, TaskCreationOptions.None);
		}
		
		public static Future<T> Create<T>(Func<T> function, TaskManager tm, TaskCreationOptions options)
		{
			Future<T> future = new Future<T>(tm, function, options, true);
			
			return future;
		}
	}
	
	public sealed class Future<T>: Task
	{
		T value;
		int alreadySet;
		readonly Action func;
		
		public T Value {
			get {
				if (func != null)
					Wait();
				return value;
			}
			set {
				int result = Interlocked.Exchange(ref alreadySet, 1);
				if (result == 1)
					throw new Exception("Value has already been set for this Future");
				this.value = value;
			}
		}
		
		internal Future(TaskManager tm, Func<T> f, TaskCreationOptions options):
			this(tm, f, options, false)
		{	
		}
		
		internal Future(TaskManager tm, Func<T> f, TaskCreationOptions options, bool scheduleNow):
			base(tm, null, null, options)
		{
			func = delegate {
				if (f != null) {
					Value = f();
				}
			};
			if (scheduleNow)
				Schedule();
		}
		
		protected override void InnerInvoke ()
		{
			func();
		}
		
		public static Future<T> Create<T>()
		{
			return Create<T>(null, TaskManager.Current, TaskCreationOptions.None);
		}
		
		public static Future<T> Create<T>(Func<T> function)
		{
			return Create<T>(function, TaskManager.Current, TaskCreationOptions.None);
		}
		
		public static Future<T> Create<T>(Func<T> function, TaskCreationOptions options)
		{
			return Create<T>(function, TaskManager.Current, options);
		}
		
		public static Future<T> Create<T>(Func<T> function, TaskManager tm)
		{
			return Create<T>(function, tm, TaskCreationOptions.None);
		}
		
		public static Future<T> Create<T>(Func<T> function, TaskManager tm, TaskCreationOptions options)
		{
			Future<T> future = new Future<T>(tm, function, options, true);
			
			return future;
		}
	}
}
