using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using BMAPI;
using o_RA;
using ReplayAPI;

namespace ReplayInfoPlugin
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
            oRA.Data.ReplayChanged += HandleReplayChanged;
        }

        public void HandleReplayChanged(Replay r, Beatmap b)
        {
            customListView1.Items.Clear();
            customListView1.Items.Add(new ListViewItem());
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_Format"], r.FileFormat.ToString(CultureInfo.InvariantCulture) }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_FName"], r.Filename }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_FSize"], File.OpenRead(r.Filename).Length + " " + oRA.Data.Language["text_bytes"] }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_FHash"], r.ReplayHash }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_ReplayFrames"], r.ReplayFrames.Count.ToString(CultureInfo.InvariantCulture) }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_KeysPressed"], r.ClickFrames.Count.ToString(CultureInfo.InvariantCulture) }));
            customListView1.Items.Add(new ListViewItem());
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_RepMode"], r.GameMode.ToString() }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_RepPlayer"], r.PlayerName }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_RepScore"], r.TotalScore.ToString(CultureInfo.InvariantCulture) }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_RepCombo"], r.MaxCombo.ToString(CultureInfo.InvariantCulture) }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_Rep300Count"], r.Count_300.ToString(CultureInfo.InvariantCulture) }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_Rep100Count"], r.Count_100.ToString(CultureInfo.InvariantCulture) }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_Rep50Count"], r.Count_50.ToString(CultureInfo.InvariantCulture) }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_RepMissCount"], r.Count_Miss.ToString(CultureInfo.InvariantCulture) }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_RepGekiCount"], r.Count_Geki.ToString(CultureInfo.InvariantCulture) }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_RepKatuCount"], r.Count_Katu.ToString(CultureInfo.InvariantCulture) }));
            customListView1.Items.Add(new ListViewItem());
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_RepMods"], r.Mods.ToString() }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_ErrorRate"], Math.Abs(oRA.Data.NegativeErrorAverage).ToString(".00") + "ms - " + "+" + oRA.Data.PositiveErrorAverage.ToString(".00") + "ms" }));
        }
    }
}
