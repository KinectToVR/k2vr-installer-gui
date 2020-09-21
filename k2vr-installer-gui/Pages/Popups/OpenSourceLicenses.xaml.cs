using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace k2vr_installer_gui.Pages
{
    /// <summary>
    /// Interaction logic for OpenSourceLicenses.xaml
    /// </summary>
    public partial class OpenSourceLicenses : Window
    {
        public OpenSourceLicenses()
        {
            InitializeComponent();
            TabControl_OpenSourceLicenses.DataContext = new VersionContext();
        }
    }

    internal class VersionContext
    {
        public string Version { get; set; }

        public VersionContext()
        {
            Version = "KinectToVR Installer " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
