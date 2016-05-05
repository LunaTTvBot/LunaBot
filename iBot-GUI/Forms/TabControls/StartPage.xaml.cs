using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace iBot_GUI.Forms.TabControls
{
    /// <summary>
    ///     Interaktionslogik für StartPage.xaml
    /// </summary>
    public partial class StartPage : UserControl
    {
        public StartPage()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}