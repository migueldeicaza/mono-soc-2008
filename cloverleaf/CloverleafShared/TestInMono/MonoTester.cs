﻿//
// MonoTester.cs: Base support for Cloverleaf's "Test In Mono" feature
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
using System.Text;

namespace CloverleafShared.TestInMono
{
    /// <summary>
    /// The starter class for testing application projects
    /// locally, using Mono.
    /// </summary>
    public class MonoTester : BaseAppTester
    {
        public MonoTester(String slnFile, String slnDirectory, Boolean isAddin)
            : base(slnFile, slnDirectory, isAddin)
        {

        }

        public override void Go()
        {
            List<ProjectInfo> projects = FindProjects(solutionDirectory, true);

			MonoSelector ms = new MonoSelector(solutionDirectory, projects);

			if (CloverleafEnvironment.IsAddin == true)
				ms.ShowDialog();
			else
				System.Windows.Forms.Application.Run(ms);
        }
    }
}
