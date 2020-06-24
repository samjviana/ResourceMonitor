using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Server
{
    public partial class MainForm : Form
    {
        private Server server;
        private SettingsManager settingsManager;
        private DiscoveryForm discoveryForm;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            server = new Server(Convert.ToInt32(this.portField.Value), this);
            discoveryForm = new DiscoveryForm(server.NetworkInterfaces);
            server.Start();

            Thread progressThread = new Thread(new ThreadStart(ProgressUpdate));
            progressThread.Start();

            startButton.Enabled = false;

            string configFileName = Path.ChangeExtension(Application.ExecutablePath, ".config");

            settingsManager = new SettingsManager();
            if (File.Exists(configFileName))
            {
                string configData = File.ReadAllText(configFileName);
                Dictionary<string, object> configs = JsonConvert.DeserializeObject<Dictionary<string, object>>(configData);

                foreach (var config in configs)
                {
                    if (config.Key == "DiscoveryForm")
                    {
                        Dictionary<string, object> configDict = ((JObject)config.Value).ToObject<Dictionary<string, object>>();
                        foreach (var discoveryFormCfg in configDict)
                        {
                            Boolean controlState = Boolean.Parse(discoveryFormCfg.Value.ToString());
                            if(controlState)
                            {
                                discoveryForm = new DiscoveryForm(server.NetworkInterfaces, configDict);
                            }
                        }
                    }
                }

                if (!ParseSettings(settingsManager.Load(configData)))
                {
                    this.serverOutput.AppendText("Erro ao carregar configurações" + Environment.NewLine);
                }
            }
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            server.Start();
            this.runningLabel.Text = "Servidor em execução...";
            startButton.Enabled = false;
            stopButton.Enabled = true;
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            server.Stop();
            this.runningLabel.Text = "Servidor parado...";
            startButton.Enabled = true;
            stopButton.Enabled = false;
        }

        private void mainForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized && minimizeToTrayCheckBox.Checked)
            {
                Hide();
                trayIcon.Visible = true;
            }
        }

        public string serverOutputText
        {
            get
            {
                return this.serverOutput.Text;
            }
            set
            {
                try
                {
                    this.serverOutput.Invoke((Action)delegate
                    {
                        this.serverOutput.AppendText(value + Environment.NewLine);
                        this.serverOutput.SelectionStart = this.serverOutput.Text.Length;
                        this.serverOutput.ScrollToCaret();
                    });
                }
                catch (Exception ex)
                {
                    ReportError(ex.Message);
                }
            }
        }

        private void trayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (closeToTrayCheckBox.Checked)
            {
                e.Cancel = true;
                Hide();
                trayIcon.Visible = true;
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            server.Stop();
            settingsManager.Save(this, discoveryForm);
        }

        private void startWithWindowsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (startWithWindowsCheckBox.Checked)
            {
                StartupManager.AddToStartup();
            }
            else
            {
                StartupManager.RemoveFromStartup();
            }
        }

        private Boolean ParseSettings(Dictionary<string, object> settings)
        {
            foreach (var setting in settings)
            {
                if (setting.Value.GetType() == typeof(JObject))
                {
                    if (!ParseSettings(JsonConvert.DeserializeObject<Dictionary<string, object>>(setting.Value.ToString())))
                    {
                        return false;
                    }
                }
                else
                {
                    ParseControls(setting, this.Controls);
                }
            }
            return true;
        }

        private void ParseControls(KeyValuePair<string, object> valuePair, Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                if (control.GetType() == typeof(CheckBox) && control.Name == valuePair.Key)
                {
                    Boolean value = Convert.ToBoolean((string)valuePair.Value);
                    ((CheckBox)controls[valuePair.Key]).Checked = value;
                }
                else if (control.GetType() == typeof(NumericUpDown) && control.Name == valuePair.Key)
                {
                    Decimal value = Convert.ToDecimal(valuePair.Value);
                    ((NumericUpDown)controls[valuePair.Key]).Value = value;
                }

                if (control.HasChildren)
                {
                    ParseControls(valuePair, control.Controls);
                }
            }
        }

        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            server.Stop();
            settingsManager.Save(this, discoveryForm);
            Dispose();
            Close();
        }

        private void expandMenuItem_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private static void ReportError(string Message)
        {
            StackFrame CallStack = new StackFrame(1, true);
            Console.Write("Error: " + Message + ", File: " + CallStack.GetFileName() + ", Line: " + CallStack.GetFileLineNumber());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            discoveryForm.ShowDialog();

            return;

            string discoveryData = server.GetDiscoveryData();
            if (discoveryData == null)
            {
                MessageBox.Show("A descoberta de rede ainda não foi finalizada");
            }
            else
            {
                try
                {
                    File.WriteAllText("Discovery.json", discoveryData);
                }
                catch
                {

                }
            }
        }

        private void ProgressUpdate()
        {
            while (!server.DiscoveryDone && server.IsRunning)
            {
                try
                {
                    this.textBox1.Invoke((Action)delegate
                    {
                        this.textBox1.Text = server.NetworkProgress;
                    });
                    this.textBox2.Invoke((Action)delegate
                    {
                        this.textBox2.Text = server.IpProgress;
                    });
                }
                catch (Exception ex)
                {
                    ReportError(ex.Message);
                }
                Thread.Sleep(1000);
            }
        }

        private bool discoveryStarted = false;
        private void button2_Click(object sender, EventArgs e)
        {
            if (!server.DiscoveryDone && server.IsRunning && !discoveryStarted)
            {
                discoveryStarted = true;
                server.StartDiscovery(discoveryForm.InterfacesToDiscover);
                button2.Text = "Get Discovery";
            }

            if (server.DiscoveryDone && server.IsRunning && discoveryStarted)
            {
                discoveryStarted = false;
                File.WriteAllText("TRASH.json", server.GetDiscoveryData());
                button2.Text = "Start Discovery";
            }
        }
    }
}
