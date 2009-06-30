// 
// CancellationToken.cs
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
using System.Collections.Generic;

namespace System.Threading
{
	public struct CancellationToken
	{
		bool canceled;
		int currId;
		
		// TODO: move these out to CancellationTokenSource and redirect the Register calls there
		// (id management for registration token should move too)
		Dictionary<CancellationTokenRegistration, Action> callbacks;
		Dictionary<CancellationTokenRegistration, ICancelableOperation> cancelables;
		
		public CancellationToken (bool canceled)
		{
			this.canceled = canceled;
			this.callbacks = new Dictionary<CancellationTokenRegistration, Action> ();
			this.cancelables = new Dictionary<CancellationTokenRegistration, ICancelableOperation> ();
			this.currId = int.MinValue;
		}
		
		public CancellationTokenRegistration Register (Action callback)
		{
			return Register (callback, false);
		}
		
		public CancellationTokenRegistration Register (ICancelableOperation cancelable)
		{
			return Register (cancelable, false);
		}
		
		public CancellationTokenRegistration Register (Action callback, bool useSynchronizationContext)
		{
			CancellationTokenRegistration reg = GetTokenReg ();
			
			/*if (canceled)
				callback ();*/
			
			return reg;
		}
		
		public CancellationTokenRegistration Register (Action<object> callback, object state)
		{
			return Register (callback, state, false);
		}
		
		public CancellationTokenRegistration Register (ICancelableOperation cancelable, bool useSynchronizationContext)
		{
			CancellationTokenRegistration reg = GetTokenReg ();
			
			/*if (canceled)
				callback ();*/
			
			return reg;
		}
		
		public CancellationTokenRegistration Register (Action<object> callback, object state, bool useSynchronizationContext)
		{
			return Register (() => callback (state), useSynchronizationContext);
		}
		
		public bool CanBeCanceled {
			get {
				return true;
			}
		}
		
		public bool IsCancellationRequest {
			get {
				
				return false;
			}
		}
		
		CancellationTokenRegistration GetTokenReg ()
		{
			CancellationTokenRegistration registration = new CancellationTokenRegistration ();
			registration.Id = currId++;
			
			return registration;
		}
	}
}
