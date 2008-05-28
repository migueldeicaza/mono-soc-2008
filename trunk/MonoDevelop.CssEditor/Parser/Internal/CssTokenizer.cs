//
// CssTokenizer.cs -- Reads input stream and passes tokens to the parser
//
// Authors: Andreas Louca (andreas@louca.org)
//
// Copyright (C) 2008 Andreas Louca
//

using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace CssEditor.Parser.Internal {

	class CssTokenizer {
		
		int CurrentLineNumber = 0;
		int CurrentOffset = 0;
		char CurrentCharacter;
		string CurrentLine;
		
		StringBuilder buffer;
		
		TextReader input;
	
		public CssTokenizer(TextReader file) 
		{
			input = file;
			
			CurrentLine = input.ReadLine();
		}
		
		private char GetChar()
		{
			if (CurrentLine == null)
	            return (char)26;
	        else if (CurrentOffset >= CurrentLine.Length)
			{
	            CurrentLine = input.ReadLine() ;
	            CurrentOffset = 0 ;
	            return '\n';
	        }
	        else
	            return CurrentLine[CurrentOffset++];
		}
		
		private void ConsumeWhitespace()
		{
			// Handle White Space
			while ((char.IsWhiteSpace(CurrentCharacter)) || (CurrentCharacter == '\t') )
			{
	            CurrentCharacter = GetChar();
			}
		}
		
		private string ReadString()
		{
			StringBuilder str = new StringBuilder();
			
			// String patterns as specified
			//Regex StrPattern1 = new Regex("\"([\t !#$%&(-~]|\n|\r\n|\r|\f|\'|[^\0-\177]|\\[ -~\200-\4177777]|\\[0-9a-f]{1,6}[ \n\r\t\f]?)*\"");
			//Regex StrPattern2 = new Regex("\'([\t !#$%&(-~]|\n|\r\n|\r|\f|\"|[^\0-\177]|\\[ -~\200-\4177777]|\\[0-9a-f]{1,6}[ \n\r\t\f]?)*\'");
			
			if (CurrentCharacter == '\'' || CurrentCharacter == '\"')
			{
				// String must end with the opening delimeter
				int BeginChar = CurrentCharacter;
				
				do
				{
					// Escape char?
					if ( CurrentCharacter == '\\' )
					{
						str.Append(CurrentCharacter);
						CurrentCharacter = GetChar();
						str.Append(CurrentCharacter);
					}
					else
					{
						str.Append(CurrentCharacter);
						CurrentCharacter = GetChar();
					}
					
				} while(CurrentCharacter != BeginChar );
				
				str.Append(CurrentCharacter);
			}
			
			return str.ToString();
			
		}
		
		public CssToken GetNextToken() 
		{
			CurrentCharacter = GetChar();
			
			ConsumeWhitespace();
			
			if ( CurrentCharacter == '\r' )
				CurrentCharacter = GetChar();
			
			if ( CurrentCharacter == '\n' )
			{
				CurrentLineNumber++;
			}
			
			// Handle IDENT (start with letters) or uri
			if (Char.IsLetter(CurrentCharacter))
			{
				buffer = new StringBuilder();

				do
				{
					buffer.Append(CurrentCharacter);
					CurrentCharacter = GetChar();
				} while (Char.IsLetter(CurrentCharacter) || Char.IsDigit(CurrentCharacter));
				
				// We have a URI
				if ( String.Compare(buffer.ToString().ToLower(), "url" ) == 0 )
				{
					ConsumeWhitespace();
					if (CurrentCharacter == '\'' || CurrentCharacter == '\"')
					{
						buffer.Append(ReadString());
						return new CssToken(buffer.ToString(), CssToken.URI, CurrentLineNumber, CurrentOffset);
					}
					else
					{
						return new CssToken(buffer.ToString(), CssToken.ERROR, CurrentLineNumber, CurrentOffset);
					}
					
				}
				else
					return new CssToken(buffer.ToString(), CssToken.IDENT, CurrentLineNumber, CurrentOffset);
			}
			
			// Handle ATKEYWORD
			if (CurrentCharacter == '@')
			{
				buffer = new StringBuilder();
				
				CurrentCharacter = GetChar();
				
				// then IDENT follows (Start with letters)
				if ( Char.IsLetter(CurrentCharacter))
				{
					do
					{
						buffer.Append(CurrentCharacter);
						CurrentCharacter = GetChar();
					} while (Char.IsLetter(CurrentCharacter)|| Char.IsDigit(CurrentCharacter));
					
					return new CssToken(buffer.ToString(), CssToken.ATKEYWORD, CurrentLineNumber, CurrentOffset);
				}
				else
				{
					// In case where we have a lexical error
					return new CssToken(buffer.ToString(), CssToken.ERROR, CurrentLineNumber, CurrentOffset);
				}
			}
			
			// Handle STRING
			if (CurrentCharacter == '\"' || CurrentCharacter == '\'' )
			{
				return new CssToken(ReadString(), CssToken.STRING, CurrentLineNumber, CurrentOffset);
			}
			
			// Handle HASH
			if (CurrentCharacter == '#')
			{
				buffer = new StringBuilder();
				buffer.Append(CurrentCharacter);
				CurrentCharacter = GetChar();
				
				if ( Char.IsLetter(CurrentCharacter) || Char.IsDigit(CurrentCharacter) )
				{
					do
					{
						buffer.Append(CurrentCharacter);
						CurrentCharacter = GetChar();
					} while(Char.IsLetter(CurrentCharacter) || Char.IsDigit(CurrentCharacter));
					
					return new CssToken(buffer.ToString(), CssToken.HASH, CurrentLineNumber, CurrentOffset);
				}
				else
				{
					// In case where we have a lexical error
					return new CssToken(buffer.ToString(), CssToken.ERROR, CurrentLineNumber, CurrentOffset);
				}
			}
			
			// Handle NUM, PERCENTAGE, DIMENSION
			if (Char.IsDigit(CurrentCharacter))
			{
				buffer = new StringBuilder();
				
				do
				{
					buffer.Append(CurrentCharacter);
					CurrentCharacter = GetChar();
				} while( Char.IsDigit(CurrentCharacter) );
				
				// if % follows then its Percentage
				if ( CurrentCharacter == '%' )
				{
					buffer.Append(CurrentCharacter);
					CurrentCharacter = GetChar();
					return new CssToken(buffer.ToString(), CssToken.PERCENTAGE, CurrentLineNumber, CurrentOffset);
				}
				// If its a letter, then its a DIMENSION
				else if ( Char.IsLetter(CurrentCharacter) )
				{
					do
					{
						buffer.Append(CurrentCharacter);
						CurrentCharacter = GetChar();
					} while (Char.IsLetter(CurrentCharacter) || Char.IsDigit(CurrentCharacter));
					
					return new CssToken(buffer.ToString(), CssToken.DIMENSION, CurrentLineNumber, CurrentOffset);
				}
				// NUM
				else
				{
					return new CssToken(buffer.ToString(), CssToken.NUM, CurrentLineNumber, CurrentOffset);
				}
			}
			
			// CDO
			if (CurrentCharacter == '<')
			{
				buffer = new StringBuilder();
				buffer.Append(CurrentCharacter);
				for ( int i = 0; i < 3; i++)
					buffer.Append(GetChar());
				
				if (String.Compare(buffer.ToString(), "<!--") == 0)
					return new CssToken(buffer.ToString(), CssToken.CDO, CurrentLineNumber, CurrentOffset);
				else
					return new CssToken(buffer.ToString(), CssToken.ERROR, CurrentLineNumber, CurrentOffset);
			}
			
			// CDC
			if (CurrentCharacter == '-')
			{
				buffer = new StringBuilder();
				buffer.Append(CurrentCharacter);
				for ( int i = 0; i < 2; i++)
					buffer.Append(GetChar());
					
				if (String.Compare(buffer.ToString(), "-->") == 0)
					return new CssToken(buffer.ToString(), CssToken.CDC, CurrentLineNumber, CurrentOffset);
				else
					return new CssToken(buffer.ToString(), CssToken.ERROR, CurrentLineNumber, CurrentOffset);		
			}
			
			// Comment
			
			
			// Single chars
			switch(CurrentCharacter)
			{
				case ';':
					return new CssToken(";", CssToken.SEMICOLON, CurrentLineNumber, CurrentOffset);
				case '{':
					return new CssToken("{", CssToken.LEFTCURLY, CurrentLineNumber, CurrentOffset);
				case '}':
					return new CssToken("}", CssToken.RIGHTCURLY, CurrentLineNumber, CurrentOffset);
				case '(':
					return new CssToken("(", CssToken.LEFTPARA, CurrentLineNumber, CurrentOffset);
				case ')':
					return new CssToken(")", CssToken.RIGHTPARA, CurrentLineNumber, CurrentOffset);
				case '[':
					return new CssToken("[", CssToken.LEFTSQUARE, CurrentLineNumber, CurrentOffset);
				case ']':
					return new CssToken("]", CssToken.RIGHTSQUARE, CurrentLineNumber, CurrentOffset);
						
			}
			return null;
		}
	}
}
