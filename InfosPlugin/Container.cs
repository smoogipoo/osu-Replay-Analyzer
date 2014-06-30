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
        private class ComboBoxItem
        {
            public string Text { get; set; }
            public Control Content { get; set; }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < DisplaySelectCB.Items.Count; i++)
            {
                ComboBoxItem item = (ComboBoxItem)DisplaySelectCB.Items[i];
                item.Content.Visible = i == ((ComboBox)sender).SelectedIndex;
            }
        }

        private void Container_Load(object sender, EventArgs e)
        {
            //Doesn't matter how you order these, as long as TWChart is last
            //As it is the default selection.
            DisplaySelectCB.DisplayMember = "Text";
            DisplaySelectCB.Items.Add(new ComboBoxItem { Text = oRA.Data.Language["oRA_ReplayInformation"], Content = new ReplayInfo { Dock = DockStyle.Fill } });
            DisplaySelectCB.Items.Add(new ComboBoxItem { Text = "Beatmap Preview", Content = new MapPreview { Dock = DockStyle.Fill } });
            DisplaySelectCB.Items.Add(new ComboBoxItem { Text = oRA.Data.Language["oRA_BeatmapInformation"], Content = new MapInfo { Dock = DockStyle.Fill } });

            foreach (ComboBoxItem item in DisplaySelectCB.Items)
            {
                ContentPanel.Controls.Add(item.Content);
            }
            DisplaySelectCB.SelectedIndex = 0;
        }
    }
}
