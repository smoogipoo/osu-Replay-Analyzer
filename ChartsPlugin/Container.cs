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
            ContentPanel.Controls.Add(new TWChart { Dock = DockStyle.Fill, Name = "Timing Windows", Visible = false });
            ContentPanel.Controls.Add(new SRPMChart { Dock = DockStyle.Fill, Name = "Spinner RPM", Visible = false});
            ContentPanel.Controls.Add(new AimChart { Dock = DockStyle.Fill, Name = "Aim Accuracy", Visible = false });
            DisplaySelectCB.SelectedIndex = 0;
        }
    }
}
