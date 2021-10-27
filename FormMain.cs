using System;
using System.Drawing;
using System.Net.Http;
using System.Windows.Forms;
using System.Timers;
using Timer = System.Timers.Timer;

namespace WindowsFormsApp1
{
    public partial class FormMain : Form
    {
        private Timer Interval;
        DateTime timeStart;
        int ErrorCount;

        public FormMain()
        {
            InitializeComponent();
            Interval = new Timer();
            Interval.Elapsed += new ElapsedEventHandler(async (o, e) =>
            {
                HttpClient httpClient = new HttpClient();
                try
                {
                    var content = await httpClient.GetStringAsync("https://www.google.com");
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        AddListItem("Acessando google.com", "OK", false);
                    }
                    else throw new Exception("Nenhum conteúdo!");
                }
                catch (Exception ex)
                {
                    AddListItem("Acessando google.com", $"ERRO: {ex.Message}", true);
                }
            });
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            if (Interval.Enabled)
            {
                Interval.Enabled = false;
                buttonStart.Text = "Iniciar testes";
            }
            else
            {
                Interval.Interval = 3000;
                Interval.Enabled = true;
                buttonStart.Text = "Parar testes";
                if (timeStart == DateTime.MinValue) timeStart = DateTime.Now;
                label1.Text = $"INICIADO EM {timeStart:HH:mm:ss} | ERROS ENCONTRADO {ErrorCount}";
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
                label1.Text = $"INICIADO EM {timeStart:HH:mm:ss} | ERROS ENCONTRADO {ErrorCount}";
            }
            else
            {
                Invoke(new Action<string, string, bool>(AddListItem), status, result, error);
            }
        }
    }
}
