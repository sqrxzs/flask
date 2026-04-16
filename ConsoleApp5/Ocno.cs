// WinFormsClient.cs
using System;
using System.Windows.Forms;
using MessagingLibrary;

namespace WinFormsClient
{
    public partial class ClientForm : Form
    {
        private MessageClient client;
        private TextBox txtIp;
        private ComboBox cmbType;
        private Button btnConnect;
        private ListBox lstMessages;
        private Label lblStatus;

        public ClientForm()
        {
            InitializeComponent();
            client = new MessageClient();
            client.OnMessageReceived += (msg) => AddMessage(msg);
            client.OnError += (err) => AddMessage($"Ошибка: {err}");
        }

        private void InitializeComponent()
        {
            this.Text = "Клиент рассылки";
            this.Size = new System.Drawing.Size(500, 400);
            txtIp = new TextBox { Text = "127.0.0.1", Location = new System.Drawing.Point(10, 10), Width = 150 };
            cmbType = new ComboBox { Items = { "Новости", "Акции", "Техподдержка" }, Location = new System.Drawing.Point(170, 10), Width = 120 };
            btnConnect = new Button { Text = "Подключиться", Location = new System.Drawing.Point(300, 8), Size = new System.Drawing.Size(100, 25) };
            lstMessages = new ListBox { Location = new System.Drawing.Point(10, 50), Size = new System.Drawing.Size(460, 280) };
            lblStatus = new Label { Text = "Статус: не подключён", Location = new System.Drawing.Point(10, 340), AutoSize = true };
            Controls.Add(txtIp); Controls.Add(cmbType); Controls.Add(btnConnect); Controls.Add(lstMessages); Controls.Add(lblStatus);
            btnConnect.Click += BtnConnect_Click;
        }

        private async void BtnConnect_Click(object sender, EventArgs e)
        {
            btnConnect.Enabled = false;
            lblStatus.Text = "Статус: подключаюсь...";
            bool ok = await client.ConnectAsync(txtIp.Text, 8888, cmbType.SelectedItem.ToString());
            if (ok)
                lblStatus.Text = "Статус: подключён, ожидание сообщений";
            else
            {
                lblStatus.Text = "Статус: ошибка подключения";
                btnConnect.Enabled = true;
            }
        }

        private void AddMessage(string msg)
        {
            if (lstMessages.InvokeRequired)
                lstMessages.Invoke(new Action(() => lstMessages.Items.Add(msg)));
            else
                lstMessages.Items.Add(msg);
        }
    }
}