// CssNode.cs created with MonoDevelop
// User: alouca at 8:49 PM 6/11/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Collections.Generic;

using CssAddin.Parser.Internal;

namespace CssAddin.Parser.Dom
{
	
	
	public abstract class CssNode
	{
		CssToken token;
		
		List<CssNode> children;
		CssNode parent;
		
		public CssNode (CssToken token)
		{
			this.token = token;
			parent = null;
		}
		
		public void AddChild(CssNode t)
		{
			if (children == null)
				children = new List<CssNode> ();
			children.add(t);
		}
		
		public CssNode GetLatest ()
		{
			if (children == null)
				return null;
			return children[children.Count - 1];
		}
		
		public CssToken Token {
			get { return token; }
		}
	}
}
