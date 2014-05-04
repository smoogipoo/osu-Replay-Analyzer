using System;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Linq;
using o_RA;
using BMAPI;
using ReplayAPI;

namespace MapInfoPlugin
{
    class CustomListView : ListView
    {
        public CustomListView()
        {
            DoubleBuffered = true;
            BackColor = oRAColours.Colour_BG_P0;
            OwnerDraw = true;
            UseCompatibleStateImageBehavior = false;
            FullRowSelect = true;
            View = View.Details;
            AllowColumnReorder = false;
            GridLines = false;

            DrawColumnHeader += HandleDrawHeader;
            DrawSubItem += HandleDrawSubItem;
            Resize += HandleResize;
        }

        public void HandleReplayChanged(Replay r, Beatmap b)
        {
            Items.Clear();
            Items.Add(new ListViewItem());
            Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_Format"], b.Format.ToString() }));
            Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_FName"], b.Filename }));
            Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_FSize"], File.OpenRead(b.Filename).Length + " bytes" }));
            string beatmapHash;
            oRA.Data.BeatmapHashes.TryGetValue(b.Filename, out beatmapHash);
            Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_FHash"], beatmapHash }));
            Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_TotalHitObjects"], b.HitObjects.Count.ToString(CultureInfo.InvariantCulture) }));
            Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_MapAFN"], b.AudioFilename }));
            Items.Add(new ListViewItem());
            Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_MapName"], b.Title + (!string.IsNullOrEmpty(b.TitleUnicode) && b.TitleUnicode != b.Title ? "(" + b.TitleUnicode + ")" : "") }));
            Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_MapArtist"], b.Artist + (!string.IsNullOrEmpty(b.ArtistUnicode) && b.ArtistUnicode != b.Artist ? "(" + b.ArtistUnicode + ")" : "") }));
            if (b.Source != null)
                Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_MapSource"], b.Source }));
            Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_MapCreator"], b.Creator }));
            Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_MapVersion"], b.Version }));
            if (b.BeatmapID != null)
                Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_MapID"], b.BeatmapID.ToString() }));
            if (b.BeatmapSetID != null)
                Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_MapSetID"], b.BeatmapSetID.ToString() }));
            Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_MapTags"], string.Join(", ", b.Tags) }));
            Items.Add(new ListViewItem());
            Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_MapOD"], b.OverallDifficulty.ToString(".00").Substring(b.OverallDifficulty.ToString(".00").LastIndexOf(".", StringComparison.InvariantCulture) + 1) == "00" ? b.OverallDifficulty.ToString(CultureInfo.InvariantCulture) : b.OverallDifficulty.ToString(".00") }));
            Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_MapAR"], b.ApproachRate.ToString(".00").Substring(b.ApproachRate.ToString(".00").LastIndexOf(".", StringComparison.InvariantCulture) + 1) == "00" ? b.ApproachRate.ToString(CultureInfo.InvariantCulture) : b.CircleSize.ToString(".00") }));
            Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_MapHP"], b.HPDrainRate.ToString(".00").Substring(b.HPDrainRate.ToString(".00").LastIndexOf(".", StringComparison.InvariantCulture) + 1) == "00" ? b.HPDrainRate.ToString(CultureInfo.InvariantCulture) : b.HPDrainRate.ToString(".00") }));
            Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_MapCS"], b.CircleSize.ToString(".00").Substring(b.CircleSize.ToString(".00").LastIndexOf(".", StringComparison.InvariantCulture) + 1) == "00" ? b.CircleSize.ToString(CultureInfo.InvariantCulture) : b.CircleSize.ToString(".00") }));
            foreach (ComboInfo combo in b.ComboColours)
            {
                ListViewItem li = new ListViewItem(oRA.Data.Language["info_MapComboColour"] + " " + combo.ComboNumber + ":");
                ListViewItem.ListViewSubItem colorItem = new ListViewItem.ListViewSubItem();
                colorItem.Text = combo.Colour.R + @", " + combo.Colour.G + @", " + combo.Colour.B;
                colorItem.ForeColor = Color.FromArgb(255, combo.Colour.R, combo.Colour.G, combo.Colour.B);
                li.SubItems.Add(colorItem);
                Items.Add(li);
            }
            Items.Add(new ListViewItem());
            int totalTime = b.HitObjects[b.HitObjects.Count - 1].StartTime - b.HitObjects[0].StartTime;
            Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_MapTotalTime"], TimeSpan.FromMilliseconds(totalTime).Minutes + ":" + TimeSpan.FromMilliseconds(totalTime).Seconds.ToString("00") }));
            totalTime = b.Events.Where(brk => brk.GetType() == typeof(BreakInfo)).Aggregate(totalTime, (current, brk) => current - (((BreakInfo)brk).EndTime - brk.StartTime));
            Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_MapDrainTime"], TimeSpan.FromMilliseconds(totalTime).Minutes + ":" + TimeSpan.FromMilliseconds(totalTime).Seconds.ToString("00") }));
        }

        private static void HandleDrawHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(oRAColours.Colour_BG_Main), e.Bounds);
            e.Graphics.FillRectangle(new SolidBrush(oRAColours.Colour_BG_P0), new Rectangle(e.Bounds.X + e.Bounds.Width - 1, e.Bounds.Y, 1, e.Bounds.Height));
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            e.Graphics.DrawString(e.Header.Text, oRAFonts.Font_Description, new SolidBrush(oRAColours.Colour_Text_N), e.Bounds.Left + e.Bounds.Width / 2 - e.Graphics.MeasureString(e.Header.Text, oRAFonts.Font_Description).Width / 2, e.Bounds.Top + e.Bounds.Height / 2 - e.Graphics.MeasureString(e.Header.Text, oRAFonts.Font_Description).Height / 2);
        }
        private static void HandleDrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            if (e.ItemIndex == -1)
                return;
            e.Graphics.FillRectangle(new SolidBrush(oRAColours.Colour_BG_P0), e.Bounds);
            if (e.ItemState.HasFlag(ListViewItemStates.Selected))
            {
                e.Graphics.FillRectangle(new SolidBrush(oRAColours.Colour_Item_BG_1), e.Bounds);
                e.Graphics.DrawRectangle(new Pen(oRAColours.Colour_Item_BG_0), e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);
            }
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            e.Graphics.DrawString(e.SubItem.Text, oRAFonts.Font_SubDescription, e.ItemState.HasFlag(ListViewItemStates.Selected) ? new SolidBrush(oRAColours.Colour_Text_H) : new SolidBrush(oRAColours.Colour_Text_N), e.Bounds.Left + 22, e.Bounds.Top + e.Bounds.Height / 2 - e.Graphics.MeasureString(e.Item.Text, oRAFonts.Font_SubDescription).Height / 2);
        }

        private void HandleResize(object sender, EventArgs e)
        {
            if (Columns.Count == 0)
                return;
            int columnWidth = Columns.Cast<ColumnHeader>().Sum(cH => cH.Width);
            int widthDifference = Width - columnWidth - 1;

            if (widthDifference > 0 || widthDifference < 0)
            {
                int widthIncreaseAmnt = widthDifference / Columns.Count;
                for (int i = 0; i < Columns.Count; i++)
                {
                    Columns[i].Width += widthIncreaseAmnt;
                }
            }
        }
    }
}
