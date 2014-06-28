using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using o_RAResources;
using o_RA.Controls;

namespace o_RA.Forms
{
    public partial class LocaleSelectForm : Form
    {
        public LocaleSelectForm()
        {
            InitializeComponent();
        }
        const int LocaleBoxSpacing = 10;
        const int LocaleBoxWidth = 160;
        const int LocaleBoxHeight = 100;
        const int MaxRowWidth = 3 * LocaleBoxWidth + 3 * LocaleBoxSpacing;

        readonly List<UserControl> LocaleBoxes = new List<UserControl>();
        int currentStart = 0;


        private void LocaleSelectForm_Load(object sender, System.EventArgs e)
        {

            DirectoryInfo dInfo = new DirectoryInfo(Path.Combine(Application.StartupPath, "Locales"));
            FileInfo[] localeFiles = dInfo.GetFiles("*.xml");
            if (localeFiles.Length == 0)
            {
                MessageBox.Show(@"Error - No o!RA locales exist! Re-install the application and try again.\nApplication will now exit.");
                Application.Exit();
            }
            //Get all the locales and put usercontrols on form
            foreach (FileInfo file in localeFiles)
            {
                string localeName = file.Name.Substring(0, file.Name.LastIndexOf(".", StringComparison.InvariantCulture));
                Stream str = ResourceHelper.GetResourceStream(localeName + ".gif");
                if (str != null)
                {
                    //Add usercontrol
                    GifBitmapDecoder gDecoder = new GifBitmapDecoder(str, BitmapCreateOptions.None, BitmapCacheOption.OnDemand);
                    LanguageBox lb = new LanguageBox { Locale = localeName, MouseOverImage = BitmapFromSource(gDecoder.Frames[1]), NormalImage = BitmapFromSource(gDecoder.Frames[0]), Visible = false};
                    Controls.Add(lb);
                    LocaleBoxes.Add(lb);
                }
            }
            PopulateForm(0);
        }

        private void PopulateForm(int start)
        {
            foreach (UserControl c in LocaleBoxes)
                c.Visible = false;
            if (start != 0)
            { } //Todo: Add previous button
            int count = LocaleBoxes.Count - start;

            int midY = ClientRectangle.Height / 2;
            int topMidY = ClientRectangle.Height / 4;
            int botMidY = 3 * ClientRectangle.Height / 4;

            switch (count)
            {
                case 1: case 2: case 3:
                {
                    //Middle row
                    int totalWidth = count * LocaleBoxWidth + count * LocaleBoxSpacing;
                    for (int i = LocaleBoxes.Count - count; i < LocaleBoxes.Count; i++)
                    {
                        LocaleBoxes[i].Location = new Point(Width / 2 - totalWidth / 2 + i * LocaleBoxWidth + i * LocaleBoxSpacing, midY - LocaleBoxHeight / 2);
                        LocaleBoxes[i].Visible = true;
                    }
                }
                    break;
                case 4: case 5: case 6:
                {
                    //Top and bottom rows
                    int totalBotWidth = (count - 3) * LocaleBoxWidth + (count - 3) * LocaleBoxSpacing;
                    for (int i = LocaleBoxes.Count - count; i < (LocaleBoxes.Count - count) + 3; i++)
                    {
                        //Fill top row
                        LocaleBoxes[i].Location = new Point(Width / 2 - MaxRowWidth / 2 + i * LocaleBoxWidth + i * LocaleBoxSpacing, topMidY - LocaleBoxHeight / 2);
                        LocaleBoxes[i].Visible = true;
                    }
                    for (int i = (LocaleBoxes.Count - count) + 3; i < LocaleBoxes.Count; i++)
                    {
                        //Fill bottom row
                        LocaleBoxes[i].Location = new Point(Width / 2 - totalBotWidth / 2 + i * LocaleBoxWidth + i * LocaleBoxSpacing, botMidY - LocaleBoxHeight / 2);
                        LocaleBoxes[i].Visible = true;
                    }
                }
                    break;
                case 7: case 8: case 9:
                    //All rows
                    int totalMidWidth = (count - 6) * LocaleBoxWidth + (count - 6) * LocaleBoxSpacing;
                    for (int i = LocaleBoxes.Count - count; i < (LocaleBoxes.Count - count) + 3; i++)
                    {
                        //Fill top row
                        LocaleBoxes[i].Location = new Point(Width / 2 - MaxRowWidth / 2 + i * LocaleBoxWidth + i * LocaleBoxSpacing, topMidY - LocaleBoxHeight / 2);
                        LocaleBoxes[i].Visible = true;
                    }
                    for (int i = (LocaleBoxes.Count - count) + 3; i < (LocaleBoxes.Count - count) + 6; i++)
                    {
                        //Fill bottom row
                        LocaleBoxes[i].Location = new Point(Width / 2 - MaxRowWidth / 2 + i * LocaleBoxWidth + i * LocaleBoxSpacing, botMidY - LocaleBoxHeight / 2);
                        LocaleBoxes[i].Visible = true;
                    }
                    for (int i = (LocaleBoxes.Count - count) + 6; i < LocaleBoxes.Count; i++)
                    {
                        //Fill bottom row
                        LocaleBoxes[i].Location = new Point(Width / 2 - totalMidWidth / 2 + i * LocaleBoxWidth + i * LocaleBoxSpacing, midY - LocaleBoxHeight / 2);
                        LocaleBoxes[i].Visible = true;
                    }
                    break;
            }
        }

        private void AddControl(UserControl control)
        {
            Controls.Add(control);
        }

        /// <summary>
        /// Generates a Bitmap object from a BitmapSource object.
        /// </summary>
        /// <param name="source">The bitmap source.</param>
        /// <returns>The bitmap object.</returns>
        private Bitmap BitmapFromSource(BitmapSource source)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(source));
                encoder.Save(stream);
                return new Bitmap(stream);
            }
        }

        private void LocaleSelectForm_Resize(object sender, EventArgs e)
        {
            PopulateForm(currentStart);
        }
    }
}
