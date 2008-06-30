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
	internal enum PopResult	{
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
		
		public DynamicDeque()
		{
			Node temp = new Node();
			bottom = EncodeBottom(temp, ArraySize - 1);
			top = EncodeTop(temp, ArraySize - 1, int.MinValue);
		}
		
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
		
		public PopResult PopTop(out T result)
		{
			TopInfo    currTop = top;
			BottomInfo currBottom =  bottom;
			
			Node currTopNode, newTopNode;
			int currTopIndex, newTopIndex;
			int currTopTag, newTopTag;
			
			DecodeTop(currTop, out currTopNode, out currTopIndex, out currTopTag);
			
			if (EmptinessTest(currTop, currBottom)) {
				result = null;
				if (currTop == top) 
					return PopResult.Empty;
				else
					return PopResult.Abort;
			}
			
			if (currTopIndex != 0) {
				newTopNode = currTopNode;
				newTopIndex = currTopIndex - 1;
				newTopTag = currTopTag;
			} else {
				newTopTag = currTopTag + 1;
				newTopIndex = ArraySize - 1;
				newTopNode = currTopNode.Previous;
			}
			
			TopInfo newTop = EncodeTop(newTopNode, newTopIndex, newTopTag);
			T retVal = currTopNode.Data[currTopIndex];
			if (Interlocked.CompareExchange(ref top, newTop, currTop) == currTop) {
				result = retVal;
				return PopResult.Succeed;
			}
			result = null;
			return PopResult.Abort;
		}
		
		public PopResult PopBottom(out T result)
		{
			Node oldBotNode, newBotNode;
			int oldBotIndex, newBotIndex;
			DecodeBottom(bottom, out oldBotNode, out oldBotIndex);
			
			if (oldBotIndex != ArraySize - 1) {
				newBotNode = oldBotNode;
				newBotIndex = oldBotIndex + 1;
			} else {
				newBotNode = oldBotNode.Next;
				newBotIndex = 0;
			}
			T retVal = newBotNode.Data[newBotIndex];
			// It's ok to touch Bottom like this since only the thread owning DynamicDeque will touch bottom
			bottom = EncodeBottom(newBotNode, newBotIndex);
			
			TopInfo currTop = top;
			Node currTopNode;
			int currTopIndex;
			int currTopTag;
			DecodeTop(currTop, out currTopNode, out currTopIndex, out currTopTag);
			
			// We are attempting to make Bottom cross over Top. Bad. Revert the last EncodeBottom
			if (oldBotNode == currTopNode && oldBotIndex == currTopIndex) {
				bottom = EncodeBottom(oldBotNode, oldBotIndex);
				result = null;
				return PopResult.Empty;
			// Same as before but in the case of the updated bottom info
			} else if (newBotNode == currTopNode && newBotIndex == currTopIndex) {
				// We update top's tag to prevent a concurrent PopTop crossing over
				TopInfo newTop = EncodeTop(currTopNode, currTopIndex, currTopTag + 1);
				// If the CAS fails then it's already to late and we revert back the Bottom position like before to prevent
				// the cross-over
				if (Interlocked.CompareExchange(ref top, newTop, currTop) == currTop) {
					result = retVal;
					return PopResult.Succeed;
				} else {
					bottom = EncodeBottom(oldBotNode, oldBotIndex);
					result = null;
					return PopResult.Empty;
				}
			} else {
				result = retVal;
				return PopResult.Succeed;
			}
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
		
		TopInfo EncodeTop(Node node, int index, int tag)
		{
			TopInfo temp = new TopInfo();
			temp.Top = node;
			temp.TopIndex = index;
			temp.TopTag = tag;
			return temp;
		}
		
		// Take care both of emptiness and cross-over
		bool EmptinessTest(TopInfo topInfo, BottomInfo bottomInfo)
		{
			return object.ReferenceEquals(topInfo.Top, bottomInfo.Bottom) && topInfo.TopIndex >= bottomInfo.BottomIndex;
		}
	}
}
