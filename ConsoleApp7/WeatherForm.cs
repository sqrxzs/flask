using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WeatherApp
{
    public class WeatherForm : Form
    {
        private TextBox txtCity;
        private Button btnGetWeather;
        private RichTextBox txtResult;

        public WeatherForm()
        {
            this.Text = "Погода сегодня";
            this.Size = new System.Drawing.Size(500, 400);
            txtCity = new TextBox { Location = new System.Drawing.Point(10, 10), Width = 200 };
            btnGetWeather = new Button { Text = "Узнать погоду", Location = new System.Drawing.Point(220, 8), Size = new System.Drawing.Size(100, 25) };
            txtResult = new RichTextBox { Location = new System.Drawing.Point(10, 50), Size = new System.Drawing.Size(460, 300), ReadOnly = true };
            btnGetWeather.Click += async (s, e) => await GetWeatherAsync();
            Controls.Add(txtCity);
            Controls.Add(btnGetWeather);
            Controls.Add(txtResult);
        }

        private async Task GetWeatherAsync()
        {
            string city = txtCity.Text.Trim();
            if (string.IsNullOrEmpty(city))
            {
                txtResult.Text = "Введите название города";
                return;
            }

            string apiKey = "YOUR_OPENWEATHER_API_KEY"; // замените на свой ключ
            string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric&lang=ru";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string json = await client.GetStringAsync(url);
                    using JsonDocument doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    string temp = root.GetProperty("main").GetProperty("temp").GetDouble().ToString("0.0");
                    string feels = root.GetProperty("main").GetProperty("feels_like").GetDouble().ToString("0.0");
                    string description = root.GetProperty("weather")[0].GetProperty("description").GetString();
                    string wind = root.GetProperty("wind").GetProperty("speed").GetDouble().ToString("0.0");
                    string humidity = root.GetProperty("main").GetProperty("humidity").GetInt32().ToString();

                    txtResult.Text = $"Город: {city}\nТемпература: {temp}°C\nОщущается как: {feels}°C\n{description}\nВетер: {wind} м/с\nВлажность: {humidity}%";
                }
                catch (HttpRequestException)
                {
                    txtResult.Text = "Ошибка: город не найден или нет подключения";
                }
            }
        }
    }
}