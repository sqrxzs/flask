using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;

class TcpChatServer
{
    static StreamWriter logWriter;

    static void Main()
    {
        int port = 8888;
        IPAddress localAddr = IPAddress.Any;
        TcpListener listener = new TcpListener(localAddr, port);
        listener.Start();
        Console.WriteLine($"[СЕРВЕР] Запущен на порту {port}. Ожидание подключения...");

        using (TcpClient client = listener.AcceptTcpClient())
        using (NetworkStream stream = client.GetStream())
        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
        using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
        using (logWriter = new StreamWriter("chat_log.txt", true, Encoding.UTF8))
        {
            Console.WriteLine("[СЕРВЕР] Клиент подключён.");
            LogMessage("СЕРВЕР", "Клиент подключён.");

            while (true)
            {
                string received = reader.ReadLine();
                if (received == null) break;

                Console.WriteLine($"[КЛИЕНТ] {received}");
                LogMessage("КЛИЕНТ", received);

                if (received.ToLower() == "exit")
                {
                    writer.WriteLine("exit");
                    LogMessage("СЕРВЕР", "exit");
                    break;
                }

                string response = $"Эхо: {received}";
                writer.WriteLine(response);
                LogMessage("СЕРВЕР", response);
            }
        }

        listener.Stop();
        Console.WriteLine("[СЕРВЕР] Завершён.");
    }

    static void LogMessage(string sender, string message)
    {
        string logEntry = $"[{DateTime.Now:HH:mm:ss}] {sender}: {message}";
        Console.WriteLine(logEntry);
        logWriter.WriteLine(logEntry);
        logWriter.Flush();
    }
}