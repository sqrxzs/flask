using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GutenbergApp
{
    public class HamletForm : Form
    {
        private Button btnLoad;
        private RichTextBox txtContent;

        public HamletForm()
        {
            this.Text = "Гамлет";
            this.Size = new System.Drawing.Size(800, 600);
            btnLoad = new Button { Text = "Загрузить Гамлета", Location = new System.Drawing.Point(10, 10), Size = new System.Drawing.Size(150, 30) };
            txtContent = new RichTextBox { Location = new System.Drawing.Point(10, 50), Size = new System.Drawing.Size(760, 500) };
            btnLoad.Click += async (s, e) => await LoadHamletAsync();
            Controls.Add(btnLoad);
            Controls.Add(txtContent);
        }

        private async Task LoadHamletAsync()
        {
            string url = "https://www.gutenberg.org/files/1524/1524-0.txt";
            using (HttpClient client = new HttpClient())
            {
                string text = await client.GetStringAsync(url);
                txtContent.Text = text;
            }
        }
    }
}