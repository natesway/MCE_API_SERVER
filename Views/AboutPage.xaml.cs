using Xamarin.Forms;

namespace MCE_API_SERVER.Views
{
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();

            VersionLabel.Text = $"Version: {Server.AppVersion}";
        }
    }
}