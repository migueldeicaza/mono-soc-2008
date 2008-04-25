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
