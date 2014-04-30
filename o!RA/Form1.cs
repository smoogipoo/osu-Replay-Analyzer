using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace o_RA
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Bitmap b = new Bitmap(60, 60);
            Graphics g = Graphics.FromImage(b);
            g.FillRectangle(Brushes.White, 0, 0, 60, 60);

            oRAPage p = new oRAPage();
            p.Contents = new UserControl();
            ((UserControl)p.Contents).Controls.Add(new TextBox());
            p.Description = "a";
            p.Name = "b";
            p.Icon = b;
            oRATabControl1.Add(p);

            oRAPage p1 = new oRAPage();
            p1.Contents = new UserControl();
            ((UserControl)p1.Contents).Controls.Add(new TextBox());
            p1.Description = "a";
            p1.Name = "b";
            p1.Icon = b;
            oRATabControl1.Add(p1);
        }
    }
}
