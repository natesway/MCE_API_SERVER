using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MCE_API_SERVER.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ServerPage : ContentPage
    {
        public ServerPage()
        {
            InitializeComponent();
        }

        bool notifAllowBackgroundDone;

        private void Btn_StartStop_Clicked(object sender, EventArgs e)
        {
            Thread t = new Thread(() =>
            {
                try {
                    // needs to run on seperate thread, otherwise hangs and crasches
                    if (!Util.FileExists("askedBackgroudLimit")) {
                        notifAllowBackgroundDone = false;
                        // needs to run on ui thread
                        Device.BeginInvokeOnMainThread(() => AskTurnOnBackgroundUnrestricted());
                        while (notifAllowBackgroundDone == false) Thread.Sleep(1);
                        AppInfo.ShowSettingsUI();
                        Util.SaveFile("askedBackgroudLimit", new byte[0]);
                    }

                    // this can be run on UI thread
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        try {
                            bool succeeded = true;
                            if (Server.Running)
                                Server.Stop();
                            else
                                succeeded = Server.Start();

                            if (succeeded) {
                                Button b = (Button)sender;
                                b.Text = Server.Running ? "Stop" : "Start";
                                b.BackgroundColor = Server.Running ? Color.Red : Color.Green;
                            }
                            else {
                                AskDownloadResourcePack();
                            }
                        } catch (Exception ex) {
                            Log.Error("Failed to start/stop server");
                            Log.Exception(ex);
                        }
                    });
                }
                catch (Exception ex) {
                    Log.Error("Failed to start/stop server");
                    Log.Exception(ex);
                }
            });
            t.Start();
        }

        private async Task AskTurnOnBackgroundUnrestricted()
        {
            await DisplayAlert("Allow background activity", "App info will be open, go to \"Battery usage\", run on \"Allow background activity\"", "Ok");
            notifAllowBackgroundDone = true;
        }
        
        private async Task AskDownloadResourcePack()
        {
            bool download = await DisplayAlert("Resource pack wan't found", $"File {Util.SavePath_Server}resourcepacks/vanilla.zip doesn't exist. Download it, rename to vanilla.zip",
                    "Download", "Cancel");
            if (download) {
                Util.OpenBrowser(new Uri("https://web.archive.org/web/20210624200250/https://cdn.mceserv.net/availableresourcepack/resourcepacks/dba38e59-091a-4826-b76a-a08d7de5a9e2-1301b0c257a311678123b9e7325d0d6c61db3c35"));
            }
        }
    }
}