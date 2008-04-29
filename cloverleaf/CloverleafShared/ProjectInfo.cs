//
// ProjectInfo.cs: Parses data required by Cloverleaf out of Visual Studio
//                 project files
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
    public class ProjectInfo
    {
        String fileName;
        String directory;
        String assemblyName;
        List<String> outputPaths;
        Boolean executable;

        XmlDocument xmlDoc;

        public ProjectInfo(String path)
        {
            fileName = Path.GetFileName(path);
            directory = Path.GetDirectoryName(path);

            xmlDoc = new XmlDocument();
            xmlDoc.Load(path);

            XmlNodeList nodeList = xmlDoc.GetElementsByTagName("OutputType");
            // this will crash if OutputType isn't there, but if OutputType
            // isn't there we have way bigger problems--like a bad project
            XmlNode node = nodeList.Item(0);

            executable = node.InnerText.ToLowerInvariant().Trim().Contains("exe");

            nodeList = xmlDoc.GetElementsByTagName("AssemblyName");
            node = nodeList.Item(0);
            assemblyName = node.InnerText.Trim();

            outputPaths = new List<String>();
            nodeList = xmlDoc.GetElementsByTagName("OutputPath");
            foreach (XmlNode n in nodeList)
            {
                outputPaths.Add(n.InnerText.Trim());
            }
        }

        public String FileName
        {
            get { return fileName; }
        }
        public String Directory
        {
            get { return directory; }
        }
        public String AssemblyName
        {
            get { return assemblyName; }
        }
        public Boolean Executable
        {
            get { return executable; }
        }
        public List<String> OutputPaths
        {
            // couple ProjectInfo.Directory with the output paths to get files to run
            // for executable projects:
            // ProjectInfo.Directory + OutputPath + AssemblyName + ".exe"
            get { return outputPaths; }
        }
    }
}
