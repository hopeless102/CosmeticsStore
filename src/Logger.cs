using System;
using System.IO;

namespace CosmeticsStore
{
    public static class Logger
    {
        private static string _logFile = "app.log";

        public static void Info(string message)
        {
            WriteLog("INFO", message);
        }

        public static void Error(string message, Exception? ex = null)
        {
            string msg = message;
            if (ex != null)
                msg += $"\nОшибка: {ex.Message}\n{ex.StackTrace}";
            WriteLog("ERROR", msg);
        }

        private static void WriteLog(string level, string message)
        {
            try
            {
                string log = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
                File.AppendAllText(_logFile, log + Environment.NewLine);
            }
            catch { }
        }
    }
}