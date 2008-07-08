// WriteOnce.cs
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
	
	
	public struct WriteOnce<T>: IEquatable<WriteOnce<T>>
	{
		T value;
		int setFlag;
		
		public bool HasValue {
			get {
				return setFlag == 1;
			}
		}
		
		public T Value {
			get {
				if (!HasValue)
					throw new InvalidOperationException("An attempt was made to retrieve the value, but no value had been set, or an attempt was made to set the value when the value was already set.");
				return value;
			}
			set {
				int result = Interlocked.Exchange(ref setFlag, 1);
				if (result == 1)
					throw new InvalidOperationException("An attempt was made to retrieve the value, but no value had been set, or an attempt was made to set the value when the value was already set.");
				this.value = value;
			}
		}
		
		public bool Equals(WriteOnce<T> other)
		{
			return value.Equals(other.value);
		}
		
		public override bool Equals(object other)
		{
			return (other is WriteOnce<T>) ? Equals((WriteOnce<T>)other) : false;
		}
		
		public override int GetHashCode()
		{
			return value.GetHashCode();
		}
	}
}
