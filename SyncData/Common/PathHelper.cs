using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SyncData.Common
{
    public class PathHelper
    {
        public static string ForceGetAppDir(string sdir, bool bCreate=false)
        {
            var spath = System.AppDomain.CurrentDomain.BaseDirectory + sdir;
            if (bCreate && !System.IO.Directory.Exists(spath))
            {
                System.IO.Directory.CreateDirectory(spath);
            }
            return spath;
        }
        public static string GetFile(string sdir, string sfile)
        {
            var spath = System.AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(spath, sdir, sfile);
        }

    }
}
