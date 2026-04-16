using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

class ServerForm : Form
{
    private Button btnStart;
    private Button btnStop;
    private ListBox listBoxLog;
    private Label lblStatus;
    private Socket listener;
    private bool isRunning;

    public ServerForm()
    {
        this.Text = "Сервер времени/даты";
        this.Size = new System.Drawing.Size(400, 300);

        btnStart = new Button { Text = "Запустить", Location = new System.Drawing.Point(10, 10), Size = new System.Drawing.Size(100, 30) };
        btnStop = new Button { Text = "Остановить", Location = new System.Drawing.Point(120, 10), Size = new System.Drawing.Size(100, 30), Enabled = false };
        lblStatus = new Label { Text = "Статус: остановлен", Location = new System.Drawing.Point(10, 50), AutoSize = true };
        listBoxLog = new ListBox { Location = new System.Drawing.Point(10, 80), Size = new System.Drawing.Size(360, 150) };

        btnStart.Click += BtnStart_Click;
        btnStop.Click += BtnStop_Click;

        Controls.Add(btnStart);
        Controls.Add(btnStop);
        Controls.Add(lblStatus);
        Controls.Add(listBoxLog);
    }

    private void BtnStart_Click(object sender, EventArgs e)
    {
        isRunning = true;
        btnStart.Enabled = false;
        btnStop.Enabled = true;
        lblStatus.Text = "Статус: запущен (порт 8888)";
        Thread serverThread = new Thread(RunServer);
        serverThread.IsBackground = true;
        serverThread.Start();
    }

    private void BtnStop_Click(object sender, EventArgs e)
    {
        isRunning = false;
        listener?.Close();
        btnStart.Enabled = true;
        btnStop.Enabled = false;
        lblStatus.Text = "Статус: остановлен";
    }

    private void RunServer()
    {
        int port = 8888;
        IPAddress localAddr = IPAddress.Any;
        listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(new IPEndPoint(localAddr, port));
        listener.Listen(10);
        AppendLog($"Сервер запущен на порту {port}");

        while (isRunning)
        {
            try
            {
                Socket client = listener.Accept();
                IPEndPoint clientEP = (IPEndPoint)client.RemoteEndPoint;
                AppendLog($"Подключился {clientEP.Address}");

                byte[] buffer = new byte[256];
                int bytes = client.Receive(buffer);
                string request = Encoding.UTF8.GetString(buffer, 0, bytes).Trim().ToLower();

                string response = "";
                if (request == "time") response = DateTime.Now.ToString("HH:mm:ss");
                else if (request == "date") response = DateTime.Now.ToString("yyyy-MM-dd");
                else response = "Ошибка: используйте 'time' или 'date'";

                client.Send(Encoding.UTF8.GetBytes(response));
                AppendLog($"Ответ: {response}");

                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
            catch { }
        }
        listener.Close();
    }

    private void AppendLog(string text)
    {
        if (listBoxLog.InvokeRequired)
            listBoxLog.Invoke(new Action(() => listBoxLog.Items.Add($"[{DateTime.Now:HH:mm:ss}] {text}")));
        else
            listBoxLog.Items.Add($"[{DateTime.Now:HH:mm:ss}] {text}");
    }

    [STAThread]
    static void Main() => Application.Run(new ServerForm());
}