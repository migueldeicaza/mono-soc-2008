//
// CssParser.cs -- Parses the Css file constructing a tree
//
// Authors: Andreas Louca (andreas@louca.org)
//
// Copyright (C) 2008 Andreas Louca
//

namespace CssEditor.Parser.Internal {

	class CssParser {
		private CssTokenizer tokenizer = null;
		
		public CssParser(TextReader input) 
		{
			if (input == null) 
			{
				throw new Exception("Empty filename");
			}
			
			tokenizer = new CssTokenizer(new StringReader(input.ReadToEnd());
		}
		
		private void acceptTerminal(CssToken t) 
		{
		}
		
		
	}

}