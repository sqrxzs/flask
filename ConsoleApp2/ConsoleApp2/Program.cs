using System;
using System.Threading;

namespace CurrencyServer
{
    class Program
    {
        static void Main(string[] args)
        {
            int port = 8888;
            ServerLogic server = new ServerLogic(port);
            server.Start();
            Console.WriteLine($"Сервер запущен на порту {port}. Нажмите Enter для остановки...");
            Console.ReadLine();
            server.Stop();
        }
    }
}