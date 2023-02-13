using MCE_API_SERVER.Models.Player;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private static bool initialized = false;
        public static void Init()
        {
            SavePath += "/";
            if (!Directory.Exists(SavePath))
                Directory.CreateDirectory(SavePath);
            SavePath_Server = SavePath + "server/";
            if (!Directory.Exists(SavePath_Server))
                Directory.CreateDirectory(SavePath_Server);
            initialized = true;
        }

        public static Assembly CurrentAssembly = Assembly.GetExecutingAssembly();

        public static byte[] Ok()
        {
            string respHeaderString = $"HTTP/1.1 200 OK\r\nServer: csharp_server\r\n\r\n";
            byte[] respHeader = Encoding.UTF8.GetBytes(respHeaderString);
            return respHeader;
        }

        public static byte[] Accepted()
        {
            string respHeaderString = $"HTTP/1.1 202 Accepted\r\nServer: csharp_server\r\n\r\n";
            byte[] respHeader = Encoding.UTF8.GetBytes(respHeaderString);
            return respHeader;
        }

        public static byte[] Unauthorized()
        {
            string respHeaderString = $"HTTP/1.1 401 Unauthorized\r\nServer: csharp_server\r\n\r\n";
            byte[] respHeader = Encoding.UTF8.GetBytes(respHeaderString);
            return respHeader;
        }

        public static byte[] BadRequest()
        {
            string respHeaderString = $"HTTP/1.1 400 Bad Request\r\nServer: csharp_server\r\n\r\n";
            byte[] respHeader = Encoding.UTF8.GetBytes(respHeaderString);
            return respHeader;
        }

        public static byte[] NoContent()
        {
            string respHeaderString = $"HTTP/1.1 204 No Content\r\nServer: csharp_server\r\n\r\n";
            byte[] respHeader = Encoding.UTF8.GetBytes(respHeaderString);
            return respHeader;
        }

        public static byte[] Content(ServerHandleArgs args, string respData, string respType, params string[] headers)
            => CreateResp(args, Encoding.UTF8.GetBytes(respData), respType, headers);
        public static byte[] CreateResp(ServerHandleArgs args, byte[] respData, string respType, params string[] headers)
        {
            string respHeaderString = $"HTTP/1.1 200 OK\r\nServer: csharp_server\r\nContent-Type: {respType}\r\ncharset: UTF-8";

            if (headers != null && headers.Length > 0) {
                for (int i = 0; i < headers.Length; i++)
                    respHeaderString += "\r\n" + headers[i];
            }

            respHeaderString += "\r\n\r\n";

            byte[] respHeader = Encoding.UTF8.GetBytes(respHeaderString);

            if (args.Method == "HEAD")
                return respHeader;

            byte[] resp = new byte[respHeader.Length + respData.Length];

            Array.Copy(respHeader, resp, respHeader.Length);
            Array.Copy(respData, 0, resp, respHeader.Length, respData.Length);

            return resp;
        }

        public static byte[] File(ServerHandleArgs args, byte[] respData, string respType, System.Net.Mime.ContentDisposition cd)
        {
            string respHeaderString = $"HTTP/1.1 200 OK\r\nServer: csharp_server\r\nContent-Type: {respType}\r\nContent-Length: {cd.Size}\r\nContent-Disposition: {cd.ToString()}\r\nContent-Transfer-Encoding: binary\r\n\r\n";

            byte[] respHeader = Encoding.UTF8.GetBytes(respHeaderString);

            if (args.Method == "HEAD")
                return respHeader;

            byte[] resp = new byte[respHeader.Length + respData.Length];

            Array.Copy(respHeader, resp, respHeader.Length);
            Array.Copy(respData, 0, resp, respHeader.Length, respData.Length);

            return resp;
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
            } catch {
                parsedobj = Utf8Json.JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(invjson));
            }
            return parsedobj;
        }

        private static bool SetupJsonFile<T>(string playerId, string filepath) where T : new()
        {
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
                    } catch (Exception e) {
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
            try {
                string filepath =  $"players/{playerId}/{fileNameWithoutJsonExtension}.json"; // Path should exist, as you cant really write to the file before reading it first

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
