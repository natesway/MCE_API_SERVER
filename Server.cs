using MCE_API_SERVER.Attributes;
using MCE_API_SERVER.Models.Features;
using MCE_API_SERVER.Models.Login;
using MCE_API_SERVER.Models.Player;
using MCE_API_SERVER.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;

namespace MCE_API_SERVER
{
    public static class Server
    {
        public const int AppVersion_Major = 1;
        public const int AppVersion_Minor = 1;
        public static readonly string AppVersion = AppVersion_Major + "." + AppVersion_Minor;

        public static Thread serverThread;

        public static bool Running { get; private set; }

        public static TcpListener listener;

        private static bool initialized = false;

        private static List<ServerHandle> handles = new List<ServerHandle>();

        private static void Init()
        {
            initialized = true;

            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            Type[] types = currentAssembly.GetTypes();
            List<Type> containerTypes = new List<Type>();
            for (int i = 0; i < types.Length; i++)
                if (types[i].GetCustomAttributes(typeof(ServerHandleContainerAttribute), true).Length > 0) {
                    if (!(types[i].IsAbstract && types[i].IsSealed))
                        Log.Error($"Class {types[i]} is ServerHandleContainer, but isn't static");
                    else
                        containerTypes.Add(types[i]);
                }

            for (int i = 0; i < containerTypes.Count; i++) {
                MethodInfo[] functions = containerTypes[i].GetMethods();
                for (int j = 0; j < functions.Length; j++) {
                    if (functions[j].GetCustomAttributes(typeof(ServerHandleAttribute), true).Length > 0) {
                        ParameterInfo[] parameters = functions[j].GetParameters();
                        if (parameters.Length == 1 && parameters[0].ParameterType == typeof(ServerHandleArgs)) {
                            ServerHandleAttribute attrib = (ServerHandleAttribute)functions[j].GetCustomAttribute(typeof(ServerHandleAttribute), true);
                            handles.Add(new ServerHandle(attrib.Urls, attrib.Types,
                                (ServerHandleFunction)functions[j].CreateDelegate(typeof(ServerHandleFunction))));
                        }
                    }
                }
            }

            try {
                ExtractFiles();
                Log.Debug("Extracted files");
            }
            catch (Exception ex) {
                Log.Error($"Couldn't extract files, deleting \"{Util.SavePath_Server}\" might help");
                Log.Exception(ex);
            }

            StateSingleton.config = StateSingleton.ServerConfig.getFromFile();
            StateSingleton.catalog = CatalogResponse.FromFiles(Util.SavePath_Server + StateSingleton.config.itemsFolderLocation, Util.SavePath_Server + StateSingleton.config.efficiencyCategoriesFolderLocation);
            StateSingleton.recipes = Recipes.FromFile(Util.SavePath_Server + StateSingleton.config.recipesFileLocation);
            StateSingleton.settings = SettingsResponse.FromFile(Util.SavePath_Server + StateSingleton.config.settingsFileLocation);
            StateSingleton.challengeStorage = ChallengeStorage.FromFiles(Util.SavePath_Server + StateSingleton.config.challengeStorageFolderLocation);
            StateSingleton.productCatalog = ProductCatalogResponse.FromFile(Util.SavePath_Server + StateSingleton.config.productCatalogFileLocation);
            StateSingleton.tappableData = TappableUtils.loadAllTappableSets();
            StateSingleton.activeTappables = new Dictionary<Guid, LocationResponse.ActiveLocationStorage>();
            StateSingleton.levels = ProfileUtils.readLevelDictionary();
            StateSingleton.shopItems = ShopUtils.readShopItemDictionary();

            Log.Information("Server initialized");
        }

