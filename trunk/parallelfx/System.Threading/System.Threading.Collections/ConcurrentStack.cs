// ConcurrentStack.cs
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
	
	
	public class ConcurrentStack<T>: IConcurrentCollection<T>, IEnumerable<T>, ICollection, IEnumerable, ISerializable, IDeserializationCallback
	{
		class Node
		{
			public T Value = default(T);
			public Node Next = null;
		}
		
		//Node head = new Node();
		Node head = null;
		
		int count;
		
		public bool Add(T elem)
		{
			Push(elem);
			return true;
		}
		
		/// <summary>
		/// </summary>
		/// <param name="element"></param>
		public void Push(T element)
		{
			Node temp = new Node();
			temp.Value = element;
			
			//temp.Next = Interlocked.Exchange<Node>(ref head, temp);
			do {
				temp.Next = head;
			} while (Interlocked.CompareExchange<Node>(ref head, temp, temp.Next) != temp.Next);
			
			Interlocked.Increment(ref count);
		}
		
		/// <summary>
		/// </summary>
		/// <returns></returns>
		// The doc seems to say that we should remove the do ... while. Stupid ?
		public bool TryPop(out T value)
		{
			if (IsEmpty) {
				value = default(T);
				return false;
			}	
			
			Node temp;// = Interlocked.Exchange(ref head, head.Next);
			do {
				temp = head;
			} while (Interlocked.CompareExchange<Node>(ref head, temp.Next, temp) != temp);
			
			Interlocked.Decrement(ref count);
			
			value = temp.Value;
			return true;
		}
		
		/// <summary>
		/// </summary>
		/// <returns></returns>
		public bool TryPeek(out T value)
		{
			if (IsEmpty) {
				value = default(T);
				return false;
			}
			
			value = head.Value;
			return true;
		}
		
		public void Clear()
		{
			count = 0;
			head = null;
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
			if (my_head == null) {
				yield break;
			} else {
				do {
					yield return my_head.Value;
				} while ((my_head = my_head.Next) != null);
			}
		}
		
		public void CopyTo (Array array, int index)
		{
			T[] dest = array as T[];
			if (dest == null)
				return;
			CopyTo(dest, index);
		}
		
		void CopyTo(T[] dest, int index)
		{
			IEnumerator<T> e = InternalGetEnumerator();
			int i = index;
			while (e.MoveNext()) {
				dest[i++] = e.Current;
			}
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
			T[] dest = new T[count];
			CopyTo(dest, 0);
			return dest;
		}
		
		/// <value>
		/// </value>
		public int Count {
			get {
				return count;
			}
		}
		
		public bool IsEmpty {
			get {
				return Count == 0;
			}
		}
	}
}