// ConsoleServer.cs
using System;
using System.Threading.Tasks;
using MessagingLibrary;

class ConsoleServer
{
    static MessageServer server;
    static async Task Main(string[] args)
    {
        server = new MessageServer();
        server.OnLog += (msg) => Console.WriteLine($"[LOG] {msg}");
        _ = server.StartAsync(8888);
        Console.WriteLine("Сервер запущен. Команды: send <тип> <текст>, urgent <текст>, list, kick <ID>, exit");
        while (true)
        {
            string input = Console.ReadLine();
            if (input == "exit") break;
            else if (input.StartsWith("send "))
            {
                var parts = input.Substring(5).Split(' ', 2);
                if (parts.Length == 2)
                    server.SendToSubscribers(parts[0], parts[1]);
                else
                    Console.WriteLine("Формат: send тип текст");
            }
            else if (input.StartsWith("urgent "))
            {
                string text = input.Substring(7);
                server.SendUrgent(text);
            }
            else if (input == "list")
            {
                var clients = server.GetClientsInfo();
                Console.WriteLine("Подключенные клиенты:");
                foreach (var c in clients) Console.WriteLine(c);
            }
            else if (input.StartsWith("kick "))
            {
                string id = input.Substring(5);
                server.KickClient(id);
            }
            else
                Console.WriteLine("Неизвестная команда");
        }
        server.Stop();
    }
}