        private static void ExtractFiles()
        {
            if (!Directory.Exists(Util.SavePath_Server + "items")) {
                Util.LoadEmbededFile("items.zip", out byte[] items);
                Util.SaveServerFile("items.zip", items);
                ZipFile.ExtractToDirectory(Util.SavePath_Server + "items.zip", Util.SavePath_Server);
                File.Delete(Util.SavePath_Server + "items.zip");
            }
            // efficiency categories
            if (!Directory.Exists(Util.SavePath_Server + "efficiency_categories")) {
                Util.LoadEmbededFile("efficiency_categories.zip", out byte[] categories);
                Util.SaveServerFile("efficiency_categories.zip", categories);
                ZipFile.ExtractToDirectory(Util.SavePath_Server + "efficiency_categories.zip", Util.SavePath_Server);
                File.Delete(Util.SavePath_Server + "efficiency_categories.zip");
            }
            // challenges
            if (!Directory.Exists(Util.SavePath_Server + "challenges")) {
                Util.LoadEmbededFile("challenges.zip", out byte[] challenges);
                Util.SaveServerFile("challenges.zip", challenges);
                ZipFile.ExtractToDirectory(Util.SavePath_Server + "challenges.zip", Util.SavePath_Server);
                File.Delete(Util.SavePath_Server + "challenges.zip");
            }
            // items
            if (!Directory.Exists(Util.SavePath_Server + "tappable")) {
                Util.LoadEmbededFile("tappable.zip", out byte[] tappable);
                Util.SaveServerFile("tappable.zip", tappable);
                ZipFile.ExtractToDirectory(Util.SavePath_Server + "tappable.zip", Util.SavePath_Server);
                File.Delete(Util.SavePath_Server + "tappable.zip");
            }
            // buildplates
            if (!Directory.Exists(Util.SavePath_Server + "buildplates")) {
                Util.LoadEmbededFile("buildplates.zip", out byte[] items);
                Util.SaveServerFile("buildplates.zip", items);
                ZipFile.ExtractToDirectory(Util.SavePath_Server + "buildplates.zip", Util.SavePath_Server);
                File.Delete(Util.SavePath_Server + "buildplates.zip");
            }
            // config
            if (!Directory.Exists(Util.SavePath_Server + "config"))
                Directory.CreateDirectory(Util.SavePath_Server + "config");
            if (!File.Exists(Util.SavePath_Server + "config/apiconfig.json")) {
                Util.LoadEmbededFile("apiconfig.json", out byte[] config);
                Util.SaveServerFile("config/apiconfig.json", config);
            }
            // other
            if (!File.Exists(Util.SavePath_Server + "encounterLocations.json")) {
                Util.LoadEmbededFile("encounterLocations.json", out byte[] encounterLocations);
                Util.SaveServerFile("encounterLocations.json", encounterLocations);
            }
            if (!File.Exists(Util.SavePath_Server + "journalCatalog.json")) {
                Util.LoadEmbededFile("journalCatalog.json", out byte[] journalCatalog);
                Util.SaveServerFile("journalCatalog.json", journalCatalog);
            }
            if (!File.Exists(Util.SavePath_Server + "levelDictionary.json")) {
                Util.LoadEmbededFile("levelDictionary.json", out byte[] levelDictionary);
                Util.SaveServerFile("levelDictionary.json", levelDictionary);
            }
            if (!File.Exists(Util.SavePath_Server + "productCatalog.json")) {
                Util.LoadEmbededFile("productCatalog.json", out byte[] productCatalog);
                Util.SaveServerFile("productCatalog.json", productCatalog);
            }
            if (!File.Exists(Util.SavePath_Server + "seasonChallenges.json")) {
                Util.LoadEmbededFile("seasonChallenges.json", out byte[] seasonChallenges);
                Util.SaveServerFile("seasonChallenges.json", seasonChallenges);
            }
            if (!File.Exists(Util.SavePath_Server + "settings.json")) {
                Util.LoadEmbededFile("settings.json", out byte[] settings);
                Util.SaveServerFile("settings.json", settings);
            }
            if (!File.Exists(Util.SavePath_Server + "shopItemDictionary.json")) {
                Util.LoadEmbededFile("shopItemDictionary.json", out byte[] shopItemDictionary);
                Util.SaveServerFile("shopItemDictionary.json", shopItemDictionary);
            }
            if (!File.Exists(Util.SavePath_Server + "recipes.json")) {
                Util.LoadEmbededFile("recipes.json", out byte[] recipes);
                Util.SaveServerFile("recipes.json", recipes);
            }
        }

        public static bool Start()
        {
            if (!Util.ServerFileExists("resourcepacks/vanilla.zip")) {
                if (!Directory.Exists(Util.SavePath_Server + "resourcepacks"))
                    Directory.CreateDirectory(Util.SavePath_Server + "resourcepacks");
                return false;
            }

            if (!initialized)
                Init();

            listener = new TcpListener(IPAddress.Loopback, Settings.ServerPort);
            listener.Start();

            serverThread = new Thread(Run);
            Running = true;
            serverThread.Start();
            Log.Information($"Server started at port {Settings.ServerPort}");
            return true;
        }

        public static void Stop()
        {
            Running = false;
            listener.Stop();
            listener = null;
            Log.Information("Server stopped");
        }

