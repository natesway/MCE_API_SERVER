using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

using static MCE_API_SERVER.Util;

namespace MCE_API_SERVER
{
    public static class Settings
    {
        const string saveFileName = "Settings.json";

        private static SettingsSave save;

        // so that I don't have to use Instance
        public static bool[] MesLogFilter { get => save.MesLogFilter; set => save.MesLogFilter = value; }
        public static bool LogRequests { get => save.LogRequests; set => save.LogRequests = value; }
        public static bool LogMesTime { get => save.LogMesTime; set => save.LogMesTime = value; }
        public static bool LogMesType { get => save.LogMesType; set => save.LogMesType = value; }
        public static int MaxMessagesInConsole { get => save.MaxMessagesInConsole; set => save.MaxMessagesInConsole = value; }

        public static ushort ServerPort { get => save.ServerPort; set => save.ServerPort = value; }

        private static bool initialized = false;
        public static void Init()
        {
            if (initialized)
                return;

            if (!Load()) { // failed to load
                save = new SettingsSave();

                for (int i = 0; i < MesLogFilter.Length; i++)
                    MesLogFilter[i] = true;

                LogRequests = true;
                LogMesTime = true;
                LogMesType = false;

                MaxMessagesInConsole = 100;

                ServerPort = 5001;

                Log.Warning("Failed to load server setings, created default");
                Save();
            }
            else
                Log.Information("Loaded settings");
            initialized = true;
        }

        public static void Save()
        {
            try {
                SaveFile(saveFileName, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(save)));
                Log.Information("Saved settings");
            }
            catch (Exception ex) {
                Log.Error("Failed to save settings");
                Log.Exception(ex);
            }
        }

        public static bool Load()
        {
            if (!FileExists(saveFileName))
                return false;

            try {
                save = JsonConvert.DeserializeObject<SettingsSave>(LoadSavedFileString(saveFileName));
                return true;
            } catch {
                return false;
            }
        }

        private class SettingsSave
        {
            // Console
            public bool[] MesLogFilter { get; set; } = new bool[5];

            public bool LogRequests { get; set; }
            public bool LogMesTime { get; set; }
            public bool LogMesType { get; set; }

            public int MaxMessagesInConsole { get; set; }

            // Server
            public ushort ServerPort { get; set; }
        }
    }
}
