// CssLanguageBinding.cs created with MonoDevelop
// User: alouca at 8:12 PM 6/11/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

using MonoDevelop.Projects;
using MonoDevelop.Projects.Parser;


namespace CssAddin
{
	
	
	public class CssLanguageBinding : ILanguageBinding
	{
		IParser cssParser = null;
		
		public string Language 
		{
			get {
				return "CSS";
			}
		}
		
		public string CommentTag 
		{
			get { return "//"; }
		}
		
		public IParser Parser 
		{
			get {
				if (cssParser == null)
					cssParser = new CssAddin.Parser.CssAddinParser ();
				return cssParser;
			}
		}

		public MonoDevelop.Projects.CodeGeneration.IRefactorer Refactorer {
			get { return null; }
		}
		
		public bool IsSourceCodeFile (string fileName)
		{
			return fileName.EndsWith (".css", StringComparison.InvariantCultureIgnoreCase);
		}

		public string GetFileName (string baseName)
		{
			return baseName + ".css";
		}
	}
}
