using k2vr_installer_gui.Tools.OpenVRFiles;
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
            Xbox360Kinect, // also refers to kinect for windows v1
            XboxOneKinect, // also refers to kinect for windows v2
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
        public string steamVrSettingsPath = "";
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
            catch (DirectoryNotFoundException)
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
                        if (
                            devId.Contains("02B0") || // kinect 360 main
                            devId.Contains("02BB") || // kinect 360 audio
                            devId.Contains("02AE") || // kinect 360 camera
                            devId.Contains("02C2") || // kinect v1 main
                            devId.Contains("02BE") || // kinect v1 audio
                            devId.Contains("02BF") || // kinect v1 camera
                            devId.Contains("02C3"))   // kinect v1 security
                        {
                            pluggedInDevice = TrackingDevice.Xbox360Kinect;
                        }
                        if (devId.Contains("02C4") || // kinect one main
                            devId.Contains("02D8") || // kinect v2 main
                            devId.Contains("02D9"))   // kinect v2 hub
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

            steamVrPath = "";
            vrPathReg = "";
            try
            {
                var openVrPaths = OpenVrPaths.Read();
                foreach (string runtimePath in openVrPaths.runtime)
                {
                    string tempVrPathReg = Path.Combine(runtimePath, "bin", "win64", "vrpathreg.exe");
                    if (File.Exists(tempVrPathReg))
                    {
                        steamVrPath = runtimePath;
                        vrPathReg = tempVrPathReg;
                        break;
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("SteamVR installation folder not found!" + Environment.NewLine +
                                "Are you sure it is installed?" + Environment.NewLine +
                                "If you are, please join our Discord server for further assistance (link on www.k2vr.tech)");
                Application.Current.Shutdown(1);
                return;
            }
            if (vrPathReg == "")
            {
                MessageBox.Show("VRPathReg not found!" + Environment.NewLine +
                                "Please join our Discord server for further assistance (link on www.k2vr.tech)");
                Application.Current.Shutdown(1);
                return;
            }

            steamVrSettingsPath = Path.Combine(steamPath, "config", "steamvr.vrsettings");
            copiedDriverPath = Path.Combine(steamVrPath, "drivers", "KinectToVR");

            if (!File.Exists(steamVrSettingsPath))
            {
                MessageBox.Show("steamvr.vrsettings not found!" + Environment.NewLine +
                                "Make sure SteamVR has been launched at least once on this machine." + Environment.NewLine +
                                "Please join our Discord server for further assistance (link on www.k2vr.tech)");
                Application.Current.Shutdown(1);
            }
        }

        public void Update()
        {
            UpdatePluggedInDevice();

            UpdateSdkInstalled();

            UpdateSteamPaths();
        }
    }
}
