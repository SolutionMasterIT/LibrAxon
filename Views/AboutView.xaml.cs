using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace AxionControl.Views
{
    public partial class AboutView : UserControl
    {
        public AboutView()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });

            e.Handled = true;
        }
		
		private void OpenYoutube_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://www.youtube.com/@SolutionMasterIT");
        }

        private void OpenVillamGyorsPc_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://villamgyorspc.hu");
        }

        private void OpenUrl(string url)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
    }
}
