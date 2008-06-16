// ConcurrentVector.cs
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

namespace System.Threading.Tasks
{
	internal enum PopTopResult	{
		Succeed,
		Empty,
		Abort
	}
	
	internal class DynamicDeque<T> where T : class
	{
		const int ArraySize = 8;
		
		internal class Node
		{
			public T[] Data = new T[ArraySize];
			public Node Previous;
			public Node Next;
		}
		
		class BottomInfo
		{
			public Node Bottom;
			public int BottomIndex;
		}
		
		class TopInfo
		{
			public Node Top;
			public int TopIndex;
			public int TopTag;
		}
		
		BottomInfo bottom;
		TopInfo    top;
		
		public void PushBottom(T item)
		{
			Node currNode;
			int currIndex;
			DecodeBottom(bottom, out currNode, out currIndex);
			currNode.Data[currIndex] = item;
			
			Node newNode;
			int newIndex;
			if (currIndex > 0) {
				newNode = currNode;
				newIndex = currIndex - 1;
			} else {
				// TODO: this should come out of a shared pool but let's create it everytime for the moment
				newNode = new Node();
				newNode.Next = currNode;
				currNode.Previous = newNode;
				newIndex = ArraySize - 1;
			}
			bottom = EncodeBottom(newNode, newIndex);
		}
		
		public T PopTop(out PopTopResult result)
		{
			TopInfo currTop = top;
			BottomInfo currBottom =  bottom;
			
			Node currTopNode;
			int currTopIndex;
			int currTopTag;
			DecodeTop(currTop, out currTopNode, out currTopIndex, out currTopTag);
		}
		
		public T PopBottom()
		{
			return default(T);
		}
		
		void DecodeBottom(BottomInfo info, out Node node, out int index)
		{
			node = info.Bottom;
			index = info.BottomIndex;
		}
		
		BottomInfo EncodeBottom(Node node, int index)
		{
			BottomInfo temp = new BottomInfo();
			temp.Bottom = node;
			temp.BottomIndex = index;
			return temp;
		}
		
		void DecodeTop(TopInfo info, out Node node, out int index, out int tag)
		{
			node  = info.Top;
			index = info.TopIndex;
			tag = info.TopTag;
		}
		
		TopInfo EncodeBottom(Node node, int index, int tag)
		{
			BottomInfo temp = new BottomInfo();
			temp.Bottom = node;
			temp.BottomIndex = index;
			return temp;
		}
	}
}
