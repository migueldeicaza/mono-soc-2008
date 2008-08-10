// BufferedActor.cs
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Collections;

namespace System.Threading.Actors
{	
	public class BufferedActor<T>: IActor<T>
	{	
		ConcurrentQueue<ActorMessage<T>> mbox = new ConcurrentQueue<ActorMessage<T>>();
		
		public BufferedActor(Action<BufferedActor<T>> body)
		{
			Task.Create(delegate { body(this); });
		}
		
		public void Send(object sender, T message)
		{
			mbox.Enqueue(new ActorMessage<T>(sender, message));
		}
		
		public void Receive(Action<ActorMessage<T>> handler)
		{
			bool flag = true;
			
			Combinators.LoopWhile(delegate {
				flag = !TryReceive(handler);
			}, () => flag).Wait();
		}
		
		public T SendToAndWait<U>(IActor<U> dest, U message)
		{
			T data;
			dest.Send(this, message);
			bool flag = true;
			while (flag) {
				Receive((m) => {
					if (m.Sender == dest) {
						data = m.Message;
						flag = false;
					} else {
						mbox.Enqueue(m);
					}	
				});
			}
			
			return data;
		}
		
		public bool TryReceive(Action<ActorMessage<T>> handler)
		{
			ActorMessage<T> message;
			if (mbox.TryDequeue(out message)) {
				Task.Create(delegate { handler(message); });
				return true;
			}

			return false;
		}
	}
}
