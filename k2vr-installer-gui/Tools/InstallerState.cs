using System;
using System.IO;
using System.Management;
using System.Windows;
using System.Xml.Serialization;

namespace k2vr_installer_gui.Tools
{
    public class InstallerState
    {
        static readonly string path = App.exeDirectory + "installerSettings.xml";

        public enum TrackingDevice
        {
            Xbox360Kinect,
            XboxOneKinect,
            PlayStationMove,
            None
        }

        public TrackingDevice trackingDevice;
        public bool allowAnalytics;
        public string installationPath;

        public TrackingDevice pluggedInDevice = TrackingDevice.None;
        public bool kinect_v1_sdk_installed = false;
        public bool kinect_v2_sdk_installed = false;
        public bool ovrie_installed = false;
        public bool ovrie_fix_installed = false;
        public string steamvr_path = "";

        public void Write()
        {
            using (var writer = new StreamWriter(path))
            {
                var xmlSerializer = new XmlSerializer(typeof(InstallerState));
                xmlSerializer.Serialize(writer, this);
            }
        }

        public static InstallerState Read()
        {
            try
            {
                using (var reader = new StreamReader(path))
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

        public void Update()
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
    }
}
