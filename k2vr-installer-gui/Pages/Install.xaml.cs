using k2vr_installer_gui.Tools;
using k2vr_installer_gui.Tools.OpenVRFiles;
using k2vr_installer_gui.Uninstall;
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

        public void Log(string text, bool newLine = true)
        {
            if (newLine) text += Environment.NewLine;
            TextBlock_installLog.Dispatcher.Invoke(() =>
            {
                TextBlock_installLog.Text += text;
            });
            App.Log(text);
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
                   so we close that, if it's open as well (monitor will complain if you close server first) */
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
                            Cancel();
                            return;
                        }
                    }
                }
                Log("Done!");

                Log("Checking for legacy installations...", false);
                Uninstaller.UninstallK2VrLegacy(this);

                Log("Checking for other K2EX installations...", false);
                if (!Uninstaller.UninstallAllK2EX(this)) return;
                Log("Done!");

                Log("Checking install directory...", false);
                bool dirExists = Directory.Exists(App.state.GetFullInstallationPath());
                Log("Done!");
                Log("Creating install directory...", false);
                if (!dirExists) Directory.CreateDirectory(App.state.GetFullInstallationPath());
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
                            App.state.installedFolders.Add(fullPath);
                        }
                        else
                        {
                            entry.ExtractToFile(fullPath, true);
                            App.state.installedFiles.Add(fullPath);
                        }
                    }
                }
                Log("Done!");

                Log("Registering application...", false);
                App.state.Write();
                File.Copy(Assembly.GetExecutingAssembly().Location, Path.Combine(App.state.GetFullInstallationPath(), "k2vr-installer-gui.exe"), true);
                Uninstaller.RegisterUninstaller();

                if ((App.state.trackingDevice == InstallerState.TrackingDevice.Xbox360Kinect && !App.state.kinectV1SdkInstalled) ||
                    (App.state.trackingDevice == InstallerState.TrackingDevice.XboxOneKinect && !App.state.kinectV2SdkInstalled))
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
                    catch (Exception) { } // Don't want the whole install to fail for something that mundane
                    sdkInstallerProcess.WaitForExit();

                    App.state.UpdateSdkInstalled();

                    if ((App.state.trackingDevice == InstallerState.TrackingDevice.Xbox360Kinect && !App.state.kinectV1SdkInstalled) ||
                        (App.state.trackingDevice == InstallerState.TrackingDevice.XboxOneKinect && !App.state.kinectV2SdkInstalled))
                    {
                        Log("Failed!");
                        MessageBox.Show("Kinect SDK was not installed successfully!" + Environment.NewLine +
                            "Restart this installer to try again" + Environment.NewLine +
                            "Please join our Discord server for further assistance (link on www.k2vr.tech)");
                        Cancel();
                        return;
                    }
                    Log("Done!");
                } else {
                    Log("Kinect SDK is already installed.");
                }

                Log("Registering OpenVR driver...", false);
                string driverPath = Path.Combine(App.state.GetFullInstallationPath(), "KinectToVR");
                Process.Start(App.state.vrPathReg, "adddriver \"" + driverPath + "\"").WaitForExit();
                Log("Checking...", false);
                var openVrPaths = OpenVrPaths.Read();
                if (!openVrPaths.external_drivers.Contains(driverPath))
                {
                    Log("Copying...", false);
                    CopyFilesRecursively(new DirectoryInfo(driverPath), new DirectoryInfo(App.state.copiedDriverPath));
                }
                Log("Done!");

                string kinectProcessName = "KinectV" + (App.state.trackingDevice == InstallerState.TrackingDevice.XboxOneKinect ? "2" : "1") + "Process";

                Log("Registering OpenVR overlay...", false);

                var appConfig = AppConfig.Read();
                string manifestPath = Path.Combine(App.state.GetFullInstallationPath(), kinectProcessName + ".vrmanifest");
                if (!appConfig.manifest_paths.Contains(manifestPath))
                {
                    appConfig.manifest_paths.Add(manifestPath);
                    appConfig.Write();
                    Log("Done!");
                }
                else
                {
                    Log("Already done!");
                }

                Log("Creating start menu entry...", false);
                if (!Directory.Exists(App.startMenuFolder)) Directory.CreateDirectory(App.startMenuFolder);
                // https://stackoverflow.com/a/4909475/
                var shell = new IWshRuntimeLibrary.WshShell();
                string shortcutAddress = Path.Combine(App.startMenuFolder, "KinectToVR.lnk");
                var shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutAddress);
                shortcut.Description = "Launch KinectToVR";
                shortcut.TargetPath = Path.Combine(App.state.GetFullInstallationPath(), kinectProcessName + ".exe");
                shortcut.IconLocation = Path.Combine(App.state.GetFullInstallationPath(), "k2vr.ico");
                shortcut.Save();
                Log("Refreshing...", false);
                foreach (Process process in Process.GetProcesses())
                {
                    if (process.ProcessName == "StartMenuExperienceHost")
                    {
                        process.Kill();
                        Thread.Sleep(500);
                    }
                }
                Log("Done!");

                Log("Installation complete!");
            });
        }
    }
}
