using k2vr_installer_gui.Pages.Popups;
using k2vr_installer_gui.Tools;
using k2vr_installer_gui.Tools.OpenVRFiles;
using k2vr_installer_gui.Uninstall;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        public void Cancel()
        {
            Dispatcher.Invoke(() =>
            {
                Application.Current.Shutdown(1);
            });
        }

        // https://stackoverflow.com/a/58779/
        static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            foreach (FileInfo file in source.GetFiles())
                file.CopyTo(Path.Combine(target.FullName, file.Name));
        }

        public async void OnSelected()
        {
            Logger.LogEvent += (LogEventArgs e) =>
            {
                if (!e.isUserRelevant) return;
                TextBlock_installLog.Dispatcher.Invoke(() =>
                {
                    TextBlock_installLog.Text += e.text;
                });
            };
            await Task.Run(() =>
            {
                Logger.Log("K2EX Installer " + Assembly.GetExecutingAssembly().GetName().Version.ToString() + " on " + DateTime.Now.ToString());

                Logger.Log("Checking if SteamVR is open...", false);
                foreach (Process process in Process.GetProcesses())
                {
                    if (process.ProcessName == "vrmonitor")
                    {
                        Logger.Log("Closing vrmonitor...", false);
                        process.CloseMainWindow();
                        Thread.Sleep(5000);
                        if (!process.HasExited)
                        {
                            Logger.Log("Force closing...", false);
                            /* When SteamVR is open with no headset detected,
                               CloseMainWindow will only close the "headset not found" popup
                               so we kill it, if it's still open */
                            process.Kill();
                            Thread.Sleep(3000);
                        }
                    }
                }

                /* Apparently, SteamVR server can run without the monitor,
                   so we close that, if it's open as well (monitor will complain if you close server first) */
                foreach (Process process in Process.GetProcesses())
                {
                    if (process.ProcessName == "vrserver")
                    {
                        Logger.Log("Closing vrserver...", false);
                        // CloseMainWindow won't work here because it doesn't have a window
                        process.Kill();
                        Thread.Sleep(5000);
                        if (!process.HasExited)
                        {
                            MessageBox.Show(Properties.Resources.install_steamvr_close_failed);
                            Cancel();
                            return;
                        }
                    }
                }
                Logger.Log("Done!");

                Logger.Log("Checking for legacy installations...", false);
                Uninstaller.UninstallK2VrLegacy();

                Logger.Log("Checking for other K2EX installations...", false);
                try
                {
                    if (!Uninstaller.UninstallAllK2EX(this)) return;
                }
                catch (Exception e)
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (new ExceptionDialog(e, true).ShowDialog().Value != true)
                        {
                            Application.Current.Shutdown(1);
                        }
                    });
                }
                Logger.Log("Done!");

                Logger.Log("Checking install directory...", false);
                bool dirExists = Directory.Exists(App.state.GetFullInstallationPath());
                Logger.Log("Done!");
                Logger.Log("Creating install directory...", false);
                if (!dirExists) Directory.CreateDirectory(App.state.GetFullInstallationPath());
                Logger.Log("Done!");

                Logger.Log("Extracting OpenVR driver...", false);
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
                            App.state.installedFolders.Add(fullPath);
                        }
                        else
                        {
                            entry.ExtractToFile(fullPath, true);
                            App.state.installedFiles.Add(fullPath);
                        }
                    }
                }
                Logger.Log("Done!");

                Logger.Log("Registering application...", false);
                App.state.Write();
                // we need to manually set file attribs before copying.
                try
                {
                    File.SetAttributes(Path.Combine(App.state.GetFullInstallationPath(), "k2vr-installer-gui.exe"), FileAttributes.Normal);
                }
                catch (Exception)
                {
                }
                File.Copy(Assembly.GetExecutingAssembly().Location, Path.Combine(App.state.GetFullInstallationPath(), "k2vr-installer-gui.exe"), true);
                Uninstaller.RegisterUninstaller();

                if ((App.state.trackingDevice == InstallerState.TrackingDevice.Xbox360Kinect && !App.state.kinectV1SdkInstalled) ||
                    (App.state.trackingDevice == InstallerState.TrackingDevice.XboxOneKinect && !App.state.kinectV2SdkInstalled))
                {
                    Logger.Log("Installing the Kinect SDK...", false);
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
                    catch (Exception) { } // Don't want the whole install to fail for something that mundane
                    sdkInstallerProcess.WaitForExit();

                    App.state.UpdateSdkInstalled();

                    if ((App.state.trackingDevice == InstallerState.TrackingDevice.Xbox360Kinect && !App.state.kinectV1SdkInstalled) ||
                        (App.state.trackingDevice == InstallerState.TrackingDevice.XboxOneKinect && !App.state.kinectV2SdkInstalled))
                    {
                        Logger.Log("Failed!");
                        MessageBox.Show(Properties.Resources.install_sdk_failed);
                        Cancel();
                        return;
                    }
                    Logger.Log("Done!");
                }
                else
                {
                    Logger.Log("Kinect SDK is already installed.");
                }

                Logger.Log("Installing Visual C++ Redistributable...", false);
                string vcRedistPath = Path.Combine(App.downloadDirectory, FileDownloader.files["vc_redist2019"].OutName);
                Process.Start(vcRedistPath, "/quiet /norestart").WaitForExit();
                Logger.Log("Done!");

                Logger.Log("Registering OpenVR driver...", false);
                string driverPath = Path.Combine(App.state.GetFullInstallationPath(), "KinectToVR");
                Process.Start(App.state.vrPathReg, "adddriver \"" + driverPath + "\"").WaitForExit();
                Logger.Log("Checking...", false);
                var openVrPaths = OpenVrPaths.Read();
                if (!openVrPaths.external_drivers.Contains(driverPath))
                {
                    Logger.Log("Copying...", false);
                    CopyFilesRecursively(new DirectoryInfo(driverPath), new DirectoryInfo(App.state.copiedDriverPath));
                }
                Logger.Log("Done!");

                string kinectProcessName = "KinectV" + (App.state.trackingDevice == InstallerState.TrackingDevice.XboxOneKinect ? "2" : "1") + "Process";
                if (App.state.trackingDevice == InstallerState.TrackingDevice.PlayStationMove)
                {
                    kinectProcessName = "psmsprocess";
                }

                Logger.Log("Registering OpenVR overlay...", false);

                var appConfig = AppConfig.Read();
                string manifestPath = Path.Combine(App.state.GetFullInstallationPath(), kinectProcessName + ".vrmanifest");
                if (!appConfig.manifest_paths.Contains(manifestPath))
                {
                    appConfig.manifest_paths.Add(manifestPath);
                    appConfig.Write();
                    Logger.Log("Done!");
                }
                else
                {
                    Logger.Log("Already done!");
                }

                Logger.Log("Disabling SteamVR Home, enabling advanced settings...", false);
                var steamVrSettings = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(App.state.steamVrSettingsPath));
                try
                {
                    steamVrSettings["steamvr"]["enableHomeApp"] = false;
                    steamVrSettings["steamvr"]["showAdvancedSettings"] = true;
                    JsonFile.Write(App.state.steamVrSettingsPath, steamVrSettings, 3, ' ');
                    Logger.Log("Done!");
                }
                catch (Exception)
                {
                    Logger.Log("Failed (uncritical)!");
                }
                Logger.Log("Registering tracker roles...", false);
                try
                {
                    if (steamVrSettings["trackers"] == null)
                    {
                        steamVrSettings["trackers"] = new JObject();
                    }
                    steamVrSettings["trackers"]["/devices/htc/vive_trackerLHR-CB11ABEC"] = "TrackerRole_Waist";
                    steamVrSettings["trackers"]["/devices/htc/vive_trackerLHR-CB1441A7"] = "TrackerRole_RightFoot";
                    steamVrSettings["trackers"]["/devices/htc/vive_trackerLHR-CB9AD1T2"] = "TrackerRole_LeftFoot";
                    JsonFile.Write(App.state.steamVrSettingsPath, steamVrSettings, 3, ' ');
                    Logger.Log("Done!");
                }
                catch (Exception)
                {
                    Logger.Log("Failed (uncritical)!");
                }
                Logger.Log("Enabling driver in SteamVR...", false);
                try
                {
                    if (steamVrSettings["driver_kinecttovr"] == null)
                    {
                        steamVrSettings["driver_kinecttovr"] = new JObject();
                    }
                    steamVrSettings["driver_kinecttovr"]["enable"] = true;
                    steamVrSettings["driver_kinecttovr"]["blocked_by_safe_mode"] = false;
                    JsonFile.Write(App.state.steamVrSettingsPath, steamVrSettings, 3, ' ');
                    Logger.Log("Done!");
                }
                catch (Exception)
                {
                    Logger.Log("Failed (uncritical)!");
                }

                Logger.Log("Creating start menu entry...", false);
                if (!Directory.Exists(App.startMenuFolder)) Directory.CreateDirectory(App.startMenuFolder);
                // https://stackoverflow.com/a/4909475/
                var shell = new IWshRuntimeLibrary.WshShell();
                string shortcutAddress = Path.Combine(App.startMenuFolder, "KinectToVR.lnk");
                var shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutAddress);
                shortcut.Description = "Launch KinectToVR";
                shortcut.TargetPath = Path.Combine(App.state.GetFullInstallationPath(), kinectProcessName + ".exe");
                shortcut.IconLocation = Path.Combine(App.state.GetFullInstallationPath(), "k2vr.ico");
                shortcut.WorkingDirectory = Path.Combine(App.state.GetFullInstallationPath());
                shortcut.Save();
                Logger.Log("Refreshing...", false);
                foreach (Process process in Process.GetProcesses())
                {
                    if (process.ProcessName == "StartMenuExperienceHost")
                    {
                        process.Kill();
                        Thread.Sleep(500);
                    }
                }
                Logger.Log("Done!");

                Logger.Log("Installation complete!");
                Logger.Log("The installation log can be found in \"" + App.downloadDirectory + "\"", false);
                Button_Complete_Install.Dispatcher.Invoke(() =>
                {
                    Button_Complete_Install.IsEnabled = true;
                });
            });
        }

        private void Button_Complete_Install_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).GoToTab(4);
        }
    }
}
