// TaskManagerPolicy.cs
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

namespace System.Threading
{
	
	
	public class TaskManagerPolicy
	{
		readonly int defaultMinThreads = 0;
		readonly int defaultIdealThreads = Environment.ProcessorCount;
		readonly int defaultMaxThread = Environment.ProcessorCount;
		
		int minThreads;
		int idealThreads;
		int maxThreads;
		int maxStackSize;
		bool suppressExecutionContextFlow;
		bool fatalUnhandledExceptions;
		
		public int MinThreads {
			get {
				return minThreads;
			}
		}

		public int IdealThreads {
			get {
				return idealThreads;
			}
		}

		public int MaxThreads {
			get {
				return maxThreads;
			}
		}

		public int MaxStackSize {
			get {
				return maxStackSize;
			}
		}

		public bool SuppressExecutionContextFlow {
			get {
				return suppressExecutionContextFlow;
			}
		}

		public bool FatalUnhandledExceptions {
			get {
				return fatalUnhandledExceptions;
			}
		}
		
		public TaskManagerPolicy()
		{
		}
		
		public TaskManagerPolicy(int minThreads, int idealThreads)
		{
			
		}
		
		public TaskManagerPolicy(int minThreads, int idealThreads, int maxThreads)
		{
			
		}
		
		public TaskManagerPolicy(int maxStackSize)
		{
			
		}
		
		public TaskManagerPolicy(bool suppressExecutionContextFlow)
		{
			
		}
		
		public TaskManagerPolicy(int minThreads, int idealThreads, int maxThreads, int maxStackSize, bool suppressExecutionContextFlow)
			: this(minThreads, idealThreads, maxThreads, maxStackSize, suppressExecutionContextFlow, false)
		{	
		}
		
		public TaskManagerPolicy(int minThreads, int idealThreads, int maxThreads, int maxStackSize, bool suppressExecutionContextFlow,
		                         bool fatalUnhandledExceptions)
		{
			this.minThreads = minThreads;
			this.idealThreads = idealThreads;
			this.maxThreads = maxThreads;
			this.maxStackSize = maxStackSize;
			this.suppressExecutionContextFlow = suppressExecutionContextFlow;
			this.fatalUnhandledExceptions = fatalUnhandledExceptions;
		}
	}
}
