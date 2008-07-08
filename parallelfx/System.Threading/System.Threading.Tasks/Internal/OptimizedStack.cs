// OptimizedStack.cs
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

namespace System.Threading.Tasks
{
	internal class OptimizedStack<T>
	{
		enum Operation {
			Pop,
			Push
		}
		
		class Cell
		{
			public Cell(T data)
			{
				Data = data;
			}
			public T Data;
			public Cell Next;
		}
		
		class ThreadInfo
		{
			public ThreadInfo(Operation op, Cell cell)
			{
				Id = Thread.CurrentThread.ManagedThreadId;
				Op = op;
				Cell = cell;
				Spin = 5;
			
				CollisionFactor = 0.5f;
			}
			
			public int  Id;
			public Operation Op;
			public Cell Cell;
			public int  Spin;
			
			public float CollisionFactor;
			
			public int TimeCounter;
			public int CollisionCounter;
		}
		
		Cell         top;
		ThreadInfo[] location;
		int[]        collision;
		SpinWait wait = new SpinWait();
		
		const int UpperCollisionLimit = 10;
		const int LowerCollisionLimit = -1;
		
		const int UpperTimeLimit = 10;
		const int LowerTimeLimit = -10;
		
		//Random r = new Random();
		
		int count;
		
		public OptimizedStack(int numThread)
		{
			location  = new ThreadInfo[numThread + 1];
			collision = new int[numThread + 1];
		}
		
		/*public T Pop()
		{
			ThreadInfo p = new ThreadInfo(Operation.Pop, null);
			StackOp(p);
			return (p.Cell == null) ? default(T) : p.Cell.Data;
		}*/
		
		public bool TryPop(out T value)
		{
			ThreadInfo p = new ThreadInfo(Operation.Pop, null);
			StackOp(p);
			if (p.Cell == null) {
				value = default(T);
				return false;
			}
			Interlocked.Decrement(ref count);
			value = p.Cell.Data;
			return true;
		}
		
		public void Push(T value)
		{
			Cell cell = new Cell(value);
			ThreadInfo p = new ThreadInfo(Operation.Push, cell);
			StackOp(p);
			Interlocked.Increment(ref count);
		}
		
		public bool IsEmpty {
			get {
				return count == 0;
			}
		}
		
		void StackOp(ThreadInfo p)
		{
			if (!TryStackOp(p))
				LesOp(p);
		}
		
		bool TryStackOp(ThreadInfo p)
		{
			Cell head, next;
			if (p.Op == Operation.Push) {
				head = top;
				p.Cell.Next = head;
				return Interlocked.CompareExchange(ref top, p.Cell, head) == head;
			} else {
				head = top;
				if (head == null) {
					p.Cell = null;
					return true;
				}
				
				next = head.Next;
				if (Interlocked.CompareExchange(ref top, next, head) == head) {
					p.Cell = head;
					return true;
				} else {
					p.Cell = null;
					return false;
				}
			}
		}
		
		void LesOp(ThreadInfo p)
		{
			while (true) {
				location[p.Id] = p;
				int pos = GetPos(p);
				int him = collision[pos];
				while (Interlocked.CompareExchange(ref collision[pos], p.Id, him) != him)
					him = collision[pos];
				if (him != 0) {
					ThreadInfo q = location[him];
					if (q == null) {
						if (++p.CollisionCounter >= UpperCollisionLimit) {
							p.CollisionFactor /= 2;
							p.CollisionCounter = 0;
						}	
					} else if (q.Id == him && q.Op != p.Op) {
						if (Interlocked.CompareExchange(ref location[p.Id], null, p) == p) {
							if (TryCollision(p, q)) {
								if (++p.TimeCounter >= UpperTimeLimit) {
									p.TimeCounter = 0;
									p.Spin *= 2;
								}
								return;
							} else {
								if (--p.CollisionCounter <= LowerCollisionLimit) {
									p.CollisionFactor *= 2;
									p.CollisionCounter = 0;
								}
								if (--p.TimeCounter <= LowerTimeLimit) {
									p.TimeCounter = 0;
									p.Spin /= 2;
								}
								goto stack;
							}
						} else {
							FinishCollision(p);
							return;
						}
					}
				}
				Sleep(p.Spin);
				if (Interlocked.CompareExchange(ref location[p.Id], null, p) != p) {
					FinishCollision(p);
					return;
				}
				stack:
					if (TryStackOp(p))
						return;
			}
		}
		
		bool TryCollision(ThreadInfo p, ThreadInfo q)
		{
			if (p.Op == Operation.Push) {
				return Interlocked.CompareExchange(ref location[q.Id], p, q) == q;
			} else if (p.Op == Operation.Pop) {
				if (Interlocked.CompareExchange(ref location[q.Id], null, q) == q) {
					p.Cell = q.Cell;
					location[p.Id] = null;
					return true;
				}
			}
			
			return false;
		}
		
		int rr = 0;
		int GetPos(ThreadInfo p)
		{
			int temp = Interlocked.Increment(ref rr);
			int reduced = (int)(collision.Length * p.CollisionFactor);
			int factor = (collision.Length - reduced) / 2;
			int up = collision.Length - 1 - factor, down = factor;
			return (temp % (up - down)) + down;
			//return r.Next(down, up);
		}
		
		void FinishCollision(ThreadInfo p)
		{
			if (p.Op == Operation.Pop) {
				p.Cell = location[p.Id].Cell;
				location[p.Id] = null;
			}
		}
		
		void Sleep(int iterations)
		{
			for (int i = 0; i < iterations; i++)
				wait.SpinOnce();
		}
	}
}
