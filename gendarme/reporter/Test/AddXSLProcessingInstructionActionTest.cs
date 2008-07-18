//
// Unit tests for Gendarme.Reporter.AddXSLProcessingINstructionAction
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
using System.Collections;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Gendarme.Reporter;
using NUnit.Framework;

namespace Gendarme.Reporter.Test {
	[TestFixture]
	public class AddXSLProcessingInstructionActionTest {
		static readonly string xmlFile = "Test/Fakes/06-18-2008.xml";	
		XDocument document;

		private XDocument GetProcessedDocument ()
		{
			XDocument document = XDocument.Load (xmlFile);
			Assert.IsNotNull (document);

			XDocument[] documents = AddXSLProcessingInstructionAction.GendarmeStyle.Process (document);	
			Assert.IsNotNull (documents);
			Assert.AreEqual (1, documents.Length);	
			return documents[0];
		}

		[TestFixtureSetUp]
		public void TestFixtureSetUp ()
		{
			document = GetProcessedDocument ();
		}

		[Test]
		public void ProcessExistenceTest ()
		{
			var query = from element in document.Nodes ()
				where element is XProcessingInstruction
				select element;

			Assert.AreEqual (1, query.Count ());
		}

		[Test]
		public void ProcessLocationTest ()
		{
			var query = from element in document.Nodes ()
				where element is XProcessingInstruction
				select element;

			XProcessingInstruction xslProcessing = (XProcessingInstruction) query.First ();
			
			//This enables browsers format the xml page
			Assert.IsTrue (xslProcessing.IsBefore (document.Root));
		}

		[Test]
		public void SkipOnAlreadyAddedTest ()
		{
			XDocument newDocument = AddXSLProcessingInstructionAction.GendarmeStyle.Process (new XDocument[] {document})[0];

			var query = from element in newDocument.Nodes ()
				where element is XProcessingInstruction
				select element;

			Assert.AreEqual (1, query.Count ());
		}
	}
}
