using k2vr_installer_gui.Pages;
using k2vr_installer_gui.Tools;
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

namespace k2vr_installer_gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        public void GoToTab(int index)
        {
            this.TabControl_tabs.SelectedIndex = index;
        }

        private void TabControl_tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                ((IInstallerPage)((TabItem)this.TabControl_tabs.SelectedItem).Content).OnSelected();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (TabControl_tabs.SelectedIndex == 3) // we can't cancel on the install tab or it might be left in a broken state
            {
                e.Cancel = true;
            } else if (TabControl_tabs.SelectedIndex == 2 && // Download page
                MessageBox.Show("Are you sure you want to cancel the download(s)?", "Confirm exit", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }
    }
}
