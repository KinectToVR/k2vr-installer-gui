using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace k2vr_installer_gui.Pages.Popups
{
    /// <summary>
    /// Interaction logic for DownloadError.xaml
    /// </summary>
    public partial class DownloadError : Window
    {
        public DownloadError(string file, Exception exception, bool cancelled)
        {
            InitializeComponent();
            TextBlock_title.Text = $"Download of {file} failed!";
            if (exception != null)
            {
                if (exception is WebException)
                {
                    TextBlock_status.Text = "Status code: " + ((WebException)exception).Status.ToString();
                    if (((WebException)exception).Status == WebExceptionStatus.NameResolutionFailure)
                    {
                        TextBlock_hint.Text = "Please check your internet connection and try again." + Environment.NewLine + TextBlock_hint.Text;
                    }
                }
                TextBlock_reason.Text = exception.Message;
            }
            else if (cancelled)
            {
                TextBlock_status.Text = "Status code: Cancelled";
            }
            else
            {
                TextBlock_reason.Text = "The downloaded file was corrupted.";
                TextBlock_status.Text = "Status code: Checksum mismatch";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://discord.gg/Mu28W4N");
        }
    }
}
