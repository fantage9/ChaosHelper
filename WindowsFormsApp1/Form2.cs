using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace WindowsFormsApp1
{
    /* public class TransparentPanel : Panel
     {
         protected override CreateParams CreateParams
         {
             get
             {
                 CreateParams cp = base.CreateParams;
                 cp.ExStyle |= 0x00000020; //WS_EX_TRANSPARENT
                 return cp;
             }
         }

         protected override void OnPaintBackground(PaintEventArgs pevent)
         {
         }

         protected override void OnResize(EventArgs eventargs)
         {
             if (Parent == null)
                 return;

             Rectangle rc = new Rectangle(this.Location, this.Size);
             Parent.Invalidate(rc, true);
         }
     }*/

    public partial class Form2 : Form
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern UInt32 GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        [DllImport("user32.dll")]
        static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        public const int GWL_STYLE = -16;
        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_LAYERED = 0x80000;
        public const int WS_EX_TRANSPARENT = 0x20;
        public const int LWA_ALPHA = 0x2;
        public const int LWA_COLORKEY = 0x1;

        public Form1 F;
        public PictureBox label;
        double w;
        double h;
        OnjMessageFilter mf;

        int CPX = Cursor.Position.X;
        int CPY = Cursor.Position.Y;

        public Form2(Form1 fm)
        {
            InitializeComponent();
            fm = F;
            mf = new OnjMessageFilter();
            //SetWindowLong(this.Handle, GWL_EXSTYLE, (IntPtr)(GetWindowLong(this.Handle, GWL_EXSTYLE) ^ WS_EX_LAYERED ^ WS_EX_TRANSPARENT));
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            //Invalidate();
        }

        public void Form2_draw(Rectangle r, bool[] arr, int stash)
        {
            int unit = stash * 12;

            Console.WriteLine(stash);

            w = Convert.ToDouble(r.Width) / unit;
            h = Convert.ToDouble(r.Height) / unit;
            this.Controls.Clear();

            for (int j = 0; j < unit; j ++)
            {
                for (int i = 0; i < unit; i ++)
                {
                    if (arr[unit * j + i])
                        continue;
                    
                    label = new PictureBox();
                    label.MouseClick += new MouseEventHandler(btnClick);
                    label.Location = new Point(Convert.ToInt32(w * i), Convert.ToInt32(h * j));
                    label.Name = "label" + i * 100 + j;
                    label.Size = new Size(Convert.ToInt32(w), Convert.ToInt32(h));
                    label.TabIndex = i;
                    label.BackColor = Color.FromArgb(64,0,0,0);
                    SetWindowLong(label.Handle, GWL_EXSTYLE, (IntPtr)(GetWindowLong(label.Handle, GWL_EXSTYLE) ^ WS_EX_LAYERED ^ WS_EX_TRANSPARENT));

                    Image img = Properties.Resources.Bitmap1;
                    label.Image = ChangeOpacity(img, 0.5f);
                    img.Dispose();
                    this.Controls.Add(label);
                }
            }
            //Application.AddMessageFilter(mf);
        }

        private const int SW_SHOWNORMAL = 1;
        void btnClick(object sender, EventArgs e)
        {
            IntPtr hWnd = FindWindow(null, "Path of Exile");
            ShowWindowAsync(hWnd, SW_SHOWNORMAL);
            SetForegroundWindow(hWnd);
        }

        public void Form2_start()
        {
            //Application.AddMessageFilter(mf);
        }

        private void Form2_Paint(System.Object sender, System.Windows.Forms.PaintEventArgs e)
        {

            //Invalidate();
        }
        public Bitmap ChangeOpacity(Image img, float opacityvalue)
        {
            Bitmap bmp = new Bitmap(img.Width, img.Height);
            Graphics graphics = Graphics.FromImage(bmp);
            ColorMatrix colormatrix = new ColorMatrix();
            colormatrix.Matrix33 = opacityvalue;
            ImageAttributes imgAttribute = new ImageAttributes();
            imgAttribute.SetColorMatrix(colormatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            graphics.DrawImage(img, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, imgAttribute);
            graphics.Dispose();
            return bmp;
        }

        private void Form2_MouseDown(object sender, MouseEventArgs e)
        {

            //Application.RemoveMessageFilter(mf);
            return;
        }

    }

    class OnjMessageFilter : IMessageFilter
    {
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
        //응용프로그램에서 받은 마우스 클릭 이벤트를 필터링 시킴 

        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMINIMIZED = 2;
        private const int SW_SHOWMAXIMIZED = 3;

        public bool PreFilterMessage(ref Message m)
        {
            //마우스 클릭시 필터링 시킴  
            if (m.Msg == 0x201)
            {
                IntPtr hWnd = FindWindow(null, "Path of Exile");
                ShowWindowAsync(hWnd, SW_SHOWNORMAL);
                SetForegroundWindow(hWnd);
                //MessageBox.Show("마우스 클릭 -> 필터링됨...");
                //필터에서 처리했으니 응용프로그램에서 처리안해도된다는 의미
                //Form에 걸려 있는 Click 이벤트 동작안함.... 즉 윈도우가 종료되지않는다
                //이 부분을 false로 바꾼 후 실행해 보라
                return true;
            }
            return false;
        }
    }
}
