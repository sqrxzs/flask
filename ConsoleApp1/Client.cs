using System;
using System.Net.Sockets;
using System.Text;

class TcpChatClient
{
    static void Main()
    {
        string serverIP = "127.0.0.1";
        int port = 8888;

        using (TcpClient client = new TcpClient(serverIP, port))
        using (NetworkStream stream = client.GetStream())
        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
        using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
        {
            Console.WriteLine("[КЛИЕНТ] Подключён к серверу. Введите сообщение (exit для выхода):");

            while (true)
            {
                Console.Write("> ");
                string input = Console.ReadLine();
                writer.WriteLine(input);

                string response = reader.ReadLine();
                if (response == null) break;
                Console.WriteLine($"[СЕРВЕР] {response}");

                if (input.ToLower() == "exit")
                    break;
            }
        }

        Console.WriteLine("[КЛИЕНТ] Завершён.");
    }
}