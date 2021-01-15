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
            string[] splashes = { "🎵 I'M HAN SOLO! I'M HAN SOLO! I'M HAN SOLO SOLO! 🎵", "Welcome to the bone zone.", "Unlike Facebook, we don't track your life. Rather, your legs.", "Cross your fingers!", "Ruining your ERP sessions since 2018!", "Connect your Kinect to start the connection.", "Rehoming abandoned Kinects", "Auto-bribing soccer moms on Facebook Marketplace...", "Developers were harmed in the creation of this software...", "Did you know, NUI means Natural User Interface and was coined by Steve Mann.", "LOOK AT MY LEGS!!! LOOK!!!", "LOOK MORTY! I'M KINECT RIIIICK!", "Foot do a spinny", "Haha Kinect go SPEEN!", "The Kinect V2 hates you, and you'll just have to accept that", "Bandwidth? You won't be needing that where we're going!", "The K2EX Enrichment Center would like to remind you, that the Kinect, cannot speak... Yet.", "Laptops beware, you're in for a scare", "ASMedia.", "Renesas Renaissance", "Legs and hips ONLY!", "Only available at Walmart!", "not Kinect2VR, not Kinect4VR, not Kinect 2 VR, not kinnect2vr and certainly not Driver4VR.", "no. it does not work on oculus games", "Its Kinect! Not connect!", "Now works on Commodore 64!", "Trans Rights!", "The user is always at fault!", "No keyboard found, press any key to continue!", "[insert clever splash text here]", "Imagine using a 2010 kids' toy a decade later for VR.", "Fortunately, we have a product for people who aren't able to get some form of connectivity. It's called Xbox 360.", "John Madden!", "TV, TV, TV, TV, sports, sports, Call of Duty", "exclamation point, question mark, exclamation point, question mark!", "C'mon Cranky, take it to the fridge!", "The kinect was first invented in 1895 when Nikola tesla started throwing lightbulbs at horses to track their position", "Discounted from it's original price of $19,97", "Hi! I'm Scott from Domino's Pizza. Have you heard about Hatsune Miku?", "After 9 years in development, hopefully it'll have been worth the wait.", "But the human eye can only see 30fps!1", "Now in HD!", "Number 15: Burger King Foot Rotation...", "Due to the limitations of our software, we are unable to provide support to fourth-dimensional entities.", "Try not to release the magic smoke.", "*xbox noises*", "Xbox One X Pro Max Mini Ultra Edge Active Cosmos Product Red with Deluxe Audio Strap.", "Arguing over mechanical keyboards...", "Needs more purple.", "Plotting the downfall of the Western Roman Empire...", "feet", "legs", "Made possible with the power of caffeine.", "Have a great day!", "Procedurally Generated!", "Intentionally making spelling mistakes...", "Do a 360!", "Gaining Sentience...", "Kinecting...", "Giving people feet, one step at a time.", "Hello, World!", "Azure Kinect is magic.", "It's Alive!", "Debugging and Rebugging.", "Ever wonder what the bottom of an avatar's shoe looks like?", "Mom said it's my turn on the Xbox", "Talk to us on Discord!", "Gameing", "Searching for spare USB ports...", "Check out the 360 Kinect through an IR camera, it's pretty neat!", "Insert Funny Text Here.", "Woooo! Yeah baby, that's what I've been waiting for.", "Ollie stole Tyler McVicker's free Vive!", "Don't knock the fan method!", "That's never happened before.", "Just give us Wii Sports, but with Kinect." };
            Random rand = new Random();
            int index = rand.Next(splashes.Length);
            splashtext.Text = $"\"{splashes[index]}\"";
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
