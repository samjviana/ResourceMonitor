using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace Server
{
    public partial class MainForm : Form
    {
        private Server server;
        private SettingsManager settingsManager;
        private Logger logger;

        public MainForm()
        {
            InitializeComponent();

            this.logger = new Logger(Path.Combine(Application.StartupPath, "mainFormServer.txt"));

            server = new Server(Convert.ToInt32(this.portField.Value), this);
            server.Start();
            startButton.Enabled = false;

            string configFileName = Path.ChangeExtension(Application.ExecutablePath, ".config");

            settingsManager = new SettingsManager();
            if (File.Exists(configFileName))
            {
                string configData = File.ReadAllText(configFileName);
            
                if(!ParseSettings(settingsManager.Load(configData)))
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
            else
            {
                //trayIcon.Visible = false;
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
                catch
                {

                }
                logger.Log(value);
            }
        }

        private void trayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            //trayIcon.Visible = false;
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
            settingsManager.Save(this);
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
            foreach(var setting in settings)
            {
                if(setting.Value.GetType() == typeof(Newtonsoft.Json.Linq.JObject))
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
            settingsManager.Save(this);
            Dispose();
            Close();
        }

        private void expandMenuItem_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            //trayIcon.Visible = false;
        }
    }
}
