using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class TcpServer
{
    static void Main()
    {
        // IP-адрес и порт сервера (слушаем на всех интерфейсах)
        IPAddress localAddr = IPAddress.Any;
        int port = 8888;

        // 1. Создаем сокет (IPv4, потоковый, TCP)
        Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            // 2. Привязываем к порту
            listener.Bind(new IPEndPoint(localAddr, port));
            // 3. Начинаем прослушивание (максимальная очередь подключений – 10)
            listener.Listen(10);
            Console.WriteLine($"Сервер запущен. Ожидание подключений на порту {port}...");

            while (true)
            {
                // 4. Принимаем клиента (блокирующий вызов)
                Socket clientSocket = listener.Accept();
                // Получаем IP-адрес клиента
                IPEndPoint clientEndPoint = (IPEndPoint)clientSocket.RemoteEndPoint;
                string clientIP = clientEndPoint.Address.ToString();

                // 5. Получаем данные от клиента
                byte[] buffer = new byte[1024];
                int bytesRead = clientSocket.Receive(buffer);
                string receivedMsg = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                // Выводим с временем
                Console.WriteLine($"[{DateTime.Now:HH:mm}] от {clientIP} получена строка: {receivedMsg}");

                // 6. Отправляем ответ
                string reply = "Привет, клиент!";
                byte[] replyData = Encoding.UTF8.GetBytes(reply);
                clientSocket.Send(replyData);

                // 7. Закрываем соединение с этим клиентом
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка: " + ex.Message);
        }
        finally
        {
            listener.Close();
        }
    }
}