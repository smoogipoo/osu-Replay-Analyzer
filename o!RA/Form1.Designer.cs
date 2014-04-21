namespace o_RA
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.ReplaySelectPanel = new System.Windows.Forms.Panel();
            this.Progress = new System.Windows.Forms.ProgressBar();
            this.ReplaysList = new System.Windows.Forms.TreeView();
            this.MainContainer = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.TWChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.MapInfoTB = new System.Windows.Forms.RichTextBox();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.ReplayInfoTB = new System.Windows.Forms.RichTextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.ReplayTimelineLB = new System.Windows.Forms.ListBox();
            this.ChartToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.ReplaySelectPanel.SuspendLayout();
            this.MainContainer.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TWChart)).BeginInit();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ReplaySelectPanel
            // 
            this.ReplaySelectPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.ReplaySelectPanel.BackColor = System.Drawing.SystemColors.Control;
            this.ReplaySelectPanel.Controls.Add(this.Progress);
            this.ReplaySelectPanel.Controls.Add(this.ReplaysList);
            this.ReplaySelectPanel.Location = new System.Drawing.Point(0, 0);
            this.ReplaySelectPanel.Name = "ReplaySelectPanel";
            this.ReplaySelectPanel.Size = new System.Drawing.Size(297, 753);
            this.ReplaySelectPanel.TabIndex = 3;
            // 
            // Progress
            // 
            this.Progress.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.Progress.Location = new System.Drawing.Point(0, 727);
            this.Progress.Name = "Progress";
            this.Progress.Size = new System.Drawing.Size(297, 26);
            this.Progress.TabIndex = 1;
            this.Progress.MouseEnter += new System.EventHandler(this.Progress_MouseEnter);
            this.Progress.MouseLeave += new System.EventHandler(this.Progress_MouseLeave);
            // 
            // ReplaysList
            // 
            this.ReplaysList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ReplaysList.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ReplaysList.Location = new System.Drawing.Point(0, 0);
            this.ReplaysList.Name = "ReplaysList";
            this.ReplaysList.Size = new System.Drawing.Size(297, 727);
            this.ReplaysList.TabIndex = 0;
            this.ReplaysList.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.ReplaysList_AfterSelect);
            // 
            // MainContainer
            // 
            this.MainContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainContainer.Controls.Add(this.tabPage1);
            this.MainContainer.Controls.Add(this.tabPage2);
            this.MainContainer.Controls.Add(this.tabPage3);
            this.MainContainer.Controls.Add(this.tabPage4);
            this.MainContainer.Location = new System.Drawing.Point(303, 0);
            this.MainContainer.Margin = new System.Windows.Forms.Padding(10);
            this.MainContainer.Name = "MainContainer";
            this.MainContainer.SelectedIndex = 0;
            this.MainContainer.Size = new System.Drawing.Size(978, 523);
            this.MainContainer.TabIndex = 5;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.White;
            this.tabPage1.Controls.Add(this.TWChart);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(970, 497);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Timing Windows";
            // 
            // TWChart
            // 
            chartArea1.BackColor = System.Drawing.Color.WhiteSmoke;
            chartArea1.CursorX.IsUserSelectionEnabled = true;
            chartArea1.Name = "ChartArea1";
            this.TWChart.ChartAreas.Add(chartArea1);
            this.TWChart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TWChart.Location = new System.Drawing.Point(3, 3);
            this.TWChart.Name = "TWChart";
            this.TWChart.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.None;
            series1.ChartArea = "ChartArea1";
            series1.Color = System.Drawing.Color.Blue;
            series1.Name = "Data";
            series2.ChartArea = "ChartArea1";
            series2.Color = System.Drawing.Color.Fuchsia;
            series2.Name = "TLSelection";
            this.TWChart.Series.Add(series1);
            this.TWChart.Series.Add(series2);
            this.TWChart.Size = new System.Drawing.Size(964, 491);
            this.TWChart.TabIndex = 9;
            this.TWChart.Text = "chart1";
            this.TWChart.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TWChart_MouseDown);
            this.TWChart.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TWChart_MouseMove);
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Size = new System.Drawing.Size(970, 497);
            this.tabPage2.TabIndex = 4;
            this.tabPage2.Text = "Spinner RPM";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.MapInfoTB);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(970, 497);
            this.tabPage3.TabIndex = 3;
            this.tabPage3.Text = "Map Information";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // MapInfoTB
            // 
            this.MapInfoTB.BackColor = System.Drawing.Color.White;
            this.MapInfoTB.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.MapInfoTB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MapInfoTB.Location = new System.Drawing.Point(0, 0);
            this.MapInfoTB.Margin = new System.Windows.Forms.Padding(10);
            this.MapInfoTB.Name = "MapInfoTB";
            this.MapInfoTB.ReadOnly = true;
            this.MapInfoTB.Size = new System.Drawing.Size(970, 497);
            this.MapInfoTB.TabIndex = 0;
            this.MapInfoTB.Text = "";
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.ReplayInfoTB);
            this.tabPage4.ForeColor = System.Drawing.SystemColors.ControlText;
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.tabPage4.Size = new System.Drawing.Size(970, 497);
            this.tabPage4.TabIndex = 2;
            this.tabPage4.Text = "Replay Information";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // ReplayInfoTB
            // 
            this.ReplayInfoTB.BackColor = System.Drawing.Color.White;
            this.ReplayInfoTB.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ReplayInfoTB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ReplayInfoTB.Location = new System.Drawing.Point(0, 0);
            this.ReplayInfoTB.Name = "ReplayInfoTB";
            this.ReplayInfoTB.ReadOnly = true;
            this.ReplayInfoTB.Size = new System.Drawing.Size(970, 497);
            this.ReplayInfoTB.TabIndex = 0;
            this.ReplayInfoTB.Text = "";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.ReplayTimelineLB);
            this.panel1.Location = new System.Drawing.Point(303, 524);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(978, 228);
            this.panel1.TabIndex = 12;
            // 
            // ReplayTimelineLB
            // 
            this.ReplayTimelineLB.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ReplayTimelineLB.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ReplayTimelineLB.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ReplayTimelineLB.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.ReplayTimelineLB.FormattingEnabled = true;
            this.ReplayTimelineLB.IntegralHeight = false;
            this.ReplayTimelineLB.ItemHeight = 11;
            this.ReplayTimelineLB.Location = new System.Drawing.Point(3, 2);
            this.ReplayTimelineLB.Name = "ReplayTimelineLB";
            this.ReplayTimelineLB.Size = new System.Drawing.Size(583, 224);
            this.ReplayTimelineLB.TabIndex = 0;
            this.ReplayTimelineLB.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.ReplayTimelineLB_DrawItem);
            this.ReplayTimelineLB.SelectedIndexChanged += new System.EventHandler(this.ReplayTimelineLB_SelectedIndexChanged);
            // 
            // ChartToolTip
            // 
            this.ChartToolTip.AutomaticDelay = 0;
            this.ChartToolTip.BackColor = System.Drawing.Color.White;
            this.ChartToolTip.ForeColor = System.Drawing.Color.Black;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1281, 753);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.MainContainer);
            this.Controls.Add(this.ReplaySelectPanel);
            this.Name = "Form1";
            this.Text = "o!RA";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ReplaySelectPanel.ResumeLayout(false);
            this.MainContainer.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.TWChart)).EndInit();
            this.tabPage3.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel ReplaySelectPanel;
        private System.Windows.Forms.TreeView ReplaysList;
        private System.Windows.Forms.ProgressBar Progress;
        private System.Windows.Forms.TabControl MainContainer;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.RichTextBox ReplayInfoTB;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.RichTextBox MapInfoTB;
        private System.Windows.Forms.DataVisualization.Charting.Chart TWChart;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ListBox ReplayTimelineLB;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ToolTip ChartToolTip;
    }
}

