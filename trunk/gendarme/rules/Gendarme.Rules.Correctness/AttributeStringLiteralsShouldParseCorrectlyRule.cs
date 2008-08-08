//
// Gendarme.Rules.Correctness.AttributeStringLiteralShouldParseCorrectlyRule class
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
using System.Collections;
using Gendarme.Framework;
using Mono.Cecil;

namespace Gendarme.Rules.Correctness {
	[Problem ("")]
	[Solution ("")]
	public class AttributeStringLiteralShouldParseCorrectlyRule : Rule, IMethodRule, ITypeRule {

		private void CheckParametersAndValues (MethodDefinition method, ParameterDefinitionCollection parameters, IList values)
		{
			for (int index = 0; index < values.Count; index++) {
				ParameterDefinition parameter = parameters[index];
				if (String.Compare (parameter.ParameterType.FullName, "System.String") == 0) {
					try {
						string value = (string) values[index];
						if (parameter.Name.Contains ("version")) { 
							new Version (value);
							continue;
						}
						if (parameter.Name.Contains ("url")) {
							new Uri (value);
							continue;
						}
						if (parameter.Name.Contains ("guid")) {
							new Guid (value);
							continue;
						}
					}
					catch {
						Runner.Report (method, Severity.High, Confidence.Low);
					}
				}
			}
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (method.CustomAttributes.Count == 0)
				return RuleResult.DoesNotApply;
			
			foreach (CustomAttribute attribute in method.CustomAttributes) 
				CheckParametersAndValues (method, attribute.Constructor.Parameters, attribute.ConstructorParameters);

			return Runner.CurrentRuleResult;
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			return Runner.CurrentRuleResult;
		}
	}
}
