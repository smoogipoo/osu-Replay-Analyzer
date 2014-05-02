using System;
using System.Windows.Forms;
using o_RA;

namespace MapInfoPlugin
{
    public partial class MainForm : UserControl
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            BackColor = oRAColours.Colour_BG_P0;
            oRA.Data.ReplayChanged += customListView1.HandleReplayChanged;
        }
    }
}
