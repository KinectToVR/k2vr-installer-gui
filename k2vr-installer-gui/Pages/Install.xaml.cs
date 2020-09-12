using k2vr_installer_gui.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace k2vr_installer_gui.Pages
{
    /// <summary>
    /// Interaction logic for Install.xaml
    /// </summary>
    public partial class Install : UserControl, IInstallerPage
    {
        public Install()
        {
            InitializeComponent();
        }

        private void Log(string text, bool newLine = true)
        {
            if (newLine) text += Environment.NewLine;
            TextBlock_installLog.Dispatcher.Invoke(() =>
            {
                TextBlock_installLog.Text += text;
            });
            File.AppendAllText(Path.Combine(App.downloadDirectory, "install.log"), text);
        }

        public async void OnSelected()
        {
            await Task.Run(() =>
            {
                Log("K2VR Installer " + Assembly.GetExecutingAssembly().GetName().Version.ToString() + " on " + DateTime.Now.ToString());

                Log("Checking if SteamVR is open...", false);
                foreach (Process process in Process.GetProcesses())
                {
                    if (process.ProcessName == "vrmonitor")
                    {
                        Log("Closing vrmonitor...", false);
                        process.CloseMainWindow();
                        Thread.Sleep(5000);
                        if (!process.HasExited)
                        {
                            Log("Force closing...", false);
                            /* When SteamVR is open with no headset detected,
                               CloseMainWindow will only close the "headset not found" popup
                               so we kill it, if it's still open */
                            process.Kill();
                            Thread.Sleep(3000);
                        }
                    }
                }

                /* Apparently, SteamVR server can run without the monitor,
                   so we close that, if it's open aswell (monitor will complain if you close server first) */
                foreach (Process process in Process.GetProcesses())
                {
                    if (process.ProcessName == "vrserver")
                    {
                        Log("Closing vrserver...", false);
                        // CloseMainWindow won't work here because it doesn't have a window
                        process.Kill();
                        Thread.Sleep(5000);
                        if (!process.HasExited)
                        {
                            MessageBox.Show("Couldn't close SteamVR, please close it manually and try running this installer again!");
                            Dispatcher.Invoke(() =>
                            {
                                Application.Current.Shutdown(1);
                            });
                        }
                    }
                }
                Log("Done!");

                Log("Checking install directory...", false);
                bool dirExists = Directory.Exists(App.state.GetFullInstallationPath());
                Log("Done!");
                if (dirExists)
                {
                    MessageBoxResult res = MessageBox.Show("K2VR appears to be already installed in this directory. Do you wish to uninstall it?", "Already installed", MessageBoxButton.YesNo);
                    if (res == MessageBoxResult.Yes)
                    {
                        if (MessageBox.Show("The directory \"" + App.state.GetFullInstallationPath() + "\" and all its contents will be permanently deleted!" + Environment.NewLine +
                            "Press OK to confirm.", "Confirm", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                        {
                            Log("Deleting old installation...", false);
                            Directory.Delete(App.state.GetFullInstallationPath(), true);
                            Log("Done!");
                        }
                        else
                        {
                            MessageBox.Show("Process aborted, closing.");
                            Dispatcher.Invoke(() =>
                            {
                                Application.Current.Shutdown(1);
                            });
                            return;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Cannot install two versions of KinectToVR into the same directory." + Environment.NewLine +
                            "Either uninstall the old version or select another install directory." + Environment.NewLine +
                            "Restart this installer to try again");
                        Dispatcher.Invoke(() =>
                        {
                            Application.Current.Shutdown(1);
                        });
                        return;
                    }
                }
                Log("Creating install directory...", false);
                Directory.CreateDirectory(App.state.GetFullInstallationPath());
                Log("Done!");

                Log("Extracting OpenVR driver...", false);
                string zipFileName = Path.Combine(App.downloadDirectory + FileDownloader.files["k2vr"].OutName);
                using (ZipArchive archive = ZipFile.OpenRead(zipFileName))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        string newName = entry.FullName.Substring("K2EX/".Length); // Remove the top level folder
                        string fullPath = Path.GetFullPath(Path.Combine(App.state.GetFullInstallationPath(), newName));
                        if (fullPath.EndsWith(@"\", StringComparison.Ordinal))
                        {
                            Directory.CreateDirectory(fullPath);
                        }
                        else
                        {
                            entry.ExtractToFile(fullPath, true);
                        }
                    }
                }
                Log("Done!");

                if (App.state.trackingDevice == InstallerState.TrackingDevice.Xbox360Kinect ||
                App.state.trackingDevice == InstallerState.TrackingDevice.XboxOneKinect)
                {
                    Log("Installing the Kinect SDK...", false);
                    string sdkInstaller = Path.Combine(App.downloadDirectory, FileDownloader.files[(App.state.trackingDevice == InstallerState.TrackingDevice.Xbox360Kinect) ? "kinect_v1_sdk" : "kinect_v2_sdk"].OutName);
                    Process sdkInstallerProcess = Process.Start(sdkInstaller);
                    Thread.Sleep(1000);
                    try
                    {
                        // https://stackoverflow.com/a/3734322/
                        AutomationElement element = AutomationElement.FromHandle(sdkInstallerProcess.MainWindowHandle);
                        if (element != null)
                        {
                            element.SetFocus();
                        }
                    }
                    catch (Exception e) { } // Don't want the whole install to fail for something that mundane
                    sdkInstallerProcess.WaitForExit();
                    Log("Done!");
                }

                Log("Registering OpenVR driver...", false);
                string vrPathReg = Path.Combine(App.state.steamVrPath, "bin", "win64", "vrpathreg.exe");
                Process.Start(vrPathReg, "adddriver \"" + Path.Combine(App.state.GetFullInstallationPath(), "KinectToVR") + "\"");
                Log("Checking...", false);
                string openVrPathsPath = Environment.ExpandEnvironmentVariables(Path.Combine("%LocalAppData%", "openvr", "openvrpaths.vrpath"));
                if (!File.ReadAllText(openVrPathsPath).Contains("KinectToVR")) // ToDo: Do this properly
                {
                    Log("Didn't work...", false);
                }
                Log("Done!");

                Log("Registering OpenVR overlay...", false);
                string appConfigPath = Path.Combine(App.state.steamPath, "config", "appconfig.json");
                // ToDo: Do this properly
                string appConfig = File.ReadAllText(appConfigPath);
                if (!Regex.IsMatch(appConfig, @"KinectV\dProcess\.vrmanifest"))
                {
                    appConfig = appConfig.Replace("   ]", ", \"" +
                        Path.Combine(App.state.installationPath,
                            "KinectV" + (App.state.trackingDevice == InstallerState.TrackingDevice.XboxOneKinect ? "2" : "1") + "Process.vrmanifest")
                            .Replace(@"\", @"\\") +
                       "\"\n   ]");
                    File.WriteAllText(appConfigPath, appConfig);
                    Log("Done!");
                }
                else
                {
                    Log("Already done!");
                }

                Log("Installation complete!");
            });
        }
    }
}
