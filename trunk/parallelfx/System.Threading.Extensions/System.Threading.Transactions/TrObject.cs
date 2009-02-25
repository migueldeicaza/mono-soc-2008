// TrObject.cs
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

namespace System.Threading.Transactions
{
	internal class TransactionContainer
	{
		ITrObject transactionObject;
		ICloneable currentInstance;
		ICloneable baseInstance;
		
		public ITrObject TransactionObject {
			get {
				return transactionObject;
			}
		}
		
		public ICloneable CurrentInstance {
			get {
				return currentInstance;
			}
		}

		public ICloneable BaseInstance {
			get {
				return baseInstance;
			}
		}

		public TransactionContainer(ITrObject transactionObject, ICloneable currentInstance)
		{
			this.transactionObject = transactionObject;
			this.currentInstance = currentInstance;
			this.baseInstance = (ICloneable)currentInstance.Clone();
		}
	}
	
	internal class TrObject<T> : ITrObject, ITrObject<T> where T : class, ICloneable
	{	
		public TrObject(T obj)
		{
			this.Object = new Swappable<ICloneable>(obj);
		}

		public T OpenRead(Transaction tr)
		{
			return Open(tr, TransactionOpeningMode.Read);
		}
		
		public T OpenWrite(Transaction tr)
		{
			return Open(tr, TransactionOpeningMode.Read);
		}

		public T Open(Transaction tr, TransactionOpeningMode mode)
		{
			ICloneable copy = (ICloneable)Object.Value.Clone();
			//tr.Register(new TransactionContainer(this, copy));
			return (T)Object.Value.Clone();
		}
	}
	
}
