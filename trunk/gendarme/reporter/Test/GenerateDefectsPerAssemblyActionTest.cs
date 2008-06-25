//
// Unit test for Gendarme.Reporter.GenerateDefectsPerAssemblyAction
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

using System.IO;
using System.Xml.Linq;
using NUnit.Framework;

namespace Gendarme.Reporter.Test {
	[TestFixture]
	public class GenerateDefectsPerAssemblyActionTest {
		static readonly string xmlFile = "Test/Fakes/06-18-2008.xml";
		static readonly string sampleGeneratedFile = "Mono.Security.xml";

		private void GetProcessedDocument ()
		{
			XDocument document = XDocument.Load (xmlFile);
			Assert.IsNotNull (document);

			document = new GenerateDefectsPerAssemblyAction ().Process (document);
			Assert.IsNotNull (document);
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
		public void WrittenFilesTest ()
		{
			Assert.IsTrue (File.Exists (sampleGeneratedFile));
		}

		[Test]
		public void ContentTest ()
		{
				
		}
	}
}
