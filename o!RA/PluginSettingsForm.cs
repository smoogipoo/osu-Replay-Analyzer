using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace o_RA
{
    public partial class PluginSettingsForm : Form
    {
        public PluginSettingsForm()
        {
            InitializeComponent();
        }

        private void PluginSettingsForm_Load(object sender, EventArgs e)
        {
            label1.Location = new Point(Width / 2 - label1.Width / 2, label1.Location.Y);
            listView1.FullRowSelect = true;
            listView1.View = View.Details;
            listView1.AllowColumnReorder = false;
            listView1.GridLines = true;
            listView1.Columns.Add("Name", 175, HorizontalAlignment.Left);
            listView1.Columns.Add("Description", 350, HorizontalAlignment.Left);
            listView1.Columns.Add("Version", 50, HorizontalAlignment.Left);
            listView1.Columns.Add("Path", 350, HorizontalAlignment.Left);
            listView1.Groups.Add("EPG", "Enabled Plugins");
            listView1.Groups.Add("DPG", "Disabled Plugins");
            listView1.Groups.Add("IPG", "Incompatible Plugins");

            foreach (ListViewItem li in oRAMainForm.Plugins.PluginCollection.Select(p => new ListViewItem(new[] { p.Instance.p_Name, p.Instance.p_Description, p.Instance.p_Version, p.AssemblyFile}, listView1.Groups[0])))
            {
                listView1.Items.Add(li);
            }
            if (oRAMainForm.Settings.ContainsSetting("DisabledPlugins"))
            {
                foreach (ListViewItem li in oRAMainForm.Settings.GetSetting("DisabledPlugins").Split(new[] {'|'}).Where(s => s != "").Select(s => new ListViewItem(new [] { s.Substring(s.LastIndexOf(@"\", StringComparison.InvariantCulture) + 1, s.LastIndexOf(".", StringComparison.InvariantCulture) - (s.LastIndexOf(@"\", StringComparison.InvariantCulture) + 1)), "Description Unavailable", "0.0.0", s }, listView1.Groups[1])))
                {
                    listView1.Items.Add(li);
                }
            }
            foreach (ListViewItem li in oRAMainForm.Plugins.IncompatiblePlugins.Select(p => new ListViewItem(new[] { p.Substring(p.LastIndexOf(@"\", StringComparison.InvariantCulture) + 1, p.LastIndexOf(".", StringComparison.InvariantCulture) - (p.LastIndexOf(@"\", StringComparison.InvariantCulture) + 1)), "Description Unavailable", "0.0.0", p }, listView1.Groups[2])))
            {
                listView1.Items.Add(li);
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                switch (listView1.SelectedItems[0].Group.Name)
                {
                    case "EPG":
                        ChangeStateCMSItem.Text = @"Disable Plugin";
                        ChangeStateCMSItem.Enabled = true;
                        DeletePluginCMSItem.Enabled = true;
                        break;
                    case "DPG":
                        ChangeStateCMSItem.Text = @"Enable Plugin";
                        ChangeStateCMSItem.Enabled = true;
                        DeletePluginCMSItem.Enabled = true;
                        break;
                    case "IPG":
                        ChangeStateCMSItem.Text = @"Enable Plugin";
                        ChangeStateCMSItem.Enabled = false;
                        DeletePluginCMSItem.Enabled = true;
                        break;
                }
            }
            else
            {
                ChangeStateCMSItem.Text = @"Enable Plugin";
                ChangeStateCMSItem.Enabled = false;
                DeletePluginCMSItem.Enabled = false;
            }
        }

        private void ChangeStateCMSItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                switch (listView1.SelectedItems[0].Group.Name)
                {
                    case "EPG":
                        oRAMainForm.Plugins.UnloadPlugin(listView1.SelectedItems[0].SubItems[3].Text);
                        if (!oRAMainForm.Settings.ContainsSetting("DisabledPlugins") || oRAMainForm.Settings.GetSetting("DisabledPlugins") == "")
                        {
                            oRAMainForm.Settings.AddSetting("DisabledPlugins", oRAMainForm.Settings.GetSetting("DisabledPlugins") + listView1.SelectedItems[0].SubItems[3].Text);
                        }                
                        else
                        {
                            oRAMainForm.Settings.AddSetting("DisabledPlugins", oRAMainForm.Settings.GetSetting("DisabledPlugins") + "|" + listView1.SelectedItems[0].SubItems[3].Text);
                        }
                        oRAMainForm.Settings.Save();
                        listView1.SelectedItems[0].Group = listView1.Groups[1];
                        break;
                    case "DPG":
                        oRAMainForm.Settings.AddSetting("DisabledPlugins", oRAMainForm.Settings.GetSetting("DisabledPlugins").Replace(listView1.SelectedItems[0].SubItems[3].Text, ""));
                        oRAMainForm.Settings.Save();
                        label1.Visible = true;
                        ChangeStateCMSItem.Text = @"Enable Plugin";
                        ChangeStateCMSItem.Enabled = true;
                        DeletePluginCMSItem.Enabled = true;
                        break;
                }
            }
        }

        private void DeletePluginCMSItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                oRAMainForm.Plugins.UnloadPlugin(listView1.SelectedItems[0].SubItems[3].Text);
                listView1.Items.Remove(listView1.SelectedItems[0]);
                Directory.Delete(listView1.SelectedItems[0].SubItems[3].Text.Substring(0, listView1.SelectedItems[0].SubItems[3].Text.LastIndexOf(@"\", StringComparison.InvariantCulture)));
            }
        }
    }
}
