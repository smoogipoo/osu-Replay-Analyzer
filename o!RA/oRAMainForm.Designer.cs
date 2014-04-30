namespace o_RA
{
    partial class oRAMainForm
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
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            this.ReplaySelectPanel = new System.Windows.Forms.Panel();
            this.Progress = new System.Windows.Forms.ProgressBar();
            this.ReplaysList = new System.Windows.Forms.TreeView();
            this.MainContainer = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.TWChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.SRPMChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.MapInfoLV = new System.Windows.Forms.ListView();
            this.Property = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Information = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.ReplayInfoLV = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panel1 = new System.Windows.Forms.Panel();
            this.ReplayTimelineLB = new System.Windows.Forms.ListBox();
            this.ChartToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.languageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.preferencesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.PluginsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.ReplaySelectPanel.SuspendLayout();
            this.MainContainer.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TWChart)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SRPMChart)).BeginInit();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.panel1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ReplaySelectPanel
            // 
            this.ReplaySelectPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.ReplaySelectPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(43)))), ((int)(((byte)(43)))));
            this.ReplaySelectPanel.Controls.Add(this.Progress);
            this.ReplaySelectPanel.Controls.Add(this.ReplaysList);
            this.ReplaySelectPanel.Location = new System.Drawing.Point(0, 27);
            this.ReplaySelectPanel.Name = "ReplaySelectPanel";
            this.ReplaySelectPanel.Size = new System.Drawing.Size(297, 726);
            this.ReplaySelectPanel.TabIndex = 3;
            // 
            // Progress
            // 
            this.Progress.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.Progress.Location = new System.Drawing.Point(0, 700);
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
            this.ReplaysList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(43)))), ((int)(((byte)(43)))));
            this.ReplaysList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ReplaysList.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ReplaysList.Location = new System.Drawing.Point(0, 0);
            this.ReplaysList.Name = "ReplaysList";
            this.ReplaysList.Size = new System.Drawing.Size(297, 700);
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
            this.MainContainer.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.MainContainer.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MainContainer.Location = new System.Drawing.Point(303, 27);
            this.MainContainer.Margin = new System.Windows.Forms.Padding(10);
            this.MainContainer.Name = "MainContainer";
            this.MainContainer.SelectedIndex = 0;
            this.MainContainer.Size = new System.Drawing.Size(936, 496);
            this.MainContainer.TabIndex = 5;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.White;
            this.tabPage1.Controls.Add(this.TWChart);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(928, 470);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Timing Windows";
            // 
            // TWChart
            // 
            chartArea1.AxisY.MinorGrid.Enabled = true;
            chartArea1.AxisY.MinorGrid.LineColor = System.Drawing.Color.Gainsboro;
            chartArea1.BackColor = System.Drawing.Color.WhiteSmoke;
            chartArea1.CursorX.IsUserSelectionEnabled = true;
            chartArea1.Name = "ChartArea1";
            this.TWChart.ChartAreas.Add(chartArea1);
            this.TWChart.Dock = System.Windows.Forms.DockStyle.Fill;
            legend1.Alignment = System.Drawing.StringAlignment.Center;
            legend1.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom;
            legend1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            legend1.IsTextAutoFit = false;
            legend1.Name = "Legend1";
            this.TWChart.Legends.Add(legend1);
            this.TWChart.Location = new System.Drawing.Point(3, 3);
            this.TWChart.Name = "TWChart";
            this.TWChart.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.None;
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Timing Window";
            series2.ChartArea = "ChartArea1";
            series2.IsVisibleInLegend = false;
            series2.Legend = "Legend1";
            series2.Name = "Caret";
            series2.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            this.TWChart.Series.Add(series1);
            this.TWChart.Series.Add(series2);
            this.TWChart.Size = new System.Drawing.Size(922, 464);
            this.TWChart.TabIndex = 9;
            this.TWChart.Text = "chart1";
            this.TWChart.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TWChart_MouseDown);
            this.TWChart.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TWChart_MouseMove);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.SRPMChart);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Size = new System.Drawing.Size(928, 470);
            this.tabPage2.TabIndex = 4;
            this.tabPage2.Text = "Spinner RPM";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // SRPMChart
            // 
            chartArea2.BackColor = System.Drawing.Color.WhiteSmoke;
            chartArea2.CursorX.IsUserSelectionEnabled = true;
            chartArea2.Name = "ChartArea1";
            this.SRPMChart.ChartAreas.Add(chartArea2);
            this.SRPMChart.Dock = System.Windows.Forms.DockStyle.Fill;
            legend2.Alignment = System.Drawing.StringAlignment.Center;
            legend2.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom;
            legend2.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            legend2.IsTextAutoFit = false;
            legend2.Name = "Legend1";
            this.SRPMChart.Legends.Add(legend2);
            this.SRPMChart.Location = new System.Drawing.Point(0, 0);
            this.SRPMChart.Name = "SRPMChart";
            this.SRPMChart.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.None;
            this.SRPMChart.Size = new System.Drawing.Size(928, 470);
            this.SRPMChart.TabIndex = 10;
            this.SRPMChart.Text = "chart1";
            this.SRPMChart.MouseDown += new System.Windows.Forms.MouseEventHandler(this.SRPMChart_MouseDown);
            this.SRPMChart.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SRPMChart_MouseMove);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.MapInfoLV);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(928, 470);
            this.tabPage3.TabIndex = 3;
            this.tabPage3.Text = "Beatmap Information";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // MapInfoLV
            // 
            this.MapInfoLV.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Property,
            this.Information});
            this.MapInfoLV.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MapInfoLV.Location = new System.Drawing.Point(0, 0);
            this.MapInfoLV.Name = "MapInfoLV";
            this.MapInfoLV.Size = new System.Drawing.Size(928, 470);
            this.MapInfoLV.TabIndex = 0;
            this.MapInfoLV.UseCompatibleStateImageBehavior = false;
            // 
            // Property
            // 
            this.Property.Text = "Property";
            this.Property.Width = 250;
            // 
            // Information
            // 
            this.Information.Text = "Information";
            this.Information.Width = 600;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.ReplayInfoLV);
            this.tabPage4.ForeColor = System.Drawing.SystemColors.ControlText;
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.tabPage4.Size = new System.Drawing.Size(928, 470);
            this.tabPage4.TabIndex = 2;
            this.tabPage4.Text = "Replay Information";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // ReplayInfoLV
            // 
            this.ReplayInfoLV.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.ReplayInfoLV.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ReplayInfoLV.Location = new System.Drawing.Point(0, 0);
            this.ReplayInfoLV.Name = "ReplayInfoLV";
            this.ReplayInfoLV.Size = new System.Drawing.Size(928, 470);
            this.ReplayInfoLV.TabIndex = 1;
            this.ReplayInfoLV.UseCompatibleStateImageBehavior = false;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Property";
            this.columnHeader1.Width = 250;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Information";
            this.columnHeader2.Width = 600;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(43)))), ((int)(((byte)(43)))));
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
            this.ReplayTimelineLB.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(26)))), ((int)(((byte)(26)))));
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
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(26)))), ((int)(((byte)(26)))));
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.PluginsMenuItem,
            this.aboutToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1281, 24);
            this.menuStrip1.TabIndex = 13;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(232)))), ((int)(((byte)(232)))));
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.languageToolStripMenuItem,
            this.preferencesToolStripMenuItem});
            this.settingsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(232)))), ((int)(((byte)(232)))));
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.settingsToolStripMenuItem.Text = "Settings";
            // 
            // languageToolStripMenuItem
            // 
            this.languageToolStripMenuItem.Name = "languageToolStripMenuItem";
            this.languageToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.languageToolStripMenuItem.Text = "Language";
            // 
            // preferencesToolStripMenuItem
            // 
            this.preferencesToolStripMenuItem.Name = "preferencesToolStripMenuItem";
            this.preferencesToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.preferencesToolStripMenuItem.Text = "Preferences";
            // 
            // PluginsMenuItem
            // 
            this.PluginsMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem1,
            this.toolStripSeparator1});
            this.PluginsMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(232)))), ((int)(((byte)(232)))));
            this.PluginsMenuItem.Name = "PluginsMenuItem";
            this.PluginsMenuItem.Size = new System.Drawing.Size(58, 20);
            this.PluginsMenuItem.Text = "Plugins";
            // 
            // settingsToolStripMenuItem1
            // 
            this.settingsToolStripMenuItem1.Name = "settingsToolStripMenuItem1";
            this.settingsToolStripMenuItem1.Size = new System.Drawing.Size(152, 22);
            this.settingsToolStripMenuItem1.Text = "Settings";
            this.settingsToolStripMenuItem1.Click += new System.EventHandler(this.settingsToolStripMenuItem1_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(149, 6);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem1});
            this.aboutToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(232)))), ((int)(((byte)(232)))));
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.aboutToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem1
            // 
            this.aboutToolStripMenuItem1.Name = "aboutToolStripMenuItem1";
            this.aboutToolStripMenuItem1.Size = new System.Drawing.Size(152, 22);
            this.aboutToolStripMenuItem1.Text = "About";
            // 
            // oRAMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(43)))), ((int)(((byte)(43)))));
            this.ClientSize = new System.Drawing.Size(1281, 753);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.MainContainer);
            this.Controls.Add(this.ReplaySelectPanel);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "oRAMainForm";
            this.Text = "o!RA";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ReplaySelectPanel.ResumeLayout(false);
            this.MainContainer.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.TWChart)).EndInit();
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.SRPMChart)).EndInit();
            this.tabPage3.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel ReplaySelectPanel;
        private System.Windows.Forms.TreeView ReplaysList;
        private System.Windows.Forms.ProgressBar Progress;
        private System.Windows.Forms.TabControl MainContainer;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.DataVisualization.Charting.Chart TWChart;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ListBox ReplayTimelineLB;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ToolTip ChartToolTip;
        private System.Windows.Forms.DataVisualization.Charting.Chart SRPMChart;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem languageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem preferencesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem PluginsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ListView MapInfoLV;
        private System.Windows.Forms.ColumnHeader Property;
        private System.Windows.Forms.ColumnHeader Information;
        private System.Windows.Forms.ListView ReplayInfoLV;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
    }
}

