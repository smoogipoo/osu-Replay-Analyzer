namespace ReplayInfoPlugin
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.customListView1 = new ReplayInfoPlugin.CustomListView();
            this.PropertyHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.InformationHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "a";
            this.columnHeader1.Width = 280;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "b";
            this.columnHeader2.Width = 362;
            // 
            // customListView1
            // 
            this.customListView1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(43)))), ((int)(((byte)(43)))));
            this.customListView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.customListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.PropertyHeader,
            this.InformationHeader});
            this.customListView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.customListView1.ForeColor = System.Drawing.Color.Black;
            this.customListView1.FullRowSelect = true;
            this.customListView1.Location = new System.Drawing.Point(0, 0);
            this.customListView1.Name = "customListView1";
            this.customListView1.OwnerDraw = true;
            this.customListView1.Size = new System.Drawing.Size(886, 549);
            this.customListView1.TabIndex = 0;
            this.customListView1.UseCompatibleStateImageBehavior = false;
            this.customListView1.View = System.Windows.Forms.View.Details;
            // 
            // PropertyHeader
            // 
            this.PropertyHeader.Text = "Property";
            this.PropertyHeader.Width = 227;
            // 
            // InformationHeader
            // 
            this.InformationHeader.Text = "Information";
            this.InformationHeader.Width = 659;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(43)))), ((int)(((byte)(43)))));
            this.Controls.Add(this.customListView1);
            this.Name = "MainForm";
            this.Size = new System.Drawing.Size(886, 549);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private CustomListView customListView1;
        private System.Windows.Forms.ColumnHeader PropertyHeader;
        private System.Windows.Forms.ColumnHeader InformationHeader;
    }
}
