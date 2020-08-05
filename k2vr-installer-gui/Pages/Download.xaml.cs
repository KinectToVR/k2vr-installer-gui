using k2vr_installer_gui.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace k2vr_installer_gui.Pages
{
    public class DownloadItem : INotifyPropertyChanged
    {
        public string Name { get; set; }

        private int percentage;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Percentage
        {
            get { return percentage; }
            set
            {
                percentage = value;
                PercentageString = value.ToString() + "%";
                if (percentage == 100)
                {
                    Expanded = false;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Expanded"));
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Percentage"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PercentageString"));
            }
        }

        public string PercentageString { get; set; }
        public bool Expanded { get; set; }

        public DownloadItem(string name)
        {
            Name = name;
            Percentage = 0;
            Expanded = true;
        }
    }

    /// <summary>
    /// Interaction logic for Download.xaml
    /// </summary>
    public partial class Download : UserControl, IInstallerPage
    {
        public List<DownloadItem> downloadItems = new List<DownloadItem>();

        public Download()
        {
            InitializeComponent();
        }

        private void AddToDownloadQueue(File file, string targetFolder)
        {
            var downloadItem = new DownloadItem(file.PrettyName);
            var wc = new WebClient();
            wc.DownloadFileAsync(new Uri(file.Url), targetFolder + file.OutName);
            wc.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) =>
            {
                downloadItem.Percentage = e.ProgressPercentage;
            };
            downloadItems.Add(downloadItem);
        }

        public void OnSelected()
        {
            string tempPath = App.exeDirectory + @"temp\";
            if (!System.IO.Directory.Exists(tempPath))
            {
                System.IO.Directory.CreateDirectory(tempPath);
            }
            foreach (KeyValuePair<string, File> entry in FileDownloader.files)
            {
                var file = entry.Value;
                if (file.AlwaysRequired)
                {
                    AddToDownloadQueue(file, tempPath);
                }
                else if (file.RequiredForDevice == App.state.trackingDevice)
                {
                    AddToDownloadQueue(file, tempPath);
                }
            }
            ItemsControl_downloads.ItemsSource = downloadItems;
        }
    }
}