        private static void Run()
        {
            while (Running) {
                try {
                    TcpClient client = listener.AcceptTcpClient();

                    #region parse
                    string data = "";
                    List<byte> dataBytes = new List<byte>();
                    byte[] bytes = new byte[4096];

                    client.ReceiveBufferSize = bytes.Length;
                    int c = 0;
                    while (client.Client.Available > 0 || c < 5) {
                        if (client.Client.Available > 0) {
                            int numBytes = client.Client.Receive(bytes, client.Available, SocketFlags.None);
                            dataBytes.AddRange(bytes.Take(numBytes));
                            data += Encoding.UTF8.GetString(bytes, 0, numBytes);
                        }

                        Thread.Sleep(10);
                        c++;
                    }
                    // sec-ch-ua: " Not A;Brand";v="99", "Chromium";v="102", "Google Chrome";v="102"
                    // sec-ch-ua: " Not A;Brand";v="99", "Chromium";v="102", "Microsoft Edge";v="102"
                    string type = "";
                    string fullSub = "";
                    string sub = "";
                    string platform = "";
                    string language = "";
                    string content = "";

                    foreach (string line in data.Split('\n')) {
                        if (line.StartsWith("GET")) {
                            type = "GET";
                            if (line.StartsWith("GET / HTTP/1.1"))
                                fullSub = "/main";
                            else
                                fullSub = line.Split(' ')[1];
                        }
                        else if (line.StartsWith("PUT")) {
                            type = "PUT";
                            if (line.StartsWith("PUT / HTTP/1.1"))
                                fullSub = "/main";
                            else
                                fullSub = line.Split(' ')[1];
                        }
                        else if (line.StartsWith("POST")) {
                            type = "POST";
                            if (line.StartsWith("POST / HTTP/1.1"))
                                fullSub = "/main";
                            else
                                fullSub = line.Split(' ')[1];
                        }
                        else if (line.StartsWith("HEAD")) {
                            type = "HEAD";
                            if (line.StartsWith("HEAD / HTTP/1.1"))
                                fullSub = "/main";
                            else
                                fullSub = line.Split(' ')[1];
                        }
                        else if (line.StartsWith("sec-ch-ua-platform:"))
                            platform = line.Split(' ')[1];
                        else if (line.StartsWith("Accept-Language:"))
                            language = line.Split(' ')[1].Split(';')[0];
                    }

                    if (type == "PUT" || type == "POST") content = data.Split(new string[] { "\r\n\r\n" }, StringSplitOptions.None).Last();

                    fullSub = fullSub.Replace("%20", " ");
                    sub = fullSub.Split('?')[0];
                    #endregion

                    if (Settings.LogRequests)
                        Log.Debug($"{sub} {type}", true/*log even if filter for debug is false*/);

                    byte[] resp = new byte[0];

                    for (int i = 0; i < handles.Count; i++) {
                        for (int j = 0; j < handles[i].Urls.Length; j++) {
                            if (handles[i].Types.Length != 0) {
                                bool found = true;
                                for (int x = 0; x < handles[i].Types.Length; x++)
                                    if (type == handles[i].Types[x]) {
                                        found = true;
                                        break;
                                    }
                                if (!found)
                                    continue;
                            }

                            if (handles[i].Urls[j] == sub) {
                                resp = handles[i].Function(new ServerHandleArgs(fullSub, data, dataBytes.ToArray(), content, (IPEndPoint)client.Client.RemoteEndPoint, type));
                                goto response;
                            }
                            else { // invalid or url values
                                string[] recSubs = sub.Split('/'); // received
                                string[] compSubs = handles[i].Urls[j].Split('/'); // comparing
                                if (recSubs.Length != compSubs.Length || !handles[i].Urls[j].Contains('{'))
                                    continue;

                                Dictionary<string, string> urlArgs = new Dictionary<string, string>();
                                bool fail = false;
                                for (int x = 0; x < recSubs.Length; x++) {
                                    if (recSubs[x] != compSubs[x])
                                        if (compSubs[x].First() == '{' && compSubs[x].Last() == '}') {
                                            urlArgs.Add(compSubs[x].Substring(1, compSubs[x].Length - 2), recSubs[x]);
                                        }
                                        else { // invalid or multiple url values
                                            string currentArgName = "";
                                            string currentArgValue = "";
                                            int compIndex = 0;
                                            for (int y = 0; y < recSubs[x].Length && compIndex < compSubs[x].Length; y++) {
                                                if (recSubs[x][y] == compSubs[x][compIndex])
                                                    compIndex++;
                                                else if (compSubs[x][compIndex] == '{') {
                                                    compIndex++;
                                                    while (compSubs[x][compIndex] != '}') {
                                                        currentArgName += compSubs[x][compIndex];
                                                        compIndex++;
                                                    }
                                                    // comp is }, increase by one
                                                    compIndex++;

                                                    while (y < recSubs[x].Length && (compIndex >= compSubs[x].Length || recSubs[x][y] != compSubs[x][compIndex])) {
                                                        currentArgValue += recSubs[x][y];
                                                        y++;
                                                    }

                                                    if (y < recSubs[x].Length && recSubs[x][y] != compSubs[x][compIndex]) {
                                                        fail = true;
                                                        break;
                                                    }

                                                    // y will increate, because for loop
                                                    y--;

                                                    urlArgs.Add(currentArgName, currentArgValue);
                                                    currentArgName = "";
                                                    currentArgValue = "";
                                                }
                                                else {
                                                    fail = true;
                                                    break;
                                                }
                                            }

                                            if (fail)
                                                break;
                                        }
                                }

                                if (fail)
                                    continue;

                                if (content == "") {
                                    int z = 0;
                                }

                                resp = handles[i].Function(new ServerHandleArgs(fullSub, data, dataBytes.ToArray(), content, (IPEndPoint)client.Client.RemoteEndPoint, type, urlArgs));
                                goto response;
                            }
                        }
                    }

                    Log.Error($"Handle for url {sub}({fullSub}) wasn't found");

                    string mimeType = "text/plain";

                    string respHeaderString = $"HTTP/1.1 200 OK\r\nServer: csharp_server\r\nContent-Type: {mimeType}\r\ncharset: UTF-8\r\n\r\n";

                    byte[] respData = Encoding.UTF8.GetBytes("Hello World!");

                    byte[] respHeader = Encoding.UTF8.GetBytes(respHeaderString);

                    resp = new byte[respHeader.Length + respData.Length];

                    Array.Copy(respHeader, resp, respHeader.Length);
                    Array.Copy(respData, 0, resp, respHeader.Length, respData.Length);

                    response:
                    client.Client.SendTo(resp, client.Client.RemoteEndPoint);
                    client.Close();
                    client.Dispose();
                }
                catch (Exception ex) {
                    if (ex is SocketException se && se.SocketErrorCode == SocketError.Interrupted) { } // don't log this exception, happens when server is stopped
                    else
                        Log.Exception(ex);
                }
            }
            Running = false;
        }
    }

