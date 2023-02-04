using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xamarin.Forms;

namespace MCE_API_SERVER
{
    public static class Log
    {
        private const string saveFileName = "log.txt";
        private const string saveHistoryName = "LogHistory/";

        public static Queue<LogMessage> ToLog = new Queue<LogMessage>();

        private static FileStream saveStream;

        public static void Init(bool coldBoot/*also called when waking from sleep, if from sleep continue in current log file*/)
        {
            if (Util.FileExists(saveFileName) && coldBoot) {
                if (!Directory.Exists(Util.SavePath + saveHistoryName))
                    Directory.CreateDirectory(Util.SavePath + saveHistoryName);

                DateTime time = DateTime.Now;

                File.Move(Util.SavePath + saveFileName, Util.SavePath + saveHistoryName +
                    $"log_{time.Day}.{time.Month}.{time.Year}_{time.Hour}:{time.Minute}:{time.Second}.txt");
            }

            if (saveStream != null && saveStream.CanWrite)
                saveStream.Close();

            saveStream = new FileStream(Util.SavePath + saveFileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
        }

        public static void Dispose()
        {
            saveStream.Close();
            saveStream.Dispose();
            saveStream = null;
        }

        public static void Debug(string message, bool forceLog = false) {
            LogMes(new LogMessage(message, LogType.Debug), 0, forceLog);
        }
        public static void Information(string message, bool forceLog = false)
        {
            LogMes(new LogMessage(message, LogType.Information), 1, forceLog); 
        }
        public static void Warning(string message, bool forceLog = false)
        {
            LogMes(new LogMessage(message, LogType.Warning), 2, forceLog); 
        }
        public static void Error(string message, bool forceLog = false)
        {
            LogMes(new LogMessage(message, LogType.Error), 3, forceLog);
        }
        public static void Exception(Exception ex, bool forceLog = false)
        {
            LogMes(new LogMessage(ex.ToString(), LogType.Exception), 4, forceLog);
        }

        private static void LogMes(LogMessage message, int filterIndex, bool forceLog)
        {
            if (saveStream == null)
                Init(false);

            string messageText = $"[{Enum.GetName(typeof(LogType), message.Type)}] [{message.Time.Day}.{message.Time.Month}.{message.Time.Year} " +
                $"{message.Time.Hour}:{message.Time.Minute}:{message.Time.Second}] {message.Content}\n";
            byte[] bytes = Encoding.UTF8.GetBytes(messageText);
            saveStream.Write(bytes, 0, bytes.Length);
            saveStream.Flush();

            if (Settings.MesLogFilter[filterIndex] || forceLog)
                ToLog.Enqueue(message);
        }

        public struct LogMessage
        {
            public string Content;
            public LogType Type;
            public DateTime Time;

            public LogMessage(string content, LogType type)
            {
                Content = content;
                Type = type;
                Time = DateTime.Now;
            }
        }

        public enum LogType
        {
            Debug,
            Information,
            Warning,
            Error,
            Exception,
        }

        public static Dictionary<LogType, Color> TypeToColor = new Dictionary<LogType, Color>()
        {
            { LogType.Debug, Color.Gray },
            { LogType.Information, Color.White },
            { LogType.Warning, Color.Yellow },
            { LogType.Error, Color.Red },
            { LogType.Exception, Color.Red },
        };
    }
}
