using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows.Forms;

namespace Server
{
    public partial class DiscoveryForm : Form
    {
        private Dictionary<int, NetworkInterface> networkInterfaces;
        private Dictionary<int, NetworkInterface> interfacesToDiscover;

        public DiscoveryForm(Dictionary<int, NetworkInterface> networkInterfaces)
        {
            this.networkInterfaces = networkInterfaces;
            this.interfacesToDiscover = new Dictionary<int, NetworkInterface>();

            InitializeComponent();
        }

        private void DiscoveryForm_Load(object sender, EventArgs e)
        {
            foreach(var networkInterface in networkInterfaces)
            {
                textBox1.AppendText(string.Format("[{0}]-{1}{2}", networkInterface.Key, networkInterface.Value.Description, Environment.NewLine));
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            interfacesToDiscover.Add(0, networkInterfaces[0]);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            interfacesToDiscover.Add(1, networkInterfaces[1]);
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            interfacesToDiscover.Add(2, networkInterfaces[2]);
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            interfacesToDiscover.Add(3, networkInterfaces[3]);
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            interfacesToDiscover.Add(4, networkInterfaces[4]);
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            interfacesToDiscover.Add(5, networkInterfaces[5]);
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            interfacesToDiscover.Add(6, networkInterfaces[6]);
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            interfacesToDiscover.Add(7, networkInterfaces[7]);
        }

        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {
            interfacesToDiscover.Add(8, networkInterfaces[8]);
        }

        public Dictionary<int, NetworkInterface> InterfacesToDiscover
        {
            get { return this.interfacesToDiscover; }
            set { }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}
