namespace CodeBox.Test
{
    partial class MainForm
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
            this.ed = new CodeBox.Editor();
            this.SuspendLayout();
            // 
            // ed
            // 
            this.ed.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.ed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ed.Font = new System.Drawing.Font("Consolas", 11F);
            this.ed.Location = new System.Drawing.Point(0, 0);
            this.ed.Margin = new System.Windows.Forms.Padding(0);
            this.ed.Name = "ed";
            this.ed.Overtype = false;
            this.ed.Size = new System.Drawing.Size(1108, 602);
            this.ed.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1108, 602);
            this.Controls.Add(this.ed);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.Activated += new System.EventHandler(this.Form1_Activated);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.ResumeLayout(false);

        }

        #endregion

        private Editor ed;
    }
}

