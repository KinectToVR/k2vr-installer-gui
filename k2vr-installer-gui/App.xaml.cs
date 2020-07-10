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
        public string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
        public InstallerState settings;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            settings = (new InstallerState()).Read();
            settings.Update();
        }
    }
}
