//
// CssToken.cs -- Used to identify the token
//
// Authors: Andreas Louca (andreas@louca.org)
//
// Copyright (C) 2008 Andreas Louca
//

namespace CssEditor.Parser.Internal {

	public enum CssTokenType
	{
		// Token used internally
		EOF,
		ERROR,
	
		// Token Definitions from the CSS Spec
		IDENT,
		ATKEYWORD,
		STRING,
		HASH,
		COLON,
		COMMA,
		NUM,
		PERCENTAGE,
		DIMENSION,
		URI,
		UNICODE,
		CDO,
		CDC,
		SEMICOLON,
		LEFTCURLY,
		RIGHTCURLY,
		DOT,
		LEFTPARA,
		RIGHTPARA,
		LEFTSQUARE,
		RIGHTSQUARE,
		SPACE,
		COMMENT,
		FUNCTION,
		INCLUDES,
		DASHMATCH,
		DELIM
	}

	class CssToken {
		
		int line = 0;
		int col = 0;
		CssTokenType type;
		string value = null;
		
		public CssToken(string value, CssTokenType type, int line, int col)
		{
			this.value = value;
			this.type = type;
			this.col = col;
			this.line = line;
		}
		
		public CssTokenType GetTokenType()
		{
			return type;
		}
		
		public string GetValue()
		{
			return value;
		}
		
		public int GetColumn()
		{
			return col;
		}
		
		public int GetLine()
		{
			return line;
		}
	}
}