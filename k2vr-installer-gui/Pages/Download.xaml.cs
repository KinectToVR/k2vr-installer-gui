using k2vr_installer_gui.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace k2vr_installer_gui.Pages
{
    public class DownloadItem: INotifyPropertyChanged
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

        public string PercentageString { get; set;  }
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
                var downloadItem = new DownloadItem(file.PrettyName);
                var wc = new WebClient();
                wc.DownloadFileAsync(new Uri(file.Url), tempPath + file.OutName);
                wc.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) =>
                {
                    downloadItem.Percentage = e.ProgressPercentage;
                };
                downloadItems.Add(downloadItem);
            }
            ItemsControl_downloads.ItemsSource = downloadItems;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(downloadItems);
        }
    }
}
