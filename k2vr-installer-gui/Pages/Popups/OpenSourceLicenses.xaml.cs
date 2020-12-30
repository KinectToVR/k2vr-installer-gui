using k2vr_installer_gui.Tools;
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
            string ver = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Version = "Installer Version " + ver.Remove(ver.Length - 2) +
                " for K2EX Version " + FileDownloader.files["k2vr"].Version;
        }
    }
}
