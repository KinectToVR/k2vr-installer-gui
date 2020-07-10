using System;
using System.Collections.Generic;
using System.Windows;
using System.Xml.Serialization;

namespace k2vr_installer_gui.Tools
{
    public class File
    {
        public string Name;
        public string Md5;
        public string OutName;
        public string PrettyName;
        public string Url;
    }

    public class FilesToDownload
    {
        public File[] Files { get; set; }
    }


    static class FileDownloader
    {
        public static readonly Dictionary<string, File> files = new Dictionary<string, File>();

        static FileDownloader()
        {
            var xmlSerializer = new XmlSerializer(typeof(FilesToDownload));
            var s = Application.GetResourceStream(new Uri("pack://application:,,,/Resources/FilesToDownload.xml"));
            FilesToDownload xmlFiles = (FilesToDownload)xmlSerializer.Deserialize(s.Stream);
            foreach (File file in xmlFiles.Files)
            {
                files[file.Name] = file;
            }
            s.Stream.Dispose();
        }
    }
}
