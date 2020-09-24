using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    /// Interaction logic for Finish.xaml
    /// </summary>
    public partial class Finish : UserControl, IInstallerPage
    {
        public Finish()
        {
            InitializeComponent();
        }

        public void OnSelected()
        {
        }

        private void Button_Visit_website_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://k2vr.tech/");
        }

        private void Button_Visit_discord_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://discord.gg/YBQCRDG");
        }

        private void Button_Close_Installer_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
