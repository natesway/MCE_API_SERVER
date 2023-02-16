using System;
using System.ComponentModel;
using System.Net;
using System.Threading;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MCE_API_SERVER.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DownloadPage : ContentPage
    {
        public static int lastDownloadStatus;

        string downloadUrl;
        string savePath;
        bool completed;
        bool stopDownload;

        Button btnCancel;

        public DownloadPage(string _downloadUrl, string _savePath, string downloadName, bool canBecanceled)
        {
            InitializeComponent();

            downloadUrl = _downloadUrl;
            savePath = _savePath;
            completed = false;
            stopDownload = false;

            label.Text = $"Downloading {downloadName}...";

            if (canBecanceled) {
                btnCancel = new Button()
                {
                    Text = "Cancel",
                };
                layout.Children.Add(btnCancel);
            }

            lastDownloadStatus = 0;
            Thread t = new Thread(() =>
            {
                try {
                    Download();
                }
                catch (Exception ex) {
                    Log.Error($"Failed to download file {downloadUrl}");
                    Log.Exception(ex);
                    Device.BeginInvokeOnMainThread(() => Navigation.PopAsync());
                }
            });
            t.Start();
        }

        protected override void OnDisappearing()
        {
            stopDownload = true;
            if (lastDownloadStatus == 0)
                lastDownloadStatus = 1;
            base.OnDisappearing();
        }

        protected override bool OnBackButtonPressed()
        {
            stopDownload = true;
            if (lastDownloadStatus == 0)
                lastDownloadStatus = 1;
            return base.OnBackButtonPressed();
        }

        private void Download()
        {
            bool canceled = false;

            using (WebClient client = new WebClient()) {
                client.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) =>
                {
                    Device.BeginInvokeOnMainThread(() => progressBar.Progress = (double)e.BytesReceived / (double)e.TotalBytesToReceive);
                };
                client.DownloadFileCompleted += (object sender, AsyncCompletedEventArgs e) =>
                {
                    canceled = e.Cancelled;
                    completed = true;
                };

                client.DownloadFileAsync(new Uri(downloadUrl), savePath);
                while (!completed) {
                    Thread.Sleep(0);
                    if (stopDownload) {
                        lastDownloadStatus = 1;
                        return;
                    }
                }
            }

            if (canceled)
                lastDownloadStatus = 1;
            else
                lastDownloadStatus = 2;

            if (btnCancel != null)
                layout.Children.Remove(btnCancel);

            Device.BeginInvokeOnMainThread(() => Navigation.PopAsync());
        }
    }
}