using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MCE_API_SERVER.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DownloadPage : ContentPage
    {
        public static int lastDownloadStatus;

        FileToDownload[] filesToDownload;
        bool completed;
        bool stopDownload;

        Button btnCancel;

        public DownloadPage(FileToDownload _fileToDownload, bool canBecanceled)
            : this(new FileToDownload[] { _fileToDownload }, canBecanceled)
        { }
        public DownloadPage(FileToDownload[] _filesToDownload, bool canBecanceled)
        {
            InitializeComponent();

            filesToDownload = _filesToDownload;
            completed = false;
            stopDownload = false;

            if (canBecanceled) {
                btnCancel = new Button()
                {
                    Text = "Cancel",
                };
                btnCancel.Clicked += Btn_Cancel_Clicked;
                layout.Children.Add(btnCancel);
            }

            lastDownloadStatus = 0;
            Thread t = new Thread(() =>
            {
                try {
                    if (filesToDownload.Length == 1)
                        DownloadOneFile();
                    else
                        Download();
                }
                catch (Exception ex) {
                    Log.Error($"Failed to download files");
                    Log.Exception(ex);
                    Device.BeginInvokeOnMainThread(() => Navigation.PopAsync());
                }
            });
            t.Start();
        }

        private void Btn_Cancel_Clicked(object sender, EventArgs e)
        {
            stopDownload = true;
            if (lastDownloadStatus == 0)
                lastDownloadStatus = 1;

            Device.BeginInvokeOnMainThread(() => Navigation.PopAsync());
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
            using (HttpClient client = new HttpClient()) {
                for (int i = 0; i < filesToDownload.Length; i++) {
                    FileToDownload ftd = filesToDownload[i];
                    int urlIndex = 0;
                    if (!Directory.Exists(Path.GetDirectoryName(ftd.SavePath)))
                        Directory.CreateDirectory(Path.GetDirectoryName(ftd.SavePath));
                    Device.BeginInvokeOnMainThread(() => label.Text = $"Downloading {ftd.DisplayName}...");

                    download:
                    try {
                        HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, ftd.DownloadUrls[urlIndex]);
                        req.Headers.UserAgent.Clear();
                        req.Headers.UserAgent.Add(new ProductInfoHeaderValue("Mce_Api_Server", "1.0"));
                        req.Headers.Remove("Cache-Control");
                        req.Headers.Remove("Pragma");

                        HttpResponseMessage resp = client.SendAsync(req).Result;
                        if (resp.StatusCode != HttpStatusCode.OK)
                            throw new Exception($"Status code isn't OK, but {resp.StatusCode}");
                        File.WriteAllBytes(ftd.SavePath, resp.Content.ReadAsByteArrayAsync().Result);
                        resp.Dispose();
                    } catch (Exception ex) {
                        urlIndex++;
                        if (urlIndex < ftd.DownloadUrls.Length)
                            goto download;
                        Log.Error($"Failed to download {ftd.SavePath}");
                        Log.Exception(ex);
                    }
                    Device.BeginInvokeOnMainThread(() => progressBar.Progress = (double)(i + 1) / (double)filesToDownload.Length);
                }
            }

            if (btnCancel != null)
                layout.Children.Remove(btnCancel);

            Device.BeginInvokeOnMainThread(() => Navigation.PopAsync());
        }

        private void DownloadOneFile()
        {
            bool canceled = false;

            label.Text = $"Downloading {filesToDownload[0].DisplayName}...";
            using (WebClient client = new WebClient()) {
                client.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) =>
                {
                    Device.BeginInvokeOnMainThread(() => progressBar.Progress = (double)e.BytesReceived / (double)e.TotalBytesToReceive);
                };
                client.DownloadFileCompleted += (object sender, AsyncCompletedEventArgs e) =>
                {
                    canceled = e.Cancelled;
                };

                int urlIndex = 0;
                if (!Directory.Exists(Path.GetDirectoryName(filesToDownload[0].SavePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(filesToDownload[0].SavePath));

                download:
                try {
                    client.DownloadFile(new Uri(filesToDownload[0].DownloadUrls[urlIndex]), filesToDownload[0].SavePath);
                }
                catch (Exception ex) {
                    urlIndex++;
                    if (urlIndex < filesToDownload[0].DownloadUrls.Length)
                        goto download;
                    Log.Error($"Failed to download {filesToDownload[0].SavePath}");
                    Log.Exception(ex);
                    canceled = true;
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

    public struct FileToDownload
    {
        public string[] DownloadUrls { get; }
        public string SavePath { get; }
        public string DisplayName { get; }

        public FileToDownload(string downloadUrl, string savePath, string displayName)
            : this(new string[] { downloadUrl }, savePath, displayName)
        { }
        public FileToDownload(string[] downloadUrls, string savePath, string displayName)
        {
            DownloadUrls = downloadUrls;
            SavePath = savePath;
            DisplayName = displayName;
        }
    }
}