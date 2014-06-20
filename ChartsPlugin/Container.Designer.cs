namespace ChartsPlugin
{
    partial class Container
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
            this.ContentPanel = new System.Windows.Forms.Panel();
            this.DisplaySelectCB = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // ContentPanel
            // 
            this.ContentPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ContentPanel.Location = new System.Drawing.Point(0, 38);
            this.ContentPanel.Name = "ContentPanel";
            this.ContentPanel.Size = new System.Drawing.Size(966, 556);
            this.ContentPanel.TabIndex = 0;
            this.ContentPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.ContentPanel_Paint);
            // 
            // DisplaySelectCB
            // 
            this.DisplaySelectCB.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.DisplaySelectCB.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.DisplaySelectCB.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.DisplaySelectCB.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DisplaySelectCB.FormattingEnabled = true;
            this.DisplaySelectCB.Items.AddRange(new object[] {
            "Timing Windows",
            "Spinner RPM",
            "Aim Accuracy"});
            this.DisplaySelectCB.Location = new System.Drawing.Point(383, 8);
            this.DisplaySelectCB.Name = "DisplaySelectCB";
            this.DisplaySelectCB.Size = new System.Drawing.Size(200, 25);
            this.DisplaySelectCB.TabIndex = 1;
            this.DisplaySelectCB.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // Container
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(43)))), ((int)(((byte)(43)))));
            this.Controls.Add(this.DisplaySelectCB);
            this.Controls.Add(this.ContentPanel);
            this.Name = "Container";
            this.Size = new System.Drawing.Size(966, 594);
            this.Load += new System.EventHandler(this.Container_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel ContentPanel;
        private System.Windows.Forms.ComboBox DisplaySelectCB;
    }
}
