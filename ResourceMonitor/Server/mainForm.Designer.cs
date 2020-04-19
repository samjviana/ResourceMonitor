namespace Server
{
    partial class MainForm
    {
        /// <summary>
        /// Variável de designer necessária.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpar os recursos que estão sendo usados.
        /// </summary>
        /// <param name="disposing">true se for necessário descartar os recursos gerenciados; caso contrário, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código gerado pelo Windows Form Designer

        /// <summary>
        /// Método necessário para suporte ao Designer - não modifique 
        /// o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.startButton = new System.Windows.Forms.Button();
            this.stopButton = new System.Windows.Forms.Button();
            this.runningLabel = new System.Windows.Forms.Label();
            this.optionsGroupBox = new System.Windows.Forms.GroupBox();
            this.portField = new System.Windows.Forms.NumericUpDown();
            this.ipHostnameLabel = new System.Windows.Forms.Label();
            this.startWithWindowsCheckBox = new System.Windows.Forms.CheckBox();
            this.closeToTrayCheckBox = new System.Windows.Forms.CheckBox();
            this.minimizeToTrayCheckBox = new System.Windows.Forms.CheckBox();
            this.trayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.trayMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.expandMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.serverOutput = new System.Windows.Forms.TextBox();
            this.optionsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.portField)).BeginInit();
            this.trayMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(12, 12);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(75, 23);
            this.startButton.TabIndex = 0;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // stopButton
            // 
            this.stopButton.Location = new System.Drawing.Point(105, 12);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(75, 23);
            this.stopButton.TabIndex = 1;
            this.stopButton.Text = "Stop";
            this.stopButton.UseVisualStyleBackColor = true;
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // runningLabel
            // 
            this.runningLabel.AutoSize = true;
            this.runningLabel.Font = new System.Drawing.Font("DejaVu Sans Mono", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.runningLabel.Location = new System.Drawing.Point(9, 43);
            this.runningLabel.Name = "runningLabel";
            this.runningLabel.Size = new System.Drawing.Size(168, 14);
            this.runningLabel.TabIndex = 2;
            this.runningLabel.Text = "Servidor em execução...";
            // 
            // optionsGroupBox
            // 
            this.optionsGroupBox.Controls.Add(this.portField);
            this.optionsGroupBox.Controls.Add(this.ipHostnameLabel);
            this.optionsGroupBox.Controls.Add(this.startWithWindowsCheckBox);
            this.optionsGroupBox.Controls.Add(this.closeToTrayCheckBox);
            this.optionsGroupBox.Controls.Add(this.minimizeToTrayCheckBox);
            this.optionsGroupBox.Location = new System.Drawing.Point(12, 63);
            this.optionsGroupBox.Name = "optionsGroupBox";
            this.optionsGroupBox.Size = new System.Drawing.Size(168, 286);
            this.optionsGroupBox.TabIndex = 3;
            this.optionsGroupBox.TabStop = false;
            this.optionsGroupBox.Text = "Opções";
            // 
            // portField
            // 
            this.portField.Location = new System.Drawing.Point(50, 19);
            this.portField.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.portField.Minimum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.portField.Name = "portField";
            this.portField.Size = new System.Drawing.Size(112, 20);
            this.portField.TabIndex = 4;
            this.portField.Value = new decimal(new int[] {
            8084,
            0,
            0,
            0});
            // 
            // ipHostnameLabel
            // 
            this.ipHostnameLabel.AutoSize = true;
            this.ipHostnameLabel.Location = new System.Drawing.Point(6, 22);
            this.ipHostnameLabel.Name = "ipHostnameLabel";
            this.ipHostnameLabel.Size = new System.Drawing.Size(38, 13);
            this.ipHostnameLabel.TabIndex = 3;
            this.ipHostnameLabel.Text = "Porta: ";
            // 
            // startWithWindowsCheckBox
            // 
            this.startWithWindowsCheckBox.AutoSize = true;
            this.startWithWindowsCheckBox.Location = new System.Drawing.Point(6, 45);
            this.startWithWindowsCheckBox.Name = "startWithWindowsCheckBox";
            this.startWithWindowsCheckBox.Size = new System.Drawing.Size(133, 17);
            this.startWithWindowsCheckBox.TabIndex = 2;
            this.startWithWindowsCheckBox.Text = "Iniciar com o Windows";
            this.startWithWindowsCheckBox.UseVisualStyleBackColor = true;
            this.startWithWindowsCheckBox.CheckedChanged += new System.EventHandler(this.startWithWindowsCheckBox_CheckedChanged);
            // 
            // closeToTrayCheckBox
            // 
            this.closeToTrayCheckBox.AutoSize = true;
            this.closeToTrayCheckBox.Location = new System.Drawing.Point(6, 68);
            this.closeToTrayCheckBox.Name = "closeToTrayCheckBox";
            this.closeToTrayCheckBox.Size = new System.Drawing.Size(124, 17);
            this.closeToTrayCheckBox.TabIndex = 1;
            this.closeToTrayCheckBox.Text = "Fechar para bandeja";
            this.closeToTrayCheckBox.UseVisualStyleBackColor = true;
            // 
            // minimizeToTrayCheckBox
            // 
            this.minimizeToTrayCheckBox.AutoSize = true;
            this.minimizeToTrayCheckBox.Location = new System.Drawing.Point(6, 91);
            this.minimizeToTrayCheckBox.Name = "minimizeToTrayCheckBox";
            this.minimizeToTrayCheckBox.Size = new System.Drawing.Size(134, 17);
            this.minimizeToTrayCheckBox.TabIndex = 0;
            this.minimizeToTrayCheckBox.Text = "Minimizar para bandeja";
            this.minimizeToTrayCheckBox.UseVisualStyleBackColor = true;
            // 
            // trayIcon
            // 
            this.trayIcon.ContextMenuStrip = this.trayMenu;
            this.trayIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("trayIcon.Icon")));
            this.trayIcon.Text = "Resource Monitor";
            this.trayIcon.Visible = true;
            this.trayIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.trayIcon_MouseDoubleClick);
            // 
            // trayMenu
            // 
            this.trayMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.expandMenuItem,
            this.toolStripSeparator1,
            this.exitMenuItem});
            this.trayMenu.Name = "trayMenu";
            this.trayMenu.Size = new System.Drawing.Size(121, 54);
            // 
            // expandMenuItem
            // 
            this.expandMenuItem.Name = "expandMenuItem";
            this.expandMenuItem.Size = new System.Drawing.Size(120, 22);
            this.expandMenuItem.Text = "Expandir";
            this.expandMenuItem.Click += new System.EventHandler(this.expandMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(117, 6);
            // 
            // exitMenuItem
            // 
            this.exitMenuItem.Name = "exitMenuItem";
            this.exitMenuItem.Size = new System.Drawing.Size(120, 22);
            this.exitMenuItem.Text = "Sair";
            this.exitMenuItem.Click += new System.EventHandler(this.exitMenuItem_Click);
            // 
            // serverOutput
            // 
            this.serverOutput.BackColor = System.Drawing.Color.Black;
            this.serverOutput.Font = new System.Drawing.Font("DejaVu Sans Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.serverOutput.ForeColor = System.Drawing.Color.White;
            this.serverOutput.Location = new System.Drawing.Point(186, 12);
            this.serverOutput.Multiline = true;
            this.serverOutput.Name = "serverOutput";
            this.serverOutput.ReadOnly = true;
            this.serverOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.serverOutput.Size = new System.Drawing.Size(286, 337);
            this.serverOutput.TabIndex = 4;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 361);
            this.Controls.Add(this.serverOutput);
            this.Controls.Add(this.optionsGroupBox);
            this.Controls.Add(this.runningLabel);
            this.Controls.Add(this.stopButton);
            this.Controls.Add(this.startButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Resource Monitor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Resize += new System.EventHandler(this.mainForm_Resize);
            this.optionsGroupBox.ResumeLayout(false);
            this.optionsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.portField)).EndInit();
            this.trayMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.Label runningLabel;
        private System.Windows.Forms.GroupBox optionsGroupBox;
        private System.Windows.Forms.CheckBox startWithWindowsCheckBox;
        private System.Windows.Forms.CheckBox closeToTrayCheckBox;
        private System.Windows.Forms.CheckBox minimizeToTrayCheckBox;
        private System.Windows.Forms.Label ipHostnameLabel;
        private System.Windows.Forms.NumericUpDown portField;
        private System.Windows.Forms.NotifyIcon trayIcon;
        private System.Windows.Forms.TextBox serverOutput;
        private System.Windows.Forms.ContextMenuStrip trayMenu;
        private System.Windows.Forms.ToolStripMenuItem expandMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitMenuItem;
    }
}

