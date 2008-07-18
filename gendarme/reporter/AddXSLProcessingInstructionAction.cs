//
// Gendarme.Reporter.AddXSLProcessingInstructionAction class
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
	public class AddXSLProcessingInstructionAction : IAction {
		string xslReference;

		public AddXSLProcessingInstructionAction (string xslReference)
		{
			this.xslReference = xslReference;
		}

		public XDocument[] Process (XDocument[] documents)
		{
			List<XDocument> results = new List<XDocument> ();
			foreach (XDocument document in documents) {
				results.Add (new XDocument (new XDeclaration ("1.0", "utf-8", "yes"),
					new XProcessingInstruction ("xml-stylesheet", String.Format ("type='text/xsl' href='{0}'", xslReference)), 
					document.Root));
			}

			return results.ToArray ();
		}
		
		public static AddXSLProcessingInstructionAction GendarmeStyle {
			get {
				return new AddXSLProcessingInstructionAction ("gendarme.xsl");
			}
		}

		public static AddXSLProcessingInstructionAction MasterStyle {
			get {
				return new AddXSLProcessingInstructionAction ("master.xsl");
			}
		}
	}
}
