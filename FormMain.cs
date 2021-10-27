using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Threading;

namespace WindowsFormsApp1
{
    public partial class FormMain : Form
    {
        int ErrorCount;
        DateTime TimeStart;

        const string START_CAPTION = "Iniciar";
        const string STOP_CAPTION = "Parar";

        CancellationTokenSource cts;

        public FormMain()
        {
            InitializeComponent();
        }

        private async Task CheckPingAsync(string ip, CancellationToken ct)
        {
            var timeout = 15000; // 12 seconds

            while (true)
            {
                if (ct.IsCancellationRequested) break;
                await PingAsync(ip, timeout, (t, m, e) =>
                {
                    AddListItem(t, m, e);
                });
                await Task.Delay(10000, ct);
            }
        }

        private async Task PingAsync(string ip, int timeOut, Action<string, string, bool> onResult)
        {
            var buffer = new byte[32];
            var options = new PingOptions(64, true);

            using var ping = new Ping();
            try
            {
                var reply = await ping.SendPingAsync(ip, timeOut, buffer, options);
                if (reply.Status == IPStatus.Success)
                    onResult($"Ping {ip}", $"OK: {reply.Address}", false);
                else
                    onResult($"Ping {ip}", $"PING ERRO: {reply.Status}", true);
            }
            catch (PingException ex)
            {
                var errorMsg = ex.InnerException?.Message ?? ex.Message;
                onResult($"Ping {ip}", $"PING ERRO: {errorMsg}", true);
            }
            catch (Exception ex)
            {
                var errorMsg = ex.InnerException?.Message ?? ex.Message;
                onResult($"Ping {ip}", $"ERRO: {ex.Message}", true);
            }
        }

        private async void buttonStart_ClickAsync(object sender, EventArgs e)
        {
            try
            {
                if (buttonStart.Text == STOP_CAPTION)
                {
                    cts.Cancel();
                    buttonStart.Text = START_CAPTION;
                }
                else
                {
                    using (cts = new CancellationTokenSource())
                    {
                        var ct = cts.Token;
                        TimeStart = DateTime.Now;
                        ErrorCount = 0;
                        buttonStart.Text = STOP_CAPTION;
                        label1.Text = $"INICIADO EM {TimeStart:HH:mm:ss} | ERROS ENCONTRADO {ErrorCount}";
                        await CheckPingAsync(textBoxIp.Text, ct);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                AddListItem("Parado", "Operação Cancelada", false);
            }
            catch (Exception ex)
            {
                AddListItem("Erro Fatal", ex.Message, true);
            }
        }

        public void AddListItem(string status, string result, bool error)
        {
            if (!InvokeRequired)
            {
                string[] row = { DateTime.Now.ToString("HH:mm:ss"), status, result };
                var listViewItem = new ListViewItem(row);
                listView1.Items.Add(listViewItem);
                listViewItem.EnsureVisible();
                listViewItem.ForeColor = error ? Color.Red : Color.LightGreen;
                if (error) ErrorCount++;
                label1.Text = $"INICIADO EM {TimeStart:HH:mm:ss} | ERROS ENCONTRADO {ErrorCount}";
            }
            else
            {
                Invoke(new Action<string, string, bool>(AddListItem), status, result, error);
            }
        }
    }
}
