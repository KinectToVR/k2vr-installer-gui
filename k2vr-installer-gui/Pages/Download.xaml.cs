using k2vr_installer_gui.Pages.Popups;
using k2vr_installer_gui.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xaml;

namespace k2vr_installer_gui.Pages
{
    public class DownloadItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string originalTitle;

        public string Title { get; set; }

        public FileToDownload FileToDownload;

        public string Destination;

        public int FailedDownloadAttempts = 0;

        private int percentage;

        public int Percentage
        {
            get { return percentage; }
            set
            {
                percentage = value;
                PercentageString = value.ToString() + "%";
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Percentage"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PercentageString"));
            }
        }

        public string PercentageString { get; set; }

        private bool expanded = false;

        public bool Expanded
        {
            get { return expanded; }
            set
            {
                expanded = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Expanded"));
            }
        }

        public DownloadItem(string title, FileToDownload fileToDownload, string destination)
        {
            originalTitle = title;
            Title = title;
            FileToDownload = fileToDownload;
            Destination = destination;
            Percentage = 0;
        }

        public void SetStatus(string value)
        {
            Title = originalTitle + " - " + value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Title"));
        }
    }

    /// <summary>
    /// Interaction logic for Download.xaml
    /// </summary>
    public partial class Download : UserControl, IInstallerPage
    {
        public const int MaxFailedDownloadAttemps = 2;

        public List<DownloadItem> downloadQueue = new List<DownloadItem>();
        public int currentQueuePosition = -1;

        public Download()
        {
            InitializeComponent();
        }

        private void AddToDownloadQueue(FileToDownload file, string targetFolder)
        {
            var downloadItem = new DownloadItem(file.PrettyName, file, targetFolder + file.OutName);
            downloadItem.SetStatus("pending");
            downloadQueue.Add(downloadItem);
        }

        private async Task<bool> CheckMD5(DownloadItem item)
        {
            return await Task<bool>.Run(() =>
            {
                if (File.Exists(item.Destination))
                {
                    using (var md5 = MD5.Create())
                    {
                        using (var stream = File.OpenRead(item.Destination))
                        {
                            // https://stackoverflow.com/a/10520086/
                            if (BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "") == item.FileToDownload.Md5)
                            {
                                return true;
                            }
                            else if (item.FileToDownload.Md5 == "0")
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            });
        }

        private async Task ProcessNextQueueItem(WebClient wc)
        {
            currentQueuePosition++;
            if (currentQueuePosition < downloadQueue.Count)
            {
                DownloadItem item = downloadQueue[currentQueuePosition];
                if (await CheckMD5(item))
                {
                    item.SetStatus("already downloaded");
                    item.Percentage = 100;
                    item.Expanded = false;
                    await ProcessNextQueueItem(wc);
                }
                else
                {
                    item.SetStatus("downloading");
                    item.Expanded = true;
                    wc.DownloadFileAsync(new Uri(item.FileToDownload.Url), item.Destination);
                }
            }
            else
            {
                wc.Dispose();
                ((MainWindow)Application.Current.MainWindow).GoToTab(3);
            }
        }

        public async void OnSelected()
        {
            if (!Directory.Exists(App.downloadDirectory))
            {
                Directory.CreateDirectory(App.downloadDirectory);
            }
            foreach (KeyValuePair<string, FileToDownload> entry in FileDownloader.files)
            {
                var file = entry.Value;
                if (file.AlwaysRequired)
                {
                    AddToDownloadQueue(file, App.downloadDirectory);
                }
                else if (file.RequiredForDevice == App.state.trackingDevice)
                {
                    if ((App.state.trackingDevice == InstallerState.TrackingDevice.Xbox360Kinect && !App.state.kinectV1SdkInstalled) ||
                        (App.state.trackingDevice == InstallerState.TrackingDevice.XboxOneKinect && !App.state.kinectV2SdkInstalled))
                    {
                        AddToDownloadQueue(file, App.downloadDirectory);
                    }
                }
            }

            ItemsControl_downloads.ItemsSource = downloadQueue;

            var wc = new WebClient();

            await ProcessNextQueueItem(wc);

            wc.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) =>
            {
                downloadQueue[currentQueuePosition].Percentage = e.ProgressPercentage;
            };
            wc.DownloadFileCompleted += async (object sender, AsyncCompletedEventArgs e) =>
            {
                DownloadItem item = downloadQueue[currentQueuePosition];
                item.SetStatus("checking");
                if (e.Error == null && !e.Cancelled && await CheckMD5(item))
                {
                    item.SetStatus("downloaded");
                    item.Expanded = false;
                }
                else
                {
                    item.FailedDownloadAttempts += 1;
                    if (item.FailedDownloadAttempts >= MaxFailedDownloadAttemps)
                    {
                        new DownloadError(item.FileToDownload.PrettyName, e.Error, e.Cancelled).ShowDialog();
                        Application.Current.Shutdown(1);
                        return;
                    }
                    currentQueuePosition--;
                }
                await ProcessNextQueueItem(wc);
            };
        }
    }
}
