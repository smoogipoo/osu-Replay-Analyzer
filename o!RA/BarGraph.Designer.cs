namespace o_RA
{
    partial class BarGraph
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
            axisFont.Dispose();
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
            // BarGraph
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Name = "BarGraph";
            this.Size = new System.Drawing.Size(667, 417);
            this.Load += new System.EventHandler(this.BarGraph_Load);
            this.SizeChanged += new System.EventHandler(this.BarGraph_SizeChanged);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.BarGraph_Paint);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
