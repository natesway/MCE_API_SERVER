using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

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