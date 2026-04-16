using System.Collections.Generic;

namespace CurrencyServer
{
    public static class CurrencyService
    {
        // База: курс к RUB (можно расширять)
        private static readonly Dictionary<string, double> RatesToRub = new Dictionary<string, double>
        {
            { "USD", 75.0 },
            { "EUR", 88.5 },
            { "GBP", 103.2 },
            { "JPY", 0.68 }
        };

        public static double GetCrossRate(string from, string to)
        {
            if (!RatesToRub.ContainsKey(from) || !RatesToRub.ContainsKey(to))
                return -1;

            double rateFromRub = RatesToRub[from];
            double rateToRub = RatesToRub[to];
            // 1 from = ? to
            return rateFromRub / rateToRub;
        }
    }
}