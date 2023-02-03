using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MCE_API_SERVER.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MesFilterPage : ContentPage
    {
        public static readonly string[] FilterNames = new string[]
        {
            "Debug",
            "Information",
            "Warning",
            "Error",
            "Exception"
        };

        private bool[] values;

        public MesFilterPage()
        {
            InitializeComponent();

            values = new bool[Settings.MesLogFilter.Length];

            // copy into temporary storage, if user doesn't tap "OK" Settings value won't be changed
            for (int i = 0; i < values.Length; i++)
                values[i] = Settings.MesLogFilter[i];

            for (int i = 0; i < FilterNames.Length; i++) {
                Grid g = new Grid();

                // add image showing color of this mes type
                string name = FilterNames[i];
                if (name == "Exception") // colors are same, would be same image
                    name = "Error";
                ImageSource imageSource = ImageSource.FromResource(Util.GetFullResourceName($"FilterColorImages/{name}.PNG"), Util.CurrentAssembly);
                Image image = new Image()
                {
                    Source = imageSource,
                    Aspect = Aspect.AspectFit,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Start
                };
                g.Children.Add(image);

                // add filter name
                Label l = new Label();
                l.Text = $"     {FilterNames[i]}";
                l.HorizontalOptions = LayoutOptions.Start;
                l.VerticalOptions = LayoutOptions.Center;
                g.Children.Add(l);

                // add checkbox
                CheckBox cb = new CheckBox();
                cb.IsChecked = values[i];
                cb.Color = Color.Black;
                int index = i; // i changes
                cb.CheckedChanged += (object sender, CheckedChangedEventArgs e) =>
                {
                    values[index] = e.Value;
                };
                cb.HorizontalOptions = LayoutOptions.End;
                cb.VerticalOptions = LayoutOptions.Center;
                g.Children.Add(cb);

                layout.Children.Add(g);
            }
        }

        private void OK_Button_Clicked(object sender, EventArgs e)
        {
            for (int i = 0; i < values.Length; i++)
                Settings.MesLogFilter[i] = values[i];

            Navigation.PopAsync();
        }
    }
}