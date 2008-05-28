//
// CssToken.cs -- Used to identify the token
//
// Authors: Andreas Louca (andreas@louca.org)
//
// Copyright (C) 2008 Andreas Louca
//

namespace CssEditor.Parser.Internal {

	class CssToken {
		// Token used internally
		public static int ERROR = 0;
	
		// Token Definitions from the CSS Spec
		public static int IDENT = 1;
		public static int ATKEYWORD = 2;
		public static int STRING = 3;
		public static int HASH = 4;
		public static int NUM = 5;
		public static int PERCENTAGE = 6;
		public static int DIMENSION = 7;
		public static int URI = 8;
		public static int UNICODE = 9;
		public static int CDO = 10;
		public static int CDC = 11;
		public static int SEMICOLON = 12;
		public static int LEFTCURLY = 13;
		public static int RIGHTCURLY = 14;
		public static int LEFTPARA = 15;
		public static int RIGHTPARA = 16;
		public static int LEFTSQUARE = 17;
		public static int RIGHTSQUARE = 18;
		public static int SPACE = 19;
		public static int COMMENT = 20;
		public static int FUNCTION = 21;
		public static int INCLUDES = 22;
		public static int DASHMATCH = 23;
		public static int DELIM = 24;
		
		int line = 0;
		int col = 0;
		int type = 0;
		string value = null;
		
		public CssToken(string value, int type, int line, int col)
		{
			this.value = value;
			this.type = type;
			this.col = col;
			this.line = line;
		}
		
		public int GetTokenType()
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