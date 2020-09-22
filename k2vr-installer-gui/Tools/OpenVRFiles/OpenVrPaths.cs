using System;
using System.Collections.Generic;
using System.IO;

namespace k2vr_installer_gui.Tools.OpenVRFiles
{
    class OpenVrPaths
    {
        static string path = Environment.ExpandEnvironmentVariables(Path.Combine("%LocalAppData%", "openvr", "openvrpaths.vrpath"));

        // Prevent Warning CS0649: Field '...' is never assigned to, and will always have its default value null:
#pragma warning disable 0649
        public List<string> config;
        public List<string> external_drivers = new List<string>();
        public string jsonid;
        public List<string> log;
        public List<string> runtime;
        public int version;
#pragma warning restore 0649

        public static OpenVrPaths Read()
        {
            return JsonFile.Read<OpenVrPaths>(path);
        }

        public void Write()
        {
            JsonFile.Write(path, this, 1, '\t');
        }
    }
}
