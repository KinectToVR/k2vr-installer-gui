using k2vr_installer_gui.Tools;
using Ookii.Dialogs.Wpf;
using System;
using System.Configuration;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using static k2vr_installer_gui.Tools.InstallerState;

namespace k2vr_installer_gui.Pages
{
    public class Device
    {
        public string Name { get; set; }
        public string PrettyName { get; set; }
        public string Image { get; set; }
        public string Information { get; set; }

        public Device() { }
    }

    /// <summary>
    /// Interaction logic for Setup.xaml
    /// </summary>
    public partial class Setup : UserControl, IInstallerPage
    {
        public Setup()
        {
            InitializeComponent();
            switch (App.state.pluggedInDevice)
            {
                case TrackingDevice.XboxOneKinect:
                    ListBox_devices.SelectedIndex = 0;
                    break;
                case TrackingDevice.Xbox360Kinect:
                    ListBox_devices.SelectedIndex = 1;
                    break;
            }
            if (App.state.pluggedInDevice != TrackingDevice.None)
            {
                ((Device)ListBox_devices.SelectedItem).Information = "(Detected)";
            }
            switch (App.state.trackingDevice)
            {
                case TrackingDevice.XboxOneKinect:
                    ListBox_devices.SelectedIndex = 0;
                    break;
                case TrackingDevice.Xbox360Kinect:
                    ListBox_devices.SelectedIndex = 1;
                    break;
                case TrackingDevice.PlayStationMove:
                    ListBox_devices.SelectedIndex = 2;
                    break;
            }
            if (App.state.trackingDevice != TrackingDevice.None)
            {
                ((Device)ListBox_devices.SelectedItem).Information = "(Remembered)";
            }
            CheckBox_analytics.IsChecked = App.state.allowAnalytics;
            try
            {
                TextBox_installLocation.Text = App.state.GetFullInstallationPath();
            }
            catch (Exception)
            {
                TextBox_installLocation.Text = Environment.ExpandEnvironmentVariables(@"%ProgramFiles%\KinectToVR");
            }
        }

        private void Button_ChooseLocation_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog();
            if (Directory.Exists(TextBox_installLocation.Text) || new DirectoryInfo(TextBox_installLocation.Text).Root.Exists)
            {
                dialog.SelectedPath = TextBox_installLocation.Text;
            }
            else
            {
                dialog.SelectedPath = Environment.ExpandEnvironmentVariables(@"%ProgramFiles%");
            }
            if (dialog.ShowDialog() == true)
            {
                TextBox_installLocation.Text = dialog.SelectedPath;
            }
        }

        private void Button_Install_Click(object sender, RoutedEventArgs e)
        {
            if (this.ListBox_devices.SelectedItem == null)
            {
                MessageBox.Show("No device selected!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrWhiteSpace(TextBox_installLocation.Text))
            {
                MessageBox.Show("No install location selected!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string installLocation;
            try
            {
                installLocation = Path.GetFullPath(Environment.ExpandEnvironmentVariables(TextBox_installLocation.Text));
            }
            catch (Exception)
            {
                MessageBox.Show("Invalid install location!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            TrackingDevice device = TrackingDevice.None;
            switch (((Device)this.ListBox_devices.SelectedItem).Name)
            {
                case "kinect_v2":
                    device = TrackingDevice.XboxOneKinect;
                    break;
                case "kinect_v1":
                    device = TrackingDevice.Xbox360Kinect;
                    break;
                case "psmove":
                    device = TrackingDevice.PlayStationMove;
                    break;
            }

            App.state.trackingDevice = device;
            App.state.allowAnalytics = CheckBox_analytics.IsChecked == true;
            App.state.installationPath = installLocation;

            ((MainWindow)Application.Current.MainWindow).GoToTab(2);
        }

        private void Hyperlink_Analytics_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(new Analytics().ToXmlString());
        }

        public void OnSelected()
        {
        }
    }
}
