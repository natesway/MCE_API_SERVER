using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MCE_API_SERVER.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ServerPage : ContentPage
    {
        private StackLayout Logger;

        public ServerPage()
        {
            InitializeComponent();
            Logger = (StackLayout)FindByName("Console");
            Device.StartTimer(new TimeSpan(0, 0, 1), () =>
            {
                while (Log.ToLog.Count > 0) {
                    Log.LogMessage message = Log.ToLog.Dequeue();
                    Label l = new Label();
                    l.Text = $"[{Enum.GetName(typeof(Log.LogType), message.Type)}] {message.Content}";
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
                    Logger.Children.Add(l);
                }
                return true;
            });
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            if (Server.Running)
                Server.Stop();
            else
                Server.Start();

            Button b = (Button)sender;
            b.Text = Server.Running ? "Stop" : "Start";
            b.BackgroundColor = Server.Running ? Color.Red : Color.Green;
        }
    }
}