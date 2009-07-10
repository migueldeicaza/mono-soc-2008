// 
// ReaderWriterLockSlim.cs
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
using System.Collections.Generic;

namespace System.Threading
{

	// Based on the FOLL and ROLL Read-Writer locks of Lev, Luchangco and Olszewski
	public class ReaderWriterLockSlim1
	{
		// This keeps track of the local token. Each thread may store multiple ones
		// corresponding to the lock they are touching via Enter or Exit. Actually 
		// that way we can check stuff like if the thread had taken the lock pretty easily
		[ThreadStatic]
		static Dictionary<ReaderWriterLockSlim1, Local> locales;
		
		Local upgradeableLocal = null;
		
		enum LockType {
			Reader,
			Writer,
			Upgradeable
		}
		
		sealed class RwlockNode
		{
			public readonly bool IsReader;
			
			public RwlockNode Next;
			public bool Spin;
			
			// This only used in case the node represents a reader
			public CSnzi Snzi;
			
			public int UseState;
			public bool IsInUse {
				get {
					return UseState == 1;
				}
			}
			
			public RwlockNode ReadNext;
			
			public RwlockNode (bool isReader)
			{
				this.IsReader = isReader;
			}
		}
		
		sealed class Local 
		{
			// Default read node
			public readonly RwlockNode rNode = new RwlockNode (true);
			// Write node
			public readonly RwlockNode wNode = new RwlockNode (false);
			
			public RwlockNode DepartFrom;
			public ICSnziNode Ticket;
			
			public readonly LockType Type;
			
			public Local (LockType type)
			{
				this.Type = type;
			}
		}
		
		[ThreadStatic]
		SpinWait sw;
		
		RwlockNode tail;
		RwlockNode lastRead;
		
		public ReaderWriterLockSlim1 ()
		{
			
		}
		
		#region ReadMode
		public void EnterReadMode ()
		{
			TryEnterReadMode (-1);
		}
		
		public bool TryEnterReadMode (int timeout)
		{		
			return ReaderLock (InitFor (LockType.Reader), timeout == -1 ? null : GetSecondaryForTime (timeout));
		}
		
		public bool TryEnterReadMode (TimeSpan ts)
		{
			return TryEnterReadMode ((int)ts.TotalMilliseconds);
		}
		
		public void ExitReadLock ()
		{
			ReaderUnlock (CheckFor (LockType.Reader));
		}
		#endregion
		
		#region WriterMode
		public void EnterWriteMode ()
		{
			TryEnterReadMode (-1);
		}
		
		public bool TryEnterWriteMode (int timeout)
		{			
			return WriterLock (InitFor (LockType.Writer), timeout == -1 ? null : GetSecondaryForTime (timeout));
		}
		
		public bool TryEnterWriteMode (TimeSpan ts)
		{
			return TryEnterWriteMode ((int)ts.TotalMilliseconds);
		}
		
		public void ExitWriteLock ()
		{			
			WriterUnlock (CheckFor (LockType.Writer));
		}
		#endregion
		
		#region Upgradeable mode
		public void EnterUpgradeableReadLock ()
		{
			
		}
		#endregion
		
		#region Internal behavior
		RwlockNode AllocReaderNode (Local local)
		{
			RwlockNode current = local.rNode;
			do {
				if (!current.IsInUse)
					if (Interlocked.Exchange (ref current.UseState, 1) == 0)
						return current;
			} while ((current = current.Next) != null);
			
			return null;
		}
		
		void FreeReaderNode (RwlockNode node)
		{
			// Simply write the thing up
			node.UseState = 0;
		}
		
		bool WriterLock (Local local, Func<bool> secondary)
		{
			RwlockNode oldTail = Interlocked.Exchange (ref tail, local.wNode);
			if (oldTail != null) {
				local.wNode.Spin = true;
				oldTail.Next = local.wNode;
				if (!oldTail.IsReader) {
					if (local.wNode.Spin)
						SpinWait (() => !local.wNode.Spin, secondary);
				} else {
					if (!oldTail.Snzi.Query ().Open)
						SpinWait (() => oldTail.Snzi.Query ().Open, secondary);
					
					if (oldTail.Snzi.Close ()) {
						if (oldTail.Spin)
							SpinWait(() => !oldTail.Spin, secondary);
						FreeReaderNode (oldTail);
					} else {
						if (local.wNode.Spin)
							SpinWait (() => !local.wNode.Spin, secondary);
					}
				}
			}
			
			return true;
		}
		
