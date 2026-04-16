using System.Collections.Generic;

namespace QuoteServer
{
    public static class AuthService
    {
        // Хранилище логинов и паролей (можно добавлять через интерфейс сервера)
        private static Dictionary<string, string> users = new Dictionary<string, string>
        {
            { "admin", "12345" },
            { "user1", "qwerty" },
            { "guest", "guest" }
        };

        public static bool Authenticate(string login, string password)
        {
            if (users.ContainsKey(login) && users[login] == password)
                return true;
            return false;
        }

        // Метод для добавления новых пользователей (через админку)
        public static void AddUser(string login, string password)
        {
            if (!users.ContainsKey(login))
                users.Add(login, password);
        }
    }
}