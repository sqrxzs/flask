using System;
using System.Net.Sockets;
using System.Text;

namespace CurrencyClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Введите IP сервера (по умолчанию 127.0.0.1): ");
            string ip = Console.ReadLine();
            if (string.IsNullOrEmpty(ip)) ip = "127.0.0.1";

            Console.Write("Введите порт сервера (по умолчанию 8888): ");
            string portStr = Console.ReadLine();
            int port = string.IsNullOrEmpty(portStr) ? 8888 : int.Parse(portStr);

            try
            {
                using (TcpClient client = new TcpClient(ip, port))
                using (NetworkStream stream = client.GetStream())
                using (System.IO.StreamReader reader = new System.IO.StreamReader(stream, Encoding.UTF8))
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
                {
                    Console.WriteLine("Подключено к серверу. Введите команду: две валюты через пробел (например USD EUR) или exit для выхода.");

                    while (true)
                    {
                        Console.Write("> ");
                        string input = Console.ReadLine();
                        if (string.IsNullOrEmpty(input)) continue;

                        writer.WriteLine(input);

                        if (input.Trim().ToLower() == "exit")
                        {
                            string response = reader.ReadLine();
                            Console.WriteLine($"[СЕРВЕР] {response}");
                            break;
                        }

                        string responseMsg = reader.ReadLine();
                        Console.WriteLine($"[СЕРВЕР] {responseMsg}");
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