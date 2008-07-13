//
// MonoRunner.Designer.cs: Display form for user to select project to
//                         test in Mono
//
// Authors:
//  Ed Ropple <ed@edropple.com>
//
// Copyright (C) 2008 Edward Ropple III (http://www.edropple.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

namespace CloverleafShared.Remote.AppTest
{
    partial class RemoteAppSelector
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RemoteAppSelector));
            this.label1 = new System.Windows.Forms.Label();
            this.lstLaunchItems = new System.Windows.Forms.ListBox();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.lstAvailableHosts = new System.Windows.Forms.ListBox();
            this.lblHostBoxFluff = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtHostName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.cboLocalIPs = new System.Windows.Forms.ComboBox();
            this.zeroconfBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.cmdRecheck = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(82, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(650, 68);
            this.label1.TabIndex = 0;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // lstLaunchItems
            // 
            this.lstLaunchItems.FormattingEnabled = true;
            this.lstLaunchItems.Location = new System.Drawing.Point(12, 406);
            this.lstLaunchItems.Name = "lstLaunchItems";
            this.lstLaunchItems.Size = new System.Drawing.Size(720, 108);
            this.lstLaunchItems.TabIndex = 2;
            this.lstLaunchItems.SelectedIndexChanged += new System.EventHandler(this.lstLaunchItems_SelectedIndexChanged);
            // 
            // cmdCancel
            // 
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(657, 520);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(75, 23);
            this.cmdCancel.TabIndex = 3;
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // cmdOK
            // 
            this.cmdOK.Enabled = false;
            this.cmdOK.Location = new System.Drawing.Point(576, 520);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(75, 23);
            this.cmdOK.TabIndex = 4;
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(12, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(64, 64);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(12, 80);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(720, 55);
            this.label2.TabIndex = 5;
            this.label2.Text = resources.GetString("label2.Text");
            // 
            // lstAvailableHosts
            // 
            this.lstAvailableHosts.Enabled = false;
            this.lstAvailableHosts.FormattingEnabled = true;
            this.lstAvailableHosts.Location = new System.Drawing.Point(12, 160);
            this.lstAvailableHosts.Name = "lstAvailableHosts";
            this.lstAvailableHosts.Size = new System.Drawing.Size(720, 147);
            this.lstAvailableHosts.TabIndex = 6;
            this.lstAvailableHosts.SelectedIndexChanged += new System.EventHandler(this.lstAvailableHosts_SelectedIndexChanged);
            // 
            // lblHostBoxFluff
            // 
            this.lblHostBoxFluff.AutoSize = true;
            this.lblHostBoxFluff.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHostBoxFluff.Location = new System.Drawing.Point(12, 144);
            this.lblHostBoxFluff.Name = "lblHostBoxFluff";
            this.lblHostBoxFluff.Size = new System.Drawing.Size(148, 13);
            this.lblHostBoxFluff.TabIndex = 7;
            this.lblHostBoxFluff.Text = "Searching for Services...";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 316);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(32, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Host:";
            this.label4.Click += new System.EventHandler(this.label4_Click);
            // 
            // txtHostName
            // 
            this.txtHostName.Location = new System.Drawing.Point(50, 313);
            this.txtHostName.Name = "txtHostName";
            this.txtHostName.Size = new System.Drawing.Size(150, 20);
            this.txtHostName.TabIndex = 9;
            this.txtHostName.TextChanged += new System.EventHandler(this.formtextboxes_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(12, 387);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(116, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Application to Run:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 338);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(135, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Remote System Username:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 359);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(133, 13);
            this.label7.TabIndex = 14;
            this.label7.Text = "Remote System Password:";
            // 
            // txtUsername
            // 
            this.txtUsername.Location = new System.Drawing.Point(153, 335);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(150, 20);
            this.txtUsername.TabIndex = 15;
            this.txtUsername.TextChanged += new System.EventHandler(this.formtextboxes_TextChanged);
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(153, 356);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '#';
            this.txtPassword.Size = new System.Drawing.Size(150, 20);
            this.txtPassword.TabIndex = 16;
            this.txtPassword.TextChanged += new System.EventHandler(this.formtextboxes_TextChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(360, 313);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(251, 13);
            this.label8.TabIndex = 18;
            this.label8.Text = "Local IP Address On Remote Host Network";
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(360, 326);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(372, 37);
            this.label9.TabIndex = 19;
            this.label9.Text = "Please select an IP address on the same network as the remote host (such\r\nas a vi" +
                "rtual network, in the case of a remote host on a virtual network). The\r\ndefault " +
                "should be correct in most cases.";
            // 
            // cboLocalIPs
            // 
            this.cboLocalIPs.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLocalIPs.FormattingEnabled = true;
            this.cboLocalIPs.Location = new System.Drawing.Point(363, 366);
            this.cboLocalIPs.Name = "cboLocalIPs";
            this.cboLocalIPs.Size = new System.Drawing.Size(369, 21);
            this.cboLocalIPs.TabIndex = 17;
            // 
            // zeroconfBackgroundWorker
            // 
            this.zeroconfBackgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.zeroconfBackgroundWorker_DoWork);
            this.zeroconfBackgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.zeroconfBackgroundWorker_RunWorkerCompleted);
            // 
            // cmdRecheck
            // 
            this.cmdRecheck.Enabled = false;
            this.cmdRecheck.Location = new System.Drawing.Point(657, 131);
            this.cmdRecheck.Name = "cmdRecheck";
            this.cmdRecheck.Size = new System.Drawing.Size(75, 23);
            this.cmdRecheck.TabIndex = 20;
            this.cmdRecheck.Text = "Recheck";
            this.cmdRecheck.UseVisualStyleBackColor = true;
            this.cmdRecheck.Click += new System.EventHandler(this.cmdRecheck_Click);
            // 
            // RemoteAppSelector
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(744, 555);
            this.ControlBox = false;
            this.Controls.Add(this.cmdRecheck);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.cboLocalIPs);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.txtUsername);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtHostName);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lblHostBoxFluff);
            this.Controls.Add(this.lstAvailableHosts);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmdOK);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.lstLaunchItems);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "RemoteAppSelector";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Cloverleaf: Remote Application Test";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ListBox lstLaunchItems;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox lstAvailableHosts;
        private System.Windows.Forms.Label lblHostBoxFluff;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtHostName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox cboLocalIPs;
        private System.ComponentModel.BackgroundWorker zeroconfBackgroundWorker;
        private System.Windows.Forms.Button cmdRecheck;
    }
}