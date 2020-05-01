using Newtonsoft.Json;
using OpenHardwareMonitor.Hardware;
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

namespace Client
{
    public partial class MainForm : Form
    {
        private CurlService curlService;
        private SettingsManager settingsManager;
        private Logger logger;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.timer.Start();

            this.logger = new Logger("log.txt");

            this.curlService = new CurlService("http://samjviana.ddns.net:8084/", this);

            this.curlService.Start();

            string configFileName = Path.ChangeExtension(Application.ExecutablePath, ".config");

            settingsManager = new SettingsManager();
            if (File.Exists(configFileName))
            {
                string configData = File.ReadAllText(configFileName);

                if (!ParseSettings(settingsManager.Load(configData)))
                {
                    this.serverOutput.AppendText("Erro ao carregar configurações" + Environment.NewLine);
                    this.logger.Log("Erro ao carregar configurações.");
                }
            }

            ConfigStartup(startWithWindowsCheckBox.Checked);
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
                        this.logger.Log(value);
                    });
                }
                catch
                {
                    Console.WriteLine("Erro ao gerar output!");
                    this.logger.Log("Erro ao gerar output.");
                }
            }
        }

        private void startWithWindowsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ConfigStartup(startWithWindowsCheckBox.Checked);
            try
            {
                settingsManager.Save(this);
            }
            catch
            {
                this.serverOutput.AppendText("Erro ao salvar configurações" + Environment.NewLine);
                this.logger.Log("Erro ao salvar configurações.");
            }

        }

        private void ConfigStartup(Boolean add)
        {
            try
            {
                if (add)
                {
                    StartupManager.AddToStartup();
                }
                else
                {
                    StartupManager.RemoveFromStartup();
                }
            }
            catch
            {

            }
        }

        private Boolean ParseSettings(Dictionary<string, object> settings)
        {
            foreach (var setting in settings)
            {
                if (setting.Value.GetType() == typeof(Newtonsoft.Json.Linq.JObject))
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

                if (control.HasChildren)
                {
                    ParseControls(valuePair, control.Controls);
                }
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (closeToTrayCheckBox.Checked)
            {
                e.Cancel = true;
                Hide();
                trayIcon.Visible = true;
            }
            else
            {
                curlService.Stop();
                try
                {
                    settingsManager.Save(this);
                }
                catch
                {
                    this.serverOutput.AppendText("Erro ao salvar configurações" + Environment.NewLine);
                    this.logger.Log("Erro ao salvar configurações.");
                }
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            curlService.Stop();
            try
            {
                settingsManager.Save(this);
            }
            catch
            {
                this.serverOutput.AppendText("Erro ao salvar configurações" + Environment.NewLine);
                this.logger.Log("Erro ao salvar configurações.");
            }
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            trayIcon.Visible = true;
        }

        private void trayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void MainForm_Resize(object sender, EventArgs e)
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

        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            curlService.Stop();
            try
            {
                settingsManager.Save(this);
            }
            catch
            {
                this.serverOutput.AppendText("Erro ao salvar configurações" + Environment.NewLine);
                this.logger.Log("Erro ao salvar configurações.");
            }
            Dispose();
            Close();
        }

        private void expandMenuItem_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            //trayIcon.Visible = false;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            //this.serverOutput.AppendText("curlService.IsRunning: " + curlService.IsRunning + Environment.NewLine);
            if(!curlService.IsRunning)
            {
                curlService = new CurlService("http://samjviana.ddns.net:8084/", this);

                this.curlService.Start();
            }
        }

        private void closeToTrayCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                settingsManager.Save(this);
            }
            catch
            {
                this.serverOutput.AppendText("Erro ao salvar configurações" + Environment.NewLine);
                this.logger.Log("Erro ao salvar configurações.");
            }
        }

        private void minimizeToTrayCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                settingsManager.Save(this);
            }
            catch
            {
                this.serverOutput.AppendText("Erro ao salvar configurações" + Environment.NewLine);
                this.logger.Log("Erro ao salvar configurações.");
            }
        }
    }
}
