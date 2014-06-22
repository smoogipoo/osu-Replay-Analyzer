using System;
using System.Windows.Forms;

namespace InfosPlugin
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
            //Doesn't matter how you order these, as long as TWChart is last
            //As it is the default selection.
            ContentPanel.Controls.Add(new ReplayInfo { Dock = DockStyle.Fill, Name = "Replay Information" });
            ContentPanel.Controls.Add(new MapInfo { Dock = DockStyle.Fill, Name = "Beatmap Information" });
            DisplaySelectCB.SelectedIndex = 0;
        }
    }
}
