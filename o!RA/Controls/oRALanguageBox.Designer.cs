namespace o_RA.Controls
{
    partial class LanguageBox
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // LanguageBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "LanguageBox";
            this.Size = new System.Drawing.Size(155, 100);
            this.Click += new System.EventHandler(this.LanguageBox_Click);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LanguageBox_MouseDown);
            this.MouseEnter += new System.EventHandler(this.LanguageBox_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.LanguageBox_MouseLeave);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.LanguageBox_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
