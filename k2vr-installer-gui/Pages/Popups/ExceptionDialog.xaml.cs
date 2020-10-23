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
using System.Windows.Shapes;

namespace k2vr_installer_gui.Pages.Popups
{
    /// <summary>
    /// Interaction logic for ExceptionDialog.xaml
    /// </summary>
    public partial class ExceptionDialog : Window
    {
        private Exception exception;

        public ExceptionDialog(Exception exception, bool skippable = false)
        {
            InitializeComponent();
            this.exception = exception;
            TextBox_exception.Text = exception.ToString();
            if (skippable)
            {
                Button_Skip.Visibility = Visibility.Visible;
            }
        }

        private void Button_Copy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(exception.ToString());
        }

        private void Button_Discord_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://discord.gg/Mu28W4N");
        }

        private void Button_Exit_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Button_Skip_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
