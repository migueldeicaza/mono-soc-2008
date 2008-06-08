// ConcurrentQueue.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace System.Threading.Collections
{
	
	
	public class ConcurrentQueue<T>: IConcurrentCollection<T>, IEnumerable<T>, ICollection, IEnumerable, ISerializable, IDeserializationCallback
	{
		class Node
		{
			public T Value;
			public Node Next;
		}
		
		Node head = new Node();
		Node tail;
		int count;
		
		/// <summary>
		/// </summary>
		public ConcurrentQueue()
		{
			tail = head;
		}
		
		/// <summary>
		/// </summary>
		/// <param name="item"></param>
		public void Enqueue(T item)
		{
			Node temp  = new Node();
			temp.Value = item;
			
			Node oldTail;
			do {
				oldTail = tail;
				oldTail.Next = temp;
			} while (Interlocked.CompareExchange(ref tail, temp, oldTail) != oldTail);
			
			/*Node oldTail = Interlocked.Exchange<Node>(ref tail, temp);
			oldTail.Next = temp;*/
			
			Interlocked.Increment(ref count);
		}
		
		public bool Add (T item)
		{
			Enqueue(item);
			return true;
		}
		
		/// <summary>
		/// </summary>
		/// <returns></returns>
		// FIXME: Same as TryPop in ConcurrentStack. WTF with the Try ?
		public bool TryDequeue(out T value)
		{
			Node temp;
			do {
				temp = head.Next;
			} while (Interlocked.CompareExchange<Node>(ref head.Next, temp.Next, temp) != temp);
			
			Interlocked.Decrement(ref count);
			
			value = temp.Value;
			return true;
		}
		
		/// <summary>
		/// </summary>
		/// <returns></returns>
		public bool TryPeek(out T value)
		{
			Node first = head.Next;
			value = first.Value;
			return true;
		}
		
		public void Clear()
		{
			count = 0;
			tail  = head;
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return (IEnumerator)InternalGetEnumerator();
		}
		
		IEnumerator<T> IEnumerable<T>.GetEnumerator ()
		{
			return InternalGetEnumerator();
		}
		
		IEnumerator<T> InternalGetEnumerator()
		{
			Node my_head = head;
			while ((my_head = my_head.Next) != null) {
				yield return my_head.Value;
			}
		}
		
		public void CopyTo (Array array, int index)
		{
			throw new NotImplementedException ();
		}
		
		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}
		
		public bool IsSynchronized {
			get { return true; }
		}

		public void OnDeserialization (object sender)
		{
			throw new NotImplementedException ();
		}

		public bool Remove (T item)
		{
			throw new InvalidOperationException("Cannot remove on a Stack");
		}
		
		object syncRoot = new object();
		public object SyncRoot {
			get { return syncRoot; }
		}
		
		public T[] ToArray ()
		{
			throw new NotImplementedException ();
		}

		
		public int Count {
			get {
				return count;
			}
		}
		
		public bool IsEmpty {
			get {
				return count == 0;
			}
		}
	}
}
