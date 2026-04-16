using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MovieApp
{
    public class MovieForm : Form
    {
        private TextBox txtMovie;
        private Button btnSearch;
        private RichTextBox txtResult;

        public MovieForm()
        {
            this.Text = "Информация о фильме";
            this.Size = new System.Drawing.Size(600, 500);
            txtMovie = new TextBox { Location = new System.Drawing.Point(10, 10), Width = 200 };
            btnSearch = new Button { Text = "Найти фильм", Location = new System.Drawing.Point(220, 8), Size = new System.Drawing.Size(100, 25) };
            txtResult = new RichTextBox { Location = new System.Drawing.Point(10, 50), Size = new System.Drawing.Size(560, 400), ReadOnly = true };
            btnSearch.Click += async (s, e) => await SearchMovieAsync();
            Controls.Add(txtMovie);
            Controls.Add(btnSearch);
            Controls.Add(txtResult);
        }

        private async Task SearchMovieAsync()
        {
            string title = txtMovie.Text.Trim();
            if (string.IsNullOrEmpty(title))
            {
                txtResult.Text = "Введите название фильма";
                return;
            }

            string apiKey = "YOUR_OMDB_API_KEY"; // получите на omdbapi.com
            string url = $"http://www.omdbapi.com/?apikey={apiKey}&t={Uri.EscapeDataString(title)}&plot=full&r=json";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string json = await client.GetStringAsync(url);
                    using JsonDocument doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("Response", out var response) && response.GetString() == "False")
                    {
                        txtResult.Text = "Фильм не найден";
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
                }
            }
        }
    }
}