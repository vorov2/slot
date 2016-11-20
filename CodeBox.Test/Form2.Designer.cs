namespace CodeBox.Test
{
    partial class Form2
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
            this.cmd = new CodeBox.Test.ControlPanel();
            this.ed = new CodeBox.Editor();
            this.SuspendLayout();
            // 
            // cmd
            // 
            this.cmd.CommandEditor = null;
            this.cmd.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.cmd.Location = new System.Drawing.Point(0, 703);
            this.cmd.Name = "cmd";
            this.cmd.Size = new System.Drawing.Size(1029, 23);
            this.cmd.TabIndex = 0;
            this.cmd.Text = "controlPanel1";
            // 
            // ed
            // 
            this.ed.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.ed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ed.FirstEditLine = 0;
            this.ed.LastEditLine = 0;
            this.ed.LimitedMode = false;
            this.ed.Location = new System.Drawing.Point(0, 0);
            this.ed.Name = "ed";
            this.ed.Overtype = false;
            this.ed.ReadOnly = false;
            this.ed.Size = new System.Drawing.Size(1029, 703);
            this.ed.TabIndex = 1;
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1029, 726);
            this.Controls.Add(this.ed);
            this.Controls.Add(this.cmd);
            this.Name = "Form2";
            this.Text = "Form2";
            this.Load += new System.EventHandler(this.Form2_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private ControlPanel cmd;
        private Editor ed;
    }
}