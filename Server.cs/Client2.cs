using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class TimeDateClient
{
    static void Main()
    {
        string serverIP = "127.0.0.1"; // или реальный IP сервера
        int serverPort = 8888;

        try
        {
            Console.Write("Что запросить (time / date)? ");
            string request = Console.ReadLine()?.Trim().ToLower();
            if (request != "time" && request != "date")
            {
                Console.WriteLine("Неверный запрос. Введите 'time' или 'date'.");
                return;
            }

            // Создаём и подключаем сокет
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);
            clientSocket.Connect(serverEndPoint);
            Console.WriteLine($"Подключено к серверу {serverIP}:{serverPort}");

            // Отправляем запрос
            byte[] requestData = Encoding.UTF8.GetBytes(request);
            clientSocket.Send(requestData);

            // Получаем ответ
            byte[] buffer = new byte[1024];
            int bytesRead = clientSocket.Receive(buffer);
            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"Ответ сервера: {response}");

            // Закрываем сокет
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка клиента: " + ex.Message);
        }

        Console.WriteLine("Нажмите Enter для выхода...");
        Console.ReadLine();
    }
}