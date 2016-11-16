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
            this.ed = new CodeBox.Editor();
            this.SuspendLayout();
            // 
            // ed
            // 
            this.ed.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.ed.Font = new System.Drawing.Font("Consolas", 11F);
            this.ed.Location = new System.Drawing.Point(145, 53);
            this.ed.Name = "ed";
            this.ed.Overtype = false;
            this.ed.ReadOnly = false;
            this.ed.Size = new System.Drawing.Size(212, 23);
            this.ed.TabIndex = 0;
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(472, 154);
            this.Controls.Add(this.ed);
            this.Name = "Form2";
            this.Text = "Form2";
            this.Load += new System.EventHandler(this.Form2_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private Editor ed;
    }
}