using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
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

            //Calculate UR and avg timing windows
            double unstableRate = 0, negativeErrorAverage = 0, positiveErrorAverage = 0, max = 0, min = 0, variance = 0;
            double[] keyUnstableRate = new double[4];
            double[] keyVariance = new double[4];
            int[] keyCount = new int[4];


            int nErrAvgCount = 0, pErrAvgCount = 0;

            for (int i = 0; i < oRA.Data.ReplayObjects.Count; i++)
            {
                double diff = oRA.Data.ReplayObjects[i].Frame.Time - oRA.Data.ReplayObjects[i].Object.StartTime;
                //For now, this will be used as the mean
                //We must calculate this before the actual unstable rate
                unstableRate += oRA.Data.ReplayObjects[i].Frame.Time - oRA.Data.ReplayObjects[i].Object.StartTime;
                if ((oRA.Data.ReplayObjects[i].Frame.Keys & KeyData.K1) == KeyData.K1)
                {
                    keyUnstableRate[0] += diff;
                    keyCount[0] += 1;
                }
                if ((oRA.Data.ReplayObjects[i].Frame.Keys & KeyData.K2) == KeyData.K2)
                {
                    keyUnstableRate[1] += diff;
                    keyCount[1] += 1;
                } 
                if ((oRA.Data.ReplayObjects[i].Frame.Keys & KeyData.M1) == KeyData.M1)
                {
                    keyUnstableRate[2] += diff;
                    keyCount[2] += 1;
                } 
                if ((oRA.Data.ReplayObjects[i].Frame.Keys & KeyData.M2) == KeyData.M2)
                {
                    keyUnstableRate[3] += diff;
                    keyCount[3] += 1;
                }
            }

            if (oRA.Data.ReplayObjects.Count > 0)
            {
                unstableRate /= oRA.Data.ReplayObjects.Count;
                for (int i = 0; i < 4; i++ )
                    keyUnstableRate[i] /= keyCount[i];
            }

            for (int i = 0; i < oRA.Data.ReplayObjects.Count; i++)
            {
                double diff = oRA.Data.ReplayObjects[i].Frame.Time - oRA.Data.ReplayObjects[i].Object.StartTime;

                if (diff > 0)
                {
                    positiveErrorAverage += diff;
                    pErrAvgCount += 1;
                }
                else
                {
                    negativeErrorAverage += diff;
                    nErrAvgCount += 1;
                }
                variance += Math.Pow(diff - unstableRate, 2);
                if ((oRA.Data.ReplayObjects[i].Frame.Keys & KeyData.K1) == KeyData.K1)
                    keyVariance[0] += Math.Pow(diff - keyUnstableRate[0], 2);
                if ((oRA.Data.ReplayObjects[i].Frame.Keys & KeyData.K2) == KeyData.K2)
                    keyVariance[1] += Math.Pow(diff - keyUnstableRate[0], 2);
                if ((oRA.Data.ReplayObjects[i].Frame.Keys & KeyData.M1) == KeyData.M1)
                    keyVariance[2] += Math.Pow(diff - keyUnstableRate[0], 2);
                if ((oRA.Data.ReplayObjects[i].Frame.Keys & KeyData.M2) == KeyData.M2)
                    keyVariance[3] += Math.Pow(diff - keyUnstableRate[0], 2);
                if (diff > max)
                    max = diff;
                if (diff < min)
                    min = diff;
            }
            positiveErrorAverage = pErrAvgCount != 0 ? positiveErrorAverage / pErrAvgCount : 0;
            negativeErrorAverage = nErrAvgCount != 0 ? negativeErrorAverage / nErrAvgCount : 0;
            if (oRA.Data.ReplayObjects.Count > 0)
            {
                //Calculate unstable rate
                unstableRate = Math.Round(Math.Sqrt(variance / oRA.Data.ReplayObjects.Count) * 10, 2);
                for (int i = 0; i < 4; i++)
                    keyUnstableRate[i] = Math.Round(Math.Sqrt(keyVariance[i] / keyCount[i]) * 10, 2);
            }
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_ErrorRate"], negativeErrorAverage.ToString(".00") + "ms ~ " + "+" + positiveErrorAverage.ToString(".00") + "ms" }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_UnstableRate"], unstableRate.ToString("0.00") }));
            customListView1.Items.Add(new ListViewItem());
            customListView1.Items.Add(new ListViewItem(new[] { "K1" + oRA.Data.Language["info_UnstableRate"], keyUnstableRate[0].ToString("0.00") }));
            customListView1.Items.Add(new ListViewItem(new[] { "K2" + oRA.Data.Language["info_UnstableRate"], keyUnstableRate[1].ToString("0.00") }));
            customListView1.Items.Add(new ListViewItem(new[] { "M1" + oRA.Data.Language["info_UnstableRate"], keyUnstableRate[2].ToString("0.00") }));
            customListView1.Items.Add(new ListViewItem(new[] { "M2" + oRA.Data.Language["info_UnstableRate"], keyUnstableRate[3].ToString("0.00") }));
            
        }
    }
}
