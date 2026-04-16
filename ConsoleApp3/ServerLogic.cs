using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace QuoteServer
{
    public class ServerLogic
    {
        private TcpListener listener;
        private bool isRunning;
        private List<Thread> clientThreads = new List<Thread>();
        private readonly object lockObj = new object();
        private int maxClients;
        private int currentClients;
        private int maxQuotesPerClient;

        public ServerLogic(int port, int maxClients = 5, int maxQuotesPerClient = 10)
        {
            listener = new TcpListener(IPAddress.Any, port);
            this.maxClients = maxClients;
            this.maxQuotesPerClient = maxQuotesPerClient;
            currentClients = 0;
        }

        public void Start()
        {
            isRunning = true;
            listener.Start();
            Thread acceptThread = new Thread(AcceptClients);
            acceptThread.Start();
            ConnectionLogger.Log("Сервер запущен.");
        }

        private void AcceptClients()
        {
            while (isRunning)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();
                    lock (lockObj)
                    {
                        if (currentClients >= maxClients)
                        {
                            // Отправляем сообщение о перегрузке и закрываем соединение
                            using (NetworkStream tempStream = client.GetStream())
                            using (StreamWriter tempWriter = new StreamWriter(tempStream, Encoding.UTF8) { AutoFlush = true })
                            {
                                tempWriter.WriteLine("ERROR: Сервер перегружен, попробуйте позже.");
                            }
                            client.Close();
                            ConnectionLogger.Log($"Отклонено подключение {client.Client.RemoteEndPoint} - превышено максимальное количество клиентов.");
                            continue;
                        }

                        currentClients++;
                        Thread clientThread = new Thread(() => HandleClient(client));
                        clientThread.Start();
                        lock (lockObj)
                        {
                            clientThreads.Add(clientThread);
                        }
                    }
                }
                catch { }
            }
        }

        private void HandleClient(TcpClient client)
        {
            string clientEndPoint = client.Client.RemoteEndPoint.ToString();
            ConnectionLogger.Log($"Подключился {clientEndPoint} в {DateTime.Now:HH:mm:ss}");

            int quoteCount = 0;
            bool authenticated = false;

            try
            {
                using (client)
                using (NetworkStream stream = client.GetStream())
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
                {
                    // Аутентификация
                    writer.WriteLine("AUTH: Введите логин:");
                    string login = reader.ReadLine();
                    if (login == null) return;
                    writer.WriteLine("AUTH: Введите пароль:");
                    string password = reader.ReadLine();
                    if (password == null) return;

                    if (!AuthService.Authenticate(login, password))
                    {
                        writer.WriteLine("ERROR: Неверный логин или пароль. Соединение закрыто.");
                        ConnectionLogger.Log($"{clientEndPoint} - ошибка аутентификации");
                        return;
                    }

                    authenticated = true;
                    writer.WriteLine("OK: Добро пожаловать! Введите 'quote' для получения цитаты, 'exit' для выхода.");
                    ConnectionLogger.Log($"{clientEndPoint} - аутентифицирован как {login}");

                    while (true)
                    {
                        string request = reader.ReadLine();
                        if (request == null) break;

                        if (request.Trim().ToLower() == "exit")
                        {
                            writer.WriteLine("exit");
                            break;
                        }
                        else if (request.Trim().ToLower() == "quote")
                        {
                            if (quoteCount >= maxQuotesPerClient)
                            {
                                writer.WriteLine("ERROR: Вы превысили лимит цитат. Соединение будет закрыто.");
                                ConnectionLogger.Log($"{clientEndPoint} - превышен лимит цитат ({maxQuotesPerClient}), отключение");
                                break;
                            }
                            string quote = QuoteService.GetRandomQuote();
                            writer.WriteLine($"CITATA: {quote}");
                            quoteCount++;
                            ConnectionLogger.Log($"{clientEndPoint} - выдана цитата: {quote}");
                        }
                        else
                        {
                            writer.WriteLine("Неизвестная команда. Введите 'quote' или 'exit'.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ConnectionLogger.Log($"Ошибка клиента {clientEndPoint}: {ex.Message}");
            }
            finally
            {
                if (authenticated)
                    ConnectionLogger.Log($"Отключился {clientEndPoint} в {DateTime.Now:HH:mm:ss}, получено цитат: {quoteCount}");
                lock (lockObj)
                {
                    currentClients--;
                    clientThreads.Remove(Thread.CurrentThread);
                }
            }
        }

        public void Stop()
        {
            isRunning = false;
            listener.Stop();
            lock (lockObj)
            {
                foreach (var thread in clientThreads)
                    thread.Join(500);
            }
            ConnectionLogger.Log("Сервер остановлен.");
        }
    }
}