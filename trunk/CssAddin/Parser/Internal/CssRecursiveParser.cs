// CssRecursiveParser.cs created with MonoDevelop
// User: alouca at 20:08 31/05/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace CssEditor.Parser.Internal
{
	
	
	public class CssRecursiveParser
	{

		IEnumerator<CssToken> tokenizer;
		CssToken currentToken;
		
		public CssRecursiveParser(TextReader input)
		{
			tokenizer = CssTokenizer.Tokenize(input).GetEnumerator();
			tokenizer.MoveNext ();
		}
		
		public void Parse()
		{
			stylesheet();
		}
		
		private void stylesheet ()
		{
			Console.WriteLine("PARSER: {0}", tokenizer.Current.GetTokenType ().ToString ());
			
			switch(tokenizer.Current.GetTokenType ()) {
			case CssTokenType.CDC:
				acceptElement (CssTokenType.CDC);
				break;
			case CssTokenType.CDO:
				acceptElement (CssTokenType.CDC);
				break;
			case CssTokenType.ATKEYWORD:
				AtKeyword();
				break;
			case CssTokenType.IDENT:
			case CssTokenType.HASH:
				RuleSet();
				break;
			default:
				acceptElement (CssTokenType.ERROR);
				break;
			}
			
			
			
			if (tokenizer.Current.GetTokenType () == CssTokenType.EOF) {
				acceptElement(CssTokenType.EOF);
			} else {
				stylesheet ();
			}
		}

		// Handles AtKeyword Parsing
		private void AtKeyword ()
		{
			acceptElement (CssTokenType.ATKEYWORD);
			acceptElement (CssTokenType.STRING);
			acceptElement (CssTokenType.SEMICOLON);
			return;
		}
		
		private void RuleSet ()
		{
			while (tokenizer.Current.GetTokenType () == CssTokenType.IDENT 
			       || tokenizer.Current.GetTokenType () == CssTokenType.HASH
			       || tokenizer.Current.GetTokenType () == CssTokenType.CLASS)
			{
			selector ();
				if (tokenizer.Current.GetTokenType () == CssTokenType.COMMA)
					acceptElement (CssTokenType.COMMA);
			}
			
			acceptElement (CssTokenType.LEFTCURLY);
			declaration ();
			acceptElement (CssTokenType.RIGHTCURLY);
		}
		
		private void declaration () 
		{
			acceptElement (CssTokenType.IDENT);
			acceptElement (CssTokenType.COLON);
			expression ();
			
			if (tokenizer.Current.GetTokenType () == CssTokenType.SEMICOLON) {
				acceptElement (CssTokenType.SEMICOLON);
			
				if (tokenizer.Current.GetTokenType() == CssTokenType.IDENT)
					declaration ();
			}
		}
		
		private void expression ()
		{
			term ();
			while (tokenizer.Current.GetTokenType() != CssTokenType.SEMICOLON
			    && tokenizer.Current.GetTokenType () != CssTokenType.RIGHTCURLY
			    && tokenizer.Current.GetTokenType() != CssTokenType.EOF)
				term();
		}
		
		private void term ()
		{
			if (tokenizer.Current.GetTokenType() == CssTokenType.PLUS)
				acceptElement (CssTokenType.PLUS);
					
			if (tokenizer.Current.GetTokenType() == CssTokenType.DASH)
				acceptElement (CssTokenType.DASH);
			
			switch(tokenizer.Current.GetTokenType()) {
			case CssTokenType.NUM:
				acceptElement (CssTokenType.NUM);
				break;
			case CssTokenType.PERCENTAGE:
				acceptElement (CssTokenType.PERCENTAGE);
				break;
			case CssTokenType.STRING:
				acceptElement (CssTokenType.STRING);
				break;
			case CssTokenType.DIMENSION:
				acceptElement (CssTokenType.DIMENSION);
				break;
			case CssTokenType.HEXCOLOR:
				acceptElement (CssTokenType.HEXCOLOR);
				break;
			case CssTokenType.URI:
				acceptElement (CssTokenType.URI);
				break;
			case CssTokenType.IDENT:
				if (String.Compare(tokenizer.Current.GetValue ().ToLower (), "rgb") == 0)
				{
					acceptElement (CssTokenType.IDENT);
					rgb ();
				} else {
					acceptElement (CssTokenType.IDENT);
				}
				break;
			}
			Console.WriteLine("Now at: {0}", tokenizer.Current.GetTokenType ().ToString ());
		}
		
		private void rgb ()
		{
			acceptElement (CssTokenType.LEFTPARA);
			rgbMetric ();
			acceptElement (CssTokenType.COMMA);
			rgbMetric ();
			acceptElement (CssTokenType.COMMA);
			rgbMetric ();
			acceptElement (CssTokenType.RIGHTPARA);
		}
		
		private void rgbMetric () {
			if (tokenizer.Current.GetTokenType () == CssTokenType.NUM)
				acceptElement (CssTokenType.NUM);
			else if (tokenizer.Current.GetTokenType () == CssTokenType.PERCENTAGE)
				acceptElement (CssTokenType.PERCENTAGE);
			else
				acceptElement (CssTokenType.ERROR);
		}
		
		private void selector ()
		{
			switch(tokenizer.Current.GetTokenType ()) {
			case CssTokenType.IDENT:
				acceptElement (CssTokenType.IDENT);
				break;
			case CssTokenType.HASH:
				acceptElement (CssTokenType.HASH);
				break;
			default:
				return;
			}
			
			if (tokenizer.Current.GetTokenType () == CssTokenType.PLUS)
			{
				acceptElement (CssTokenType.PLUS);
			}
			else if (tokenizer.Current.GetTokenType () == CssTokenType.GREATER ) {
				acceptElement (CssTokenType.GREATER);
			}
			
			selector ();
		}
		
		private void acceptElement(CssTokenType t)
		{
			CssToken ct = tokenizer.Current;
			
			while (tokenizer.Current.GetTokenType () == CssTokenType.COMMENT) {
				tokenizer.MoveNext ();
				Console.WriteLine("ignored comment");
			}
			
			if (tokenizer.Current.GetTokenType () != t) {
				Console.WriteLine("Parser: Rejected Terminal: {0} {1} - Expected {2}", ct.GetValue (), ct.GetTokenType ().ToString (), t.ToString ());
				throw new Exception("I made a boo!");
			} else {
				Console.WriteLine("Parser: Accepted Terminal: {0} {1}", tokenizer.Current.GetValue (), tokenizer.Current.GetTokenType ().ToString ());
			}
			tokenizer.MoveNext ();
		}
	}
}
