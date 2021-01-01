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
            try
            {
                return JsonFile.Read<AppConfig>(path);
            }
            catch (FileNotFoundException)
            {
                var appConfig = new AppConfig()
                {
                    manifest_paths = new List<string>()
                };
                appConfig.manifest_paths.Add(
                    Path.Combine(App.state.steamPath, "config", "steamapps.vrmanifest")
                );
                return appConfig;
            }
        }

        public void Write()
        {
            JsonFile.Write(path, this, 3, ' ');
        }
    }
}
