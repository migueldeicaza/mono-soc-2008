//
// BaseTester.cs: Base tester class for use by MonoTester/GendarmeTester
//
// Authors:
//  Ed Ropple <ed@edropple.com>
//
// Copyright (C) 2008 Edward Ropple III (http://www.edropple.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text;

namespace CloverleafShared
{
    /// <summary>
    /// The base testing class for handling application projects (--monotest and --appremote).
    /// </summary>
    public abstract class BaseAppTester
    {
        protected String solutionDirectory;
        protected String solutionFileName;

        

        public BaseAppTester(String slnFile, String slnDirectory)
        {
			this.Initialize(slnFile, slnDirectory, false);
        }
		public BaseAppTester(String slnFile, String slnDirectory, Boolean isAddin)
		{
			this.Initialize(slnFile, slnDirectory, isAddin);
		}

		private void Initialize(String slnFile, String slnDirectory, Boolean isAddin)
		{
			solutionFileName = slnFile;
			solutionDirectory = slnDirectory;
			if (isAddin == false)
				CloverleafEnvironment.Initialize();
			else
				CloverleafEnvironment.InitializeAddin();
		}

        public abstract void Go();

        /// <summary>
        /// Locate all application projects under "folder".
        /// </summary>
        /// <remarks>
        /// Hackish and in need of eventual replacement. The MonoDevelop codebase has
        /// a solution parser, but I couldn't figure out a clean way to separate it
        /// from MonoDevelop.
        /// </remarks>
        /// <param name="folder">The folder under which to search.</param>
        /// <param name="omitNonExecutables">
        /// Omit projects that aren't executable (class libraries, etc.)
        /// </param>
        /// <returns>A list of ProjectInfo objects containing the data for all found projects.</returns>
        protected List<ProjectInfo> FindProjects(String folder, Boolean omitNonExecutables)
        {
            List<ProjectInfo> projList = new List<ProjectInfo>();

            foreach (String s in Directory.GetFiles(folder, "*.*proj", SearchOption.AllDirectories))
            {
                ProjectInfo p = new ProjectInfo(s);

                if (omitNonExecutables == false)
                {
                    projList.Add(p);
                }
                else if (p.Executable == true)
                {
                    projList.Add(p);
                }
            }

            return projList;
        }
    }
}
