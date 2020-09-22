using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using static k2vr_installer_gui.Tools.InstallerState;

namespace k2vr_installer_gui.Tools
{
    public class Analytics
    {
        public TrackingDevice trackingDevice;
        public string installerVersion;
        public string windowsReleaseId;
        public string windowsBuild;
        public string language;
        public string headsetManufacturer = "";
        public string headsetModel = "";

        public Analytics()
        {
            trackingDevice = App.state.trackingDevice;
            installerVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            windowsReleaseId = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ReleaseId", "").ToString();
            windowsBuild = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentBuild", "").ToString();
            language = CultureInfo.CurrentUICulture.Name;
            try
            {
                var steamVrSettings = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(App.state.steamVrSettingsPath));
                headsetManufacturer = steamVrSettings["LastKnown"]["HMDManufacturer"];
                headsetModel = steamVrSettings["LastKnown"]["HMDModel"];
            }
            catch (Exception) { }
        }

        public string ToXmlString()
        {
            using (var writer = new StringWriter())
            {
                var xmlSerializer = new XmlSerializer(typeof(Analytics));
                xmlSerializer.Serialize(writer, this);
                return writer.ToString();
            }
        }
    }
}
