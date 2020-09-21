using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Windows;
using System.Windows.Documents;
using System.Xml.Serialization;

namespace k2vr_installer_gui.Tools
{
    public class InstallerState
    {
        public const string fileName = "installerSettings.xml";

        public enum TrackingDevice
        {
            Xbox360Kinect,
            XboxOneKinect,
            PlayStationMove,
            None
        }

        public TrackingDevice trackingDevice = TrackingDevice.None;
        public bool allowAnalytics = false;
        public string installationPath;
        public List<string> installedFiles = new List<string>();
        public List<string> installedFolders = new List<string>();

        public TrackingDevice pluggedInDevice = TrackingDevice.None;
        public bool kinectV1SdkInstalled = false;
        public bool kinectV2SdkInstalled = false;
        public string steamPath = "";
        public string steamVrPath = "";
        public string vrPathReg = "";
        public string copiedDriverPath = "";

        public string GetFullInstallationPath()
        {
            return Path.GetFullPath(Environment.ExpandEnvironmentVariables(installationPath));
        }

        public void Write()
        {
            using (var writer = new StreamWriter(GetInstallerStatePath()))
            {
                var xmlSerializer = new XmlSerializer(typeof(InstallerState));
                xmlSerializer.Serialize(writer, this);
            }
        }

        public static InstallerState Read(string path)
        {
            if (path == "") return new InstallerState();
            try
            {
                using (var reader = new StreamReader(Path.Combine(path, fileName)))
                {
                    var xmlSerializer = new XmlSerializer(typeof(InstallerState));
                    return (InstallerState)xmlSerializer.Deserialize(reader);
                }
            }
            catch (FileNotFoundException)
            {
                return new InstallerState();
            }
        }

        public string GetInstallerStatePath()
        {
            return Path.Combine(GetFullInstallationPath(), fileName);
        }

        public void UpdatePluggedInDevice()
        {
            using (var searcher = new ManagementObjectSearcher(@"Select * From Win32_USBControllerDevice"))
            {
                ManagementObjectCollection devices = searcher.Get();
                foreach (ManagementBaseObject device in devices)
                {
                    try
                    {
                        string dependent = (string)device.GetPropertyValue("Dependent");
                        string devId = dependent.Substring(dependent.IndexOf("DeviceID=\""));
                        if (devId.Contains("02B0") || devId.Contains("02BB") || devId.Contains("02AE"))
                        {
                            pluggedInDevice = TrackingDevice.Xbox360Kinect;
                        }
                        if (devId.Contains("02C4"))
                        {
                            pluggedInDevice = TrackingDevice.XboxOneKinect;
                        }
                    }
                    catch (ManagementException) { }
                }
                devices.Dispose();
            }
        }

        public void UpdateSdkInstalled()
        {
            kinectV1SdkInstalled = false;
            if (Directory.Exists(Path.Combine("C:\\", "Program Files", "Microsoft SDKs", "Kinect", "v1.8")))
            {
                kinectV1SdkInstalled = true;
            }
            kinectV2SdkInstalled = false;
            if (Directory.Exists(Path.Combine("C:\\", "Program Files", "Microsoft SDKs", "Kinect", "v2.0_1409")))
            {
                kinectV2SdkInstalled = true;
            }
        }

        public void UpdateSteamPaths()
        {
            steamPath = "";
            steamPath = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath", "").ToString();
            if (steamPath == "")
            {
                MessageBox.Show("Steam installation folder not found!" + Environment.NewLine +
                    "Are you sure it is installed?" + Environment.NewLine +
                    "If you are, please join our Discord server for further assistance (link on www.k2vr.tech)");
                Application.Current.Shutdown(1);
                return;
            }

            string steamVrSettingsPath = Path.Combine(steamPath, "config", "steamvr.vrsettings");

            string libraryFoldersPath = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
            string[] libraryFoldersFile = File.ReadAllLines(libraryFoldersPath);
            List<string> libraryFolders = new List<string>();

            libraryFolders.Add(steamPath);

            for (int i = 4; i < libraryFoldersFile.Length - 1; i++)
            {
                string folder = libraryFoldersFile[i];
                folder = folder.Substring(7, folder.Length - (7 + 1)).Replace(@"\\", @"\");
                libraryFolders.Add(folder);
            }

            steamVrPath = "";

            foreach (string folder in libraryFolders)
            {
                string potentialSteamVrPath = Path.Combine(folder, @"steamapps", "common", "SteamVR");
                if (Directory.Exists(potentialSteamVrPath))
                {
                    steamVrPath = potentialSteamVrPath;
                    break;
                }
            }

            if (steamVrPath == "")
            {
                MessageBox.Show("SteamVR installation folder not found!" + Environment.NewLine +
                    "Are you sure it is installed?" + Environment.NewLine +
                    "If you are, please join our Discord server for further assistance (link on www.k2vr.tech)");
                Application.Current.Shutdown(1);
                return;
            }


            vrPathReg = Path.Combine(steamVrPath, "bin", "win64", "vrpathreg.exe");
            copiedDriverPath = Path.Combine(App.state.steamVrPath, "drivers", "KinectToVR");
        }

        public void Update()
        {
            UpdatePluggedInDevice();

            UpdateSdkInstalled();

            UpdateSteamPaths();
        }
    }
}
