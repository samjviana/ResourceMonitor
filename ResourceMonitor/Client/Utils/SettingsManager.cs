using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using Client;

namespace Utils
{
    class SettingsManager
    {
        private Dictionary<string, object> settings;

        public SettingsManager()
        {
            this.settings = new Dictionary<string, object>();
        }

        public void Save(MainForm mainForm)
        {
            this.settings = new Dictionary<string, object>();
            foreach (Control control in mainForm.Controls)
            {
                if (control.GetType() == typeof(CheckBox))
                {
                    this.settings.Add(control.Name, ((CheckBox)control).Checked.ToString());
                }
                else if (control.GetType() == typeof(NumericUpDown))
                {
                    this.settings.Add(control.Name, ((NumericUpDown)control).Value);
                }

                if (control.HasChildren)
                {
                    this.settings.Add(control.Name + ".Controls", recurseControl(control.Controls));
                }
            }

            Dictionary<string, object> config = new Dictionary<string, object>();
            config.Add(mainForm.Name, this.settings);

            string configData = JsonConvert.SerializeObject(config, Formatting.Indented);

            File.WriteAllText(Path.ChangeExtension(Application.ExecutablePath, ".config"), configData);
        }

        private Dictionary<string, object> recurseControl(Control.ControlCollection controls)
        {
            Dictionary<string, object> recurseSettings = new Dictionary<string, object>();

            foreach (Control control in controls)
            {
                if (!string.IsNullOrEmpty(control.Name))
                {
                    if (control.GetType() == typeof(CheckBox))
                    {
                        recurseSettings.Add(control.Name, ((CheckBox)control).Checked.ToString());
                    }
                    else if (control.GetType() == typeof(NumericUpDown))
                    {
                        recurseSettings.Add(control.Name, ((NumericUpDown)control).Value);
                    }

                    if (control.HasChildren)
                    {
                        recurseSettings.Add(control.Name + ".Controls", recurseControl(control.Controls));
                    }
                }
            }

            return recurseSettings;
        }

        public Dictionary<string, object> Load(string json)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        }
    }
}
