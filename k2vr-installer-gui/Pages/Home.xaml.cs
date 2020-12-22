using System;
using System.Collections.Generic;
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
using System.Globalization;
using k2vr_installer_gui.Pages.Popups;

namespace k2vr_installer_gui.Pages
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : UserControl, IInstallerPage
    {
        public Home()
        {
            InitializeComponent();
        }

        private void BeginButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).GoToTab(1);
        }

        public void OnSelected()
        {
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            new OpenSourceLicenses().ShowDialog();
        }

        private void LangButton_Click(object sender, RoutedEventArgs e)
        {
            //System.Threading.Thread.CurrentThread.CurrentUICulture =
            //new System.Globalization.CultureInfo("ja-JP");
            //MainWindow newWindow = new MainWindow();
            //Application.Current.MainWindow = newWindow;
            //newWindow.Show();
            //App.Current.Windows[0].Close();
            new LanguageDialog().ShowDialog();
        }

        private void Close()
        {
        }
    }
}
