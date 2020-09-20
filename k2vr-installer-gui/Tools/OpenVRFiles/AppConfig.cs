using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace k2vr_installer_gui.Tools.OpenVRFiles
{
    class AppConfig
    {
        static string path = Path.Combine(App.state.steamPath, "config", "appconfig.json");

#pragma warning disable 0649
        public List<string> manifest_paths;
#pragma warning restore 0649

        public static AppConfig Read()
        {
            return JsonFile.Read<AppConfig>(path);
        }

        public void Write()
        {
            JsonFile.Write(path, this, 3, ' ');
        }
    }
}
