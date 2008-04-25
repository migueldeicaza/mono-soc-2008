using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text;

namespace CloverleafShared
{
    public abstract class BaseTester
    {
        protected readonly String solutionDirectory;
        protected readonly String solutionFileName;

        public BaseTester(String slnFile, String slnDirectory)
        {
            solutionFileName = slnFile;
            solutionDirectory = slnDirectory;
        }

        public abstract void Go();


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
