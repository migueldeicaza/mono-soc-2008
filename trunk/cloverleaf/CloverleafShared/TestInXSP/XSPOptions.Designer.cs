namespace CloverleafShared.TestInXSP
{
    partial class XSPOptions
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(XSPOptions));
            this.label1 = new System.Windows.Forms.Label();
            this.lblProjectName = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.nudXSPPort = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.chkHTTPS = new System.Windows.Forms.CheckBox();
            this.cmdGo = new System.Windows.Forms.Button();
            this.chkBrowserAutostart = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nudXSPPort)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(70, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Project:";
            // 
            // lblProjectName
            // 
            this.lblProjectName.AutoSize = true;
            this.lblProjectName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProjectName.Location = new System.Drawing.Point(119, 9);
            this.lblProjectName.Name = "lblProjectName";
            this.lblProjectName.Size = new System.Drawing.Size(0, 13);
            this.lblProjectName.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(12, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(396, 57);
            this.label2.TabIndex = 2;
            this.label2.Text = resources.GetString("label2.Text");
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(49, 87);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(176, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Port that XSP2 should run on:";
            // 
            // nudXSPPort
            // 
            this.nudXSPPort.Location = new System.Drawing.Point(231, 85);
            this.nudXSPPort.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
            this.nudXSPPort.Name = "nudXSPPort";
            this.nudXSPPort.Size = new System.Drawing.Size(120, 20);
            this.nudXSPPort.TabIndex = 4;
            this.nudXSPPort.Value = new decimal(new int[] {
            12021,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(119, 111);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(103, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Support HTTPS?";
            // 
            // chkHTTPS
            // 
            this.chkHTTPS.AutoSize = true;
            this.chkHTTPS.Location = new System.Drawing.Point(231, 111);
            this.chkHTTPS.Name = "chkHTTPS";
            this.chkHTTPS.Size = new System.Drawing.Size(15, 14);
            this.chkHTTPS.TabIndex = 6;
            this.chkHTTPS.UseVisualStyleBackColor = true;
            // 
            // cmdGo
            // 
            this.cmdGo.Location = new System.Drawing.Point(333, 166);
            this.cmdGo.Name = "cmdGo";
            this.cmdGo.Size = new System.Drawing.Size(75, 23);
            this.cmdGo.TabIndex = 7;
            this.cmdGo.Text = "Go";
            this.cmdGo.UseVisualStyleBackColor = true;
            this.cmdGo.Click += new System.EventHandler(this.cmdGo_Click);
            // 
            // chkBrowserAutostart
            // 
            this.chkBrowserAutostart.AutoSize = true;
            this.chkBrowserAutostart.Location = new System.Drawing.Point(231, 133);
            this.chkBrowserAutostart.Name = "chkBrowserAutostart";
            this.chkBrowserAutostart.Size = new System.Drawing.Size(15, 14);
            this.chkBrowserAutostart.TabIndex = 9;
            this.chkBrowserAutostart.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(29, 133);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(193, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Automatically start web browser?";
            // 
            // XSPOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(420, 201);
            this.Controls.Add(this.chkBrowserAutostart);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cmdGo);
            this.Controls.Add(this.chkHTTPS);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.nudXSPPort);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblProjectName);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "XSPOptions";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Test in XSP";
            ((System.ComponentModel.ISupportInitialize)(this.nudXSPPort)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblProjectName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown nudXSPPort;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox chkHTTPS;
        private System.Windows.Forms.Button cmdGo;
        private System.Windows.Forms.CheckBox chkBrowserAutostart;
        private System.Windows.Forms.Label label5;
    }
}