// MessagingLibrary.cs
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MessagingLibrary
{
    // Клиентское подключение на сервере
    public class ClientConnection
    {
        public TcpClient TcpClient { get; set; }
        public StreamReader Reader { get; set; }
        public StreamWriter Writer { get; set; }
        public string SubscriptionType { get; set; } // тип подписки
        public string ClientId { get; set; } // уникальный идентификатор (IP:Port)

        public ClientConnection(TcpClient client)
        {
            TcpClient = client;
            var stream = client.GetStream();
            Reader = new StreamReader(stream, Encoding.UTF8);
            Writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
            ClientId = client.Client.RemoteEndPoint.ToString();
        }
    }

    // Класс сервера (сетевой блок)
    public class MessageServer
    {
        private TcpListener listener;
        private List<ClientConnection> clients = new List<ClientConnection>();
        private object lockObj = new object();
        private bool isRunning;

        public event Action<string> OnLog; // для логирования в UI

        public async Task StartAsync(int port)
        {
            listener = new TcpListener(System.Net.IPAddress.Any, port);
            listener.Start();
            isRunning = true;
            OnLog?.Invoke($"Сервер запущен на порту {port}");
            while (isRunning)
            {
                try
                {
                    var tcpClient = await listener.AcceptTcpClientAsync();
                    _ = Task.Run(() => HandleClientAsync(tcpClient));
                }
                catch { break; }
            }
        }

        private async Task HandleClientAsync(TcpClient tcpClient)
        {
            var client = new ClientConnection(tcpClient);
            lock (lockObj) clients.Add(client);
            OnLog?.Invoke($"Клиент {client.ClientId} подключился");

            try
            {
                // Первая строка – подписка
                string line = await client.Reader.ReadLineAsync();
                if (line != null && line.StartsWith("SUBSCRIBE|"))
                {
                    client.SubscriptionType = line.Substring(10); // после "SUBSCRIBE|"
                    OnLog?.Invoke($"Клиент {client.ClientId} подписался на тип: {client.SubscriptionType}");
                    await client.Writer.WriteLineAsync("OK");
                }
                else
                {
                    await client.Writer.WriteLineAsync("ERROR");
                    return;
                }

                // Дальше просто слушаем, пока соединение не закроется
                while (await client.Reader.ReadLineAsync() != null) { }
            }
            catch { }
            finally
            {
                lock (lockObj) clients.Remove(client);
                OnLog?.Invoke($"Клиент {client.ClientId} отключился");
                tcpClient.Close();
            }
        }

        // Отправить сообщение всем клиентам с указанным типом (или всем, если type=null)
        public void SendToSubscribers(string messageType, string messageText)
        {
            string fullMessage = $"MSG|{messageType}|{messageText}";
            List<ClientConnection> copy;
            lock (lockObj) copy = new List<ClientConnection>(clients);
            foreach (var client in copy)
            {
                if (client.SubscriptionType == messageType)
                {
                    try { client.Writer.WriteLine(fullMessage); }
                    catch { }
                }
            }
        }

        // Экстренное сообщение – всем клиентам без учёта подписки
        public void SendUrgent(string messageText)
        {
            string urgentMessage = $"URGENT|{messageText}";
            List<ClientConnection> copy;
            lock (lockObj) copy = new List<ClientConnection>(clients);
            foreach (var client in copy)
            {
                try { client.Writer.WriteLine(urgentMessage); }
                catch { }
            }
        }

        // Удалить клиента из рассылки (по идентификатору)
        public void KickClient(string clientId)
        {
            ClientConnection target = null;
            lock (lockObj)
                target = clients.Find(c => c.ClientId == clientId);
            if (target != null)
            {
                try { target.Writer.WriteLine("KICK|Администратор удалил вас из рассылки"); target.TcpClient.Close(); }
                catch { }
            }
        }

        public List<string> GetClientsInfo()
        {
            lock (lockObj)
            {
                List<string> info = new List<string>();
                foreach (var c in clients)
                    info.Add($"{c.ClientId} - подписка: {c.SubscriptionType}");
                return info;
            }
        }

        public void Stop()
        {
            isRunning = false;
            listener?.Stop();
        }
    }

    // Класс клиента (сетевой блок)
    public class MessageClient
    {
        private TcpClient tcpClient;
        private StreamReader reader;
        private StreamWriter writer;
        public event Action<string> OnMessageReceived;
        public event Action<string> OnError;

        public async Task<bool> ConnectAsync(string ip, int port, string subscriptionType)
        {
            try
            {
                tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(ip, port);
                var stream = tcpClient.GetStream();
                reader = new StreamReader(stream, Encoding.UTF8);
                writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
                await writer.WriteLineAsync($"SUBSCRIBE|{subscriptionType}");
                string response = await reader.ReadLineAsync();
                if (response == "OK")
                {
                    _ = ReceiveMessagesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex.Message);
                return false;
            }
        }

        private async Task ReceiveMessagesAsync()
        {
            try
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (line.StartsWith("MSG|"))
                    {
                        var parts = line.Split('|');
                        OnMessageReceived?.Invoke($"[{parts[1]}] {parts[2]}");
                    }
                    else if (line.StartsWith("URGENT|"))
                    {
                        var urgentText = line.Substring(7);
                        OnMessageReceived?.Invoke($"⚠️ ЭКСТРЕННОЕ: {urgentText}");
                    }
                    else if (line.StartsWith("KICK|"))
                    {
                        var reason = line.Substring(5);
                        OnMessageReceived?.Invoke($"Вы отключены от сервера. Причина: {reason}");
                        break;
                    }
                }
            }
            catch { }
            finally
            {
                tcpClient?.Close();
            }
        }

        public void Disconnect()
        {
            tcpClient?.Close();
        }
    }
}