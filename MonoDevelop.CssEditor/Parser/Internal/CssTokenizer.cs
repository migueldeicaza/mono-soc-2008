//
// CssTokenizer.cs -- Reads input stream and passes tokens to the parser
//
// Authors: Andreas Louca (andreas@louca.org)
//CssTokenType.
// Copyright (C) 2008 Andreas Louca
//

using System;
using System.Collections;
using System.IO;
using System.Text;

namespace CssEditor.Parser.Internal {

	class CssTokenizer {
		
		// Delegates used by the parser
		public delegate void CssDocumentStartHandler ();
		public delegate void NewCssTokenHandler (CssToken token);
		public delegate void CssDocumentEndHandler ();
		
		public event CssDocumentStartHandler CssDocumentStart;
		public event NewCssTokenHandler NewCssToken;
		public event CssDocumentEndHandler CssDocumentEnd;
		
		int CurrentLineNumber = 0;
		int CurrentOffset = 0;
		char CurrentCharacter;
		//string CurrentLine;
		
		StringBuilder buffer = new StringBuilder ();
		
		TextReader input;
	
		int putChar = 0;
	
		protected void OnCssDocumentStart ()
		{
			if (CssDocumentStart != null)
				CssDocumentStart ();
		}
		
		protected void OnNewCssToken (CssToken token)
		{
			if (NewCssToken != null)
				NewCssToken (token);
		}
		
		protected void OnCssDocumentEnd ()
		{
			if (CssDocumentEnd != null)
				CssDocumentEnd ();
		}
	
		public CssTokenizer () 
		{
			
		}
		
		public void ParseDocument(TextReader file)
		{
			CssToken t;
			input = file;
			
			OnCssDocumentStart ();
			
			while((t = GetNextToken()).GetTokenType() != CssTokenType.EOF) {
				OnNewCssToken (t);
			}
			
			OnCssDocumentEnd ();
		}
		
		private void putBackChar (char c)
		{
			putChar = (int)c;
		}
		
		private char GetChar ()
		{
			int c = 0;
			
			if (putChar != 0) {
				c = putChar;
				putChar = 0;
				return (char)c;
			}
				
			c = input.Read ();
			
			// EOF
			if (c == -1) {
				return (char)26;
			}
	            
			if (c == '\n')
				CurrentOffset = 0;
	
			return (char)c;
		}
		
		private void ConsumeWhitespace ()
		{
			// Handle White Space
			while ((char.IsWhiteSpace (CurrentCharacter)) || (CurrentCharacter == '\t') ) {
	            CurrentCharacter = GetChar ();
			}
		}
		
		private string ReadString ()
		{
			StringBuilder str = new StringBuilder ();
			
			// String patterns as specified
			//Regex StrPattern1 = new Regex("\"([\t !#$%&(-~]|\n|\r\n|\r|\f|\'|[^\0-\177]|\\[ -~\200-\4177777]|\\[0-9a-f]{1,6}[ \n\r\t\f]?)*\"");
			//Regex StrPattern2 = new Regex("\'([\t !#$%&(-~]|\n|\r\n|\r|\f|\"|[^\0-\177]|\\[ -~\200-\4177777]|\\[0-9a-f]{1,6}[ \n\r\t\f]?)*\'");
			
			if (CurrentCharacter == '\'' || CurrentCharacter == '\"') {
				// String must end with the opening delimeter
				int BeginChar = CurrentCharacter;
				
				do {
					// Escape char?
					if ( CurrentCharacter == '\\' ) {
						str.Append (CurrentCharacter);
						CurrentCharacter = GetChar ();
						str.Append (CurrentCharacter);
					}
					else {
						str.Append (CurrentCharacter);
						CurrentCharacter = GetChar ();
					}
					
				} while(CurrentCharacter != BeginChar && CurrentCharacter != 26 );
				
				str.Append (CurrentCharacter);
			}
			
			return str.ToString ();
			
		}
		
