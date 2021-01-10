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
            AppConfig appConfig = null;
            try
            {
                appConfig = JsonFile.Read<AppConfig>(path);
            }
            catch (FileNotFoundException) { }
            if (appConfig == null)
            {
                appConfig = new AppConfig()
                {
                    manifest_paths = new List<string>
                    {
                        Path.Combine(App.state.steamPath, "config", "steamapps.vrmanifest")
                    }
                };
            }
            return appConfig;
        }

        public void Write()
        {
            JsonFile.Write(path, this, 3, ' ');
        }
    }
}
