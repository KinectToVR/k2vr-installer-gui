using k2vr_installer_gui.Tools;
using k2vr_installer_gui.Uninstall;
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
        public static InstallerState state;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length > 1 && e.Args[0] == "/uninstall")
            {
                string uninstallPath = e.Args[1];
                state = InstallerState.Read(Path.Combine(uninstallPath, InstallerState.fileName));
                state.Update();
                if (Uninstaller.UninstallK2EX(uninstallPath))
                {
                    MessageBox.Show("Uninstalled successfully!");
                    if (Directory.Exists(uninstallPath))
                    {
                        // https://stackoverflow.com/a/1305478/
                        ProcessStartInfo Info = new ProcessStartInfo();
                        Info.Arguments = "/C choice /C Y /N /D Y /T 3 & rmdir /S /Q \"" + uninstallPath + "\"";
                        Info.WindowStyle = ProcessWindowStyle.Hidden;
                        Info.CreateNoWindow = true;
                        Info.FileName = "cmd.exe";
                        Process.Start(Info);
                    }
                }
                Current.Shutdown(0);
            }
            else
            {
                state = InstallerState.Read();
                state.Update();
            }
        }

        public static void Log(string text)
        {
            File.AppendAllText(Path.Combine(App.downloadDirectory, "install.log"), text + Environment.NewLine);
        }
    }
}
