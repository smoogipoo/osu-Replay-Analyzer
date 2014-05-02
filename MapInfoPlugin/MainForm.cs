using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BMAPI;
using ReplayAPI;
using o_RA;
using oRAInterface;

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
            BackColor = o_RA.oRAColours.Colour_BG_Main;
            oRA.Data.ReplayChanged += HandleData;
        }

        private static void HandleData(Replay r, Beatmap b)
        {

        }
    }
}
