//
// Gendarme.Report.GenerateDefectsPerAssemblyAction class
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
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Gendarme.Reporter {
	public class GenerateDefectsPerAssemblyAction : IAction {

		private XDocument AddFilesSection (XDocument generated, string assemblyName, XElement filesSection)
		{
			generated.Root.Add (new XElement ("files", 
				from file in filesSection.Elements ()
				where file.Attribute ("Name").Value.Split (',')[0] == assemblyName
				select new XElement ("file", 
					new XAttribute ("Name", file.Attribute ("Name")),
					new XText (file.Value))));
			return generated;
		}


		private XDocument CreateDocument ()
		{
			return new XDocument (new XDeclaration ("1.0", "utf-8", "yes"),
				new XProcessingInstruction ("xml-stylesheet", "type='text/xsl' href='gendarme.xsl'"));
		}

		private XDocument AddResultsSection (XDocument document, string assemblyName, XElement results)
		{
			document.Root.Add (new XElement ("results", 
				from rule in results.Elements ()
				select 
					new XElement ("rule", 
					new XAttribute ("Name", rule.Attribute ("Name").Value),
					new XAttribute ("Uri", rule.Attribute ("Uri").Value),
					new XElement ("problem", new XText (rule.Element ("problem").Value)),
					new XElement ("solution", new XText (rule.Element ("solution").Value)),
						from target in rule.Elements ("target")
						where target.Attribute ("Assembly").Value.Split (',')[0] == assemblyName
						select target
					)) 
			);

			foreach (XElement rule in document.Root.Element ("results").Elements ()) 
				if (rule.Elements ("target").Count () <= 0)
					rule.Remove ();

			return document;
		}

		public XDocument Process (XDocument document)
		{
			var filesToWrite = from file in document.Root.Element ("files").Elements ()
				select file.Attribute ("Name").Value.Split (',')[0];
			
			foreach (var file in filesToWrite) {
				XDocument generatedDocument = CreateDocument ();
				//TODO: Extrac some methods in order to preserve
				//the coherence
				generatedDocument.Add (new XElement ("gendarme-output", new XAttribute ("date", document.Root.Attribute("date").Value)));
				generatedDocument = AddFilesSection (generatedDocument, file, document.Root.Element ("files"));
				generatedDocument.Root.Add (document.Root.Element ("rules"));
				generatedDocument = AddResultsSection (generatedDocument, file, document.Root.Element ("results"));
			
				new WriteToFileAction (String.Format ("{0}.xml", file)).Process (generatedDocument);
			}

			return document;
		}
	}
}
