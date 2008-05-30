//
// CssParser.cs -- Parses the Css file constructing a tree
//
// Authors: Andreas Louca (andreas@louca.org)
//
// Copyright (C) 2008 Andreas Louca
//

using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;


namespace CssEditor.Parser.Internal {

	public enum CssParserStates 
	{
		ERROR,
		STYLESHEET,
		CDO,
		CDC,
		STATEMENT,
		ATRULE,
		ATKEYWORD,
		ATRULESTRING,
		ATRULEEND,
		RULESET,
		BLOCK,
		SELECTOR
	}

	class CssParser {
		private CssTokenizer tokenizer = null;
		
		private Stack elements = new Stack ();
		private SimpleElement currentElement;
		private SimpleElement rootElement;
	   
		
		// Maintains Parser state
		//private Stack<CssToken> tokens = new Stack<CssToken>();
		//private Stack<CssParserStates> state = new Stack<CssParserStates>();
		
		private Stack tokens = new Stack();
		private Stack state = new Stack();
		
		
		public CssParser (CssTokenizer tokenizer) 
		{
			this.tokenizer = tokenizer;
			
			tokenizer.CssDocumentStart += new CssTokenizer.CssDocumentStartHandler(CssDocumentStart);
			tokenizer.CssDocumentEnd += new CssTokenizer.CssDocumentEndHandler(CssDocumentEnd);
			tokenizer.NewCssToken += new CssTokenizer.NewCssTokenHandler(NewCssToken);
		}
		
		private void acceptTerminal (CssToken t) 
		{
			tokens.Push (t);
			Console.WriteLine("State: {0} with Token: {1} ({2})", state.Peek ().ToString (), t.GetValue (), t.GetTokenType ().ToString ());
		}
		
		
		private void acceptState (CssParserStates s)
		{
			Console.WriteLine("Accepted State: {0}", s.ToString ());
			CssParserStates popState;
			SimpleElement child;
			SimpleElement parent;
			
			// Dont pop root element
			if (elements.Count == 1)
				parent = (SimpleElement) elements.Peek ();
			else
				parent = (SimpleElement) elements.Pop ();
			
			Console.WriteLine("Adding child to: {0}", parent.TagName );
			CssToken t;
			while ((popState = (CssParserStates) state.Pop ()) != s) {
				child = new SimpleElement (popState.ToString ());
				t = (CssToken) tokens.Pop ();
				child.Text = t.GetValue ();
				parent.ChildElements.Add (child);
			}
			
			rootElement.ChildElements.Add (parent);
		}
		
		private void addParentNode (CssParserStates s)
		{
			SimpleElement element = new SimpleElement (s.ToString ());
			elements.Push (element);
		}
	
		public void CssDocumentStart ()
		{
			state.Push (CssParserStates.STYLESHEET);
			rootElement = new SimpleElement("StyleSheet");
			elements.Push (rootElement);
			currentElement = rootElement;
		}
		
		public void NewCssToken (CssToken t)
		{
			switch((CssParserStates)state.Peek ()) {
				case CssParserStates.STYLESHEET:
				switch(t.GetTokenType ()) {
					case CssTokenType.CDO:
					//state.push (CssParserStates.CDO);
					break;
					case CssTokenType.CDC:
					//state.push (CssParserStates.CDC);
					break;
					case CssTokenType.ATKEYWORD:
					addParentNode (CssParserStates.ATRULE);
					state.Push (CssParserStates.ATRULE);
					state.Push (CssParserStates.ATKEYWORD);
					acceptTerminal(t);
					break;
					default:
					state.Push (CssParserStates.RULESET);
					break;
				}
				break;
				
				case CssParserStates.BLOCK:
				// In blocks we have ruleset
				CssTokenType tp = t.GetTokenType ();
				
				if (tp == CssTokenType.IDENT || tp == CssTokenType.HASH) {
					addParentNode (CssParserStates.RULESET);
					state.Push (CssParserStates.RULESET);
					state.Push (CssParserStates.SELECTOR);
					acceptTerminal (t);
				}
				break;
				
				case CssParserStates.SELECTOR:
				// We can have multiple selectors
				
				CssTokenType tp = t.GetTokenType ();
				
				if (tp == CssTokenType.IDENT || tp == CssTokenType.HASH) {
					addParentNode (CssParserStates.RULESET);
					state.Push (CssParserStates.RULESET);
					state.Push (CssParserStates.SELECTOR);
					acceptTerminal (t);
				}
				break;
				
				case CssParserStates.ATKEYWORD:
				switch(t.GetTokenType ()) {
					case CssTokenType.STRING:
					state.Push (CssParserStates.ATRULESTRING);
					acceptTerminal (t);
					break;
					case CssTokenType.LEFTCURLY:
					state.Push (CssParserStates.BLOCK);
					acceptTerminal (t);
					break;
					default:
					state.Push (CssParserStates.ERROR);
					break;
				}
				break;

				
				case CssParserStates.ATRULESTRING:
				switch(t.GetTokenType ()) {
					case CssTokenType.SEMICOLON:
					state.Push (CssParserStates.ATRULEEND);
					acceptTerminal (t);
					acceptState (CssParserStates.ATRULE);
					break;
					default:
					state.Push (CssParserStates.ERROR);
					break;
				}
				break;
				
			}
			
			if (t == null)
				Console.WriteLine("Something went wrong, got null");
			else
				Console.WriteLine("Parser: Token: {0} Type: {1}", t.GetValue(), t.GetTokenType().ToString());
		}
		
		public void CssDocumentEnd ()
		{
			StringBuilder b = new StringBuilder ();
			printTree(rootElement, b, 1);
			Console.WriteLine("{0}", b.ToString ());
		}
		
		private static void printTree(SimpleElement se, StringBuilder sb, int depth) 
		{

		   sb.Append(new string('\t',depth) +  
		      "<" + se.TagName);
		   foreach (string attName in se.Attributes.Keys) {
		      sb.Append(" " + attName + "=" + "\"" + 
		      se.Attribute(attName) + "\"");
		   }
		   sb.Append(">" + se.Text.Trim());
		   if (se.ChildElements.Count > 0) {
		      sb.Append(System.Environment.NewLine);
		      depth +=1;
		      foreach(SimpleElement ch in se.ChildElements) 
		      {
		         //sb.Append(System.Environment.NewLine);
		         printTree (ch, sb, depth);            
		      }
		      depth -= 1;
		      sb.Append(new string('\t',depth) + 
		         "</" + se.TagName + 
		         ">" + System.Environment.NewLine);
		   }    
		   else {
		      sb.Append("</" + se.TagName + ">" + 
		         System.Environment.NewLine);
		   }
		}
		
	}
}
