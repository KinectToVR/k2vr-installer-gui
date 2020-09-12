using k2vr_installer_gui.Tools;
using Microsoft.WindowsAPICodePack.Dialogs;
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
                TextBox_installLocation.Text = Path.GetFullPath(Environment.ExpandEnvironmentVariables(App.state.installationPath));
            }
            catch (Exception)
            {
                TextBox_installLocation.Text = Environment.ExpandEnvironmentVariables(@"%ProgramFiles%\KinectToVR");
            }
        }

        private void Button_ChooseLocation_Click(object sender, RoutedEventArgs e)
        {
            // METHOD 1: Windows Forms
            // Problem: Can't paste path
            //using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            //{
            //    System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            //    if (result == System.Windows.Forms.DialogResult.OK &&
            //        !string.IsNullOrWhiteSpace(dialog.SelectedPath))
            //    {
            //        TextBox_installLocation.Text = dialog.SelectedPath;
            //    }
            //}

            // METHOD 2: Hack adapted from https://stackoverflow.com/a/50261723/
            // Problem: Hacky af
            //string nbsp = "\u00a0";
            //var dialog = new Microsoft.Win32.SaveFileDialog
            //{
            //    InitialDirectory = TextBox_installLocation.Text,
            //    Title = "Select a Directory", // instead of default "Save As"
            //    Filter = "Directory|" + nbsp, // Prevents displaying files
            //    FileName = nbsp // Filename will be the nbsp character
            //};
            //if (dialog.ShowDialog() == true)
            //{
            //    string path = dialog.FileName;
            //    // Remove fake filename from resulting path
            //    path = path.Replace("\\" + nbsp, "");
            //    path = path.Replace(nbsp, "");
            //    // Our final value is in path
            //    TextBox_installLocation.Text = path;
            //}

            // METHOD 3: WindowsAPICodePack
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            if (Directory.Exists(TextBox_installLocation.Text))
            {
                dialog.InitialDirectory = TextBox_installLocation.Text;
            }
            else
            {
                dialog.InitialDirectory = Environment.ExpandEnvironmentVariables(@"%ProgramFiles%");
            }
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                TextBox_installLocation.Text = dialog.FileName;
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

            App.state.Write();
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
