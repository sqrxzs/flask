// ConsoleClient.cs
using System;
using System.Threading.Tasks;
using MessagingLibrary;

class ConsoleClient
{
    static async Task Main(string[] args)
    {
        Console.Write("Введите IP сервера (по умолчанию 127.0.0.1): ");
        string ip = Console.ReadLine();
        if (string.IsNullOrEmpty(ip)) ip = "127.0.0.1";
        Console.Write("Введите тип подписки (Новости, Акции, Техподдержка): ");
        string subType = Console.ReadLine();
        var client = new MessageClient();
        client.OnMessageReceived += (msg) => Console.WriteLine($"Получено: {msg}");
        client.OnError += (err) => Console.WriteLine($"Ошибка: {err}");

        bool ok = await client.ConnectAsync(ip, 8888, subType);
        if (!ok)
        {
            Console.WriteLine("Не удалось подключиться.");
            return;
        }
        Console.WriteLine("Подключено. Ожидание сообщений... Нажмите Enter для выхода.");
        Console.ReadLine();
        client.Disconnect();
    }
}