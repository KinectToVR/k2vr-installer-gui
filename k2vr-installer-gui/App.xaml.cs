using k2vr_installer_gui.Pages.Popups;
using k2vr_installer_gui.Tools;
using k2vr_installer_gui.Uninstall;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Globalization;

namespace k2vr_installer_gui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly string startMenuFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "K2EX");
        public static readonly string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
        public static readonly string downloadDirectory = exeDirectory + @"k2vr-installer\";
        public const string installedPathRegKeyName = "KinectToVR";
        public static InstallerState state;
        public static bool isUninstall = false;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length > 0 && e.Args[0] == "/uninstall") isUninstall = true;

            string installPath = (string)Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\" + installedPathRegKeyName, "InstallPath", "") ?? "";

            // get system display language
            string displayLanguage = CultureInfo.InstalledUICulture.TwoLetterISOLanguageName;
            // import strings.json from resources and parse it into a JObject


            state = InstallerState.Read(installPath);
            state.Update();
            if (isUninstall)
            {
                if (MessageBox.Show("Are you sure you want to uninstall KinectToVR?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try {
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
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error happened while trying to uninstall. Join our Discord for help on manually uninstalling.");
                        new ExceptionDialog(ex).ShowDialog();
                    }
                }
                Current.Shutdown(0);
            }
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            new ExceptionDialog(e.Exception).ShowDialog();
            Current.Shutdown(1);
        }
    }
}
