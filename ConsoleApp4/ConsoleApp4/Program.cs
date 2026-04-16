// Server.cs
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ChatServer server = new ChatServer(8888);
            server.Start();
            Console.WriteLine("Сервер запущен. Нажмите Enter для остановки.");
            Console.ReadLine();
            server.Stop();
        }
    }

    public class ChatServer
    {
        private TcpListener listener;
        private bool running;
        private List<ClientHandler> clients = new List<ClientHandler>();
        private object clientsLock = new object();
        private HashSet<string> bannedWords = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            "дурак", "идиот", "глупый", "плохой" // примеры
        };
        private Dictionary<string, Room> rooms = new Dictionary<string, Room>();
        private readonly object roomsLock = new object();

        public ChatServer(int port)
        {
            listener = new TcpListener(IPAddress.Any, port);
            // Создаём общую комнату "Главная"
            rooms["Главная"] = new Room("Главная");
        }

        public void Start()
        {
            running = true;
            listener.Start();
            _ = Task.Run(AcceptClientsAsync);
        }

        public void Stop()
        {
            running = false;
            listener.Stop();
            lock (clientsLock)
            {
                foreach (var client in clients)
                    client.Disconnect();
            }
        }

        private async Task AcceptClientsAsync()
        {
            while (running)
            {
                try
                {
                    TcpClient tcpClient = await listener.AcceptTcpClientAsync();
                    ClientHandler handler = new ClientHandler(tcpClient, this);
                    lock (clientsLock)
                        clients.Add(handler);
                    _ = handler.HandleAsync();
                }
                catch { }
            }
        }

        public void RemoveClient(ClientHandler client)
        {
            lock (clientsLock)
                clients.Remove(client);
            // Оповещаем всех о выходе пользователя
            BroadcastToRoom("Главная", $"Пользователь {client.Login} покинул чат.", null);
        }

        public bool Authenticate(string login, string password)
        {
            // Проверка по файлу users.txt (логин:пароль)
            if (!File.Exists("users.txt"))
                File.WriteAllText("users.txt", "admin:admin123\nuser1:pass1\n");
            var lines = File.ReadAllLines("users.txt");
            foreach (var line in lines)
            {
                var parts = line.Split(':');
                if (parts[0] == login && parts[1] == password)
                    return true;
            }
            return false;
        }

        public bool Register(string login, string password)
        {
            lock (clientsLock)
            {
                if (clients.Any(c => c.Login == login))
                    return false;
            }
            File.AppendAllText("users.txt", $"{login}:{password}\n");
            return true;
        }

        public void BroadcastToRoom(string roomName, string message, ClientHandler sender)
        {
            lock (clientsLock)
            {
                var targets = clients.Where(c => c.CurrentRoom == roomName && c != sender);
                foreach (var client in targets)
                    client.SendMessage($"MSG|{sender?.Login ?? "Сервер"}|{message}");
            }
        }

        public void SendPrivateMessage(string fromLogin, string toLogin, string text)
        {
            lock (clientsLock)
            {
                var target = clients.FirstOrDefault(c => c.Login == toLogin);
                target?.SendMessage($"PRIVATE|{fromLogin}|{text}");
            }
        }

        public bool CreateRoom(string roomName, ClientHandler creator)
        {
            lock (roomsLock)
            {
                if (rooms.ContainsKey(roomName))
                    return false;
                rooms[roomName] = new Room(roomName);
                // Добавляем создателя в комнату
                creator.JoinRoom(roomName);
                return true;
            }
        }

        public bool JoinRoom(string roomName, ClientHandler client)
        {
            lock (roomsLock)
            {
                if (!rooms.ContainsKey(roomName))
                    return false;
                client.JoinRoom(roomName);
                return true;
            }
        }

        public List<string> GetUsersInRoom(string roomName)
        {
            lock (clientsLock)
            {
                return clients.Where(c => c.CurrentRoom == roomName).Select(c => c.Login).ToList();
            }
        }

        public void KickUser(string adminLogin, string targetLogin)
        {
            lock (clientsLock)
            {
                var target = clients.FirstOrDefault(c => c.Login == targetLogin);
                if (target != null)
                {
                    target.SendMessage("KICKED");
                    target.Disconnect();
                }
            }
        }

        public void BanUser(string adminLogin, string targetLogin, int minutes)
        {
            // Простая реализация: запись в banned.txt
            File.AppendAllText("banned.txt", $"{targetLogin}|{DateTime.Now.AddMinutes(minutes):yyyy-MM-dd HH:mm:ss}\n");
            KickUser(adminLogin, targetLogin);
        }

        public bool IsBanned(string login)
        {
            if (!File.Exists("banned.txt")) return false;
            var lines = File.ReadAllLines("banned.txt");
            foreach (var line in lines)
            {
                var parts = line.Split('|');
                if (parts[0] == login)
                {
                    if (DateTime.TryParse(parts[1], out DateTime until) && until > DateTime.Now)
                        return true;
                }
            }
            return false;
        }

        public bool IsBadWord(string text)
        {
            foreach (var word in bannedWords)
                if (text.Contains(word, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            return false;
        }

        public void AddBadWord(string word)
        {
            bannedWords.Add(word);
        }

        public List<string> GetAllUsers()
        {
            lock (clientsLock)
                return clients.Select(c => c.Login).ToList();
        }
    }

    public class Room
    {
        public string Name { get; set; }
        public Room(string name) => Name = name;
    }

    public class ClientHandler
    {
        private TcpClient tcpClient;
        private StreamReader reader;
        private StreamWriter writer;
        private ChatServer server;
        public string Login { get; private set; }
        public string CurrentRoom { get; private set; }

        public ClientHandler(TcpClient client, ChatServer server)
        {
            tcpClient = client;
            this.server = server;
            var stream = client.GetStream();
            reader = new StreamReader(stream, Encoding.UTF8);
            writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
        }

        public async Task HandleAsync()
        {
            try
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    var parts = line.Split('|');
                    switch (parts[0])
                    {
                        case "LOGIN":
                            Login = parts[1];
                            string pass = parts[2];
                            if (server.IsBanned(Login))
                            {
                                await writer.WriteLineAsync("ERROR|Вы забанены");
                                return;
                            }
                            if (server.Authenticate(Login, pass))
                            {
                                await writer.WriteLineAsync("LOGIN_OK");
                                CurrentRoom = "Главная";
                                server.BroadcastToRoom("Главная", $"Пользователь {Login} вошёл в чат.", this);
                                await SendUserList();
                            }
                            else
                                await writer.WriteLineAsync("ERROR|Неверный логин или пароль");
                            break;

                        case "REG":
                            if (server.Register(parts[1], parts[2]))
                                await writer.WriteLineAsync("REG_OK");
                            else
                                await writer.WriteLineAsync("ERROR|Логин уже существует");
                            break;

                        case "MSG":
                            string msg = parts[1];
                            if (server.IsBadWord(msg))
                            {
                                // Оповестить администраторов (можно в отдельный метод)
                                // Пока просто заменим
                                msg = "###";
                            }
                            server.BroadcastToRoom(CurrentRoom, $"{Login}: {msg}", this);
                            break;

                        case "PRIVATE":
                            string target = parts[1];
                            string privateMsg = parts[2];
                            server.SendPrivateMessage(Login, target, privateMsg);
                            break;

                        case "CREATE_ROOM":
                            string roomName = parts[1];
                            if (server.CreateRoom(roomName, this))
                                await writer.WriteLineAsync($"OK|Комната {roomName} создана");
                            else
                                await writer.WriteLineAsync("ERROR|Комната уже существует");
                            break;

                        case "JOIN_ROOM":
                            string joinRoom = parts[1];
                            if (server.JoinRoom(joinRoom, this))
                            {
                                CurrentRoom = joinRoom;
                                await writer.WriteLineAsync($"OK|Вы вошли в {joinRoom}");
                                await SendUserList();
                            }
                            else
                                await writer.WriteLineAsync("ERROR|Нет такой комнаты");
                            break;

                        case "GET_USERS":
                            await SendUserList();
                            break;

                        case "KICK":
                            if (Login == "admin")
                                server.KickUser(Login, parts[1]);
                            break;

                        case "BAN":
                            if (Login == "admin" && parts.Length == 3)
                                server.BanUser(Login, parts[1], int.Parse(parts[2]));
                            break;

                        case "ADD_BADWORD":
                            if (Login == "admin")
                                server.AddBadWord(parts[1]);
                            break;
                    }
                }
            }
            catch { }
            finally
            {
                server.RemoveClient(this);
                tcpClient.Close();
            }
        }

        private async Task SendUserList()
        {
            var users = server.GetUsersInRoom(CurrentRoom);
            string list = string.Join(",", users);
            await writer.WriteLineAsync($"USER_LIST|{list}");
        }

        public void SendMessage(string message)
        {
            try
            {
                writer.WriteLine(message);
            }
            catch { }
        }

        public void JoinRoom(string roomName)
        {
            CurrentRoom = roomName;
        }

        public void Disconnect()
        {
            try
            {
                tcpClient.Close();
            }
            catch { }
        }
    }
}