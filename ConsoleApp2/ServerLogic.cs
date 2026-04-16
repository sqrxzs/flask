using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace CurrencyServer
{
    public class ServerLogic
    {
        private TcpListener listener;
        private bool isRunning;
        private List<Thread> clientThreads = new List<object>();
        private readonly object lockObj = new object();

        public ServerLogic(int port)
        {
            listener = new TcpListener(IPAddress.Any, port);
        }

        public void Start()
        {
            isRunning = true;
            listener.Start();
            Thread acceptThread = new Thread(AcceptClients);
            acceptThread.Start();
        }

        private void AcceptClients()
        {
            while (isRunning)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Thread clientThread = new Thread(() => HandleClient(client));
                    clientThread.Start();
                    lock (lockObj)
                    {
                        clientThreads.Add(clientThread);
                    }
                }
                catch { }
            }
        }

        private void HandleClient(TcpClient client)
        {
            string clientEndPoint = client.Client.RemoteEndPoint.ToString();
            ConnectionLogger.Log($"Подключился {clientEndPoint} в {DateTime.Now:HH:mm:ss}");

            try
            {
                using (client)
                using (NetworkStream stream = client.GetStream())
                using (System.IO.StreamReader reader = new System.IO.StreamReader(stream, System.Text.Encoding.UTF8))
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(stream, System.Text.Encoding.UTF8) { AutoFlush = true })
                {
                    while (true)
                    {
                        string request = reader.ReadLine();
                        if (request == null) break;

                        if (request.Trim().ToLower() == "exit")
                        {
                            writer.WriteLine("exit");
                            break;
                        }

                        string[] parts = request.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 2)
                        {
                            string from = parts[0].ToUpper();
                            string to = parts[1].ToUpper();
                            double rate = CurrencyService.GetCrossRate(from, to);
                            if (rate >= 0)
                            {
                                string response = $"{from} -> {to} = {rate:F4}";
                                writer.WriteLine(response);
                                ConnectionLogger.Log($"{clientEndPoint} запросил {request} -> {response}");
                            }
                            else
                            {
                                writer.WriteLine("Ошибка: неизвестная валюта или пара не поддерживается");
                                ConnectionLogger.Log($"{clientEndPoint} ошибка запроса {request}");
                            }
                        }
                        else
                        {
                            writer.WriteLine("Ошибка: введите две валюты через пробел (например USD EUR)");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка клиента {clientEndPoint}: {ex.Message}");
            }
            finally
            {
                ConnectionLogger.Log($"Отключился {clientEndPoint} в {DateTime.Now:HH:mm:ss}");
                lock (lockObj)
                {
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
                {
                    thread.Join(500);
                }
            }
        }
    }
}