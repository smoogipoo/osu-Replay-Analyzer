using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChartsPlugin
{
    public partial class Container : UserControl
    {
        public Container()
        {
            InitializeComponent();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (UserControl c in ContentPanel.Controls)
            {
                if (c.Name == ((ComboBox)sender).GetItemText(((ComboBox)sender).SelectedItem))
                    c.Show();
                else
                    c.Hide();
            }
        }
        private void Container_Load(object sender, EventArgs e)
        {
            //Todo: Set names as they are in the combobox
            ContentPanel.Controls.Add(new TWChart { Dock = DockStyle.Fill, Name = "Timing Windows", Visible = false });
            ContentPanel.Controls.Add(new SRPMChart { Dock = DockStyle.Fill, Name = "Spinner RPM", Visible = false});
            DisplaySelectCB.SelectedIndex = 0;
        }

        private void ContentPanel_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
