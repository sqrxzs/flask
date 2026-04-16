using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class TcpClient
{
    static void Main()
    {
        // IP-адрес и порт сервера (укажите реальный IP, если сервер на другой машине)
        string serverIP = "127.0.0.1"; // локальный сервер для теста
        int serverPort = 8888;

        try
        {
            // 1. Создаем сокет
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // 2. Подключаемся к серверу
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);
            clientSocket.Connect(serverEndPoint);
            Console.WriteLine($"Подключено к серверу {serverIP}:{serverPort}");

            // 3. Отправляем приветствие
            string message = "Привет, сервер!";
            byte[] msgData = Encoding.UTF8.GetBytes(message);
            clientSocket.Send(msgData);

            // 4. Получаем ответ
            byte[] buffer = new byte[1024];
            int bytesRead = clientSocket.Receive(buffer);
            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            // Выводим с временем и IP-адресом сервера
            IPEndPoint serverRemote = (IPEndPoint)clientSocket.RemoteEndPoint;
            Console.WriteLine($"[{DateTime.Now:HH:mm}] от {serverRemote.Address} получена строка: {response}");

            // 5. Закрываем сокет
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка: " + ex.Message);
        }

        Console.WriteLine("Нажмите любую клавишу для выхода...");
        Console.ReadKey();
    }
}