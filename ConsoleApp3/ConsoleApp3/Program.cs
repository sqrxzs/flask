using System;

namespace QuoteServer
{
    class Program
    {
        static void Main(string[] args)
        {
            int port = 8888;
            ServerLogic server = new ServerLogic(port);
            server.Start();
            Console.WriteLine($"Сервер цитат запущен на порту {port}. Нажмите Enter для остановки...");
            Console.ReadLine();
            server.Stop();
        }
    }
}