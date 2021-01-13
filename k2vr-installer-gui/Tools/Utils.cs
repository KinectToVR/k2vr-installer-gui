using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace k2vr_installer_gui.Tools
{
    static class Utils
    {
        public static bool IsDirectoryEmpty(string path)
        {
            return (Directory.GetFiles(path).Length + Directory.GetDirectories(path).Length) == 0;
        }
    }
}
