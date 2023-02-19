using MCE_API_SERVER.Github;
using MCE_API_SERVER.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
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
        private static bool checkedApkAvailable = false;
        private static DateTime lastTileDownload;

        public ServerPage()
        {
            InitializeComponent();
            if (!checkedApkAvailable) {
                CheckDownloadAvailable();
                checkedApkAvailable = true;
            }
        }

        private void Btn_ClearLogs_Clicked(object sender, EventArgs e)
            => ClearLogs();

        private void Btn_DownloadTiles_Clicked(object sender, EventArgs e)
        {
            if (lastTileDownload == null || lastTileDownload.AddMinutes(10) < DateTime.Now)
                Navigation.PushAsync(new TileDownloadPage(DownloadTiles));
            else {
                TimeSpan span = lastTileDownload.AddMinutes(10) - DateTime.Now;
                DisplayAlert("Error", $"You need to wait {span.Minutes}:{span.Seconds}", "Ok");
            }
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
                        }
                        catch (Exception ex) {
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

        private async Task ClearLogs()
        {
            if (!Directory.Exists(Util.SavePath + Log.saveHistoryName)) {
                await DisplayAlert("Info", "You don't have any logs", "Ok");
                return;
            }

            string[] logs = Directory.GetFiles(Util.SavePath + Log.saveHistoryName);
            if (logs.Length == 0) {
                await DisplayAlert("Info", "You don't have any logs", "Ok");
                return;
            }
            else if (await DisplayAlert("Confirm", $"Do you want to delete {logs.Length} logs?", "Ok", "Cancel"))
                for (int i = 0; i < logs.Length; i++)
                    File.Delete(logs[i]);
        }

        private async Task AskTurnOnBackgroundUnrestricted()
        {
            await DisplayAlert("Allow background activity", "App info will be open, go to \"Battery usage\", run on \"Allow background activity\"", "Ok");
            notifAllowBackgroundDone = true;
        }

        private async Task AskDownloadResourcePack()
        {
            if (await DisplayAlert(
                "Resource pack wan't found", $"File {Util.SavePath_Server}resourcepacks/vanilla.zip doesn't exist. Download it, rename to vanilla.zip",
                    "Open download page", "Cancel"))
                Util.OpenBrowser(new Uri(
                    "https://web.archive.org/web/20210624200250/https://cdn.mceserv.net/availableresourcepack/resourcepacks/dba38e59-091a-4826-b76a-a08d7de5a9e2-1301b0c257a311678123b9e7325d0d6c61db3c35"));
        }

        private void CheckDownloadAvailable()
        {
            try {
                using (HttpClient client = new HttpClient()) {
                    HttpResponseMessage resp = client.GetAsync("https://api.github.com/repos/SuperMatejCZ/MCE_API_SERVER/releases/latest").Result;
                    if (resp.StatusCode != HttpStatusCode.OK) {
                        Log.Error($"Got not OK response when checking release: {resp.StatusCode}");
                        return;
                    }
                    ReleaseLatest release = Utf8Json.JsonSerializer.Deserialize<ReleaseLatest>(resp.Content.ReadAsStringAsync().Result);
                    resp.Dispose();

                    if (!release.tag_name.Contains('.')) {
                        Log.Error($"Release not in correct format ({release.tag_name}), this might be developer's fault");
                        return;
                    }

                    // prelease or alpha or beta (don't download automatically)
                    if (release.tag_name.Contains("pre") || release.tag_name.Contains("a") || release.tag_name.Contains("b")) {
                        Log.Information("No new versions detected");
                        return;
                    }

                    string[] tagSplit = release.tag_name.Split('.');
                    int majorVersion, minorVersion;
                    if (!int.TryParse(tagSplit[0], out majorVersion) || !int.TryParse(tagSplit[1], out minorVersion)) {
                        Log.Error($"Release tag not in correct format ({release.tag_name}), this might be developer's fault");
                        return;
                    }

                    // outdated version
                    if (majorVersion > Server.AppVersion_Major || (majorVersion == Server.AppVersion_Major && minorVersion > Server.AppVersion_Minor)) {
                        Log.Information($"New version of app available ({release.tag_name})");

                        // make sure release is valid
                        if (release.assets == null || release.assets.Length < 1) {
                            Log.Error("Release has no assets");
                            return;
                        }

                        Thread t = new Thread(() =>
                        {
                            try {
                                DownloadAPK(release);
                            }
                            catch (Exception ex) {
                                Log.Error("Failed to download update");
                                Log.Exception(ex);
                            }
                        });
                        t.Start();
                    }
                    else
                        Log.Information("No new versions detected");
                }
            }
            catch (Exception ex) {
                Log.Error("Couldn't get release info");
                Log.Exception(ex);
            }
        }

        int askedDownloadAPKStatus;
        private void DownloadAPK(ReleaseLatest release)
        {
            askedDownloadAPKStatus = 0;
            // needs to run on ui thread
            Device.BeginInvokeOnMainThread(() => AskDownloadApk(release.tag_name));
            while (askedDownloadAPKStatus == 0) Thread.Sleep(1);

            string downloadPath = Util.SavePath + "update.apk";

            // user wants to update
            if (askedDownloadAPKStatus == 2) {
                DownloadPage.lastDownloadStatus = 0;
                Device.BeginInvokeOnMainThread(() =>
                    Navigation.PushAsync(new DownloadPage(new FileToDownload(release.assets[0].browser_download_url, downloadPath, release.assets[0].name), false))
                );

                while (DownloadPage.lastDownloadStatus == 0) Thread.Sleep(1);

                if (DownloadPage.lastDownloadStatus != 2)
                    Device.BeginInvokeOnMainThread(() => DisplayAlert("Error", "Failed to donwload apk or download was canceled", "Ok"));
                else {
                    Log.Information("Downloaded update");
                    // assinged by current platform (only android for now, I don't think iOS allows this)
                    //Util.InstallUpdate(downloadPath);
                    Device.BeginInvokeOnMainThread(() => PromptInstall(downloadPath));
                }
            }
        }

        private async Task PromptInstall(string downloadPath)
        {
            if (await DisplayAlert("Install", $"Please navigate to {downloadPath} and install the update", "Copy path", "Ok"))
                Clipboard.SetTextAsync(downloadPath);
        }

        private async Task AskDownloadApk(string newVersion)
        {
            askedDownloadAPKStatus = await DisplayAlert("Update app", $"New version was detected ({newVersion})", "Download", "Cancel") ? 2 : 1;
        }

        // called from TileDownloadPage
        private async Task DownloadTiles(Mapsui.Geometries.Point p1, Mapsui.Geometries.Point p2)
        {
            const double maxSizeX = 0.12d;
            const double maxSizeY = 0.12d;

            if (p1.X > p2.X) {
                double i = p1.X;
                p1.X = p2.X;
                p2.X = i;
            }
            if (p1.Y > p2.Y) {
                double i = p1.Y;
                p1.Y = p2.Y;
                p2.Y = i;
            }

            if (Math.Abs(p1.X - p2.X) > maxSizeX || Math.Abs(p1.Y - p2.Y) > maxSizeY) {
                await DisplayAlert("Error", $"Selected area is too big (x:{MathPlus.Round(Math.Abs(p1.X - p2.X), 3)}, y:{MathPlus.Round(Math.Abs(p1.Y - p2.Y), 3)}, " +
                    $"max: x:{maxSizeX}, y:{maxSizeY})", "Ok");
                return;
            }

            if (!await DisplayAlert("Confirm", $"Tiles between {p1} and {p2} will be downloaded", "Ok", "Cancel"))
                return;

            await Navigation.PopAsync();

            // convert lon/lat to tile coordinates
            p1 = Tile.getTileForCoordinates(p1);
            p2 = Tile.getTileForCoordinates(p2);

            // sometimes needed
            if (p1.X > p2.X) {
                double i = p1.X;
                p1.X = p2.X;
                p2.X = i;
            }
            if (p1.Y > p2.Y) {
                double i = p1.Y;
                p1.Y = p2.Y;
                p2.Y = i;
            }

            if (StateSingleton.config == null)
                StateSingleton.config = StateSingleton.ServerConfig.getFromFile();

            List<FileToDownload> toDownload = new List<FileToDownload>();
            for (int x = (int)p1.X; x <= (int)p2.X; x++) {
                for (int y = (int)p1.Y; y <= (int)p2.Y; y++) {
                    string savePath = Path.Combine(Util.SavePath_Server + @"tiles/16/", x.ToString(), $"{x}_{y}_16.png");
                    if (File.Exists(savePath))
                        continue; // don't download tiles again

                    toDownload.Add(new FileToDownload(new string[] {
                        StateSingleton.config.tileServerUrl + x + "/" + y + ".png",
                        StateSingleton.config.tileServerUrl2 + x + "/" + y + ".png"
                    }, savePath, Path.GetFileName(savePath)));
                }
            }

            Navigation.PushAsync(new DownloadPage(toDownload.ToArray(), false));
            lastTileDownload = DateTime.Now;
        }
    }
}