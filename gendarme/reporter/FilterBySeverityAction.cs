//
// Gendarme.Reporter.FilterBySeverityAction class
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
using System.Linq;
using System.Xml.Linq;

namespace Gendarme.Reporter {
	public class FilterBySeverityAction : IAction {
		static readonly string Critical = "Critical";
		static readonly string High = "High";
		static readonly string Medium = "Medium";
		static readonly string Low = "Low";

		public XDocument CreateDefectlessDocument (XDocument original)
		{
			return new XDocument (new XDeclaration ("1.0", "utf-8", "yes"),
				new XProcessingInstruction ("xml-stylesheet","type='text/xsl' href='gendarme.xsl'"),
				new XElement ("gendarme-output", original.Root.Attribute ("date"),
				original.Root.Element ("files"),
				original.Root.Element ("rules"),
				new XElement ("results")));

		}

		public void AddDefectsWithSeverity (string severity, XDocument newDocument, XDocument original)
		{
		}

		public XDocument Process (XDocument document)
		{
			string assemblyName = (from file in document.Root.Element ("files").Elements ()
				select file.Attribute ("Name").Value.Split
				(',')[0]).First ();
			
			XDocument critical = CreateDefectlessDocument (document);
			XDocument high = CreateDefectlessDocument (document);
			XDocument medium = CreateDefectlessDocument (document);
			XDocument low = CreateDefectlessDocument (document);

			AddDefectsWithSeverity (Critical, critical, document);
			AddDefectsWithSeverity (High, high, document);
			AddDefectsWithSeverity (Medium, medium, document);
			AddDefectsWithSeverity (Low, low, document);

			new WriteToFileAction (String.Format ("{0}.Critical.xml", assemblyName)).Process (critical);
			new WriteToFileAction (String.Format ("{0}.High.xml", assemblyName)).Process (high);
			new WriteToFileAction (String.Format ("{0}.Medium.xml", assemblyName)).Process (medium);
			new WriteToFileAction (String.Format ("{0}.Low.xml", assemblyName)).Process (low);

			return document;
		}
	}
}
