using k2vr_installer_gui.Tools;
using k2vr_installer_gui.Tools.OpenVRFiles;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace k2vr_installer_gui.Uninstall
{
    class K2EXInstallProperties
    {
        public bool DriverRegistered = false;
        public bool AppConfigRegistered = false;
    }
    public class OpenVrDriverFiles
    {
        public List<string> Files;
        public List<string> Folders;

        public static OpenVrDriverFiles Read()
        {
            var xmlSerializer = new XmlSerializer(typeof(OpenVrDriverFiles));
            var s = Application.GetResourceStream(new Uri("pack://application:,,,/Resources/OpenVrDriverFiles.xml"));
            OpenVrDriverFiles obj = (OpenVrDriverFiles)xmlSerializer.Deserialize(s.Stream);
            s.Stream.Dispose();
            return obj;
        }
    }
    static class Uninstaller
    {
        static string k2exDefaultPath = Path.Combine("C:\\", "K2EX");
        static string k2vrLegacyDefaultPath = Path.Combine("C:\\", "KinectToVR");
        static Guid uninstallGuid = new Guid("ba21a8d1-e588-48ab-bf4c-b37e8fb3708e"); // this was randomly generated

        public static void UninstallK2VrLegacy(Pages.Install installPage)
        {
            string path = k2vrLegacyDefaultPath;
            if (Directory.Exists(path))
            {
                installPage.Log("K2VR Legacy installation found...", false);
                if (MessageBox.Show("Legacy K2VR installation found!" + Environment.NewLine +
                    "Do you wish to uninstall it? (recommended)", "Legacy K2VR found", MessageBoxButton.YesNo) == MessageBoxResult.Yes &&
                    MessageBox.Show("Uninstalling K2VR legacy will delete the folder \"" + path + "\" and all its contents",
                    "Confirm uninstall", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    installPage.Log("Deleting...", false);
                    Directory.Delete(path, true);

                    installPage.Log("Removing start menu shortcuts...", false);
                    string startMenuFolder = Path.Combine("C:\\", "ProgramData", "Microsoft", "Windows", "Start Menu", "Programs", "KinectToVR");
                    if (Directory.Exists(startMenuFolder))
                    {
                        File.Delete(Path.Combine(startMenuFolder, "KinectToVR (Xbox 360).lnk"));
                        File.Delete(Path.Combine(startMenuFolder, "KinectToVR (Xbox One).lnk"));
                        try { Directory.Delete(startMenuFolder, false); } catch (IOException) { }
                    }
                    installPage.Log("Done!");
                }
                else
                {
                    installPage.Log("Keeping!");
                }
            }
            else
            {
                installPage.Log("Not found!");
            }
            string ovrieFolder = Path.Combine("C:\\", "Program Files", "OpenVR-InputEmulator");
            if (File.Exists(Path.Combine(ovrieFolder, "Uninstall.exe")))
            {
                if (MessageBox.Show("OpenVR Input Emulator found!" + Environment.NewLine +
                "Do you wish to uninstall it? (recommended)", "Uninstall OpenVR Input Emulator?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    installPage.Log("Uninstalling OpenVR Input Emulator...", false);
                    // copy installer to temp
                    File.Copy(Path.Combine(ovrieFolder, "Uninstall.exe"), Path.Combine(App.downloadDirectory, "Uninstall.exe"), true);
                    Process.Start(Path.Combine(App.downloadDirectory, "Uninstall.exe"), "_?=" + ovrieFolder + "\\").WaitForExit();
                    installPage.Log("Done!");
                }
            }
        }
        private static string GetBasePath(string path, string end)
        {
            if (path.EndsWith(end))
            {
                return path.Substring(0, path.Length - end.Length - 1); // -1 because of the \ at the end
            }
            return null;
        }

        public static bool UninstallAllK2EX(Pages.Install installPage)
        {
            var paths = FindK2EX();
            foreach (KeyValuePair<string, K2EXInstallProperties> pair in paths)
            {
                string path = pair.Key;
                if (path == App.state.GetFullInstallationPath()) continue;
                installPage.Log("Found old installation at \"" + path + "\"...", false);
                if (pair.Value.DriverRegistered || pair.Value.AppConfigRegistered)
                {
                    if (MessageBox.Show("K2EX appears to also be installed in " + "\"" + path + "\"" + ". Do you wish to disable it?", "Already installed", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        installPage.Log("Disabling...", false);
                        UnregisterK2EX(path);
                    }
                    else
                    {
                        MessageBox.Show("Multiple installations of K2EX would interfere with each other." + Environment.NewLine +
                            "Restart this installer to try again" + Environment.NewLine +
                            "Please join our Discord server for further assistance (link on www.k2vr.tech)");
                        installPage.Cancel();
                        return false;
                    }
                }
                DeleteK2EXFolder(path);
            }
            if (Directory.Exists(App.state.copiedDriverPath))
            {
                installPage.Log("Deleting copied driver...", false);
                var ovrDriverFiles = OpenVrDriverFiles.Read();
                foreach (string file in ovrDriverFiles.Files)
                {
                    string delFile = Path.Combine(App.state.copiedDriverPath, file);
                    if (File.Exists(delFile)) File.Delete(delFile);
                }
                for (int i = ovrDriverFiles.Folders.Count - 1; i >= 0; i--)
                {
                    string delDir = Path.Combine(App.state.copiedDriverPath, ovrDriverFiles.Folders[i]);
                    if (Directory.Exists(delDir)) Directory.Delete(delDir, false);
                }
                try { Directory.Delete(App.state.copiedDriverPath, false); } catch (IOException) { }
                installPage.Log("Deleted...", false);
            }
            if (Directory.Exists(App.state.GetFullInstallationPath()))
            {
                installPage.Log("Removing previous version...", false);
                if (MessageBox.Show("KinectToVR appears to be already installed in \"" +App.state.GetFullInstallationPath() + "\"." + Environment.NewLine +
                                    "Do you wish to update this installation?", "Confirm update", MessageBoxButton.YesNo) != MessageBoxResult.Yes ||
                    !DeleteK2EXFolder(App.state.GetFullInstallationPath()))
                {
                    // We need to abort because we can't unzip into a dir with files in it
                    MessageBox.Show("Cannot install two versions of KinectToVR into the same directory." + Environment.NewLine +
                        "Either uninstall the old version or select another install directory." + Environment.NewLine +
                        "Restart this installer to try again");
                    installPage.Cancel();
                    return false;
                }
            }
            if (Directory.Exists(App.startMenuFolder))
            {
                installPage.Log("Removing start menu shortcuts...", false);
                DeleteK2EXStartMenuShortcuts();
            }
            return true;
        }

        public static void DeleteK2EXStartMenuShortcuts()
        {
            if (Directory.Exists(App.startMenuFolder))
            {
                File.Delete(Path.Combine(App.startMenuFolder, "KinectToVR.lnk"));
                File.Delete(Path.Combine(App.startMenuFolder, "K2EX (Xbox 360).lnk"));
                File.Delete(Path.Combine(App.startMenuFolder, "K2EX (Xbox One).lnk"));
                try { Directory.Delete(App.startMenuFolder, false); } catch (IOException) { }
            }
        }

        public static void UnregisterK2EX(string path)
        {
            string driverPath = path + @"\KinectToVR";
            Process.Start(App.state.vrPathReg, "removedriver \"" + driverPath + "\"").WaitForExit();

            var openVrPaths = OpenVrPaths.Read();
            if (openVrPaths.external_drivers.Contains(driverPath))
            {
                openVrPaths.external_drivers.Remove(driverPath);
                openVrPaths.Write();
            }
            var appConfig = AppConfig.Read();
            appConfig.manifest_paths.Remove(path + @"\KinectV1Process.vrmanifest");
            appConfig.manifest_paths.Remove(path + @"\KinectV2Process.vrmanifest");
            appConfig.Write();
        }

        public static bool DeleteK2EXFolder(string path, bool uninstallFromSettings = false)
        {
            if (Directory.Exists(path))
            {
                if ((path == App.state.GetFullInstallationPath() && App.state.installedFiles.Count > 0) || uninstallFromSettings)
                {
                    foreach (string file in App.state.installedFiles)
                    {
                        if (File.Exists(file)) File.Delete(file);
                    }
                    for (int i = App.state.installedFolders.Count - 1; i >= 0; i--)
                    {
                        string dir = App.state.installedFolders[i];
                        if (Directory.Exists(dir)) Directory.Delete(dir, false);
                    }
                    if (uninstallFromSettings)
                    {
                        File.Delete(Path.Combine(path, InstallerState.fileName));
                        string confSettingsPath = Path.Combine(path, "ConfigSettings.cfg");
                        if (File.Exists(confSettingsPath) && MessageBox.Show("Do you wish to delete your calibration settings as well?" + Environment.NewLine +
                            "Note: To reuse them, you will need to install KinectToVR in the directory \"" + path + "\" again!",
                            "Delete calibration settings?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            File.Delete(confSettingsPath);
                        }
                        string[] files = Directory.GetFiles(path);
                        if (files.Length == 1 && files[0] == Path.Combine(path, "k2vr-installer-gui.exe"))
                        {
                            return true;
                        }
                        else
                        {
                            MessageBox.Show("Directory \"" + path + "\" contained non-standard files." + Environment.NewLine +
                                            "Clicking OK will open the file explorer to allow you to investigate.");
                            Process.Start("explorer.exe", path);
                            return false;
                        }
                    }
                    else
                    {
                        App.state.installedFiles.Clear();
                        App.state.installedFolders.Clear();
                        App.state.Write();
                    }
                    return true;
                }
                else
                {
                    try
                    {
                        Directory.Delete(path, false);
                        return true;
                    }
                    catch (IOException e)
                    {
                        if (e.HResult == -2147024751) // Directory not empty
                        {
                            if (MessageBox.Show("Do you wish to permanently delete the (old) installation directory \"" +
                                path + "\" and all its contents?", "Remove old installation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            {
                                Directory.Delete(path, true);
                                return true;
                            }
                        }
                        return false;
                    }
                }
            }
            return true;
        }

        public static Dictionary<string, K2EXInstallProperties> FindK2EX()
        {
            var paths = new Dictionary<string, K2EXInstallProperties>();

            var openVrPaths = OpenVrPaths.Read();
            foreach (string path in openVrPaths.external_drivers)
            {
                if (GetBasePath(path, "KinectToVR") != null)
                {
                    paths[GetBasePath(path, "KinectToVR")] = new K2EXInstallProperties() { DriverRegistered = true };
                }
            }

            var appConfig = AppConfig.Read();
            foreach (string path in appConfig.manifest_paths)
            {
                foreach (string manifest in new string[] { "KinectV1Process.vrmanifest", "KinectV2Process.vrmanifest" })
                {
                    string basePath = GetBasePath(path, manifest);
                    if (basePath != null)
                    {
                        if (!paths.ContainsKey(basePath)) paths[basePath] = new K2EXInstallProperties();
                        paths[basePath].AppConfigRegistered = true;
                    }
                }
            }

            if (Directory.Exists(k2exDefaultPath))
            {
                if (!paths.ContainsKey(k2exDefaultPath)) paths[k2exDefaultPath] = new K2EXInstallProperties();
            }
            return paths;
        }

        public static bool UninstallK2EX(string path)
        {
            UnregisterK2EX(path);
            bool retVal = DeleteK2EXFolder(path, true);
            DeleteK2EXStartMenuShortcuts();
            UnregisterUninstaller();
            return retVal;
        }

        public static void UnregisterUninstaller()
        {
            using (RegistryKey parent = Registry.LocalMachine.OpenSubKey("SOFTWARE", true))
            {
                parent.DeleteSubKey(App.installedPathRegKeyName);
            }
            string uninstallRegKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            using (RegistryKey parent = Registry.LocalMachine.OpenSubKey(uninstallRegKeyPath, true))
            {
                if (parent == null) return;
                string guidText = uninstallGuid.ToString("B").ToUpper();
                parent.DeleteSubKey(guidText);
            }
        }

        public static void RegisterUninstaller()
        {
            using (RegistryKey parent = Registry.LocalMachine.OpenSubKey("SOFTWARE", true))
            {
                RegistryKey key = parent.OpenSubKey(App.installedPathRegKeyName, true) ?? parent.CreateSubKey(App.installedPathRegKeyName);
                key.SetValue("InstallPath", App.state.GetFullInstallationPath());
            }

            string uninstallRegKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            using (RegistryKey parent = Registry.LocalMachine.OpenSubKey(uninstallRegKeyPath, true))
            {
                if (parent == null)
                {
                    App.Log("Uninstall registry key not found.");
                    return;
                }
                try
                {
                    RegistryKey key = null;

                    try
                    {
                        string guidText = uninstallGuid.ToString("B").ToUpper();
                        key = parent.OpenSubKey(guidText, true) ??
                              parent.CreateSubKey(guidText);

                        if (key == null)
                        {
                            App.Log(string.Format("Unable to create uninstaller '{0}\\{1}'", uninstallRegKeyPath, guidText));
                            return;
                        }

                        Assembly asm = Assembly.GetExecutingAssembly();

                        key.SetValue("DisplayName", "KinectToVR");
                        key.SetValue("ApplicationVersion", FileDownloader.files["k2vr"].Version);
                        key.SetValue("Publisher", asm.GetCustomAttribute<AssemblyCompanyAttribute>().Company);
                        key.SetValue("DisplayIcon", Path.Combine(App.state.GetFullInstallationPath(), "k2vr.ico"));
                        key.SetValue("DisplayVersion", FileDownloader.files["k2vr"].Version);
                        key.SetValue("URLInfoAbout", "https://k2vr.tech");
                        key.SetValue("Contact", "https://k2vr.tech");
                        key.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"));
                        key.SetValue("UninstallString", Path.Combine(App.state.GetFullInstallationPath(), "k2vr-installer-gui.exe") + " /uninstall");
                        key.SetValue("InstallLocation", App.state.GetFullInstallationPath());
                        // https://stackoverflow.com/a/1765801/ and https://stackoverflow.com/a/22111211
                        key.SetValue("EstimatedSize", new DirectoryInfo(App.state.GetFullInstallationPath()).EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => file.Length) / 1024, RegistryValueKind.DWord);
                    }
                    finally
                    {
                        if (key != null)
                        {
                            key.Close();
                        }
                    }
                }
                catch (Exception)
                {
                    App.Log("An error occurred writing uninstall information to the registry.  The service is fully installed but can only be uninstalled manually through the command line.");
                }
            }
        }
    }
}
