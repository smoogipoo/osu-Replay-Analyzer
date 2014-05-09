namespace SpinnerRPMPlugin
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            this.SRPMChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.ChartToolTip = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.SRPMChart)).BeginInit();
            this.SuspendLayout();
            // 
            // SRPMChart
            // 
            this.SRPMChart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(232)))), ((int)(((byte)(232)))));
            chartArea2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(232)))), ((int)(((byte)(232)))));
            chartArea2.CursorX.IsUserSelectionEnabled = true;
            chartArea2.Name = "ChartArea1";
            this.SRPMChart.ChartAreas.Add(chartArea2);
            this.SRPMChart.Dock = System.Windows.Forms.DockStyle.Fill;
            legend2.Alignment = System.Drawing.StringAlignment.Center;
            legend2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            legend2.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom;
            legend2.Font = new System.Drawing.Font("Segoe UI", 8F);
            legend2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(26)))), ((int)(((byte)(26)))));
            legend2.IsTextAutoFit = false;
            legend2.Name = "Legend1";
            this.SRPMChart.Legends.Add(legend2);
            this.SRPMChart.Location = new System.Drawing.Point(0, 0);
            this.SRPMChart.Name = "SRPMChart";
            this.SRPMChart.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.None;
            this.SRPMChart.Size = new System.Drawing.Size(715, 437);
            this.SRPMChart.TabIndex = 0;
            this.SRPMChart.Text = "chart1";
            this.SRPMChart.MouseClick += new System.Windows.Forms.MouseEventHandler(this.SRPMChart_MouseClick);
            this.SRPMChart.MouseDown += new System.Windows.Forms.MouseEventHandler(this.SRPMChart_MouseDown);
            this.SRPMChart.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SRPMChart_MouseMove);
            // 
            // ChartToolTip
            // 
            this.ChartToolTip.AutomaticDelay = 0;
            this.ChartToolTip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(26)))), ((int)(((byte)(26)))));
            this.ChartToolTip.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(232)))), ((int)(((byte)(232)))));
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.SRPMChart);
            this.Name = "MainForm";
            this.Size = new System.Drawing.Size(715, 437);
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.SRPMChart)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart SRPMChart;
        private System.Windows.Forms.ToolTip ChartToolTip;
    }
}
