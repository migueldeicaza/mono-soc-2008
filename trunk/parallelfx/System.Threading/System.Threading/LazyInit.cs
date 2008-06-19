// LazyInit.cs
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
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace System.Threading
{
	//FIXME: This should be a struct. In a perfect world made this a intern class and construct the corresponding struct as a wrapper
	public class LazyInit<T>: IEquatable<LazyInit<T>>, ISerializable where T : class
	{
		LazyInitMode mode;
		Func<T>      valueSelector;
		Func<bool>   isInitialized;
		T            value;
		Func<T>      specializedValue;
		
		class DataSlotWrapper
		{
			public bool Init;
			public Func<T> Getter;
		}
		
		public LazyInit(Func<T> valueSelector): this(valueSelector, LazyInitMode.AllowMultipleExecution)
		{
		}
		
		public LazyInit(Func<T> valueSelector, LazyInitMode mode)
		{
			this.valueSelector = valueSelector;
			this.mode = mode;
			
			Func<T> finalValue = delegate { return value; };
			Func<bool> finalIsInitialized = delegate { return true; };
			isInitialized = delegate { return false; };
			
			switch (mode) {
				case LazyInitMode.AllowMultipleExecution:
					specializedValue = delegate {
						value = valueSelector();
						isInitialized = finalIsInitialized;
						specializedValue = finalValue;
						return value;
					};
					break;
				case LazyInitMode.EnsureSingleExecution:
					SpinLock sl = new SpinLock(false);
					specializedValue = delegate {
						try {
							sl.Enter();
							if (!isInitialized()) {
								isInitialized = finalIsInitialized;
								value = valueSelector();
								specializedValue = finalValue;
							}
						} finally { sl.Exit(); }
						return value;
					};
					break;
				case LazyInitMode.ThreadLocal:
					LocalDataStoreSlot localStore = Thread.AllocateDataSlot();
					DataSlotWrapper wrapper = new DataSlotWrapper();
					
					wrapper.Getter = delegate {
						T val = valueSelector();
						wrapper.Init = true;
						wrapper.Getter = delegate { return val; };
						return val;
					};
					
					Thread.SetData(localStore, wrapper);
					
					specializedValue = delegate {
						DataSlotWrapper myWrapper = (DataSlotWrapper)Thread.GetData(localStore);
						return myWrapper.Getter();
					};
					isInitialized = delegate {
						DataSlotWrapper myWrapper = (DataSlotWrapper)Thread.GetData(localStore);
						return myWrapper.Init;
					};
					
					break;
			}
		}
		
		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}
		
		
		public bool Equals (LazyInit<T> other)
		{
			throw new NotImplementedException ();
		}
		
		public override bool Equals (object other)
		{
			throw new NotImplementedException ();
		}
		
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		
		public T Value {
			get {
				return specializedValue();
			}
		}
		
		public LazyInitMode Mode {
			get {
				return mode;
			}
		}
		
		public bool IsInitialized {
			get {
				return isInitialized();
			}
		}
	}
}
