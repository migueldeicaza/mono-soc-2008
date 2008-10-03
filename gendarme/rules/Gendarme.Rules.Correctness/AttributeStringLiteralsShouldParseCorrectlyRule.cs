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
using Gendarme.Framework.Rocks;
using Mono.Cecil;

namespace Gendarme.Rules.Correctness {
	[Problem ("As you are representing urls, versions or guids as strings, those parameters could be bad formatted and could cause some troubles at run time.")]
	[Solution ("You should format correctly the reported parameters.")]
	public class AttributeStringLiteralsShouldParseCorrectlyRule : Rule, IMethodRule, ITypeRule, IAssemblyRule {

		private static bool Contains (string original, string value)
		{
			return original.IndexOf (value, 0, StringComparison.OrdinalIgnoreCase) != -1;
		}
		
		private static bool TryParseUri (string uri)
		{
			Uri parsed = null;
			return Uri.TryCreate (uri, UriKind.Absolute, out parsed);
		}

		private static bool TryParseVersion (string version)
		{
			try {
				new Version (version);
				return true;
			}
			catch (FormatException) {
				return false;
			}
			catch (ArgumentException) {
				return false;
			}
		}

		private static bool TryParseGuid (string guid)
		{
			try {
				new Guid (guid);
				return true;
			}
			catch (FormatException) {
				return false;
			}
		}

		private void CheckParametersAndValues (IMetadataTokenProvider provider, MethodReference constructor, IList values)
		{
			for (int index = 0; index < values.Count; index++) {
				ParameterDefinition parameter = constructor.Parameters[index];
				if (String.Compare (parameter.ParameterType.FullName, "System.String") == 0) {
					string value = (string) values[index];
					if (Contains (parameter.Name, "version")) { 
						if (!TryParseVersion (value)) {
							string msg = String.Format ("The value passed: {0} can't be parsed to a valid Version.", value);
							Runner.Report (provider, Severity.High, Confidence.High, msg);
						}
						continue;
					}
					if (Contains (parameter.Name, "url") ||
						Contains (parameter.Name, "uri") ||
						Contains (parameter.Name, "urn")) {
						if (!TryParseUri (value)) {
							string msg = String.Format ("The valued passed {0} can't be parsed to a valid Uri.", value);
							Runner.Report (provider, Severity.High, Confidence.High, msg);
						}
						continue;
					}
					if (Contains (parameter.Name, "guid")) {
						if (!TryParseGuid (value)) {
							string msg = String.Format ("The valued passed {0} can't be parsed to a valid Guid.", value);
							Runner.Report (provider, Severity.High, Confidence.High, msg);
						}
						continue;
					}
				}
			}
		}
		
		private RuleResult CheckAttributesIn (ICustomAttributeProvider provider)
		{
			//There isn't a relationship between
			//IMetadataTokenProvider and ICustomAttributeProvider,
			//altough a method, or type, implements both interfaces.
			IMetadataTokenProvider metadataProvider = provider as IMetadataTokenProvider;

			foreach (CustomAttribute attribute in provider.CustomAttributes) 
				CheckParametersAndValues (metadataProvider, attribute.Constructor, attribute.ConstructorParameters);
			return Runner.CurrentRuleResult;
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (method.CustomAttributes.Count == 0 && !ContainsCustomAttributes (method.Parameters) && method.ReturnType.CustomAttributes.Count == 0)
				return RuleResult.DoesNotApply;
			
			CheckAttributesIn (method);		

			foreach (ParameterDefinition parameter in method.Parameters) 
				CheckAttributesIn (parameter);

			CheckAttributesIn (method.ReturnType);

			return Runner.CurrentRuleResult;
		}

		static bool ContainsCustomAttributes (IEnumerable providers)
		{
			foreach (ICustomAttributeProvider provider in providers) {
				if (provider.CustomAttributes.Count != 0)
					return true;
			}
			return false;
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			if (type.CustomAttributes.Count == 0 &&
				!ContainsCustomAttributes (type.Fields) &&
				!ContainsCustomAttributes (type.Properties) &&
				!ContainsCustomAttributes (type.Events))
				return RuleResult.DoesNotApply;
			
			CheckAttributesIn (type);
			
			foreach (FieldDefinition field in type.Fields)
				CheckAttributesIn (field); 

			foreach (PropertyDefinition property in type.Properties)
				CheckAttributesIn (property);

			foreach (EventDefinition eventDefinition in type.Events) 
				CheckAttributesIn (eventDefinition);

			return Runner.CurrentRuleResult;
		}

		public RuleResult CheckAssembly (AssemblyDefinition assembly)
		{
			if (assembly.CustomAttributes.Count == 0)
				return RuleResult.DoesNotApply;
			
			return CheckAttributesIn (assembly);			
		}
	}
}
