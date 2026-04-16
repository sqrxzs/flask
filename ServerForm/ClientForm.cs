using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

class ClientForm : Form
{
    private TextBox txtIP;
    private TextBox txtPort;
    private ComboBox cmbRequest;
    private Button btnSend;
    private Label lblResult;
    private Label lblStatus;

    public ClientForm()
    {
        this.Text = "Клиент запроса времени/даты";
        this.Size = new System.Drawing.Size(400, 250);

        Label lblIP = new Label { Text = "IP сервера:", Location = new System.Drawing.Point(10, 10), AutoSize = true };
        txtIP = new TextBox { Text = "127.0.0.1", Location = new System.Drawing.Point(120, 8), Width = 150 };
        Label lblPort = new Label { Text = "Порт:", Location = new System.Drawing.Point(10, 40), AutoSize = true };
        txtPort = new TextBox { Text = "8888", Location = new System.Drawing.Point(120, 38), Width = 80 };
        cmbRequest = new ComboBox { Items = { "time", "date" }, DropDownStyle = ComboBoxStyle.DropDownList, Location = new System.Drawing.Point(120, 70), Width = 80 };
        cmbRequest.SelectedIndex = 0;
        btnSend = new Button { Text = "Запросить", Location = new System.Drawing.Point(220, 68), Size = new System.Drawing.Size(100, 25) };
        lblResult = new Label { Text = "Результат: ", Location = new System.Drawing.Point(10, 110), AutoSize = true };
        lblStatus = new Label { Text = "", Location = new System.Drawing.Point(10, 140), AutoSize = true };

        btnSend.Click += BtnSend_Click;

        Controls.Add(lblIP); Controls.Add(txtIP);
        Controls.Add(lblPort); Controls.Add(txtPort);
        Controls.Add(cmbRequest); Controls.Add(btnSend);
        Controls.Add(lblResult); Controls.Add(lblStatus);
    }

    private void BtnSend_Click(object sender, EventArgs e)
    {
        try
        {
            string ip = txtIP.Text;
            int port = int.Parse(txtPort.Text);
            string request = cmbRequest.SelectedItem.ToString();

            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.Connect(new IPEndPoint(IPAddress.Parse(ip), port));
            client.Send(Encoding.UTF8.GetBytes(request));

            byte[] buffer = new byte[1024];
            int bytes = client.Receive(buffer);
            string response = Encoding.UTF8.GetString(buffer, 0, bytes);
            lblResult.Text = $"Результат: {response}";
            lblStatus.Text = "Соединение закрыто.";

            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }
        catch (Exception ex)
        {
            lblStatus.Text = $"Ошибка: {ex.Message}";
        }
    }

    [STAThread]
    static void Main() => Application.Run(new ClientForm());
}