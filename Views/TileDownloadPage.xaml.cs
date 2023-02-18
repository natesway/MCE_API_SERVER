using Mapsui;
using Mapsui.Layers;
using Mapsui.Projection;
using Mapsui.UI.Forms;
using Mapsui.Utilities;
using Mapsui.Widgets;
using Mapsui.Widgets.ScaleBar;
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
    public partial class TileDownloadPage : ContentPage
    {
        MapView mapView;

        private Func<Mapsui.Geometries.Point, Mapsui.Geometries.Point, Task> OnExit;

        public TileDownloadPage(Func<Mapsui.Geometries.Point, Mapsui.Geometries.Point, Task> onExit)
        {
            InitializeComponent();

            OnExit = onExit;

            mapView = new MapView()
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Color.Gray,
            };

            Mapsui.Map map = new Mapsui.Map()
            {
                CRS = "EPSG:3857",
                Transformation = new MinimalTransformation(),
                RotationLock = true,
            };

            TileLayer tileLayer = OpenStreetMap.CreateTileLayer();

            map.Layers.Add(tileLayer);
            map.Widgets.Add(new ScaleBarWidget(map) 
            {
                TextAlignment = Alignment.Center, 
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom 
            });

            mapView.Map = map;

            layout.Children.Insert(0, mapView);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            SetMapHeight();
        }

        private async Task SetMapHeight()
        {
            start:
            await Task.Delay(150); // need to wait for width to update
            if (layout.Width < 0d)
                goto start;
            layout.HeightRequest = layout.Width;
        }

        private void Btn_Ok_Clicked(object sender, EventArgs e)
        {
            Mapsui.Geometries.Point _p1 = mapView.Viewport.ScreenToWorld(0d, 0d);
            Mapsui.Geometries.Point p1 = SphericalMercator.ToLonLat(_p1.X, _p1.Y);
            Mapsui.Geometries.Point _p2 = mapView.Viewport.ScreenToWorld(mapView.Viewport.Width, mapView.Viewport.Height);
            Mapsui.Geometries.Point p2 = SphericalMercator.ToLonLat(_p2.X, _p2.Y);
            OnExit(p1, p2);
        }
    }
}