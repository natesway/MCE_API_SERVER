using System;
using Xamarin.Forms;

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
            AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) =>
            {
                Log.Error("Unhandled Exception was thrown");
                if (e.ExceptionObject != null)
                    Log.Exception(e.ExceptionObject as Exception);
            };

            Settings.Init();
            Log.Init(true);
            Log.Information("Started app");
        }

        protected override void OnSleep()
        {
            Log.Debug("Going to sleep");
            Log.Dispose();
        }

        protected override void OnResume()
        {
            Log.Init(false);
            Log.Debug("Resumed from sleep");
        }
    }
}
