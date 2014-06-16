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
            int nErrAvgCount = 0, pErrAvgCount = 0;
            double[] keyPAverage = new double[4];
            double[] keyNAverage = new double[4];
            double[] keyUnstableRate = new double[4];
            double[] keyVariance = new double[4];
            int[] keyPAverageCount = new int[4];
            int[] keyNAverageCount = new int[4];
            int[] keyCount = new int[4];

            for (int i = 0; i < oRA.Data.ReplayObjects.Count; i++)
            {
                double diff = oRA.Data.ReplayObjects[i].Frame.Time - oRA.Data.ReplayObjects[i].Object.StartTime;

                //For now, this will be used as the mean
                //We must calculate this before the actual unstable rate
                unstableRate += diff;

                //Because the enum has 5 and 10 which contain 1 and 2 K1 will trigger M1 and K2 will trigged M2
                //Therefore, we need to XOR K1,K1 out.
                //Or NOT, but it's quicker to XOR (compound assignment)
                //But we do not want to set the variable since it's used later, so we'll use a temp variable
                KeyData tKD = oRA.Data.ReplayObjects[i].Frame.Keys;
                if ((tKD & KeyData.K1) == KeyData.K1)
                {
                    tKD ^= KeyData.K1;
                    keyUnstableRate[0] += diff;
                    keyCount[0] += 1;
                }
                if ((tKD & KeyData.K2) == KeyData.K2)
                {
                    tKD ^= KeyData.K2;
                    keyUnstableRate[1] += diff;
                    keyCount[1] += 1;
                }
                if ((tKD & KeyData.M1) == KeyData.M1)
                {
                    keyUnstableRate[2] += diff;
                    keyCount[2] += 1;
                }
                if ((tKD & KeyData.M2) == KeyData.M2)
                {
                    keyUnstableRate[3] += diff;
                    keyCount[3] += 1;
                }
            }

            if (oRA.Data.ReplayObjects.Count > 0)
            {
                unstableRate /= oRA.Data.ReplayObjects.Count;
                for (int i = 0; i < 4; i++)
                {   
                    if (keyCount[i] != 0)
                        keyUnstableRate[i] /= keyCount[i];
                }
            }

            for (int i = 0; i < oRA.Data.ReplayObjects.Count; i++)
            {
                double diff = oRA.Data.ReplayObjects[i].Frame.Time - oRA.Data.ReplayObjects[i].Object.StartTime;

                //This is a bit messy, but I'm too tired to improve it
                //Follows the same XOR-ing procedure as above
                KeyData tKD = oRA.Data.ReplayObjects[i].Frame.Keys;
                if (diff > 0)
                {
                    positiveErrorAverage += diff;
                    pErrAvgCount += 1;
                    if ((tKD & KeyData.K1) == KeyData.K1)
                    {
                        tKD ^= KeyData.K1;
                        keyVariance[0] += Math.Pow(diff - keyUnstableRate[0], 2);
                        keyPAverage[0] += diff;
                        keyPAverageCount[0] += 1;
                    }
                    if ((tKD & KeyData.K2) == KeyData.K2)
                    {
                        tKD ^= KeyData.K2;
                        keyVariance[1] += Math.Pow(diff - keyUnstableRate[0], 2);
                        keyPAverage[1] += diff;
                        keyPAverageCount[1] += 1;
                    }
                    if ((tKD & KeyData.M1) == KeyData.M1)
                    {
                        keyVariance[2] += Math.Pow(diff - keyUnstableRate[0], 2);
                        keyPAverage[2] += diff;
                        keyPAverageCount[2] += 1;
                    }
                    if ((tKD & KeyData.M2) == KeyData.M2)
                    {
                        keyVariance[3] += Math.Pow(diff - keyUnstableRate[0], 2);
                        keyPAverage[3] += diff;
                        keyPAverageCount[3] += 1;
                    }
                }
                else
                {
                    negativeErrorAverage += diff;
                    nErrAvgCount += 1;
                    if ((tKD & KeyData.K1) == KeyData.K1)
                    {
                        tKD ^= KeyData.K1;
                        keyVariance[0] += Math.Pow(diff - keyUnstableRate[0], 2);
                        keyNAverage[0] += diff;
                        keyNAverageCount[0] += 1;
                    }
                    if ((tKD & KeyData.K2) == KeyData.K2)
                    {
                        tKD ^= KeyData.K2;
                        keyVariance[1] += Math.Pow(diff - keyUnstableRate[0], 2);
                        keyNAverage[1] += diff;
                        keyNAverageCount[1] += 1;
                    }
                    if ((tKD & KeyData.M1) == KeyData.M1)
                    {
                        keyVariance[2] += Math.Pow(diff - keyUnstableRate[0], 2);
                        keyNAverage[2] += diff;
                        keyNAverageCount[2] += 1;
                    }
                    if ((tKD & KeyData.M2) == KeyData.M2)
                    {
                        keyVariance[3] += Math.Pow(diff - keyUnstableRate[0], 2);
                        keyNAverage[3] += diff;
                        keyNAverageCount[3] += 1;
                    }
                }
                variance += Math.Pow(diff - unstableRate, 2);

                if (diff > max)
                    max = diff;
                if (diff < min)
                    min = diff;
            }

            positiveErrorAverage = pErrAvgCount != 0 ? positiveErrorAverage / pErrAvgCount : 0;
            negativeErrorAverage = nErrAvgCount != 0 ? negativeErrorAverage / nErrAvgCount : 0;
            for (int i = 0; i < 4; i++ )
            {
                //Calculate error rate
                if (keyPAverageCount[i] != 0)
                    keyPAverage[i] /= keyPAverageCount[i];
                if (keyNAverageCount[i] != 0)
                    keyNAverage[i] /= keyNAverageCount[i];
            }
            if (oRA.Data.ReplayObjects.Count > 0)
            {
                //Calculate unstable rate
                unstableRate = Math.Round(Math.Sqrt(variance / oRA.Data.ReplayObjects.Count) * 10, 2);
                for (int i = 0; i < 4; i++)
                {
                    if (keyCount[i] != 0)
                        keyUnstableRate[i] = Math.Round(Math.Sqrt(keyVariance[i] / keyCount[i]) * 10, 2);
                }
            }
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_ErrorRate"], negativeErrorAverage.ToString("0.00") + "ms ~ " + "+" + positiveErrorAverage.ToString("0.00") + "ms" }));
            customListView1.Items.Add(new ListViewItem(new[] { oRA.Data.Language["info_UnstableRate"], unstableRate.ToString("0.00") }));
            customListView1.Items.Add(new ListViewItem());
            if (keyNAverageCount[0] != 0 || keyPAverageCount[0] != 0)
                customListView1.Items.Add(new ListViewItem(new[] { "K1 " + oRA.Data.Language["info_ErrorRate"], keyNAverage[0].ToString("0.00") + "ms ~ " + "+" + keyPAverage[0].ToString("0.00") + "ms" }));
            if (keyNAverageCount[1] != 0 || keyPAverageCount[1] != 0)
                customListView1.Items.Add(new ListViewItem(new[] { "K2 " + oRA.Data.Language["info_ErrorRate"], keyNAverage[1].ToString("0.00") + "ms ~ " + "+" + keyPAverage[1].ToString("0.00") + "ms" }));
            if (keyNAverageCount[2] != 0 || keyPAverageCount[2] != 0)
                customListView1.Items.Add(new ListViewItem(new[] { "M1 " + oRA.Data.Language["info_ErrorRate"], keyNAverage[2].ToString("0.00") + "ms ~ " + "+" + keyPAverage[2].ToString("0.00") + "ms" }));
            if (keyNAverageCount[3] != 0 || keyPAverageCount[3] != 0)
                customListView1.Items.Add(new ListViewItem(new[] { "M2 " + oRA.Data.Language["info_ErrorRate"], keyNAverage[3].ToString("0.00") + "ms ~ " + "+" + keyPAverage[3].ToString("0.00") + "ms" }));
            if (keyCount[0] != 0)
                customListView1.Items.Add(new ListViewItem(new[] { "K1 " + oRA.Data.Language["info_UnstableRate"], keyUnstableRate[0].ToString("0.00") }));
            if (keyCount[1] != 0)
                customListView1.Items.Add(new ListViewItem(new[] { "K2 " + oRA.Data.Language["info_UnstableRate"], keyUnstableRate[1].ToString("0.00") }));
            if (keyCount[2] != 0)
                customListView1.Items.Add(new ListViewItem(new[] { "M1 " + oRA.Data.Language["info_UnstableRate"], keyUnstableRate[2].ToString("0.00") }));
            if (keyCount[3] != 0)
                customListView1.Items.Add(new ListViewItem(new[] { "M2 " + oRA.Data.Language["info_UnstableRate"], keyUnstableRate[3].ToString("0.00") }));
            
        }
    }
}
