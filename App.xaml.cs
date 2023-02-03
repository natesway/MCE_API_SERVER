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
            Log.Init();
            Log.Information("Started app");
        }

        protected override void OnSleep()
        {
            Log.Debug("Going to sleep");
            Log.Dispose();
        }

        protected override void OnResume()
        {
            Log.Init();
            Log.Debug("Resumed from sleep");
        }
    }
}
