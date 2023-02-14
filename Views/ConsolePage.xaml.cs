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
    public partial class ConsolePage : ContentPage
    {
        public ConsolePage()
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

        private void Btn_Clear_Clicked(object sender, EventArgs e)
        {
            Console.Children.Clear();
            Log.Information("Cleared console");
        }
    }
}