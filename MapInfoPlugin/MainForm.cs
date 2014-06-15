using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BMAPI;
using o_RA;
using ReplayAPI;

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
            oRA.Data.ReplayChanged += HandleReplayChanged;
        }

        public void HandleReplayChanged(Replay r, Beatmap b)
        {
            customListView1.Items.Clear();
            customListView1.Items.Add(new ListViewItem());
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_Format"], b.Format.ToString() }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_FName"], b.Filename }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_FSize"], File.OpenRead(b.Filename).Length + " bytes" }));
            string beatmapHash;
            oRA.Data.BeatmapHashes.TryGetValue(b.Filename, out beatmapHash);
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_FHash"], beatmapHash }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_TotalHitObjects"], b.HitObjects.Count.ToString(CultureInfo.InvariantCulture) }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_MapAFN"], b.AudioFilename }));
            customListView1.Items.Add(new ListViewItem());
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_MapName"], b.Title + (!string.IsNullOrEmpty(b.TitleUnicode) && b.TitleUnicode != b.Title ? "(" + b.TitleUnicode + ")" : "") }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_MapArtist"], b.Artist + (!string.IsNullOrEmpty(b.ArtistUnicode) && b.ArtistUnicode != b.Artist ? "(" + b.ArtistUnicode + ")" : "") }));
            if (b.Source != null)
                customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_MapSource"], b.Source }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_MapCreator"], b.Creator }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_MapVersion"], b.Version }));
            if (b.BeatmapID != null)
                customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_MapID"], b.BeatmapID.ToString() }));
            if (b.BeatmapSetID != null)
                customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_MapSetID"], b.BeatmapSetID.ToString() }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_MapTags"], string.Join(", ", b.Tags) }));
            customListView1.Items.Add(new ListViewItem());
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_MapOD"], b.OverallDifficulty.ToString(".00").Substring(b.OverallDifficulty.ToString(".00").LastIndexOf(".", StringComparison.InvariantCulture) + 1) == "00" ? b.OverallDifficulty.ToString(CultureInfo.InvariantCulture) : b.OverallDifficulty.ToString(".00") }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_MapAR"], b.ApproachRate.ToString(".00").Substring(b.ApproachRate.ToString(".00").LastIndexOf(".", StringComparison.InvariantCulture) + 1) == "00" ? b.ApproachRate.ToString(CultureInfo.InvariantCulture) : b.CircleSize.ToString(".00") }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_MapHP"], b.HPDrainRate.ToString(".00").Substring(b.HPDrainRate.ToString(".00").LastIndexOf(".", StringComparison.InvariantCulture) + 1) == "00" ? b.HPDrainRate.ToString(CultureInfo.InvariantCulture) : b.HPDrainRate.ToString(".00") }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_MapCS"], b.CircleSize.ToString(".00").Substring(b.CircleSize.ToString(".00").LastIndexOf(".", StringComparison.InvariantCulture) + 1) == "00" ? b.CircleSize.ToString(CultureInfo.InvariantCulture) : b.CircleSize.ToString(".00") }));
            foreach (ComboInfo combo in b.ComboColours)
            {
                ListViewItem li = new ListViewItem(oRA.Data.Language["info_MapComboColour"] + " " + combo.ComboNumber + ":");
                ListViewItem.ListViewSubItem colorItem = new ListViewItem.ListViewSubItem();
                colorItem.Text = combo.Colour.R + @", " + combo.Colour.G + @", " + combo.Colour.B;
                colorItem.ForeColor = Color.FromArgb(255, combo.Colour.R, combo.Colour.G, combo.Colour.B);
                li.SubItems.Add(colorItem);
                customListView1.Items.Add(li);
            }
            customListView1.Items.Add(new ListViewItem());
            int totalTime = b.HitObjects[b.HitObjects.Count - 1].StartTime - b.HitObjects[0].StartTime;
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_MapTotalTime"], TimeSpan.FromMilliseconds(totalTime).Minutes + ":" + TimeSpan.FromMilliseconds(totalTime).Seconds.ToString("00") }));
            totalTime = b.Events.Where(brk => brk.GetType() == typeof(BreakInfo)).Aggregate(totalTime, (current, brk) => current - (((BreakInfo)brk).EndTime - brk.StartTime));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_MapDrainTime"], TimeSpan.FromMilliseconds(totalTime).Minutes + ":" + TimeSpan.FromMilliseconds(totalTime).Seconds.ToString("00") }));
        }
    }
}
