using System;
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
            //Doesn't matter how you order these, as long as TWChart is last
            //As it is the default selection.
            ContentPanel.Controls.Add(new SRPMChart { Dock = DockStyle.Fill, Name = "Spinner RPM"});
            ContentPanel.Controls.Add(new AimChart { Dock = DockStyle.Fill, Name = "Aim Accuracy" });
            ContentPanel.Controls.Add(new TWChart { Dock = DockStyle.Fill, Name = "Timing Windows" });   
            DisplaySelectCB.SelectedIndex = 0;
        }
    }
}
