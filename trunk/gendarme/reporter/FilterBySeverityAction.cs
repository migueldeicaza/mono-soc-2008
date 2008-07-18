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

		public XDocument CloneDocument (XDocument original)
		{
			return new XDocument (original);

		}

		public void RemoveDefectsWithoutSeverity (string severity, XDocument newDocument)
		{
			foreach (XElement rule in newDocument.Root.Element ("results").Elements ()) {
				foreach (XElement target in rule.Elements ("target")) {
					foreach (XElement defect in target.Elements ("defect")) {
						if (String.Compare (defect.Attribute ("Severity").Value, severity) != 0)
							defect.Remove ();
					}

					if (target.Elements ("defect").Count () == 0)
						target.Remove ();
				}
				if (rule.Elements ("target").Count () == 0)
					rule.Remove ();
			}
		}

		public XDocument[] Process (XDocument[] documents)
		{
			foreach (XDocument document in documents) {
				string assemblyName = (from file in document.Root.Element ("files").Elements ()
					select file.Attribute ("Name").Value.Split
					(',')[0]).First ();
			
				XDocument critical = CloneDocument (document);
				XDocument high = CloneDocument (document);
				XDocument medium = CloneDocument (document);
				XDocument low = CloneDocument (document);

				RemoveDefectsWithoutSeverity (Critical, critical);
				RemoveDefectsWithoutSeverity (High, high);
				RemoveDefectsWithoutSeverity (Medium, medium);
				RemoveDefectsWithoutSeverity (Low, low);

				new WriteToFileAction (String.Format ("{0}.Critical.xml", assemblyName)).Process (new XDocument[] {critical});
				new WriteToFileAction (String.Format ("{0}.High.xml", assemblyName)).Process (new XDocument[] {high});
				new WriteToFileAction (String.Format ("{0}.Medium.xml", assemblyName)).Process (new XDocument[] {medium});
				new WriteToFileAction (String.Format ("{0}.Low.xml", assemblyName)).Process (new XDocument[] {low});
			}
			return documents;
		}
	}
}
