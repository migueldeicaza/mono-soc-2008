//
// Gendarme.Reporter.Pipeline class
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//
// 	(C) 2008 Néstor Salceda
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Gendarme.Reporter {
	//What a wonderful methaphor found in the Mono.Linker!
	public class Pipeline {
		List<IAction> actions = new List<IAction> ();

		public void Append (IAction action) 
		{
			if (action == null)
				throw new ArgumentNullException ("action");

			actions.Add (action);
		}

		public IAction[] GetActions () 
		{
			return actions.ToArray ();
		}
		
		public XDocument[] ApplyActions (params XDocument[] documents)
		{
			XDocument[] results = documents;
			foreach (IAction action in actions) 
				results = action.Process (results);
			return results;
		}
	}
}
