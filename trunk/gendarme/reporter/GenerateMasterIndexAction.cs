//
// Gendarme.Reporter.GenerateMasterIndexAction class
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
	public class GenerateMasterIndexAction : IAction {
		XDocument originalDocument;
		
		private static XDocument CreateStandardXmlDocument ()
		{
			return new XDocument (new XDeclaration ("1.0", "utf-8", "yes"));
		}

		private static XElement CreateRootElementFrom (XElement data)
		{
			return new XElement ("gendarme-output",
				new XAttribute ("date", data.Attribute ("date").Value));
		}

		private static string GetShortAssemblyName (XAttribute attribute)
		{
			return attribute.Value.Split (',')[0];
		}

		//TODO: Perhaps Linq ...
		private int CountDefects (string assemblyName, string severity)
		{
			int counter = 0;
			foreach (XElement rule in originalDocument.Root.Element ("results").Elements ()) 
				foreach (XElement target in rule.Elements ("target")) 
					if (String.Compare (GetShortAssemblyName (target.Attribute ("Assembly")), assemblyName) == 0) 
						foreach (XElement defect in target.Elements ()) 
							if (String.Compare (defect.Attribute ("Severity").Value, severity) == 0)
								counter++;
			return counter;
		}

		private XElement CreateAssembliesSectionFrom (XElement files)
		{
			return new XElement ("assemblies", 
				from file in files.Elements ()
				select new XElement ("assembly",
					new XAttribute ("shortname", GetShortAssemblyName (file.Attribute ("Name"))),
					new XAttribute ("critical", CountDefects (GetShortAssemblyName (file.Attribute ("Name")), "Critical")),		
					new XAttribute ("high", CountDefects (GetShortAssemblyName (file.Attribute ("Name")), "High")),
					new XAttribute ("medium", CountDefects (GetShortAssemblyName (file.Attribute ("Name")), "Medium")),
					new XAttribute ("low", CountDefects (GetShortAssemblyName (file.Attribute ("Name")), "Low"))
			));	
		}

		public XDocument Process (XDocument document)
		{
			originalDocument = document;
			XDocument newDocument = CreateStandardXmlDocument ();

			newDocument.Add (CreateRootElementFrom (document.Root));
			newDocument.Root.Add (CreateAssembliesSectionFrom (document.Root.Element ("files")));

			return newDocument;
		}
	}
}
