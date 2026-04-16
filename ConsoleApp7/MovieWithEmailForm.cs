using System;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MovieApp
{
    public class MovieWithEmailForm : Form
    {
        private TextBox txtMovie;
        private Button btnSearch;
        private RichTextBox txtResult;
        private TextBox txtEmailTo;
        private Button btnSendEmail;
        private string lastMovieJson;

        public MovieWithEmailForm()
        {
            this.Text = "Фильм + отправить по email";
            this.Size = new System.Drawing.Size(700, 600);
            txtMovie = new TextBox { Location = new System.Drawing.Point(10, 10), Width = 200 };
            btnSearch = new Button { Text = "Найти фильм", Location = new System.Drawing.Point(220, 8), Size = new System.Drawing.Size(100, 25) };
            txtResult = new RichTextBox { Location = new System.Drawing.Point(10, 50), Size = new System.Drawing.Size(660, 350), ReadOnly = true };
            Label lblEmail = new Label { Text = "Email получателя:", Location = new System.Drawing.Point(10, 420), AutoSize = true };
            txtEmailTo = new TextBox { Location = new System.Drawing.Point(130, 418), Width = 250 };
            btnSendEmail = new Button { Text = "Отправить результат на почту", Location = new System.Drawing.Point(400, 416), Size = new System.Drawing.Size(150, 30) };
            btnSearch.Click += async (s, e) => await SearchMovieAsync();
            btnSendEmail.Click += async (s, e) => await SendEmailAsync();
            Controls.Add(txtMovie);
            Controls.Add(btnSearch);
            Controls.Add(txtResult);
            Controls.Add(lblEmail);
            Controls.Add(txtEmailTo);
            Controls.Add(btnSendEmail);
        }

        private async Task SearchMovieAsync()
        {
            string title = txtMovie.Text.Trim();
            if (string.IsNullOrEmpty(title))
            {
                txtResult.Text = "Введите название фильма";
                return;
            }

            string apiKey = "YOUR_OMDB_API_KEY";
            string url = $"http://www.omdbapi.com/?apikey={apiKey}&t={Uri.EscapeDataString(title)}&plot=full&r=json";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string json = await client.GetStringAsync(url);
                    lastMovieJson = json;
                    using JsonDocument doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("Response", out var response) && response.GetString() == "False")
                    {
                        txtResult.Text = "Фильм не найден";
                        lastMovieJson = null;
                        return;
                    }
                    string year = root.GetProperty("Year").GetString();
                    string rated = root.GetProperty("Rated").GetString();
                    string released = root.GetProperty("Released").GetString();
                    string runtime = root.GetProperty("Runtime").GetString();
                    string genre = root.GetProperty("Genre").GetString();
                    string director = root.GetProperty("Director").GetString();
                    string actors = root.GetProperty("Actors").GetString();
                    string plot = root.GetProperty("Plot").GetString();
                    string imdbRating = root.GetProperty("imdbRating").GetString();

                    txtResult.Text = $"Название: {title}\nГод: {year}\nРейтинг: {rated}\nДата выхода: {released}\nДлительность: {runtime}\nЖанр: {genre}\nРежиссёр: {director}\nАктёры: {actors}\nIMDB: {imdbRating}\n\nСюжет:\n{plot}";
                }
                catch
                {
                    txtResult.Text = "Ошибка загрузки";
                    lastMovieJson = null;
                }
            }
        }

        private async Task SendEmailAsync()
        {
            if (string.IsNullOrEmpty(lastMovieJson))
            {
                MessageBox.Show("Сначала найдите фильм");
                return;
            }
            string to = txtEmailTo.Text.Trim();
            if (string.IsNullOrEmpty(to))
            {
                MessageBox.Show("Введите email получателя");
                return;
            }

            // Настройки SMTP (пример для Gmail – нужен пароль приложения)
            string from = "your_email@gmail.com";
            string password = "your_app_password";
            string smtpHost = "smtp.gmail.com";
            int smtpPort = 587;

            try
            {
                using (SmtpClient client = new SmtpClient(smtpHost, smtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(from, password);
                    using (MailMessage message = new MailMessage(from, to))
                    {
                        message.Subject = "Результаты поиска фильма";
                        message.Body = txtResult.Text;
                        await client.SendMailAsync(message);
                    }
                }
                MessageBox.Show("Письмо отправлено");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отправки: {ex.Message}");
            }
        }
    }
}