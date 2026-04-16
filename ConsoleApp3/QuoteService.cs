using System;
using System.Collections.Generic;

namespace QuoteServer
{
    public static class QuoteService
    {
        private static readonly List<string> Quotes = new List<string>
        {
            "Программирование — это искусство думать.",
            "Простота — залог надёжности.",
            "Лучший код — тот, который не писали.",
            "Документация — это любовь.",
            "Работает — не трогай.",
            "Преждевременная оптимизация — корень всех зол.",
            "Тестирование никогда не заканчивается.",
            "Пользователь всегда прав? Не всегда."
        };

        private static readonly Random rand = new Random();

        public static string GetRandomQuote()
        {
            lock (rand)
            {
                int index = rand.Next(Quotes.Count);
                return Quotes[index];
            }
        }
    }
}