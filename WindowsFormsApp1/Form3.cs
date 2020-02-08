using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form3 : Form
    {
        Form1 F;
        public Form3(Form1 f1)
        {
            InitializeComponent();
            F = f1;
            chaos = -1;
        }
        public int chaos;
        public void ChangeValues(
            int y_W,
            int y_helmet,
            int y_chest,
            int y_boots,
            int y_gloves,
            int y_amulet,
            int y_ring,
            int y_belt,
            int n_W,
            int n_helmet,
            int n_chest,
            int n_boots,
            int n_gloves,
            int n_amulet,
            int n_ring,
            int n_belt
            )   
        {
            label14.Text = (((float)y_W)/2).ToString();
            label13.Text = y_helmet.ToString();
            label12.Text = y_chest.ToString();
            label11.Text = y_gloves.ToString();
            label10.Text = y_boots.ToString();
            label9.Text = y_amulet.ToString();
            label8.Text = y_ring.ToString();
            label23.Text = y_belt.ToString();

            label21.Text = (((float)n_W) / 2).ToString();
            label20.Text = n_helmet.ToString();
            label19.Text = n_chest.ToString();
            label18.Text = n_gloves.ToString();
            label17.Text = n_boots.ToString();
            label16.Text = n_amulet.ToString();
            label15.Text = n_ring.ToString();
            label24.Text = n_belt.ToString();

            label35.Text = ((((float)y_W) / 2)+(((float)n_W) / 2)).ToString();
            label34.Text = (y_helmet+n_helmet).ToString();
            label33.Text = (y_chest+n_chest).ToString();
            label32.Text = (y_gloves+n_gloves).ToString();
            label31.Text = (y_boots+n_boots).ToString();
            label30.Text = (y_amulet+n_amulet).ToString();
            label28.Text = (y_ring+n_ring).ToString();
            label29.Text = (y_belt+n_belt).ToString();

            int min_chaos = 999999;
            if ((y_W / 2 + y_amulet + y_belt + y_boots + y_chest + y_gloves + y_helmet + y_ring / 2) == 0)
                min_chaos = 0;
            if (min_chaos > ((((float)y_W) / 2) + (((float)n_W) / 2)))
                min_chaos = (int)((((float)y_W) / 2) + (((float)n_W) / 2));
            if (min_chaos > (y_helmet + n_helmet))
                min_chaos = (int)(y_helmet + n_helmet);
            if (min_chaos > (y_chest + n_chest))
                min_chaos = (int)(y_chest + n_chest);
            if (min_chaos > (y_gloves + n_gloves))
                min_chaos = (int)(y_gloves + n_gloves);
            if (min_chaos > (y_boots + n_boots))
                min_chaos = (int)(y_boots + n_boots);
            if (min_chaos > (y_amulet + n_amulet))
                min_chaos = (int)(y_amulet + n_amulet);
            if (min_chaos > ((y_ring + n_ring) / 2))
                min_chaos = (int)((y_ring + n_ring)/2);
            if (min_chaos > (y_belt + n_belt))
                min_chaos = (int)(y_belt + n_belt);
            label37.Text = (2*min_chaos).ToString();
            if (chaos != min_chaos && chaos != -1 && F.checkBox1.Checked) {
                timer1.Enabled = true;
                timer1.Interval = 500;
                pictureBox2.Visible = true;
                timer1.Start();
            }
            //if (chaos != min_chaos) {
            //    currentIndex = 0;
            //}
            chaos = min_chaos;
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            pictureBox2.Visible = false;
            timer1.Stop();
            timer1.Enabled = false;
        }
    }
}
