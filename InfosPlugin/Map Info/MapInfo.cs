using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BMAPI;
using o_RA;
using ReplayAPI;

namespace InfosPlugin
{
    public partial class MapInfo : UserControl
    {
        public MapInfo()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            oRA.Data.ReplayChanged += HandleReplayChanged;
        }

        public void HandleReplayChanged(Replay r, Beatmap b)
        {
            int totalTime = 0;
            if (b.HitObjects.Count > 0)
                totalTime = b.HitObjects[b.HitObjects.Count - 1].StartTime - b.HitObjects[0].StartTime;

            tpDifficulty tp = new tpDifficulty();
            tp.tpHitObjects = new List<tpHitObject>(b.HitObjects.Count);
            foreach (BaseCircle hitObject in b.HitObjects)
                tp.tpHitObjects.Add(new tpHitObject(hitObject));

            customListView1.Items.Clear();
            customListView1.Items.Add(new ListViewItem());
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["oRA_Format"], b.Format.ToString() }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["oRA_FName"], b.Filename }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["oRA_FSize"], File.OpenRead(b.Filename).Length + " bytes" }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["oRA_FHash"], r.MapHash }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["oRA_TotalHitObjects"], b.HitObjects.Count.ToString(CultureInfo.InvariantCulture) }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["oRA_MapAFN"], b.AudioFilename }));
            customListView1.Items.Add(new ListViewItem());
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["oRA_MapName"], b.Title + (!string.IsNullOrEmpty(b.TitleUnicode) && b.TitleUnicode != b.Title ? "(" + b.TitleUnicode + ")" : "") }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["oRA_MapArtist"], b.Artist + (!string.IsNullOrEmpty(b.ArtistUnicode) && b.ArtistUnicode != b.Artist ? "(" + b.ArtistUnicode + ")" : "") }));
            if (b.Source != null)
                customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["oRA_MapSource"], b.Source }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["oRA_MapCreator"], b.Creator }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["oRA_MapVersion"], b.Version }));
            if (b.BeatmapID != null)
                customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["oRA_MapID"], b.BeatmapID.ToString() }));
            if (b.BeatmapSetID != null)
                customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["oRA_MapSetID"], b.BeatmapSetID.ToString() }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["oRA_MapTags"], string.Join(", ", b.Tags) }));
            customListView1.Items.Add(new ListViewItem());
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["oRA_MapOD"], b.OverallDifficulty.ToString("0.00") }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["oRA_MapAR"], b.ApproachRate.ToString("0.00") }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["oRA_MapHP"], b.HPDrainRate.ToString("0.00") }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["oRA_MapCS"], b.CircleSize.ToString("0.00") }));
            customListView1.Items.Add(new ListViewItem());
            foreach (ComboInfo combo in b.ComboColours)
            {
                ListViewItem li = new ListViewItem(oRA.Data.Language["oRA_MapComboColour"] + " " + combo.ComboNumber + ":");
                ListViewItem.ListViewSubItem colorItem = new ListViewItem.ListViewSubItem();
                colorItem.Text = combo.Colour.R + @", " + combo.Colour.G + @", " + combo.Colour.B;
                colorItem.ForeColor = Color.FromArgb(255, combo.Colour.R, combo.Colour.G, combo.Colour.B);
                li.SubItems.Add(colorItem);
                customListView1.Items.Add(li);
            }
            customListView1.Items.Add(new ListViewItem());
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["oRA_MapTotalTime"], TimeSpan.FromMilliseconds(totalTime).Minutes + ":" + TimeSpan.FromMilliseconds(totalTime).Seconds.ToString("00") }));
            totalTime = b.Events.Where(brk => brk.GetType() == typeof(BreakInfo)).Aggregate(totalTime, (current, brk) => current - (((BreakInfo)brk).EndTime - brk.StartTime));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["oRA_MapDrainTime"], TimeSpan.FromMilliseconds(totalTime).Minutes + ":" + TimeSpan.FromMilliseconds(totalTime).Seconds.ToString("00") }));
            if (tp.CalculateStrainValues())
            {
                double SpeedDifficulty = tp.CalculateDifficulty(tpDifficulty.DifficultyType.Speed);
                double AimDifficulty = tp.CalculateDifficulty(tpDifficulty.DifficultyType.Aim);
                double SpeedStars = Math.Sqrt(SpeedDifficulty) * tpDifficulty.STAR_SCALING_FACTOR;
                double AimStars = Math.Sqrt(AimDifficulty) * tpDifficulty.STAR_SCALING_FACTOR;
                double StarRating = SpeedStars + AimStars + Math.Abs(SpeedStars - AimStars) * tpDifficulty.EXTREME_SCALING_FACTOR;
                customListView1.Items.Add(new ListViewItem());
                customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["oRA_SpeedDifficulty"], SpeedDifficulty.ToString("0.00") }));
                customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["oRA_AimDifficulty"], AimDifficulty.ToString("0.00") }));
                customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["oRA_SpeedStars"], SpeedStars.ToString("0.00") }));
                customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["oRA_AimStars"], AimStars.ToString("0.00") }));
                customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["oRA_TotalStars"], StarRating.ToString("0.00") }));
            }

        }
    }
}