		void WriterUnlock (Local local)
		{
			if (local.wNode.Next == null) {
				if (Interlocked.CompareExchange (ref tail, null, local.wNode) == local.wNode)
					return;
				else
					SpinWait (() => local.wNode.Next != null);
				local.wNode.Next.Spin = false;
				local.wNode.Next = null;
			}
		}
		
		bool ReaderLock (Local local, Func<bool> secondary)
		{
			while (true) {
				if (secondary ())
					return false;
				
				// Try to use an existing Reader node
				RwlockNode tempNode = AllocReaderNode (local);
				RwlockNode rNode = Interlocked.CompareExchange (ref lastRead, tempNode, null);
				if (rNode != null) {
					if (rNode.Spin) {
						local.Ticket = rNode.Snzi.Arrive ();
						if (local.Ticket != null) {
							FreeReaderNode (tempNode);
							local.DepartFrom = rNode;
							if (rNode.Spin)
								SpinWait (() => !rNode.Spin, secondary);
							
							return true;
						}
					}
				}
				// We proceed normaly
				rNode = tempNode;
				
				RwlockNode currTail = tail;
				
				// Case we are the first and there is no waiting node
				if (currTail == null) {
					if (rNode == null)
						rNode = AllocReaderNode (local);
					rNode.Spin = false;
					if (Interlocked.CompareExchange (ref tail, rNode, null) == null) {
						rNode.Snzi.Open ();
						local.Ticket = rNode.Snzi.Arrive ();
						if (local.Ticket != null) {
							local.DepartFrom = rNode;
							if (rNode.Spin) {
								SpinWait (() => !rNode.Spin, secondary);
								
							}
							return true;
						}
						rNode = null;
					}
					// Case where there are someone already
				} else {
					// Is it a Writer node that precede us ? Add ourselve behind him
					if (!currTail.IsReader) {
						if (rNode == null)
							rNode = AllocReaderNode (local);
						rNode.Spin = true;
						if (Interlocked.CompareExchange (ref tail, rNode, currTail) == currTail) {
							currTail.Next = rNode;
							local.Ticket = rNode.Snzi.Arrive ();
							if (local.Ticket != null) {
								local.DepartFrom = rNode;
								if (rNode.Spin)
									SpinWait (() => !rNode.Spin, secondary);
								return true;
							}
							rNode = null;
						}
						// Is it a Reader ? Take advantage of it and add ourselves to its Snzi object
						// (NB: since we are using the ROLL philosophy this part of the code shouldn't be
						// reached)
					} else {
						local.Ticket = currTail.Snzi.Arrive ();
						
						if (local.Ticket != null) {
							if (rNode != null)
								FreeReaderNode (rNode);
							local.DepartFrom = currTail;
							if (currTail.Spin)
								SpinWait (() => !currTail.Spin, secondary);
							return true;
						}
					}
				}
			}
		}
		
		void ReaderUnlock (Local local)
		{
			if (local.DepartFrom.Snzi.Depart (local.Ticket))
				return;
			
			local.DepartFrom.Next.Spin = false;
			local.DepartFrom.Next = null;
			FreeReaderNode (local.DepartFrom);
		}
		
		void SpinWait (Func<bool> predicate)
		{
			SpinWait (predicate, null);
		}
		
		bool SpinWait (Func<bool> mainPredicate, Func<bool> secondaryPredicate)
		{
			if (secondaryPredicate == null) {
				sw.SpinUntil (mainPredicate);
				
				return true;
			} else {
				bool result = true;
				sw.SpinUntil (() => { 
					if (secondaryPredicate ()) {
						result = false;
						return true;
					}
					return mainPredicate ();
				});
				
				return result;
			}			
		}
		
		static void InitLocales ()
		{
			if (locales != null)
				return;
			
			locales = new Dictionary<ReaderWriterLockSlim1, Local> ();
		}
		
		Func<bool> GetSecondaryForTime (int millisecondTimeout)
		{
			System.Diagnostics.Stopwatch stopWatch = System.Diagnostics.Stopwatch.StartNew ();
			return () => stopWatch.ElapsedMilliseconds > millisecondTimeout;
		}
		
		Local InitFor (LockType type)
		{
			InitLocales ();
			Local local;
			if (locales.TryGetValue (this, out local))
				throw new LockRecursionException ("Lock is already acquired");
			
			local = new Local (type);
			locales.Add (this, local);
			
			return local;
		}
		
		Local CheckFor (LockType type)
		{
			Local local;
			if (locales == null || !locales.TryGetValue (this, out local)
			    || local.Type != type)
				throw new SynchronizationLockException ();
			
			return local;
		}
		#endregion
	}
}
