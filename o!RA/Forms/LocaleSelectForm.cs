using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using oRAResources;
using o_RA.Controls;

namespace o_RA.Forms
{
    public partial class LocaleSelectForm : Form
    {
        public LocaleSelectForm()
        {
            InitializeComponent();
        }

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
                    LanguageBox lb = new LanguageBox { Locale = localeName, MouseOverImage = BitmapFromSource(gDecoder.Frames[1]), NormalImage = BitmapFromSource(gDecoder.Frames[0]) };
                    if (lb.Locale == "en")
                        AddControl(lb);
                }
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
    }
}
