//
// MonoRunner.Designer.cs: Display form while Cloverleaf runs an app in Mono
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

namespace CloverleafShared.TestInMono
{
    partial class MonoRunner
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
            this.components = new System.ComponentModel.Container();
            this.cmdClose = new System.Windows.Forms.Button();
            this.cmdKill = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.lblProcessStatus = new System.Windows.Forms.Label();
            this.timProcessMonitor = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // cmdClose
            // 
            this.cmdClose.Enabled = false;
            this.cmdClose.Location = new System.Drawing.Point(452, 117);
            this.cmdClose.Name = "cmdClose";
            this.cmdClose.Size = new System.Drawing.Size(75, 23);
            this.cmdClose.TabIndex = 1;
            this.cmdClose.Text = "Close";
            this.cmdClose.UseVisualStyleBackColor = true;
            this.cmdClose.Click += new System.EventHandler(this.cmdClose_Click);
            // 
            // cmdKill
            // 
            this.cmdKill.Enabled = false;
            this.cmdKill.Location = new System.Drawing.Point(371, 117);
            this.cmdKill.Name = "cmdKill";
            this.cmdKill.Size = new System.Drawing.Size(75, 23);
            this.cmdKill.TabIndex = 2;
            this.cmdKill.Text = "Kill";
            this.cmdKill.UseVisualStyleBackColor = true;
            this.cmdKill.Click += new System.EventHandler(this.cmdKill_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(153, 24);
            this.label1.TabIndex = 4;
            this.label1.Text = "Process Status:";
            // 
            // lblProcessStatus
            // 
            this.lblProcessStatus.AutoSize = true;
            this.lblProcessStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProcessStatus.ForeColor = System.Drawing.Color.Teal;
            this.lblProcessStatus.Location = new System.Drawing.Point(171, 9);
            this.lblProcessStatus.Name = "lblProcessStatus";
            this.lblProcessStatus.Size = new System.Drawing.Size(97, 24);
            this.lblProcessStatus.TabIndex = 5;
            this.lblProcessStatus.Text = "Waiting...";
            // 
            // timProcessMonitor
            // 
            this.timProcessMonitor.Tick += new System.EventHandler(this.timProcessMonitor_Tick);
            // 
            // frmMonoTestRunning
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(539, 152);
            this.ControlBox = false;
            this.Controls.Add(this.lblProcessStatus);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmdKill);
            this.Controls.Add(this.cmdClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "frmMonoTestRunning";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Mono Test Runner";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdClose;
        private System.Windows.Forms.Button cmdKill;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblProcessStatus;
        private System.Windows.Forms.Timer timProcessMonitor;
    }
}