using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace MCE_API_SERVER
{
    public static class Log
    {
        public static Queue<LogMessage> ToLog = new Queue<LogMessage>();

        public static void Debug(string s) => ToLog.Enqueue(new LogMessage(s, LogType.Debug));
        public static void Information(string s) => ToLog.Enqueue(new LogMessage(s, LogType.Information));
        public static void Warning(string s) => ToLog.Enqueue(new LogMessage(s, LogType.Warning));
        public static void Error(string s) => ToLog.Enqueue(new LogMessage(s, LogType.Error));
        public static void Exception(Exception ex) => ToLog.Enqueue(new LogMessage(ex.ToString(), LogType.Exception));

        public struct LogMessage
        {
            public string Content;
            public LogType Type;

            public LogMessage(string content, LogType type)
            {
                Content = content;
                Type = type;
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
