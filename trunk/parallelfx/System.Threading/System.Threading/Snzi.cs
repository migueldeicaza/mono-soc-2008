// 
// Snzi.cs
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

namespace System.Threading
{
	internal interface ISnziNode
	{
		bool Query { get; }
		void Arrive ();
		void Depart ();
		bool Reset ();
	}
	
	internal class Snzi
	{
		readonly ISnziNode[] nodes;
		
		const int Depth = 5;
		readonly int count;
		
		[ThreadStaticAttribute]
		int nextIndex;
		
		public Snzi (int num)
		{
			for (int i = 0; i < Depth; i++)
				count += 1 << i;
			
			nodes = new ISnziNode[count];
			nodes[0] = new RootNode ();
			PopulateLeafs (nodes[0], 1);
			PopulateLeafs (nodes[0], 2);
			
			for (int i = 0; i < num; i++)
				nodes[0].Arrive ();
		}
		
		public void Increment ()
		{
			ISnziNode node = nodes[GetRandomIndex ()];
			node.Arrive ();
		}
		
		public void Decrement ()
		{
			ISnziNode node = nodes[GetRandomIndex ()];
			node.Depart ();
		}
		
		public void Reset ()
		{
			ISnziNode node = nodes[GetRandomIndex ()];
			node.Reset ();
		}
		
		public bool IsSet {
			get {
				return nodes[0].Query;
			}
		}
		
		void PopulateLeafs (ISnziNode parent, int index)
		{
			if (index >= count)
				return;
			
			nodes[index] = new LeafNode (parent);
			PopulateLeafs (nodes[index], GetLeft (index));
			PopulateLeafs (nodes[index], GetRight (index));
		}
		
		int GetLeft (int index)
		{
			return 2 * index + 1;
		}
		
		int GetRight (int index)
		{
			return 2 * index + 2;
		}
		
		int GetRandomIndex ()
		{
			return nextIndex = (nextIndex + Environment.TickCount + Thread.CurrentThread.ManagedThreadId) % count;
		}
		
		class LeafNode : ISnziNode
		{
			ISnziNode parent;
			int var;
			
			public LeafNode (ISnziNode parent)
			{
				this.parent = parent;
			}

			#region ISnziNode implementation
			public void Arrive ()
			{
				bool succ = false;
				int undoArr = 0;
				while (!succ) {
					int x = var;
					short c, v;
					Decode (x, out c, out v);
					if (c >= 1) {
						if (Interlocked.CompareExchange (ref var, Encode ((short)(c + 1), v), x) == x)
							succ = true;
					} else if (c == 0) {
						int temp = Encode (-1, (short)(v + 1));
						if (Interlocked.CompareExchange (ref var, temp, x) == x) {
							succ = true;
							c = -1;
							v += 1;
							x = temp;
						}
					} else if (c == - 1) {
						parent.Arrive ();
						if (Interlocked.CompareExchange (ref var, Encode (1, v), x) != x)
							undoArr += 1;
					}
				}
				for (int i = 0; i < undoArr; i++)
					parent.Depart ();
			}
			
			public void Depart ()
			{
				while (true) {
					int x = var;
					short c, v;
					Decode (x, out c, out v);
					if (Interlocked.CompareExchange (ref var, Encode ((short)(c - 1), v), x) == x) {
						if (c == 1)
							parent.Depart ();
						return;
					}
				}
			}
			
			public bool Reset ()
			{
				throw new System.NotImplementedException();
			}
			
			public bool Query {
				get {
					return parent.Query;
				}
			}
			#endregion
			
			int Encode (short c, short v)
			{
				int temp = 0;
				temp |= c;
				temp |= ((int)v) << 16;
				
				return temp;
			}
			
			void Decode (int value, out short c, out short v)
			{
				c = (short)(value & 0xFFFF);
				v = (short)(value >> 16);
			}
		}
		
		class RootNode : ISnziNode
		{
			int var;
			int state;

			#region ISnziNode implementation
			public void Arrive ()
			{
				int temp, x = 0;
				short c, v;
				bool a;
				
				do {
					x = var;	
					
					Decode (x, out c, out a, out v);
					
					if (c == 0)
						temp = Encode (1, true, (short)(v + 1));
					else
						temp = Encode ((short)(c + 1), a, v);
				} while (Interlocked.CompareExchange (ref var, temp, x) != x);
				
				Decode (temp, out c, out a, out v);
				if (a) {
					while (true) {
						int i = state;
						int newI = (i & 0x7FFFFFFF) + 1;
						newI |= 0x8000000;
						if (Interlocked.CompareExchange (ref state, newI, i) == i)
							break;
					}
					Interlocked.CompareExchange (ref var, Encode (c, false, v), temp);
				}
			}
			
			public void Depart ()
			{
				while (true) {
					int x = var;
					short c, v;
					bool a;
					Decode (x, out c, out a, out v);
					
					if (Interlocked.CompareExchange (ref var, Encode ((short)(c - 1), false, v), x) == x) {
						if (c >= 2)
							return;
						while (true) {
							int i = state;
							if (((short)(var >> 16)) != v)
								return;
							int newI = (i & 0x7FFFFFFF) + 1;
							if (Interlocked.CompareExchange (ref state, newI, i) == i)
								return;
						}
					}
				}
			}
			
			public bool Reset ()
			{
				throw new System.NotImplementedException();
			}
			
			public bool Query {
				get {
					return (state & 0x8000000) > 0;
				}
			}
			#endregion
			
			int Encode (short c, bool a, short v)
			{
				int temp = 0;
				temp |= c;
				temp |= a ? 0x8000 : 0;
				temp |= ((int)v) << 16;
				
				return temp;
			}
			
			void Decode (int value, out short c, out bool a, out short v)
			{
				c = (short)(value & 0x7FFF);
				a = (value & 0x8000) > 0;
				v = (short)(value >> 16);
			}
		}
	}
}
