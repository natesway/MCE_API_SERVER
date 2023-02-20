using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace MCE_API_SERVER
{
    public static class Util
    {
        public static string SavePath;
        public static string SavePath_Server;
        private static bool initialized;
        public static Random rng;

        private static void Init()
        {
            SavePath = GetSavePath() + "/";
            if (!Directory.Exists(SavePath))
                Directory.CreateDirectory(SavePath);
            SavePath_Server = SavePath + "server/";
            if (!Directory.Exists(SavePath_Server))
                Directory.CreateDirectory(SavePath_Server);

            // initialize based on current time
            rng = new Random(DateTime.Now.Second << 4 + DateTime.Now.Millisecond);

            initialized = true;
        }

        public static Assembly CurrentAssembly = Assembly.GetExecutingAssembly();
        //public static Action<string> InstallUpdate;
        public static Func<string> GetSavePath;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v1">version 1</param>
        /// <param name="v2">version 2</param>
        /// <returns>
        /// -2 - invalid 
        /// -1 - v2 is newer 
        /// 0 - versions are same 
        /// 1 - v1 is newer 
        /// </returns>
        public static int CompareVersions(string v1, string v2)
        {
            if (!(v1.Contains('.') || v1.Contains(',')) || !(v2.Contains('.') || v2.Contains(',')))
                return -2;

            string[] v1s = v1.Split('.', ',');
            string[] v2s = v2.Split('.', ',');

            if (v1s[0] == v2s[0] && v1s[1] == v2s[1])
                return 0;

            if (int.TryParse(v1s[0], out int v1Major) && int.TryParse(v2s[0], out int v2Major)) {
                if (v1Major < v2Major)
                    return -1;
                else if (v1Major > v2Major)
                    return 1;
            }
            else
                return -2; // one of versions is invalid

            if (int.TryParse(v1s[1], out int v1Minor) && int.TryParse(v2s[1], out int v2Minor)) {
                if (v1Minor < v2Minor)
                    return -1;
                else if (v1Minor > v2Minor)
                    return 1;
            }
            else
                return -2; // one of versions is invalid

            // versions are same
            return 0;
        }

        public static Mapsui.Geometries.Point SwapXY(this Mapsui.Geometries.Point p)
            => new Mapsui.Geometries.Point(p.Y, p.X);

        public static void Swap(ref int i1, ref int i2)
        {
            int _i1 = i1;
            int _i2 = i2;
            i1 = _i2;
            i2 = _i1;
        }

        public static byte[] ReadToEnd(this Stream s)
        {
            byte[] bytes = new byte[s.Length];
            s.Read(bytes, 0, bytes.Length);
            return bytes;
        }

        public static HttpResponse Ok()
        {
            return new HttpResponse(200);
        }

        public static HttpResponse Accepted()
        {
            return new HttpResponse(202);
        }

        public static HttpResponse Unauthorized()
        {
            return new HttpResponse(401);
        }

        public static HttpResponse BadRequest()
        {
            return new HttpResponse(400);
        }

        public static HttpResponse NoContent()
        {
            return new HttpResponse(204);
        }

        public static HttpResponse Content(ServerHandleArgs args, string respData, string respType, Dictionary<string, string> headers)
            => Content(args, Encoding.UTF8.GetBytes(respData), respType, headers);
        public static HttpResponse Content(ServerHandleArgs args, byte[] respData, string respType, Dictionary<string, string> headers)
        {
            headers.Add("charset", "UTF-8");
            headers.Add("Server", "csharp_server");

            if (args.Method == "HEAD")
                return new HttpResponse(headers, 200, respType);

            return new HttpResponse(respData, headers, 200, respType);
        }
        public static HttpResponse Content(ServerHandleArgs args, string respData, string respType)
            => CreateResp(args, Encoding.UTF8.GetBytes(respData), respType);
        public static HttpResponse CreateResp(ServerHandleArgs args, byte[] respData, string respType)
        {
            if (args.Method == "HEAD")
                return new HttpResponse(new Dictionary<string, string>() { { "charset", "UTF-8" }, { "Server", "csharp_server" } }, 200, respType);

            return new HttpResponse(respData, new Dictionary<string, string>() { { "charset", "UTF-8" }, { "Server", "csharp_server" } }, 200, respType);
        }

        public static HttpResponse File(ServerHandleArgs args, byte[] respData, string respType, System.Net.Mime.ContentDisposition cd)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Server", "csharp_server");
            headers.Add("Content-Type", respType);
            headers.Add("Content-Length", cd.Size.ToString());
            headers.Add("Content-Disposition", cd.ToString());
            headers.Add("Content-Transfer-Encoding", "binary");

            if (args.Method == "HEAD")
                return new HttpResponse(headers, 200, respType);

            return new HttpResponse(respData, headers, 200, respType);
        }

        public static string GetFullResourceName(string name) => $"MCE_API_SERVER.Data.{name.Replace('/', '.')}";
        public static bool LoadEmbededFile(string name, out byte[] data)
        {
            data = new byte[0];

            Stream stream = CurrentAssembly.GetManifestResourceStream(GetFullResourceName(name));
            if (stream == null) {
                Log.Error($"Couldn't load embeded file \"{name}\"");
                return false;
            }

            data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);
            stream.Close();
            return true;
        }

        // normal
        public static string LoadSavedFileString(string name)
        {
            byte[] bytes = LoadSavedFile(name);
            if (bytes == null)
                return string.Empty;
            else
                return Encoding.UTF8.GetString(bytes);
        }
        public static byte[] LoadSavedFile(string name)
        {
            if (!initialized)
                Init();

            if (!System.IO.File.Exists(SavePath + name))
                return null;
            else
                return System.IO.File.ReadAllBytes(SavePath + name);
        }

        public static void SaveFile(string name, string data)
            => SaveFile(name, Encoding.UTF8.GetBytes(data));
        public static void SaveFile(string name, byte[] data)
        {
            if (!initialized)
                Init();

            System.IO.File.WriteAllBytes(SavePath + name, data);
        }

        public static bool FileExists(string name)
        {
            if (!initialized)
                Init();

            return System.IO.File.Exists(SavePath + name);
        }

        // server
        public static string LoadSavedServerFileString(string name)
        {
            byte[] bytes = LoadSavedServerFile(name);
            if (bytes == null)
                return string.Empty;
            else
                return Encoding.UTF8.GetString(bytes);
        }
        public static byte[] LoadSavedServerFile(string name)
        {
            if (!initialized)
                Init();

            if (!System.IO.File.Exists(SavePath_Server + name))
                return null;
            else
                return System.IO.File.ReadAllBytes(SavePath_Server + name);
        }

        public static void SaveServerFile(string name, string data)
            => SaveServerFile(name, Encoding.UTF8.GetBytes(data));
        public static void SaveServerFile(string name, byte[] data)
        {
            if (!initialized)
                Init();

            System.IO.File.WriteAllBytes(SavePath_Server + name, data);
        }

        public static bool ServerFileExists(string name)
        {
            if (!initialized)
                Init();

            return System.IO.File.Exists(SavePath_Server + name);
        }

        public static string ByteArrayToHexString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public static async Task OpenBrowser(Uri uri)
        {
            try {
                await Browser.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
            }
            catch { }
        }



        private static uint _streamVersion = 0;

        public static T ParseJsonFile<T>(string playerId, string fileNameWithoutJsonExtension) where T : new()
        {
            // idk why it has this
            playerId = playerId.Replace("Genoa", "");

            string filepath = $"players/{playerId}/{fileNameWithoutJsonExtension}.json";

            byte[] data = new byte[0];
            if (!ServerFileExists(filepath)) {
                if (!Directory.Exists(SavePath_Server + $"players/{playerId}"))
                    Directory.CreateDirectory(SavePath_Server + $"players/{playerId}");

                SetupJsonFile<T>(playerId, filepath); // Generic setup for each player specific json type
            }

            byte[] invjson = LoadSavedServerFile(filepath);
            T parsedobj;
            try {
                parsedobj = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(invjson));
            }
            catch {
                parsedobj = Utf8Json.JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(invjson));
            }
            return parsedobj;
        }

        private static bool SetupJsonFile<T>(string playerId, string filepath) where T : new()
        {
            // idk why it has this
            playerId = playerId.Replace("Genoa", "");

            try {
                Log.Information($"[{playerId}]: Creating default json with Type: {typeof(T)}.");
                T obj = new T(); // TODO: Implement Default Values for each player property/json we store for them

                SaveServerFile(filepath, JsonConvert.SerializeObject(obj));
                return true;
            }
            catch (Exception ex) {
                Type exType = ex.GetType();
                if (exType == typeof(InvalidCastException)) { // try again with diffrent json
                    try {
                        T obj = new T(); // TODO: Implement Default Values for each player property/json we store for them

                        SaveServerFile(filepath, Utf8Json.JsonSerializer.Serialize(obj));
                        return true;
                    }
                    catch (Exception e) {
                        Log.Error($"[{playerId}]: Creating default json failed! Type: {typeof(T)}");
                        Log.Exception(e);
                        return false;
                    }
                }
                Log.Error($"[{playerId}]: Creating default json failed! Type: {typeof(T)}");
                Log.Exception(ex);
                return false;
            }
        }

        public static bool WriteJsonFile<T>(string playerId, T objToWrite, string fileNameWithoutJsonExtension)
        {
            // idk why it has this
            playerId = playerId.Replace("Genoa", "");

            try {
                string filepath = $"players/{playerId}/{fileNameWithoutJsonExtension}.json"; // Path should exist, as you cant really write to the file before reading it first

                try {
                    string s = JsonConvert.SerializeObject(objToWrite);

                    if (s == null || s == "") // failed
                        throw new Exception();

                    SaveServerFile(filepath, s);
                }
                catch {
                    SaveServerFile(filepath, Utf8Json.JsonSerializer.Serialize(objToWrite));
                }

                return true;
            }
            catch {
                return false;
            }
        }

        public static uint GetNextStreamVersion()
        {
            _streamVersion++;
            return _streamVersion;
        }
    }
}
