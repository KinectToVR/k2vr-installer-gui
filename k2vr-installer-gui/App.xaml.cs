using k2vr_installer_gui.Tools;
using System;
using System.Windows;

namespace k2vr_installer_gui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
        public static readonly string downloadDirectory = exeDirectory + @"k2vr-installer\";
        public static readonly InstallerState state = InstallerState.Read();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            state.Update();
        }
    }
}
