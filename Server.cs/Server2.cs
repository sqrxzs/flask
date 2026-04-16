using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class TimeDateServer
{
    static void Main()
    {
        int port = 8888;
        IPAddress localAddr = IPAddress.Any;
        Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            listener.Bind(new IPEndPoint(localAddr, port));
            listener.Listen(10);
            Console.WriteLine($"Сервер запущен на порту {port}. Ожидание запросов...");

            while (true)
            {
                Socket clientSocket = listener.Accept();
                IPEndPoint clientEP = (IPEndPoint)clientSocket.RemoteEndPoint;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Подключился клиент {clientEP.Address}");

                // Буфер для запроса (максимум 256 байт)
                byte[] buffer = new byte[256];
                int bytesRead = clientSocket.Receive(buffer);
                string request = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim().ToLower();

                string response = "";
                if (request == "time")
                {
                    response = DateTime.Now.ToString("HH:mm:ss");
                }
                else if (request == "date")
                {
                    response = DateTime.Now.ToString("yyyy-MM-dd");
                }
                else
                {
                    response = "Ошибка: неизвестный запрос. Используйте 'time' или 'date'.";
                }

                byte[] responseData = Encoding.UTF8.GetBytes(response);
                clientSocket.Send(responseData);
                Console.WriteLine($"Ответ отправлен: {response}");

                // Закрываем соединение после ответа
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
                Console.WriteLine("Соединение закрыто.\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка сервера: " + ex.Message);
        }
        finally
        {
            listener.Close();
        }
    }
}