// CssAddinParser.cs created with MonoDevelop
// User: alouca at 8:17 PM 6/11/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.IO;

using MonoDevelop.Projects.Parser;

namespace CssAddin.Parser
{
	
	
	public class CssAddinParser : IParser
	{
		string[] lexerTags;
		
		public string[] LexerTags {
			get { return lexerTags; }
			set { lexerTags = value; }
		}

		public IExpressionFinder CreateExpressionFinder (string fileName)
		{
			return new AspNetExpressionFinder ();
		}

		public ICompilationUnitBase Parse (string fileName)
		{
			using (TextReader tr = new StreamReader (fileName)) {
				Document doc = new Document (tr, null, fileName);
				return BuildCU (doc);
			}
		}

		public ICompilationUnitBase Parse (string fileName, string fileContent)
		{
			using (TextReader tr = new StringReader (fileContent)) {
				Document doc = new Document (tr, null, fileName);
				return BuildCU (doc);
			}
		}
		
		AspNetCompilationUnit BuildCU (Document doc)
		{
			AspNetCompilationUnit cu = new AspNetCompilationUnit ();
			cu.Document = doc;
			cu.PageInfo = new PageInfo ();
			
			CompilationUnitVisitor cuVisitor = new CompilationUnitVisitor (cu);
			doc.RootNode.AcceptVisit (cuVisitor);
			
			PageInfoVisitor piVisitor = new PageInfoVisitor (cu.PageInfo);
			doc.RootNode.AcceptVisit (piVisitor);
			
			foreach (ParserException pe in doc.ParseErrors)
				cu.AddError (new ErrorInfo (pe.Line, pe.Column, pe.Message));
			cu.CompileErrors ();
			return cu;
		}
		
		ErrorInfo[] GetErrors (Document doc)
		{
			System.Collections.Generic.List<ErrorInfo> list = new System.Collections.Generic.List<ErrorInfo> ();
			foreach (ParserException pe in doc.ParseErrors)
				list.Add (new ErrorInfo (pe.Line, pe.Column, pe.Message));
			return list.Count == 0? null : list.ToArray ();
		}

		public ResolveResult Resolve (IParserContext parserContext, string expression, int caretLineNumber, int caretColumn, string fileName, string fileContent)
		{
			return null;
		}

		public LanguageItemCollection CtrlSpace (IParserContext parserContext, int caretLine, int caretColumn, string fileName)
		{
			return null;
		}

		public ILanguageItem ResolveIdentifier (IParserContext parserContext, string id, int line, int col, string fileName, string fileContent)
		{
			return null;
		}
	}
}
