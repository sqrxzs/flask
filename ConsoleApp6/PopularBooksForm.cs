using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GutenbergApp
{
    public class PopularBooksForm : Form
    {
        private ListBox lstBooks;
        private RichTextBox txtContent;
        private Dictionary<string, string> bookLinks = new Dictionary<string, string>();

        public PopularBooksForm()
        {
            this.Text = "100 популярных книг";
            this.Size = new System.Drawing.Size(800, 600);
            lstBooks = new ListBox { Location = new System.Drawing.Point(10, 10), Size = new System.Drawing.Size(200, 540) };
            txtContent = new RichTextBox { Location = new System.Drawing.Point(220, 10), Size = new System.Drawing.Size(550, 540) };
            lstBooks.SelectedIndexChanged += async (s, e) => await LoadBookTextAsync();
            Controls.Add(lstBooks);
            Controls.Add(txtContent);
            this.Load += async (s, e) => await LoadPopularBooksAsync();
        }

        private async Task LoadPopularBooksAsync()
        {
            string url = "https://www.gutenberg.org/browse/scores/top";
            using (HttpClient client = new HttpClient())
            {
                string html = await client.GetStringAsync(url);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                var links = doc.DocumentNode.SelectNodes("//ol[@class='browse']//li/a");
                if (links != null)
                {
                    foreach (var link in links)
                    {
                        string title = link.InnerText.Trim();
                        string href = link.GetAttributeValue("href", "");
                        if (href.StartsWith("/ebooks/"))
                            href = "https://www.gutenberg.org" + href;
                        bookLinks[title] = href;
                        lstBooks.Items.Add(title);
                        if (lstBooks.Items.Count >= 100) break;
                    }
                }
            }
        }

        private async Task LoadBookTextAsync()
        {
            if (lstBooks.SelectedItem == null) return;
            string title = lstBooks.SelectedItem.ToString();
            if (!bookLinks.TryGetValue(title, out string bookPageUrl)) return;

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
                else
                    txtContent.Text = "Текст книги не найден.";
            }
        }
    }
}