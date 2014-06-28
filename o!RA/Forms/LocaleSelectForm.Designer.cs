using o_RA.Controls;

namespace o_RA.Forms
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
            this.NextLbl = new System.Windows.Forms.LinkLabel();
            this.PrevLbl = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // NextLbl
            // 
            this.NextLbl.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(232)))), ((int)(((byte)(232)))));
            this.NextLbl.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.NextLbl.AutoSize = true;
            this.NextLbl.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.NextLbl.Location = new System.Drawing.Point(235, 393);
            this.NextLbl.Name = "NextLbl";
            this.NextLbl.Size = new System.Drawing.Size(41, 13);
            this.NextLbl.TabIndex = 0;
            this.NextLbl.TabStop = true;
            this.NextLbl.Text = "Next ▶";
            this.NextLbl.Visible = false;
            this.NextLbl.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.NextLbl.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.NextLbl_LinkClicked);
            // 
            // PrevLbl
            // 
            this.PrevLbl.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(232)))), ((int)(((byte)(232)))));
            this.PrevLbl.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.PrevLbl.AutoSize = true;
            this.PrevLbl.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.PrevLbl.Location = new System.Drawing.Point(185, 393);
            this.PrevLbl.Name = "PrevLbl";
            this.PrevLbl.Size = new System.Drawing.Size(41, 13);
            this.PrevLbl.TabIndex = 1;
            this.PrevLbl.TabStop = true;
            this.PrevLbl.Text = "◀ Prev";
            this.PrevLbl.Visible = false;
            this.PrevLbl.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.PrevLbl.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.PrevLbl_LinkClicked);
            // 
            // LocaleSelectForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(26)))), ((int)(((byte)(26)))));
            this.ClientSize = new System.Drawing.Size(458, 415);
            this.Controls.Add(this.PrevLbl);
            this.Controls.Add(this.NextLbl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "LocaleSelectForm";
            this.ShowIcon = false;
            this.Text = "Language";
            this.Load += new System.EventHandler(this.LocaleSelectForm_Load);
            this.Resize += new System.EventHandler(this.LocaleSelectForm_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.LinkLabel NextLbl;
        private System.Windows.Forms.LinkLabel PrevLbl;

    }
}