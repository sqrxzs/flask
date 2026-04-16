// ChatClientForm.cs
using System;
using System.Drawing;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatClient
{
    public partial class ChatForm : Form
    {
        private TcpClient client;
        private StreamReader reader;
        private StreamWriter writer;
        private string login;
        private bool isAdmin;

        private TextBox txtLog;
        private TextBox txtInput;
        private Button btnSend;
        private ListBox lstUsers;
        private Button btnCreateRoom;
        private Button btnPrivateMsg;
        private Button btnAdminPanel; // для админа

        public ChatForm()
        {
            InitializeComponent();
            ShowLoginDialog();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(800, 500);
            txtLog = new TextBox { Multiline = true, ScrollBars = ScrollBars.Vertical, ReadOnly = true, Location = new Point(10, 10), Size = new Size(550, 350) };
            lstUsers = new ListBox { Location = new Point(570, 10), Size = new Size(200, 350) };
            txtInput = new TextBox { Location = new Point(10, 370), Width = 550 };
            btnSend = new Button { Text = "Отправить", Location = new Point(570, 368), Size = new Size(100, 30) };
            btnCreateRoom = new Button { Text = "Создать комнату", Location = new Point(10, 410), Size = new Size(120, 30) };
            btnPrivateMsg = new Button { Text = "Личное сообщение", Location = new Point(140, 410), Size = new Size(150, 30) };
            btnAdminPanel = new Button { Text = "Админ панель", Location = new Point(300, 410), Size = new Size(100, 30), Visible = false };
            Controls.Add(txtLog);
            Controls.Add(lstUsers);
            Controls.Add(txtInput);
            Controls.Add(btnSend);
            Controls.Add(btnCreateRoom);
            Controls.Add(btnPrivateMsg);
            Controls.Add(btnAdminPanel);
            btnSend.Click += BtnSend_Click;
            btnCreateRoom.Click += BtnCreateRoom_Click;
            btnPrivateMsg.Click += BtnPrivateMsg_Click;
            btnAdminPanel.Click += BtnAdminPanel_Click;
            this.FormClosing += ChatForm_FormClosing;
        }

        private async void ShowLoginDialog()
        {
            Form loginForm = new Form() { Width = 300, Height = 200, Text = "Вход" };
            Label lblLogin = new Label() { Text = "Логин:", Location = new Point(10, 20) };
            TextBox txtLogin = new TextBox() { Location = new Point(100, 20), Width = 150 };
            Label lblPass = new Label() { Text = "Пароль:", Location = new Point(10, 50) };
            TextBox txtPass = new TextBox() { Location = new Point(100, 50), Width = 150, PasswordChar = '*' };
            Button btnLogin = new Button() { Text = "Войти", Location = new Point(30, 90) };
            Button btnReg = new Button() { Text = "Регистрация", Location = new Point(140, 90) };
            loginForm.Controls.Add(lblLogin);
            loginForm.Controls.Add(txtLogin);
            loginForm.Controls.Add(lblPass);
            loginForm.Controls.Add(txtPass);
            loginForm.Controls.Add(btnLogin);
            loginForm.Controls.Add(btnReg);

            bool done = false;
            btnLogin.Click += async (s, e) =>
            {
                string l = txtLogin.Text;
                string p = txtPass.Text;
                if (await ConnectAndLogin(l, p))
                {
                    login = l;
                    isAdmin = (l == "admin");
                    if (isAdmin) btnAdminPanel.Visible = true;
                    loginForm.Close();
                    done = true;
                }
                else
                    MessageBox.Show("Ошибка входа");
            };
            btnReg.Click += async (s, e) =>
            {
                string l = txtLogin.Text;
                string p = txtPass.Text;
                if (await Register(l, p))
                {
                    MessageBox.Show("Регистрация успешна, теперь войдите");
                }
                else
                    MessageBox.Show("Ошибка регистрации");
            };
            loginForm.ShowDialog();
            if (!done) Environment.Exit(0);
        }

        private async Task<bool> ConnectAndLogin(string login, string pass)
        {
            try
            {
                client = new TcpClient("127.0.0.1", 8888);
                var stream = client.GetStream();
                reader = new StreamReader(stream, Encoding.UTF8);
                writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
                await writer.WriteLineAsync($"LOGIN|{login}|{pass}");
                string response = await reader.ReadLineAsync();
                if (response == "LOGIN_OK")
                {
                    _ = ReceiveMessagesAsync();
                    return true;
                }
                return false;
            }
            catch { return false; }
        }

        private async Task<bool> Register(string login, string pass)
        {
            try
            {
                using (var tempClient = new TcpClient("127.0.0.1", 8888))
                using (var stream = tempClient.GetStream())
                using (var w = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
                using (var r = new StreamReader(stream, Encoding.UTF8))
                {
                    await w.WriteLineAsync($"REG|{login}|{pass}");
                    string resp = await r.ReadLineAsync();
                    return resp == "REG_OK";
                }
            }
            catch { return false; }
        }

        private async Task ReceiveMessagesAsync()
        {
            try
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    var parts = line.Split('|');
                    switch (parts[0])
                    {
                        case "MSG":
                            AppendMessage($"{parts[1]}: {parts[2]}");
                            break;
                        case "PRIVATE":
                            AppendMessage($"[Лично от {parts[1]}]: {parts[2]}", true);
                            break;
                        case "USER_LIST":
                            UpdateUserList(parts[1].Split(',', StringSplitOptions.RemoveEmptyEntries));
                            break;
                        case "USER_JOINED":
                            AppendMessage($"*** {parts[1]} присоединился к комнате ***");
                            break;
                        case "USER_LEFT":
                            AppendMessage($"*** {parts[1]} покинул комнату ***");
                            break;
                        case "KICKED":
                            AppendMessage("Вас кикнули. Приложение закроется.");
                            await Task.Delay(2000);
                            Application.Exit();
                            break;
                        default:
                            AppendMessage(line);
                            break;
                    }
                }
            }
            catch { }
        }

        private void AppendMessage(string msg, bool isPrivate = false)
        {
            if (txtLog.InvokeRequired)
                txtLog.Invoke(new Action(() => txtLog.AppendText(msg + Environment.NewLine)));
            else
                txtLog.AppendText(msg + Environment.NewLine);
        }

        private void UpdateUserList(string[] users)
        {
            if (lstUsers.InvokeRequired)
                lstUsers.Invoke(new Action(() => { lstUsers.Items.Clear(); lstUsers.Items.AddRange(users); }));
            else
            { lstUsers.Items.Clear(); lstUsers.Items.AddRange(users); }
        }

        private async void BtnSend_Click(object sender, EventArgs e)
        {
            string msg = txtInput.Text.Trim();
            if (string.IsNullOrEmpty(msg)) return;
            await writer.WriteLineAsync($"MSG|{msg}");
            txtInput.Clear();
        }

        private async void BtnCreateRoom_Click(object sender, EventArgs e)
        {
            string roomName = Microsoft.VisualBasic.Interaction.InputBox("Название комнаты", "Создание комнаты");
            if (!string.IsNullOrEmpty(roomName))
                await writer.WriteLineAsync($"CREATE_ROOM|{roomName}");
        }

        private async void BtnPrivateMsg_Click(object sender, EventArgs e)
        {
            if (lstUsers.SelectedItem == null)
            {
                MessageBox.Show("Выберите пользователя из списка");
                return;
            }
            string target = lstUsers.SelectedItem.ToString();
            string msg = Microsoft.VisualBasic.Interaction.InputBox("Введите сообщение", "Личное сообщение");
            if (!string.IsNullOrEmpty(msg))
                await writer.WriteLineAsync($"PRIVATE|{target}|{msg}");
        }

        private void BtnAdminPanel_Click(object sender, EventArgs e)
        {
            AdminPanelForm admin = new AdminPanelForm(writer);
            admin.ShowDialog();
        }

        private async void ChatForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try { await writer.WriteLineAsync("MSG|exit"); } catch { }
            client?.Close();
        }
    }

    // Простая форма админ-панели
    public class AdminPanelForm : Form
    {
        private StreamWriter writer;
        private TextBox txtUser, txtMinutes, txtWord;
        public AdminPanelForm(StreamWriter sw)
        {
            writer = sw;
            this.Text = "Админ панель";
            this.Size = new Size(300, 250);
            Label lblUser = new Label() { Text = "Пользователь:", Location = new Point(10, 10) };
            txtUser = new TextBox() { Location = new Point(100, 8), Width = 150 };
            Label lblMinutes = new Label() { Text = "Минуты бана:", Location = new Point(10, 40) };
            txtMinutes = new TextBox() { Location = new Point(100, 38), Width = 150 };
            Button btnBan = new Button() { Text = "Забанить", Location = new Point(10, 70), Width = 100 };
            Button btnKick = new Button() { Text = "Кикнуть", Location = new Point(120, 70), Width = 100 };
            Label lblWord = new Label() { Text = "Запрещённое слово:", Location = new Point(10, 110) };
            txtWord = new TextBox() { Location = new Point(120, 108), Width = 150 };
            Button btnAddWord = new Button() { Text = "Добавить слово", Location = new Point(10, 140), Width = 150 };
            btnBan.Click += async (s, e) => await writer.WriteLineAsync($"BAN|{txtUser.Text}|{txtMinutes.Text}");
            btnKick.Click += async (s, e) => await writer.WriteLineAsync($"KICK|{txtUser.Text}");
            btnAddWord.Click += async (s, e) => await writer.WriteLineAsync($"ADD_BADWORD|{txtWord.Text}");
            Controls.Add(lblUser); Controls.Add(txtUser);
            Controls.Add(lblMinutes); Controls.Add(txtMinutes);
            Controls.Add(btnBan); Controls.Add(btnKick);
            Controls.Add(lblWord); Controls.Add(txtWord);
            Controls.Add(btnAddWord);
        }
    }
}