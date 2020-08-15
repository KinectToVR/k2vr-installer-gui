using System;
using System.Collections.Generic;
using System.Windows;
using System.Xml.Serialization;

namespace k2vr_installer_gui.Tools
{
    public class FileToDownload
    {
        public string Name;
        public string Md5;
        public string OutName;
        public string PrettyName;
        public string Url;
        public bool AlwaysRequired = false;
        public InstallerState.TrackingDevice RequiredForDevice = InstallerState.TrackingDevice.None;
    }

    public class FilesToDownload
    {
        public FileToDownload[] Files { get; set; }
    }


    static class FileDownloader
    {
        public static readonly Dictionary<string, FileToDownload> files = new Dictionary<string, FileToDownload>();

        static FileDownloader()
        {
            var xmlSerializer = new XmlSerializer(typeof(FilesToDownload));
            var s = Application.GetResourceStream(new Uri("pack://application:,,,/Resources/FilesToDownload.xml"));
            FilesToDownload xmlFiles = (FilesToDownload)xmlSerializer.Deserialize(s.Stream);
            foreach (FileToDownload file in xmlFiles.Files)
            {
                files[file.Name] = file;
            }
            s.Stream.Dispose();
        }
    }
}
