namespace CloverleafShared.Remote.AppTest
{
    partial class RemoteStdOutDisplay
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
            this.label1 = new System.Windows.Forms.Label();
            this.txtStdOut = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtStdErr = new System.Windows.Forms.TextBox();
            this.cmdSaveStdOut = new System.Windows.Forms.Button();
            this.cmdSaveStdErr = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(63, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(606, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Below are the standard error and standard output streams from the remote applicat" +
                "ion you just terminated.";
            // 
            // txtStdOut
            // 
            this.txtStdOut.BackColor = System.Drawing.Color.White;
            this.txtStdOut.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtStdOut.Location = new System.Drawing.Point(12, 61);
            this.txtStdOut.Multiline = true;
            this.txtStdOut.Name = "txtStdOut";
            this.txtStdOut.ReadOnly = true;
            this.txtStdOut.Size = new System.Drawing.Size(718, 203);
            this.txtStdOut.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(85, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Standard Output";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 283);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(75, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Standard Error";
            // 
            // txtStdErr
            // 
            this.txtStdErr.BackColor = System.Drawing.Color.White;
            this.txtStdErr.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtStdErr.Location = new System.Drawing.Point(12, 299);
            this.txtStdErr.Multiline = true;
            this.txtStdErr.Name = "txtStdErr";
            this.txtStdErr.ReadOnly = true;
            this.txtStdErr.Size = new System.Drawing.Size(718, 203);
            this.txtStdErr.TabIndex = 4;
            // 
            // cmdSaveStdOut
            // 
            this.cmdSaveStdOut.Location = new System.Drawing.Point(655, 270);
            this.cmdSaveStdOut.Name = "cmdSaveStdOut";
            this.cmdSaveStdOut.Size = new System.Drawing.Size(75, 23);
            this.cmdSaveStdOut.TabIndex = 6;
            this.cmdSaveStdOut.Text = "Save...";
            this.cmdSaveStdOut.UseVisualStyleBackColor = true;
            this.cmdSaveStdOut.Click += new System.EventHandler(this.cmdSaveStdOut_Click);
            // 
            // cmdSaveStdErr
            // 
            this.cmdSaveStdErr.Location = new System.Drawing.Point(655, 508);
            this.cmdSaveStdErr.Name = "cmdSaveStdErr";
            this.cmdSaveStdErr.Size = new System.Drawing.Size(75, 23);
            this.cmdSaveStdErr.TabIndex = 7;
            this.cmdSaveStdErr.Text = "Save...";
            this.cmdSaveStdErr.UseVisualStyleBackColor = true;
            this.cmdSaveStdErr.Click += new System.EventHandler(this.cmdSaveStdErr_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(529, 538);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(201, 23);
            this.button1.TabIndex = 8;
            this.button1.Text = "Close Cloverleaf";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // RemoteStdOutDisplay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(742, 573);
            this.ControlBox = false;
            this.Controls.Add(this.button1);
            this.Controls.Add(this.cmdSaveStdErr);
            this.Controls.Add(this.cmdSaveStdOut);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtStdErr);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtStdOut);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RemoteStdOutDisplay";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Remote Application\'s stdOut and stdErr";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtStdOut;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtStdErr;
        private System.Windows.Forms.Button cmdSaveStdOut;
        private System.Windows.Forms.Button cmdSaveStdErr;
        private System.Windows.Forms.Button button1;
    }
}