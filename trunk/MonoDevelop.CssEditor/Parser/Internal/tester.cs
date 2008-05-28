//
// tester.cs -- Used to test the parser operations
//
// Authors: Andreas Louca (andreas@louca.org)
//
// Copyright (C) 2008 Andreas Louca
//

using System;
using System.Collections;
using System.IO;
using System.Text;

namespace CssEditor.Parser.Internal 
{
	class tester 
	{
		static void Main(string[] args)
		{
			TextReader cssfileReader = null;
			string filename = null;
			
			
			if ( args.Length < 1)
			{
				Console.WriteLine("CssEditor Parser Tester\n\nSyntax: tester.exe <cssfile.css>");
				return;
			}
			else 
			{
				filename = args[0];
				Console.WriteLine("CssEditor Parser tester: parsing {0}", args[0]);
			}
			
			try 
			{
				cssfileReader = new StreamReader(filename);
				CssTokenizer testParser = new CssTokenizer(cssfileReader);
				
				CssToken t;
				
				while((t = testParser.GetNextToken()) != null)
				{
					Console.WriteLine("Tigki: {0}", t.GetValue());
				}
				
			} 
			catch(FileNotFoundException e)
			{
				Console.WriteLine("The file specified ({0}) could not be found!", filename);
				return;
			}
		}
	}
}