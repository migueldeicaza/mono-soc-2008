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
  public class BufferedActor: IActor
  {	
    ConcurrentQueue<ActorMessage<MessageArgs>> mbox
    = new ConcurrentQueue<ActorMessage<MessageArgs>>();
    
    // Use this ctor if you don't want to subclass, syntax may be a little less sympathic
    // The BufferedActor passed as argument represents this.
    public BufferedActor(Action<BufferedActor> body)
    {
      Task.Create(delegate { body(this); });
    }
    
    // Use this ctor in derived class and override Act method to do the actual processing
    protected BufferedActor()
    {
      Task.Create(delegate { Act(); });
    }
    
    protected virtual void Act()
    {
    }
    
    public void Send(IActor sender, T message)
    {
      mbox.Enqueue(new ActorMessage<T>(sender, message));
    }
    
    // This method blocks by calling TryReceive repeatidly, always sure to return a good result
    public void Receive(Action<ActorMessage<MessageArgs>> handler)
    {
      bool flag = true;
      
      Combinators.LoopWhile(delegate {
	  flag = !TryReceive(handler);
	}, () => flag).Wait();
    }
    
    // This method try to dequeue an element from the message box and if successful
    // launch a Task executing the handler with the dequeued element
    public bool TryReceive(Action<ActorMessage<MessageArgs>> handler)
    {
      ActorMessage<MessageArgs> message;
      if (mbox.TryDequeue(out message)) {
	Task.Create(delegate { handler(message); });
	return true;
      }
      
      return false;
    }
    
    public MessageArgs SendToAndWait(IActor dest, MessageArgs message)
    {
      MessageArgs data;
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
  }
}
