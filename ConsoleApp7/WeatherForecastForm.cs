using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WeatherApp
{
    public class WeatherForecastForm : Form
    {
        private TextBox txtCity;
        private Button btnGetForecast;
        private RichTextBox txtResult;

        public WeatherForecastForm()
        {
            this.Text = "Прогноз на 7 дней";
            this.Size = new System.Drawing.Size(600, 500);
            txtCity = new TextBox { Location = new System.Drawing.Point(10, 10), Width = 200 };
            btnGetForecast = new Button { Text = "Прогноз", Location = new System.Drawing.Point(220, 8), Size = new System.Drawing.Size(100, 25) };
            txtResult = new RichTextBox { Location = new System.Drawing.Point(10, 50), Size = new System.Drawing.Size(560, 400), ReadOnly = true };
            btnGetForecast.Click += async (s, e) => await GetForecastAsync();
            Controls.Add(txtCity);
            Controls.Add(btnGetForecast);
            Controls.Add(txtResult);
        }

        private async Task GetForecastAsync()
        {
            string city = txtCity.Text.Trim();
            if (string.IsNullOrEmpty(city))
            {
                txtResult.Text = "Введите название города";
                return;
            }

            string apiKey = "YOUR_OPENWEATHER_API_KEY";
            string url = $"https://api.openweathermap.org/data/2.5/forecast?q={city}&appid={apiKey}&units=metric&lang=ru&cnt=40";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string json = await client.GetStringAsync(url);
                    using JsonDocument doc = JsonDocument.Parse(json);
                    var list = doc.RootElement.GetProperty("list").EnumerateArray();
                    txtResult.Clear();
                    int dayCount = 0;
                    string lastDate = "";
                    foreach (var item in list)
                    {
                        string dtTxt = item.GetProperty("dt_txt").GetString();
                        string date = dtTxt.Substring(0, 10);
                        if (date != lastDate)
                        {
                            if (dayCount >= 7) break;
                            lastDate = date;
                            dayCount++;
                            string temp = item.GetProperty("main").GetProperty("temp").GetDouble().ToString("0.0");
                            string description = item.GetProperty("weather")[0].GetProperty("description").GetString();
                            txtResult.AppendText($"{date}: {temp}°C, {description}\n");
                        }
                    }
                    if (dayCount == 0) txtResult.Text = "Нет данных";
                }
                catch
                {
                    txtResult.Text = "Ошибка загрузки";
                }
            }
        }
    }
}