    public delegate byte[] ServerHandleFunction(ServerHandleArgs args);

    public struct ServerHandleArgs
    {
        public string Data;
        public byte[] DataBytes;
        public string Content;
        public IPEndPoint Sender;
        public string Method;
        public Dictionary<string, string> UrlArgs;
        public Dictionary<string, string> Headers;
        public Dictionary<string, string> Query;

        public ServerHandleArgs(string fullUrl, string data, byte[] dataBytes, string content, IPEndPoint sender, string method, Dictionary<string, string> urlArgs = null)
        {
            Data = data;
            DataBytes = dataBytes;
            Content = content;
            Sender = sender;
            Method = method;
            if (urlArgs == null)
                UrlArgs = new Dictionary<string, string>();
            else
                UrlArgs = urlArgs;

            // Parse headers
            Headers = new Dictionary<string, string>();
            string header = Data.Split(new string[] { "\r\n\r\n" }, StringSplitOptions.None)[0];
            string[] headers = header.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            for (int i = 1; i < headers.Length; i++) {
                string s = headers[i].Replace(" ", "");
                int index = s.IndexOf(':');
                Headers.Add(s.Substring(0, index), s.Substring(index + 1));
            }

            // Parse query
            Query = new Dictionary<string, string>();
            if (fullUrl.Contains('?')) {
                try {
                    string queryString = fullUrl.Substring(fullUrl.IndexOf('?') + 1);
                    string[] querys = queryString.Split('&', ';'); // ; isn't recomended, just in case
                    for (int i = 0; i < querys.Length; i++) {
                        string[] split = querys[i].Split('=');
                        if (split.Length > 1)
                            Query.Add(split[0], split[1]);
                    }
                }
                catch {
                    Log.Error($"Failed to make query for url {fullUrl}");
                }
            }
        }
    }

    public struct ServerHandle
    {
        public string[] Urls;
        public string[] Types;
        public ServerHandleFunction Function;

        public ServerHandle(string[] _urls, string[] types, ServerHandleFunction _function)
        {
            Urls = _urls;
            for (int i = 0; i < Urls.Length; i++)
                if (Urls[i][0] != '/')
                    Urls[i] = "/" + Urls[i];
            Types = types;
            Function = _function;
        }

        public override string ToString()
        {
            return Urls[0];
        }
    }
}
