using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace GutenbergApp
{
    public class SearchForm : Form
    {
        private TextBox txtSearch;
        private Button btnSearch;
        private ListBox lstResults;
        private RichTextBox txtContent;
        private Dictionary<string, string> resultLinks = new Dictionary<string, string>();

        public SearchForm()
        {
            this.Text = "Поиск книг";
            this.Size = new System.Drawing.Size(800, 600);
            txtSearch = new TextBox { Location = new System.Drawing.Point(10, 10), Width = 200 };
            btnSearch = new Button { Text = "Искать", Location = new System.Drawing.Point(220, 8), Size = new System.Drawing.Size(80, 25) };
            lstResults = new ListBox { Location = new System.Drawing.Point(10, 40), Size = new System.Drawing.Size(200, 510) };
            txtContent = new RichTextBox { Location = new System.Drawing.Point(220, 40), Size = new System.Drawing.Size(550, 510) };
            btnSearch.Click += async (s, e) => await SearchBooksAsync();
            lstResults.SelectedIndexChanged += async (s, e) => await LoadBookTextAsync();
            Controls.Add(txtSearch);
            Controls.Add(btnSearch);
            Controls.Add(lstResults);
            Controls.Add(txtContent);
        }

        private async Task SearchBooksAsync()
        {
            string query = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(query)) return;
            lstResults.Items.Clear();
            resultLinks.Clear();
            string searchUrl = "https://www.gutenberg.org/ebooks/search/?query=" + HttpUtility.UrlEncode(query);
            using (HttpClient client = new HttpClient())
            {
                string html = await client.GetStringAsync(searchUrl);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                var items = doc.DocumentNode.SelectNodes("//li[@class='booklink']//a");
                if (items != null)
                {
                    foreach (var a in items)
                    {
                        string title = a.InnerText.Trim();
                        string href = a.GetAttributeValue("href", "");
                        if (href.StartsWith("/ebooks/"))
                            href = "https://www.gutenberg.org" + href;
                        resultLinks[title] = href;
                        lstResults.Items.Add(title);
                    }
                }
                else
                    lstResults.Items.Add("Ничего не найдено");
            }
        }

        private async Task LoadBookTextAsync()
        {
            if (lstResults.SelectedItem == null) return;
            string title = lstResults.SelectedItem.ToString();
            if (!resultLinks.TryGetValue(title, out string bookPageUrl)) return;

            using (HttpClient client = new HttpClient())
            {
                string bookPageHtml = await client.GetStringAsync(bookPageUrl);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(bookPageHtml);
                var txtLink = doc.DocumentNode.SelectSingleNode("//a[contains(@href, '.txt')]");
                if (txtLink != null)
                {
                    string txtUrl = txtLink.GetAttributeValue("href", "");
                    if (txtUrl.StartsWith("/"))
                        txtUrl = "https://www.gutenberg.org" + txtUrl;
                    string bookText = await client.GetStringAsync(txtUrl);
                    txtContent.Text = bookText;
                }
            }
        }
    }
}