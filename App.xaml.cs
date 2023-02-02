using MCE_API_SERVER.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MCE_API_SERVER
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
