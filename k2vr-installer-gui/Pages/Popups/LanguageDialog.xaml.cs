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
    /// Interaction logic for LanguageDialog.xaml
    /// </summary>
    public partial class LanguageDialog : Window
    {
        public LanguageDialog()
        {
            InitializeComponent();
            this.Title = Properties.Resources.home_language_button;
            
        }
    }
}
