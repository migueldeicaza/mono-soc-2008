// ScalaActor.cs
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
	public class ScalaActor<T>
	{
		struct Message {
			public ScalaActor Sender;
			public T Mess;
			
			public Message(ScalaActor sender, T message)
			{
				this.Sender = sender;
				this.Mess = message;
			}
		}
		
		ConcurrentQueue<Message> mbox      = new ConcurrentQueue<Message>();
		AtomicBoolean            suspended = false;
		
		Action<ScalaActor, T> continuation;
		
		public ScalaActor(Action<ScalaActor> body)
		{
			Task.Create(delegate { body(this); });
		}
		
		public void AndThen(Action<ScalaActor> initial, params Action<ScalaActor>[] chain)
		{
			Task root = Task.Create(delegate { initial(this); });
			chain.Aggregate(root, (t, a) => t.ContinueWith(delegate { a(this); }));
		}
		
		public void Loop(Action<ScalaActor> body)
		{
			AndThen(body, delegate { Loop(body); });
		}
		
		public void LoopWhile(Action<ScalaActor> body, Func<bool> predicate)
		{
			if (predicate())
				AndThen(body, delegate { Loop(body); });
		}
		
		public void SendTo(ScalaActor actor, T message)
		{
			actor.Send(this, message);
		}
		
		void Send(ScalaActor sender, T message)
		{
			if (suspended.Value)
				continuation(sender, message);
			else
				mbox.Enqueue(new Message(sender, message));
		}
		
		public void React(Action<ScalaActor, T> handler)
		{
			suspended.Value = false;
			Message message;
			if (mbox.TryDequeue(out message)) {
				Task.Create(delegate { handler(message.Sender, message.Mess); });
			} else {
				continuation    = handler;
				suspended.Value = true;
			}
		}
	}
}
