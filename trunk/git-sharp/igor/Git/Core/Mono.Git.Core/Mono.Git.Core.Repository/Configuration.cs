// Configuration.cs
//
// Author:
//   Igor Guerrero Fonseca <igfgt1@gmail.com>
//
// Copyright (c) 2008
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;

namespace Mono.Git.Core.Repository
{
	/// <summary>
	/// It holds all the user configuration
	/// </summary>
	public class Configuration : IConfiguration
	{
		private string head;
		private string description;
		private string path;
		private string template_path;
		
		public string Description
		{
			set {
				description = value;
			}
			get {
				return description;
			}
		}
		
		public string Head
		{
			set {
				head = value;
			}
			get {
				return head;
			}
		}
		
		public string ConfigPath
		{
			set {
				path = value;
			}
			get {
				return path;
			}
		}
		
		public string TemplatePath
		{
			set {
				template_path = value;
			}
			get {
				return template_path;
			}
		}
		
		public Configuration ()
		{ 
		}
		
		// These methods are used to get default configurations from C Git(mostly in UNIX systems)
		public static string GetDefaultHead ()
		{
			return "ref: refs/heads/master";
		}
		
		public static string GetDefaultTemplateDir ()
		{
			return "";
		}
		
		public static string GetDefaultConfigDir ()
		{
			return ".git";
		}
		
		public static string GetDefaultDescription ()
		{
			return "Unnamed repository; edit this file to name it for gitweb.";
		}
	}
}