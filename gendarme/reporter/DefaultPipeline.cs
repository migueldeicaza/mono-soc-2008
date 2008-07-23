//
// Gendarme.Reporter.DefaultPipeline class
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

using System.Xml.Linq;

namespace Gendarme.Reporter {
	public class DefaultPipeline : IPipeline {
		IAction validateInput = new ValidateInputXmlAction ();
		IAction generateDefects = new GenerateDefectsPerAssemblyAction ();
		IAction generateMaster = new GenerateMasterIndexAction ();
		IAction addXSL = AddXSLProcessingInstructionAction.MasterStyle;
		WriteToFileAction writeTo = new WriteToFileAction ("master-index.xml");
		IAction filterBy = new FilterBySeverityAction ();


		public XDocument[] ApplyActions (params XDocument[] documents)
		{
			XDocument[] results;
			results = validateInput.Process (documents);
			results = generateDefects.Process (results);
			foreach (XDocument assemblyDefects in results) 
				filterBy.Process (assemblyDefects);

			results = generateMaster.Process (documents);
			results = addXSL.Process (results);
			results = writeTo.Process (results);

			return results;
		}
	}
}
