using System;
using System.IO;

namespace QuoteServer
{
    public static class ConnectionLogger
    {
        private static readonly object lockObj = new object();
        private static readonly string logFile = "quote_server.log";

        public static void Log(string message)
        {
            string entry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
            Console.WriteLine(entry);
            lock (lockObj)
            {
                File.AppendAllText(logFile, entry + Environment.NewLine);
            }
        }
    }
}