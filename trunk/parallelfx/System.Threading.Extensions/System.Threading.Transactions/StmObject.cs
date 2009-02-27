// StmObject.cs
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
using Mono.Threading;

namespace Mono.Threading.Transactions
{
	public struct CloneContainer
	{
		public readonly object Initial;
		public readonly object Clone;
		
		public CloneContainer(object initial, object clone)
		{
			Clone = clone;
			Initial = initial;
		}
	}
	
	public abstract class StmObject
	{
		internal readonly Swappable<object> Object;
		readonly Func<object, object> cloneFunc;
		
		protected StmObject (object value, Func<object, object> cloneFunc)
		{
			Object = new Swappable<object> (value);
			this.cloneFunc = cloneFunc;
		}
		
		public CloneContainer GetClone ()
		{
			object temp = Object.Value;
			return new CloneContainer(temp, cloneFunc (temp));
		}
	}
	
	public class StmObject<T> : StmObject where T : class
	{
		public StmObject (ICloneable value)
			: base (value, (v) => ((ICloneable)v).Clone())
		{
		}
		
		public StmObject (T value, Func<T, T> cloneFunc)
			: base (value, (v) => cloneFunc ((T)v))
		{
		}
		
		public T Value {
			get {
				object value = Object.Value;
				return (T)value;
			}
		}
	}
}