		public CssToken GetNextToken () 
		{
			CurrentCharacter = GetChar ();
			
			ConsumeWhitespace ();
			
			if ( CurrentCharacter == '\r' )
				CurrentCharacter = GetChar();
			
			else if ( CurrentCharacter == '\n' )
				CurrentLineNumber++;
			
			// Handle IDENT (start with letters) or uri
			else if (Char.IsLetter (CurrentCharacter)) {
				buffer.Length = 0;

				do {
					buffer.Append (CurrentCharacter);
					CurrentCharacter = GetChar();
				} while ((Char.IsLetter (CurrentCharacter) || Char.IsDigit (CurrentCharacter) || CurrentCharacter == '-') && CurrentCharacter != (char)26);
				
				// We have a URI
				if ( String.Compare(buffer.ToString().ToLower(), "url" ) == 0 ) {
					
					ConsumeWhitespace ();
					CurrentCharacter = GetChar ();
					
					if (CurrentCharacter == '\'' || CurrentCharacter == '\"') {
						buffer.Append (ReadString ());
						return new CssToken (buffer.ToString (), CssTokenType.URI, CurrentLineNumber, CurrentOffset);
					}
					else {
						return new CssToken (buffer.ToString (), CssTokenType.ERROR, CurrentLineNumber, CurrentOffset);
					}
					
				}
				else {
					putBackChar (CurrentCharacter);
					return new CssToken (buffer.ToString (), CssTokenType.IDENT, CurrentLineNumber, CurrentOffset);
				}
					
			}
			
			// Handle ATKEYWORD
			if (CurrentCharacter == '@') {
				buffer.Length = 0;
				buffer.Append (CurrentCharacter);
				CurrentCharacter = GetChar ();
				
				// then IDENT follows (Start with letters)
				if ( Char.IsLetter (CurrentCharacter)) {
					do {
						buffer.Append (CurrentCharacter);
						CurrentCharacter = GetChar();
					} while ((Char.IsLetter (CurrentCharacter)|| Char.IsDigit (CurrentCharacter)) && CurrentCharacter != (char)26);
					
					return new CssToken (buffer.ToString (), CssTokenType.ATKEYWORD, CurrentLineNumber, CurrentOffset);
				}
				else {
					// In case where we have a lexical error
					return new CssToken (buffer.ToString (), CssTokenType.ERROR, CurrentLineNumber, CurrentOffset);
				}
			}
			
			// Handle STRING
			if (CurrentCharacter == '\"' || CurrentCharacter == '\'' ) {
				return new CssToken (ReadString (), CssTokenType.STRING, CurrentLineNumber, CurrentOffset);
			}
			
			// Handle HASH
			if (CurrentCharacter == '#') {
				buffer.Length = 0;
				buffer.Append (CurrentCharacter);
				CurrentCharacter = GetChar ();
				
				if ( Char.IsLetter (CurrentCharacter) || Char.IsDigit (CurrentCharacter) ) {
					do {
						buffer.Append (CurrentCharacter);
						CurrentCharacter = GetChar ();
					} while ((Char.IsLetter (CurrentCharacter) || Char.IsDigit (CurrentCharacter)) && CurrentCharacter != (char)26);
					
					putBackChar (CurrentCharacter);
					
					return new CssToken (buffer.ToString (), CssTokenType.HASH, CurrentLineNumber, CurrentOffset);
				}
				else {
					// In case where we have a lexical error
					return new CssToken (buffer.ToString (), CssTokenType.ERROR, CurrentLineNumber, CurrentOffset);
				}
			}
			
			// Handle NUM, PERCENTAGE, DIMENSION
			if (Char.IsDigit (CurrentCharacter)) {
				buffer.Length = 0;
				
				do {
					buffer.Append(CurrentCharacter);
					CurrentCharacter = GetChar();
				} while(Char.IsDigit(CurrentCharacter));
				
				// if . follows, more numbers follow
				if (CurrentCharacter == '.') {
					buffer.Append (CurrentCharacter);
					if (!Char.IsDigit ((char)input.Peek ()))
						return new CssToken (buffer.ToString (), CssTokenType.ERROR, CurrentLineNumber, CurrentOffset);
					else {
						CurrentCharacter = GetChar ();
						do {
							buffer.Append (CurrentCharacter);
							CurrentCharacter = GetChar ();
						} while (Char.IsDigit (CurrentCharacter));
					}
				}
				
				// if % follows then its Percentage
				if (CurrentCharacter == '%') {
					buffer.Append(CurrentCharacter);
					//CurrentCharacter = GetChar();
					return new CssToken(buffer.ToString(), CssTokenType.PERCENTAGE, CurrentLineNumber, CurrentOffset);
				}
				// If its a letter, then its a DIMENSION
				else if (Char.IsLetter(CurrentCharacter)) {
					do {
						buffer.Append(CurrentCharacter);
						CurrentCharacter = GetChar();
					} while (Char.IsLetter(CurrentCharacter) || Char.IsDigit(CurrentCharacter) || CurrentCharacter == (char)26);
					putBackChar(CurrentCharacter);
					return new CssToken(buffer.ToString(), CssTokenType.DIMENSION, CurrentLineNumber, CurrentOffset);
				}
				// NUM
				else {
					putBackChar(CurrentCharacter);
					return new CssToken(buffer.ToString(), CssTokenType.NUM, CurrentLineNumber, CurrentOffset);
				}
			}
			
			// CDO
			if (CurrentCharacter == '<') {
				buffer.Length = 0;
				buffer.Append(CurrentCharacter);
				for (int i = 0; i < 3; i++)
					buffer.Append(GetChar());
				
				if (String.Compare(buffer.ToString(), "<!--") == 0)
					return new CssToken(buffer.ToString(), CssTokenType.CDO, CurrentLineNumber, CurrentOffset);
				else
					return new CssToken(buffer.ToString(), CssTokenType.ERROR, CurrentLineNumber, CurrentOffset);
			}
			
			// CDC
			if (CurrentCharacter == '-') {
				buffer.Length = 0;
				buffer.Append(CurrentCharacter);
				for ( int i = 0; i < 2; i++)
					buffer.Append(GetChar());
					
				if (String.Compare(buffer.ToString(), "-->") == 0)
					return new CssToken(buffer.ToString(), CssTokenType.CDC, CurrentLineNumber, CurrentOffset);
				else
					return new CssToken(buffer.ToString(), CssTokenType.ERROR, CurrentLineNumber, CurrentOffset);		
			}
			
			// EOF
			if (CurrentCharacter == 26)
				return new CssToken("EOF", CssTokenType.EOF, CurrentLineNumber, CurrentOffset);
				
			// Comment
			if (CurrentCharacter == '/' && input.Peek() == '*') {
				buffer.Length = 0;

				do {
					buffer.Append(CurrentCharacter);
					CurrentCharacter = GetChar();
				} while(!(CurrentCharacter == '*' && input.Peek() == '/') || CurrentCharacter == (char)26);
				
				buffer.Append(CurrentCharacter);
				buffer.Append(GetChar());
				
				return new CssToken(buffer.ToString(), CssTokenType.COMMENT, CurrentLineNumber, CurrentOffset);
			}
			
			// Single chars
			switch(CurrentCharacter) {
				case ';':
				return new CssToken(";", CssTokenType.SEMICOLON, CurrentLineNumber, CurrentOffset);
				case '.':
				return new CssToken(".", CssTokenType.DOT, CurrentLineNumber, CurrentOffset);
				case ':':
				return new CssToken(":", CssTokenType.COLON, CurrentLineNumber, CurrentOffset);
				case ',':
				return new CssToken(",", CssTokenType.COMMA, CurrentLineNumber, CurrentOffset);
				case '{':
				return new CssToken("{", CssTokenType.LEFTCURLY, CurrentLineNumber, CurrentOffset);
				case '}':
				return new CssToken("}", CssTokenType.RIGHTCURLY, CurrentLineNumber, CurrentOffset);
				case '(':
				return new CssToken("(", CssTokenType.LEFTPARA, CurrentLineNumber, CurrentOffset);
				case ')':
				return new CssToken(")", CssTokenType.RIGHTPARA, CurrentLineNumber, CurrentOffset);
				case '[':
				return new CssToken("[", CssTokenType.LEFTSQUARE, CurrentLineNumber, CurrentOffset);
				case ']':
				return new CssToken("]", CssTokenType.RIGHTSQUARE, CurrentLineNumber, CurrentOffset);		
			}
			return null;
		}
	}
}
