using System;
using System.Collections.Generic;
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
        const int LocaleBoxWidth = 155;
        const int LocaleBoxHeight = 100;

        const int MaxHeight = 3 * LocaleBoxHeight;
        const int MaxWidth = 3 * LocaleBoxWidth;

        readonly List<UserControl> LocaleBoxes = new List<UserControl>();
        Size BorderSize;
        int currentStart;


        private void LocaleSelectForm_Load(object sender, EventArgs e)
        {
            BorderSize = Size - ClientRectangle.Size;
            Size = new Size(MaxWidth + 2 * BorderSize.Width, MaxHeight + BorderSize.Height + BorderSize.Width + 40);

            DirectoryInfo dInfo = new DirectoryInfo(Path.Combine(Application.StartupPath, "Locales"));
            FileInfo[] localeFiles = dInfo.GetFiles("*.xml");
            if (localeFiles.Length == 0)
            {
                MessageBox.Show(@"Error - No o!RA locales exist! Re-install the application and try again.\o!RA will now exit.");
                Application.Exit();
            }
            //Get all the locales and put usercontrols on form
            foreach (FileInfo file in localeFiles)
            {
                string localeName = file.Name.Substring(0, file.Name.LastIndexOf(".", StringComparison.InvariantCulture));
                Stream str = ResourceHelper.GetResourceStream("flag_" + localeName + ".gif");
                if (str != null) 
                {
                    //Add usercontrol
                    GifBitmapDecoder gDecoder = new GifBitmapDecoder(str, BitmapCreateOptions.None, BitmapCacheOption.OnDemand);
                    LanguageBox lb = new LanguageBox { Locale = localeName, MouseOverImage = BitmapFromSource(gDecoder.Frames[1]), NormalImage = BitmapFromSource(gDecoder.Frames[0]) };
                    Controls.Add(lb);
                    LocaleBoxes.Add(lb); 
                }
            }
            currentStart = 0;
            PopulateForm();
        }

        private void PopulateForm()
        {
            int count = Math.Min(LocaleBoxes.Count - currentStart, 9);
            PrevLbl.Visible = currentStart != 0;
            NextLbl.Visible = (currentStart + 9) <= LocaleBoxes.Count;

            switch (count)
            {
                case 1: case 2: case 3:
                {
                    //Middle row
                    int totalWidth = count * LocaleBoxWidth;
                    for (int i = LocaleBoxes.Count - count; i < count; i++)
                    {
                        LocaleBoxes[i].Location = new Point(ClientRectangle.Width / 2 - totalWidth / 2 + i * LocaleBoxWidth, BorderSize.Width / 2 + MaxHeight / 2 - LocaleBoxHeight / 2);
                    }
                }
                    break;
                case 4: case 5: case 6:
                {
                    //Top and bottom rows
                    int totalBotWidth = (count - 3) * LocaleBoxWidth;
                    for (int i = 0; i < 3; i++)
                    {
                        //Fill top row
                        LocaleBoxes[i].Location = new Point(ClientRectangle.Width / 2 - MaxWidth / 2 + i * LocaleBoxWidth, BorderSize.Width / 2 + MaxHeight / 2 - 3 * LocaleBoxHeight / 2);
                    }
                    for (int i = 3; i < LocaleBoxes.Count; i++)
                    {
                        //Fill bottom row
                        LocaleBoxes[i].Location = new Point(ClientRectangle.Width / 2 - totalBotWidth / 2 + (i - 3) * LocaleBoxWidth, BorderSize.Width / 2 + MaxHeight / 2 + LocaleBoxHeight / 2);
                    }
                }
                    break;
                case 7: case 8: case 9:
                    //All rows
                    int totalMidWidth = (count - 6) * LocaleBoxWidth;
                    for (int i = 0; i < 3; i++)
                    {
                        //Fill top row
                        LocaleBoxes[i].Location = new Point(ClientRectangle.Width / 2 - MaxWidth / 2 + i * LocaleBoxWidth, BorderSize.Width / 2 + MaxHeight / 2 - 3 * LocaleBoxHeight / 2);
                        //Fill bottom row
                        LocaleBoxes[i + 3].Location = new Point(ClientRectangle.Width / 2 - MaxWidth / 2 + i * LocaleBoxWidth, BorderSize.Width / 2 + MaxHeight / 2 + LocaleBoxHeight / 2);
                    }
                    for (int i = 6; i < LocaleBoxes.Count; i++)
                    {
                        //Fill middle row
                        LocaleBoxes[i].Location = new Point(ClientRectangle.Width / 2 - totalMidWidth / 2 + (i - 6) * LocaleBoxWidth, BorderSize.Width / 2 + MaxHeight / 2 - LocaleBoxHeight / 2);
                    }
                    break;
            }
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
            PopulateForm();
        }

        private void NextLbl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            currentStart += 9;
            PopulateForm();
        }

        private void PrevLbl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            currentStart -= 9;
            PopulateForm();
        }
    }
}
