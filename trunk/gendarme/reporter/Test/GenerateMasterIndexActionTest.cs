//
// Unit test for Gendarme.Reporter.GenerateMasterIndexAction
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

using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;

namespace Gendarme.Reporter.Test {
	[TestFixture]
	public class GenerateMasterIndexActionTest {
		static readonly string xmlFile = "Test/Fakes/06-18-2008.xml";

		private XDocument GetProcessedDocument () 
		{
			XDocument document = XDocument.Load (xmlFile);
			Assert.IsNotNull (document.Root);

			return new GenerateMasterIndexAction ().Process (document);
		}

		[Test]
		public void HeaderSectionTest ()
		{
			XDocument document = GetProcessedDocument ();
			Assert.IsNotNull (document);
			XElement element = document.Element ("gendarme-output");
			Assert.IsNotNull (element);
			
			Assert.AreEqual (XDocument.Load (xmlFile).Element ("gendarme-output").Attribute ("date"), element.Attribute ("date"));
		}

		[Test]
		public void GeneratedFilesSectionTest ()
		{

		}
	}
}
