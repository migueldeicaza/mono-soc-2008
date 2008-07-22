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
		
		WriteToFileAction writeAction;

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

		private void WriteToFile (string fileName, XDocument document)
		{
			writeAction.DestinationFile = fileName;
			writeAction.Process (document);
		}

		public XDocument[] Process (params XDocument[] documents)
		{
			if (documents.Length != 1)
				throw new ArgumentException ("You have to pass only one document.", "documents");

			XDocument document = documents[0];

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

			if (writeAction == null)
				writeAction = new WriteToFileAction (String.Format ("{0}.Critical.xml", assemblyName));
			
			WriteToFile (String.Format ("{0}.Critical.xml", assemblyName), critical); 
			WriteToFile (String.Format ("{0}.High.xml", assemblyName), high); 
			WriteToFile (String.Format ("{0}.Medium.xml", assemblyName), medium); 
			WriteToFile (String.Format ("{0}.Low.xml", assemblyName), low); 

			return new XDocument[] {critical, high, medium, low};
		}
	}
}
