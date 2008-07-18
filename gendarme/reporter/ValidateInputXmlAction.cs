//
// Gendarme.Reporter.ValidateInputXmlAction class
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
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Linq;
using System.Linq;

namespace Gendarme.Reporter {
	public class ValidateInputXmlAction : IAction {
		IList<string> validationErrors = new List<string> ();

		//Taken from Helpers.cs from Gendarme
		private static Stream GetStreamFromResource (string resourceName)
		{
			Assembly executing = Assembly.GetExecutingAssembly ();
			foreach (string resource in executing.GetManifestResourceNames ()) {
				if (resource.EndsWith (resourceName))
					return executing.GetManifestResourceStream (resource);
			}
			return null;
		}

		private void OnValidationErrors (object sender, ValidationEventArgs args)
		{
			validationErrors.Add (args.Exception.Message.Replace ("XmlSchema error", String.Format ("Error in the Xml file")));	
		}

		public XDocument[] Process (params XDocument[] documents)
		{
			foreach (XDocument document in documents) {
				using (Stream stream = GetStreamFromResource ("gendarme-output.xsd")) {
					if (stream == null)
						throw new InvalidDataException ("Could not locate Xml Schema Deficintion inside resources");

					XmlReaderSettings settings = new XmlReaderSettings ();
					settings.Schemas.Add (XmlSchema.Read (stream, OnValidationErrors));
					settings.ValidationType = ValidationType.Schema;
					settings.ValidationEventHandler += OnValidationErrors;
					using (XmlReader reader = XmlReader.Create (new StringReader (document.ToString ()), settings)) {
						while (reader.Read ()) {}
					}
				}

				foreach (String message in validationErrors)
					Console.Error.WriteLine (message);
			}
			return (validationErrors.Count () == 0) ? documents : null;
		}
	}
}
