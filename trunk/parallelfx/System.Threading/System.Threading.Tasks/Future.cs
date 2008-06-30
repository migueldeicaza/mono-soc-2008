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
			throw new NotImplementedException();
		}
		
		public static Future<T> Create<T>(Func<T> function)
		{
			throw new NotImplementedException();
		}
		
		public static Future<T> Create<T>(Func<T> function, TaskCreationOptions options)
		{
			throw new NotImplementedException();
		}
		
		public static Future<T> Create<T>(Func<T> function, TaskManager tm)
		{
			throw new NotImplementedException();
		}
		
		public static Future<T> Create<T>(Func<T> function, string name)
		{
			throw new NotImplementedException();
		}
		
		public static Future<T> Create<T>(Func<T> function, TaskManager tm, TaskCreationOptions options)
		{
			throw new NotImplementedException();
		}
		
		public static Future<T> Create<T>(Func<T> function, TaskManager tm,
		                             TaskCreationOptions options, string name)
		{
			throw new NotImplementedException();
		}
	}
	
	public class Future<T>: Task
	{
		T value;
		int alreadySet;
		Action action;
		
		public T Value {
			get {
				// TODO: Check the return of the get when there is no Func provided
				Wait();
				
				return value;
			}
			set {
				if (Interlocked.Exchange(ref alreadySet, 1) == 1)
					throw new Exception("Value has already been set for this Future");
				this.value = value;
			}
		}
		
		internal Future(TaskManager tm, Func<T> f, TaskCreationOptions options):
			base(tm, null, null, options)
		{
			this.action = delegate {
				this.Value = f();	
			};
		}
		
		protected override void InnerInvoke ()
		{
			action();
		}
		
		public static Future<T> Create<T>()
		{
			throw new NotImplementedException();
		}
		
		public static Future<T> Create<T>(Func<T> function)
		{
			throw new NotImplementedException();
		}
		
		public static Future<T> Create<T>(Func<T> function, TaskCreationOptions options)
		{
			throw new NotImplementedException();
		}
		
		public static Future<T> Create<T>(Func<T> function, TaskManager tm)
		{
			throw new NotImplementedException();
		}
		
		public static Future<T> Create<T>(Func<T> function, TaskManager tm, TaskCreationOptions options)
		{
			return new Future<T>(tm, function, options);
		}
		
	}
}
