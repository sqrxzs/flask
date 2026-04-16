using System;
using System.Net.Sockets;
using System.Text;

namespace QuoteClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Введите IP сервера (по умолчанию 127.0.0.1): ");
            string ip = Console.ReadLine();
            if (string.IsNullOrEmpty(ip)) ip = "127.0.0.1";

            Console.Write("Введите порт (по умолчанию 8888): ");
            string portStr = Console.ReadLine();
            int port = string.IsNullOrEmpty(portStr) ? 8888 : int.Parse(portStr);

            try
            {
                using (TcpClient client = new TcpClient(ip, port))
                using (NetworkStream stream = client.GetStream())
                using (System.IO.StreamReader reader = new System.IO.StreamReader(stream, Encoding.UTF8))
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
                {
                    // Аутентификация
                    Console.WriteLine(reader.ReadLine()); // "AUTH: Введите логин:"
                    string login = Console.ReadLine();
                    writer.WriteLine(login);
                    Console.WriteLine(reader.ReadLine()); // "AUTH: Введите пароль:"
                    string password = Console.ReadLine();
                    writer.WriteLine(password);

                    string authResponse = reader.ReadLine();
                    Console.WriteLine(authResponse);
                    if (authResponse.StartsWith("ERROR"))
                    {
                        Console.WriteLine("Нажмите Enter для выхода...");
                        Console.ReadLine();
                        return;
                    }

                    // Основной цикл
                    Console.WriteLine("Введите 'quote' для получения цитаты, 'exit' для выхода.");
                    while (true)
                    {
                        Console.Write("> ");
                        string input = Console.ReadLine();
                        if (string.IsNullOrEmpty(input)) continue;

                        writer.WriteLine(input);
                        string response = reader.ReadLine();
                        if (response == null) break;

                        if (response == "exit")
                        {
                            Console.WriteLine("Соединение закрыто сервером.");
                            break;
                        }
                        else if (response.StartsWith("CITATA:"))
                        {
                            Console.WriteLine(response);
                        }
                        else if (response.StartsWith("ERROR:"))
                        {
                            Console.WriteLine($"Ошибка: {response}");
                            if (response.Contains("лимит") || response.Contains("перегружен"))
                                break;
                        }
                        else
                        {
                            Console.WriteLine(response);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }

            Console.WriteLine("Нажмите Enter для выхода...");
            Console.ReadLine();
        }
    }
}