namespace o_RA.Controls
{
    partial class oRATabControl
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
            this.TabContainer = new System.Windows.Forms.Panel();
            this.TabPages = new PageCollection();
            this.SuspendLayout();
            // 
            // TabContainer
            // 
            this.TabContainer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(43)))), ((int)(((byte)(43)))));
            this.TabContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TabContainer.Location = new System.Drawing.Point(0, 0);
            this.TabContainer.Name = "TabContainer";
            this.TabContainer.Size = new System.Drawing.Size(681, 421);
            this.TabContainer.TabIndex = 1;
            // 
            // TabPages
            // 
            this.TabPages.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(26)))), ((int)(((byte)(26)))));
            this.TabPages.Dock = System.Windows.Forms.DockStyle.Right;
            this.TabPages.Location = new System.Drawing.Point(681, 0);
            this.TabPages.Name = "TabPages";
            this.TabPages.Size = new System.Drawing.Size(60, 421);
            this.TabPages.TabContainer = null;
            this.TabPages.TabIndex = 0;
            // 
            // oRATabControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(43)))), ((int)(((byte)(43)))));
            this.Controls.Add(this.TabContainer);
            this.Controls.Add(this.TabPages);
            this.Name = "oRATabControl";
            this.Size = new System.Drawing.Size(741, 421);
            this.ResumeLayout(false);

        }

        #endregion

        public PageCollection TabPages;
        private System.Windows.Forms.Panel TabContainer;

    }
}
