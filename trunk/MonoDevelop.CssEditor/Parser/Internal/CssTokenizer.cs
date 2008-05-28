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

namespace CssEditor.Parser.Internal {

	class CssTokenizer {
		
		int CurrentLine = 0;
		int CurrentOffset = 0;
		char CurrentCharacter;
		
		StringBuilder buffer;
		
		TextReader input;
	
		public CssTokenizer(TextReader file) 
		{
			input = file;
		}
		
		private int GetChar()
		{
			int c = input.Read();
			
			// Handle new lines
			if ( c == '\r' )
				c = input.Read(); // Consume carrier-return
				
			if ( c == '\n' )
			{
				CurrentLine++;
				CurrentOffset = -1;
				c = input.Read();
			}
			
			if ( c != -1 )
			{
				CurrentOffset++;
			}
			
			return c;
		}
		
		
		public CssToken GetNextToken() 
		{
			CurrentCharacter = GetChar();
			
			// Handle White Space
			while ((CurrentCharacter == CurrentCharacter.IsWhiteSpace()) || (CurrentCharacter == '\t') )
			{
	            CurrentCharacter = GetChar();
			}
			
			// Handle IDENT (start with letters) or uri
			if (CurrentCharacter.IsLetter())
			{
				buffer = new StringBuilder();
								
				do
				{
					buffer.Append(CurrentCharacter);
					CurrentCharacter = GetChar();
				} while (CurrentCharacter.IsLetter() || CurrentCharacter.IsDigit());
				
				// We have a URI
				if ( String.Compare(new String(buffer).ToLower()), "url" )
				{
					
				}
				else
					return new CssToken(new String(buffer), CssToken.IDENT, CurrentLine, CurrentOffset);
			}
			
			// Handle ATKEYWORD
			if (CurrentCharacter == '@')
			{
				buffer = new StringBuilder();
				
				// then IDENT follows (Start with letters)
				if ( input.Peek().IsLetter())
				{
					do
					{
						buffer.Append(CurrentCharacter);
						CurrentCharacter = GetChar();
					} while (CurrentCharacter.IsLetter() || CurrentCharacter.IsDigit());
					
					return new CssToken(new String(buffer), CssToken.ATKEYWORD, CurrentLine, CurrentOffset);
				}
				else
				{
					// In case where we have a lexical error
					return new CssToken(new String(buffer), CssToken.ERROR, CurrentLine, CurrentOffset);
				}
			}
			
			// Handle STRING
			if (CurrentCharacter == '\"' || CurrentCharacter == '\'') )
			{
				char EndChar = CurrentCharacter; // End with the same start
				buffer = new StringBuilder();
				
				do
				{
					buffer.Append(CurrentCharacter);
					CurrentCharacter = GetChar();
				} while(CurrentCharacter != EndChar);
				
				buffer.Append(CurrentCharacter);
				
				return new CssToken(new String(buffer), CssToken.STRING, CurrentLine, CurrentOffset);
			}
			
			// Handle HASH
			if (CurrentCharacter == '#')
			{
				buffer = new StringBuffer();
				
				if ( input.Peek().IsLetter() || input.Peek().IsDigit() )
				{
					do
					{
						buffer.Append(CurrentCharacter);
						CurrentCharacter = GetChar();
					} while(CurrentCharacter.IsLetter() || CurrentCharacter.IsDigit());
					
					return new CssToken(new String(buffer), CssToken.HASH, CurrentLine, CurrentOffset);
				}
				else
				{
					// In case where we have a lexical error
					return new CssToken(new String(buffer), CssToken.ERROR, CurrentLine, CurrentOffset);
				}
			}
			
			// Handle NUM, PERCENTAGE, DIMENSION
			if (CurrentCharacter.IsDigit())
			{
				buffer = new StringBuffer();
				
				do
				{
					buffer.Append(CurrentCharacter);
					CurrentCharacter = GetChar();
				} while( CurrentCharacter.IsDigit() );
				
				// if % follows then its Percentage
				if ( CurrentCharacter == '%' )
				{
					buffer.Append(CurrentCharacter);
					CurrentCharacter = GetChar();
					return new CssToken(new String(buffer), CssToken.PERCENTAGE, CurrentLine, CurrentOffset);
				}
				// If its a letter, then its a DIMENSION
				else if ( CurrentCharacter.IsLetter() )
				{
					do
					{
						buffer.Append(CurrentCharacter);
						CurrentCharacter = GetChar();
					} while (CurrentCharacter.IsLetter() || CurrentCharacter.IsDigit());
					
					return new CssToken(new String(buffer), CssToken.DIMENSION, CurrentLine, CurrentOffset);
				}
				// NUM
				else
				{
					return new CssToken(new String(buffer), CssToken.NUM, CurrentLine, CurrentOffset);
				}
			}
			return null;
		}
	}
}
