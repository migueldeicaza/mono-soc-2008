//
// Unit tests for Gendarme.Reporter.FilterBySeverityAction
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
using System.IO;
using System.Xml.Linq;
using NUnit.Framework;

namespace Gendarme.Reporter.Test {
	[TestFixture]
	public class GenerateDefectsPerAssemblyAndSeverityActionTest {
		XDocument critical, high, medium, low;

		private void GetProcessedDocument ()
		{
			XDocument document = XDocument.Load ("Test/Fakes/Mono.Security.xml");
			Assert.IsNotNull (document);

			XDocument[] documents = new FilterBySeverityAction ().Process (document);
			Assert.IsNotNull (document);
			Assert.AreEqual (4, documents.Length);
			
			critical = documents[0];
			high = documents[1];
			medium = documents[2];
			low = documents[3];
		}

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			GetProcessedDocument ();
		}

		[TestFixtureTearDown]
		public void FixtureTearDown ()
		{
			foreach (string file in Directory.GetFiles (Directory.GetCurrentDirectory (), "*.xml")) 
				File.Delete (file);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void PassMoreThanOneDocumentTest ()
		{
			new FilterBySeverityAction ().Process (critical, high);
		}
		
		[Test]
		public void AllDocumentsExistsTest ()
		{
			Assert.IsNotNull (critical);
			Assert.IsNotNull (high);
			Assert.IsNotNull (medium);
			Assert.IsNotNull (low);
		}

		//TODO: It's dupe, extract to common.
		private int CountDefects (XDocument document)
		{
			int counter = 0;
			
			foreach (XElement rule in document.Root.Element ("results").Elements ()) 
				foreach (XElement target in rule.Elements ("target")) 
					foreach (XElement defect in target.Elements ("defect")) 
						counter++;
			
			return counter;
		}

		[Test]
		public void AmountOfDefectsInCriticalTest ()
		{
			Assert.AreEqual (0, CountDefects (critical));
		}

		[Test]
		public void AmountOfDefectsInHighTest ()
		{
			Assert.AreEqual (13, CountDefects (high));
		}
		
		[Test]
		public void AmountOfDefectsInMediumTest ()
		{
			Assert.AreEqual (2, CountDefects (medium));
		}

		[Test]
		public void AmountOfDefectsInLowTest ()
		{
			Assert.AreEqual (3, CountDefects (low));
		}
	}
}
