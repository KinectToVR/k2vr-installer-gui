using k2vr_installer_gui.Tools;
using k2vr_installer_gui.Uninstall;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace k2vr_installer_gui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly string startMenuFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "K2EX");
        public static readonly string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
        public static readonly string downloadDirectory = exeDirectory + @"k2vr-installer\";
        public const string installedPathRegKeyName = "KinectToVR";
        public static InstallerState state;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            string installPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\" + installedPathRegKeyName, "InstallPath", "") ?? "";

            state = InstallerState.Read(installPath);
            state.Update();
            if (e.Args.Length > 0 && e.Args[0] == "/uninstall")
            {
                if (MessageBox.Show("Are you sure you want to uninstall KinectToVR?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    if (Uninstaller.UninstallK2EX(installPath))
                    {
                        MessageBox.Show("Uninstalled successfully!");
                        if (Directory.Exists(installPath))
                        {
                            // https://stackoverflow.com/a/1305478/
                            ProcessStartInfo Info = new ProcessStartInfo();
                            Info.Arguments = "/C choice /C Y /N /D Y /T 3 & rmdir /S /Q \"" + installPath + "\"";
                            Info.WindowStyle = ProcessWindowStyle.Hidden;
                            Info.CreateNoWindow = true;
                            Info.FileName = "cmd.exe";
                            Process.Start(Info);
                        }
                    }
                }
                Current.Shutdown(0);
            }
        }

        public static void Log(string text)
        {
            File.AppendAllText(Path.Combine(App.downloadDirectory, "install.log"), text + Environment.NewLine);
        }
    }
}
