namespace o_RA
{
    partial class LocaleSelectForm
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
            this.languageBox1 = new o_RA.LanguageBox();
            this.SuspendLayout();
            // 
            // languageBox1
            // 
            this.languageBox1.BackgroundImage = global::o_RA.Properties.Resources.enUSFlag;
            this.languageBox1.Locale = "enUS";
            this.languageBox1.Location = new System.Drawing.Point(171, 87);
            this.languageBox1.Name = "languageBox1";
            this.languageBox1.Size = new System.Drawing.Size(155, 100);
            this.languageBox1.TabIndex = 0;
            // 
            // LocaleSelectForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(497, 295);
            this.Controls.Add(this.languageBox1);
            this.Name = "LocaleSelectForm";
            this.ShowIcon = false;
            this.Text = "Language";
            this.ResumeLayout(false);

        }

        #endregion

        private LanguageBox languageBox1;
    }
}