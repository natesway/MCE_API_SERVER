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
            Device.StartTimer(new TimeSpan(0, 0, 1), () =>
            {
                while (Log.ToLog.Count > 0) {
                    Log.LogMessage message = Log.ToLog.Dequeue();

                    string text = "";
                    if (Settings.LogMesType)
                        text += $"[{Enum.GetName(typeof(Log.LogType), message.Type)}] ";
                    if (Settings.LogMesTime)
                        text += $"[{message.Time.Hour}:{message.Time.Minute}:{message.Time.Second}] ";
                    text += message.Content;

                    Label l = new Label();
                    l.Text = text;
                    l.TextColor = Log.TypeToColor[message.Type];
                    l.FontSize = 12d;
                    l.VerticalOptions = LayoutOptions.StartAndExpand;
                    l.HorizontalOptions = LayoutOptions.Start;
                    l.GestureRecognizers.Add(new TapGestureRecognizer()
                    {
                        Command = new Command(() =>
                        {
                            Clipboard.SetTextAsync(l.Text).Wait();
                        })
                    });

                    // Console is stack layout
                    Console.Children.Add(l);
                }

                if (Console.Children.Count > Settings.MaxMessagesInConsole)
                    while (Console.Children.Count > Settings.MaxMessagesInConsole)
                        Console.Children.RemoveAt(0);

                return true;
            });
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

        private void Btn_Clear_Clicked(object sender, EventArgs e)
        {
            Console.Children.Clear();
            Log.Information("Cleared console");
        }
    }
}