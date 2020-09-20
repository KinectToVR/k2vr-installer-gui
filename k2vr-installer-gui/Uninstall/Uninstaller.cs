using k2vr_installer_gui.Tools.OpenVRFiles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
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
                        try { Directory.Delete(startMenuFolder, false); } catch (IOException e) { }
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
                if (path == App.state.installationPath) continue;
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
            if (Directory.Exists(App.state.installationPath))
            {
                installPage.Log("Removing previous version...", false);
                if (!DeleteK2EXFolder(App.state.installationPath))
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

        public static bool DeleteK2EXFolder(string path)
        {
            if (Directory.Exists(path))
            {
                if (path == App.state.installationPath && App.state.installedFiles.Count > 0)
                {
                    foreach (string file in App.state.installedFiles)
                    {
                        File.Delete(file);
                    }
                    for (int i = App.state.installedFolders.Count - 1; i >= 0; i--)
                    {
                        Directory.Delete(App.state.installedFolders[i], false);
                    }
                    App.state.installedFiles.Clear();
                    App.state.installedFolders.Clear();
                    App.state.Write();
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
            return DeleteK2EXFolder(path); ;
        }
    }
}
