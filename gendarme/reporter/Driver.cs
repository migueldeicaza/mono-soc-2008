//
// Gendarme.Reporter.Driver class
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
using System.Xml.Linq;

namespace Gendarme.Reporter {
	public class Driver {
		static Pipeline DefaultPipeline {
			get {
				Pipeline pipeline = new Pipeline ();
				pipeline.Append (new ValidateInputXmlAction ());
				pipeline.Append (new GenerateDefectsPerAssemblyAction ());
				pipeline.Append (new GenerateMasterIndexAction ());
				pipeline.Append (AddXSLProcessingInstructionAction.MasterStyle);
				pipeline.Append (new WriteToFileAction ("master-index.xml"));
				return pipeline;
			}
		}

		static void Main (string[] args) 
		{
			if (args.Length != 1) {
				Console.WriteLine ("usage: reporter.exe inputfile");
				return;
			}
			XDocument original = XDocument.Load (args[0]);
			DefaultPipeline.ApplyActions (original);
		}
	}
}