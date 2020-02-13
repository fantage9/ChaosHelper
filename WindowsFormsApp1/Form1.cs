using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public string test(string poesessid)
        {
            //IEnumerable<Tuple<string, string>> results = ReadCookies(".pathofexile.com");
            //IEnumerable<Tuple<string, string>> results = ReadCookies(".poe.game.daum.net");
            /*Console.WriteLine(poesessid);
            Tuple<string, string> result;
            try
            {
                result = results.Where(x => x.Item1 == "POESESSID").First();
            }
            catch
            {
                MessageBox.Show("로그인을 크롬으로 해주세요");
                return "";
            }
            Console.WriteLine(result.Item2);
            MessageBox.Show(result.Item2);
            */

            try
            {
                return getJSON(poesessid);
            }
            catch (Exception e)
            {
                Button2_Click(null, null);
                throw e;
            }
        }

        public string getJSON(string POECookie)
        {
            var cookies = new CookieContainer();
            Cookie c = new Cookie("POESESSID", POECookie, "/", ".pathofexile.com");
            cookies.Add(c);

            string accountUrl = "https://www.pathofexile.com/my-account";
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(accountUrl);
                request.CookieContainer = cookies;
                request.AllowAutoRedirect = false;

                Trace.WriteLine(" ### Login : " + request.RequestUri);

                WebResponse response = request.GetResponse();

                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    string resultString = reader.ReadToEnd();
                    Trace.WriteLine(" ### Login Response : " + resultString);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(" ### Login Error : " + e);
                MessageBox.Show("로그인에 실패하였습니다.\n" + e);
                throw e;
            }

            string accountName = "";

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(accountUrl);
                request.CookieContainer = cookies;

                Trace.WriteLine(" ### GetAccountName : " + request.RequestUri);

                WebResponse response = request.GetResponse();

                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    string resultString = reader.ReadToEnd();
                    Trace.WriteLine(" ### GetAccountName Response : " + resultString);
                    //string regExp = "\\/account\\/view-profile\\/(.*?)\\"";
                    string pattern = "\\/account\\/view-profile\\/(.*?)\"";

                    var matchResult = Regex.Match(resultString, pattern);
                    if (matchResult.Groups.Count < 2)
                    {
                        throw new Exception("matchResult length error");
                    }

                    accountName = System.Web.HttpUtility.UrlDecode(matchResult.Groups[1].Value);

                    Trace.WriteLine("accountName  : " + accountName);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(" ### GetAccountName Error : " + e);
                MessageBox.Show("계정명을 불러오는데 실패하였습니다.\n" + e);
                throw e;
            }

            textBox1.Text = textBox1.Text.Replace(" ", "");
            textBox2.Text = textBox2.Text.Replace(" ", "");
            textBox3.Text = textBox3.Text.Replace(" ", "");
            //string accountName = System.Web.HttpUtility.UrlEncode(textBox1.Text);

            string tabidx = textBox2.Text.Replace(" ", "");
            string league = textBox4.Text.Replace(" ", "");
            string url = "https://www.pathofexile.com/character-window/get-stash-items?league=" + league + "&tabs=0&tabIndex=" + tabidx + "&accountName=" + accountName;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.CookieContainer = cookies;
                Trace.WriteLine(" ### GetStashes : " + request.RequestUri);

                WebResponse response = request.GetResponse();

                using(Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    string resultString = reader.ReadToEnd();
                    Trace.WriteLine(" ### GetStashes Response : " + resultString);
                    return resultString;
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(" ### ERROR : " + e);
                MessageBox.Show("창고를 불러오는데 실패하였습니다.\n" + e);
                throw e;
            }
        }

        public IEnumerable<Tuple<string, string>> ReadCookies(string hostName)
        {
            var dbPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Google\Chrome\User Data\Default\Cookies";
            if (!File.Exists(dbPath))
            {
                Console.WriteLine("Folder doesn't exist");
            }
            else
            {
                var connectionString = "Data Source=" + dbPath + ";pooling=false";
                var conn = new System.Data.SQLite.SQLiteConnection(connectionString);
                conn.BusyTimeout = 0;
                var cmd = conn.CreateCommand();

                var prm = cmd.CreateParameter();
                prm.ParameterName = "hostName";
                prm.Value = hostName;
                cmd.Parameters.Add(prm);

                cmd.CommandText = "SELECT name, encrypted_value FROM cookies WHERE host_key = @hostName";
                conn.Open();
                var reader = cmd.ExecuteReader();
                while(reader.Read())
                {
                    var encryptedData = (byte[])reader[1];
                    var decodedData = System.Security.Cryptography.ProtectedData.Unprotect(encryptedData, null, System.Security.Cryptography.DataProtectionScope.CurrentUser);
                    var plainText = Encoding.ASCII.GetString(decodedData);

                    yield return Tuple.Create(reader.GetString(0), plainText);
                }
                conn.Close();
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern UInt32 GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        [DllImport("user32.dll")]
        static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        public const int GWL_STYLE = -16;
        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_LAYERED = 0x80000;
        public const int WS_EX_TRANSPARENT = 0x20;
        public const int LWA_ALPHA = 0x2;
        public const int LWA_COLORKEY = 0x1;

        //핫키등록
        [DllImport("user32.dll")]
        private static extern int RegisterHotKey(int hwnd, int id, int fsModifiers, int vk);

        //핫키제거
        [DllImport("user32.dll")]
        private static extern int UnregisterHotKey(int hwnd, int id);

        [DllImport("User32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("User32.dll")]
        public static extern void ReleaseDC(IntPtr hwnd, IntPtr dc);



        public const int HOTKEYGap = 0x0312;

        public int left = 15;
        public int top = 160;
        public int right = 650;
        public int bottom = 795;

        int stash;
        int loaded;
        

        int currentIndex = 0;

        Form2 f2;
        Form3 f3;

        public Form1()
        {
            // 0x0 : 조합키 없이 사용, 0x1: ALT, 0x2: Ctrl, 0x3: Shift
            //RegisterHotKey(핸들러함수, 등록키의_ID, 조합키, 등록할_키)
            RegisterHotKey((int)this.Handle, 0, 0x2, (int)Keys.F9);
            RegisterHotKey((int)this.Handle, 1, 0x2, (int)Keys.F10);
            RegisterHotKey((int)this.Handle, 2, 0x2, (int)Keys.F11);
            RegisterHotKey((int)this.Handle, 3, 0x2, (int)Keys.F12);

            RegisterHotKey((int)this.Handle, 4, 0x2, (int)Keys.F7);
            RegisterHotKey((int)this.Handle, 5, 0x2, (int)Keys.F8);

            RegisterHotKey((int)this.Handle, 6, 0x2, (int)Keys.F6);

            InitializeComponent();
            f2 = new Form2(this);
            f3 = new Form3(this);
            //timer1.Enabled = false;
            //timer1.Interval = 5000; //5000ms

            //f3.BackColor = f3.TransparencyKey;
            f3.TopMost = true;
            f3.Show();
            f3.Location = new Point(Screen.PrimaryScreen.Bounds.Width - 355, 0);
            Application.EnableVisualStyles();
            SetWindowLong(f3.Handle, GWL_EXSTYLE, (IntPtr)(GetWindowLong(f3.Handle, GWL_EXSTYLE) ^ WS_EX_LAYERED ^ WS_EX_TRANSPARENT));
            int value = trackBar1.Value;
            SetLayeredWindowAttributes(f3.Handle, 0, (Byte)value, LWA_ALPHA);

            stash = 2;
            textBox4.Text = "metamorph";

            comboBox2.SelectedIndex = 2;
            comboBox3.SelectedIndex = 2;
            comboBox4.SelectedIndex = 2;
            comboBox5.SelectedIndex = 2;
            comboBox6.SelectedIndex = 2;
            comboBox7.SelectedIndex = 2;
            comboBox1.SelectedIndex = 2;

            comboBox8.SelectedIndex = 8;
            comboBox9.SelectedIndex = 9;
            comboBox10.SelectedIndex = 10;
            comboBox11.SelectedIndex = 11;
            comboBox12.SelectedIndex = 6;
            comboBox13.SelectedIndex = 7;
            comboBox14.SelectedIndex = 5;

            Items.y_W1 = new List<Item>();
            Items.y_W2 = new List<Item>();
            Items.y_helmet = new List<Item>();
            Items.y_chest = new List<Item>();
            Items.y_boots = new List<Item>();
            Items.y_gloves = new List<Item>();
            Items.y_amulet = new List<Item>();
            Items.y_ring = new List<Item>();
            Items.y_belt = new List<Item>();
            Items.n_W1 = new List<Item>();
            Items.n_W2 = new List<Item>();
            Items.n_helmet = new List<Item>();
            Items.n_chest = new List<Item>();
            Items.n_boots = new List<Item>();
            Items.n_gloves = new List<Item>();
            Items.n_amulet = new List<Item>();
            Items.n_ring = new List<Item>();
            Items.n_belt = new List<Item>();
            loaded = 0;

            
            if (File.Exists(@".\myConfig.ini"))
            {
                string[] lines = File.ReadAllLines(@".\myConfig.ini");
                //Console.WriteLine(lines.Length);
                if (lines.Length < 23)
                {
                    MessageBox.Show("Config file is not exist or Damaged. Load Default Settings.\n myConfing.ini 파일이 없거나 손상되었습니다. 기본값을 불러옵니다.");
                    return;
                }
                textBox1.Text = lines[0];
                textBox2.Text = lines[1];
                textBox3.Text = lines[2];
                textBox4.Text = lines[3];

                comboBox2.SelectedIndex = Convert.ToInt32(lines[4]);
                comboBox3.SelectedIndex = Convert.ToInt32(lines[5]);
                comboBox4.SelectedIndex = Convert.ToInt32(lines[6]);
                comboBox5.SelectedIndex = Convert.ToInt32(lines[7]);
                comboBox6.SelectedIndex = Convert.ToInt32(lines[8]);
                comboBox7.SelectedIndex = Convert.ToInt32(lines[9]);
                comboBox8.SelectedIndex = Convert.ToInt32(lines[10]);
                comboBox9.SelectedIndex = Convert.ToInt32(lines[11]);
                comboBox10.SelectedIndex = Convert.ToInt32(lines[12]);
                comboBox11.SelectedIndex = Convert.ToInt32(lines[13]);
                comboBox12.SelectedIndex = Convert.ToInt32(lines[14]);
                comboBox13.SelectedIndex = Convert.ToInt32(lines[15]);

                left = Convert.ToInt32(lines[16]);
                top = Convert.ToInt32(lines[17]);
                right = Convert.ToInt32(lines[18]);
                bottom = Convert.ToInt32(lines[19]);
                stash = Convert.ToInt32(lines[20]);
                if (stash == 1)
                {
                    radioButton1.Checked = false;
                    radioButton2.Checked = true;
                }
                else if(stash == 2)
                {
                    radioButton1.Checked = true;
                    radioButton2.Checked = false;
                }
                trackBar1.Value = Convert.ToInt32(lines[21]);
                checkBox1.Checked = Convert.ToBoolean(lines[22]);
                label1.Text = "(" + left + ", " + top + ")";
                label2.Text = "(" + right + ", " + bottom + ")";

                if (lines.Length > 23)
                    comboBox1.SelectedIndex = Convert.ToInt32(lines[23]);
                if (lines.Length > 24)
                    comboBox14.SelectedIndex = Convert.ToInt32(lines[24]);
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            UnregisterHotKey((int)this.Handle, 0); //이 폼에 ID가 0인 핫키 해제
            UnregisterHotKey((int)this.Handle, 1);
            UnregisterHotKey((int)this.Handle, 2);
            UnregisterHotKey((int)this.Handle, 3);
            UnregisterHotKey((int)this.Handle, 4);
            UnregisterHotKey((int)this.Handle, 5);
            UnregisterHotKey((int)this.Handle, 6);
        }

        // 윈도우프로시저 콜백함수
        protected override void WndProc(ref Message m)
        {

            if (m.Msg == (int)0x312) //핫키가 눌러지면 312 정수 메세지를 받게됨
            {
                if (m.WParam == (IntPtr)0x0) // 그 키의 ID가 0이면
                {
                    label1.Text = "(" + Control.MousePosition.X + ", " + Control.MousePosition.Y + ")";
                    left = Control.MousePosition.X;
                    top = Control.MousePosition.Y;
                    MessageBox.Show("Stash L/T position set\n창고 좌상단 좌표가 지정되었습니다.");
                }

                if (m.WParam == (IntPtr)0x1) // 그 키의 ID가 1이면
                {
                    label2.Text = "(" + Control.MousePosition.X + ", " + Control.MousePosition.Y + ")";
                    right = Control.MousePosition.X;
                    bottom = Control.MousePosition.Y;
                    MessageBox.Show("Stash R/B position set\n창고 우하단 좌표가 지정되었습니다.");
                }

                if (m.WParam == (IntPtr)0x6) // 그 키의 ID가 6이면 - 새로운 Map 작성
                {
                    if (loaded == 1)
                    {
                        ResetItemSet();
                    }
                    else
                    {
                        MessageBox.Show("먼저 캐릭터를 불러오세요. Please Load Character");
                    }
                }

                if (m.WParam == (IntPtr)0x2) // 그 키의 ID가 2이면 - 다음 세트 보여주기
                {
                    if (loaded == 1)
                    {
                        ShowItemSet(true);
                    }
                }

                if (m.WParam == (IntPtr)0x3) // 그 키의 ID가 3이면
                {
                    f2.Hide();
                }

                if (m.WParam == (IntPtr)0x4) // 그 키의 ID가 4이면
                {
                    //MessageBox.Show("Bank On Test");
                    f3.Show();
                }

                if (m.WParam == (IntPtr)0x5) // 그 키의 ID가 5이면
                {
                    //MessageBox.Show("Bank Off Test");
                    f3.Hide();
                }
            }
            base.WndProc(ref m);
        }

        bool ResetItemSet()
        {
            try
            {
                LoadItems();

                Items.itemSetList.Clear();

                Trace.WriteLine("f3.chaos : " + f3.chaos);

                for (int i = 0; i < f3.chaos; i++)
                {
                    List<Item> newItemSet = new List<Item>();
                    bool did = false;
                    did = PopWeapon(did, newItemSet);
                    did = PopItem(did, Items.n_amulet, Items.y_amulet, newItemSet);
                    did = PopItem(did, Items.n_belt, Items.y_belt, newItemSet);
                    did = PopItem(did, Items.n_boots, Items.y_boots, newItemSet);
                    did = PopItem(did, Items.n_chest, Items.y_chest, newItemSet);
                    did = PopItem(did, Items.n_gloves, Items.y_gloves, newItemSet);
                    did = PopItem(did, Items.n_helmet, Items.y_helmet, newItemSet);
                    did = PopItem(did, Items.n_ring, Items.y_ring, newItemSet);
                    did = PopItem(did, Items.n_ring, Items.y_ring, newItemSet);
                
                    Items.itemSetList.Add(newItemSet);
                }

                Trace.WriteLine("Item Set Count : " + Items.itemSetList.Count());

                currentIndex = Items.itemSetList.Count() - 1;

                ShowItemSet(false);

                return true;

            }
            catch (Exception e)
            {
                Trace.WriteLine(" ##ResetItemSet ERROR : " + e);

                return false;
            }
        }

        public bool PopWeapon(bool did, List<Item> itemSet)
        {
            int x = 0;
            int y = 0;
            int w = 0;
            int h = 0;
            if (did)
            {
                if (Items.n_W2.Count > 0)
                {
                    x = Items.n_W2.First().get_x();
                    y = Items.n_W2.First().get_y();
                    w = Items.n_W2.First().get_w();
                    h = Items.n_W2.First().get_h();
                    Items.n_W2.RemoveAt(0);
                }
                else if (Items.n_W1.Count + Items.y_W1.Count > 1)
                {
                    if (Items.n_W1.Count > 0)
                    {
                        x = Items.n_W1.First().get_x();
                        y = Items.n_W1.First().get_y();
                        w = Items.n_W1.First().get_w();
                        h = Items.n_W1.First().get_h();
                        Items.n_W1.RemoveAt(0);
                    }
                    else if (Items.y_W1.Count > 0)
                    {
                        x = Items.y_W1.First().get_x();
                        y = Items.y_W1.First().get_y();
                        w = Items.y_W1.First().get_w();
                        h = Items.y_W1.First().get_h();
                        Items.y_W1.RemoveAt(0);
                    }

                    itemSet.Add(new Item(x, y, w, h));

                    if (Items.n_W1.Count > 0)
                    {
                        x = Items.n_W1.First().get_x();
                        y = Items.n_W1.First().get_y();
                        w = Items.n_W1.First().get_w();
                        h = Items.n_W1.First().get_h();
                        Items.n_W1.RemoveAt(0);
                    }
                    else if (Items.y_W1.Count > 0)
                    {
                        x = Items.y_W1.First().get_x();
                        y = Items.y_W1.First().get_y();
                        w = Items.y_W1.First().get_w();
                        h = Items.y_W1.First().get_h();
                        Items.y_W1.RemoveAt(0);
                    }
                }
                else if (Items.y_W2.Count > 0)
                {
                    x = Items.y_W2.First().get_x();
                    y = Items.y_W2.First().get_y();
                    w = Items.y_W2.First().get_w();
                    h = Items.y_W2.First().get_h();
                    Items.y_W2.RemoveAt(0);
                }
            }
            else
            {
                if (Items.y_W2.Count > 0)
                {
                    x = Items.y_W2.First().get_x();
                    y = Items.y_W2.First().get_y();
                    w = Items.y_W2.First().get_w();
                    h = Items.y_W2.First().get_h();
                    Items.y_W2.RemoveAt(0);
                    did = true;
                }
                else if (Items.n_W1.Count + Items.y_W1.Count > 1)
                {
                    if (Items.y_W1.Count > 0)
                    {
                        x = Items.y_W1.First().get_x();
                        y = Items.y_W1.First().get_y();
                        w = Items.y_W1.First().get_w();
                        h = Items.y_W1.First().get_h();
                        Items.y_W1.RemoveAt(0);
                        did = true;
                    }
                    else if (Items.n_W1.Count > 0)
                    {
                        x = Items.n_W1.First().get_x();
                        y = Items.n_W1.First().get_y();
                        w = Items.n_W1.First().get_w();
                        h = Items.n_W1.First().get_h();
                        Items.n_W1.RemoveAt(0);
                    }

                    itemSet.Add(new Item(x, y, w, h));

                    if (did)
                    {
                        if (Items.n_W1.Count > 0)
                        {
                            x = Items.n_W1.First().get_x();
                            y = Items.n_W1.First().get_y();
                            w = Items.n_W1.First().get_w();
                            h = Items.n_W1.First().get_h();
                            Items.n_W1.RemoveAt(0);
                        }
                        else if (Items.y_W1.Count > 0)
                        {
                            x = Items.y_W1.First().get_x();
                            y = Items.y_W1.First().get_y();
                            w = Items.y_W1.First().get_w();
                            h = Items.y_W1.First().get_h();
                            Items.y_W1.RemoveAt(0);
                            did = true;
                        }
                    }
                    else
                    {
                        if (Items.y_W1.Count > 0)
                        {
                            x = Items.y_W1.First().get_x();
                            y = Items.y_W1.First().get_y();
                            w = Items.y_W1.First().get_w();
                            h = Items.y_W1.First().get_h();
                            Items.y_W1.RemoveAt(0);
                            did = true;
                        }
                        else if (Items.n_W1.Count > 0)
                        {
                            x = Items.n_W1.First().get_x();
                            y = Items.n_W1.First().get_y();
                            w = Items.n_W1.First().get_w();
                            h = Items.n_W1.First().get_h();
                            Items.n_W1.RemoveAt(0);
                        }
                    }
                }
                else if (Items.n_W2.Count > 0)
                {
                    x = Items.n_W2.First().get_x();
                    y = Items.n_W2.First().get_y();
                    w = Items.n_W2.First().get_w();
                    h = Items.n_W2.First().get_h();
                    Items.n_W2.RemoveAt(0);
                }
            }

            itemSet.Add(new Item(x, y, w, h));

            return did;
        }

        public bool PopItem(bool did, List<Item> itemsN, List<Item> itemY, List<Item> itemSet)
        {
            int x = 0;
            int y = 0;
            int w = 0;
            int h = 0;
            if (did)
            {
                if (itemsN.Count > 0)
                {
                    x = itemsN.First().get_x();
                    y = itemsN.First().get_y();
                    w = itemsN.First().get_w();
                    h = itemsN.First().get_h();
                    itemsN.RemoveAt(0);
                }
                else if (itemY.Count > 0)
                {
                    x = itemY.First().get_x();
                    y = itemY.First().get_y();
                    w = itemY.First().get_w();
                    h = itemY.First().get_h();
                    itemY.RemoveAt(0);
                }
            }
            else
            {
                if (itemY.Count > 0)
                {
                    x = itemY.First().get_x();
                    y = itemY.First().get_y();
                    w = itemY.First().get_w();
                    h = itemY.First().get_h();
                    itemY.RemoveAt(0);
                    did = true;
                }
                else if (Items.n_amulet.Count > 0)
                {
                    x = itemsN.First().get_x();
                    y = itemsN.First().get_y();
                    w = itemsN.First().get_w();
                    h = itemsN.First().get_h();
                    itemsN.RemoveAt(0);
                }
            }

            itemSet.Add(new Item(x, y, w, h));

            return did;
        }

        void ShowItemSet(bool next)
        {
            f2.Controls.Clear();
            f2.Show();
            if (Items.itemSetList.Count() > 0)
            {
                if (next)
                {
                    currentIndex++;
                }
                
                if (currentIndex >= Items.itemSetList.Count())
                {
                    currentIndex = 0;
                }

                Trace.WriteLine("currentIndex : " + currentIndex);

                List<Item> showingItemSet = Items.itemSetList[currentIndex];
                Trace.WriteLine("showingItemSet  Count : " + showingItemSet.Count());

                bool[] arr = new bool[576];

                Rectangle r = new Rectangle(left, top, right - left, bottom - top);

                f2.Bounds = r;
                f2.TopMost = true;
                f2.Location = new Point(left, top);

                foreach (Item item in showingItemSet)
                {
                    make_hole(item.x, item.y, item.w, item.h, arr);
                }

                f2.Form2_draw(r, arr, stash);
            } else
            {
                bool[] arr = new bool[576];

                Rectangle r = new Rectangle(left, top, right - left, bottom - top);

                f2.Bounds = r;
                f2.TopMost = true;
                f2.Location = new Point(left, top);

                f2.Form2_draw(r, arr, stash);
            }
            
            f2.Show();

            Trace.WriteLine("ShowItemSet Done");
        }

        public void make_hole(int x, int y, int w, int h, bool[] arr)
        {
            //Console.WriteLine("{0}, {1}, {2}, {3}", x, y, w, h);
            for (int i = x; i < x + w; i++) 
            {
                for (int j = y; j < y + h; j++)
                {
                    arr[j * stash * 12 + i] = true;
                }
            }
        }

        private void RadioButton1_CheckedChanged(object sender, EventArgs e)
        {
            stash = 2;
        }

        private void RadioButton2_CheckedChanged(object sender, EventArgs e)
        {
            stash = 1;
        }

        private void TextBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))
            {
                e.Handled = true;
            }
        }

        public String[] parse(String url)
        {
            char[] sep = { '/', ' ' };
            String[] ret_val = {"","",};
            string[] result = url.Split(sep);
            int checkval = 0;
            foreach (var item in result)
            {
                if (checkval == 2) { ret_val[1] = item; checkval = 3; }
                if (checkval == 1) { ret_val[0] = item; checkval = 2; }
                if ("2DItems" == item) checkval = 1;
                
            }
            return ret_val;
        }

        public void Button1_Click(object sender, EventArgs e)       // Load
        {
            //textBox1.Enabled = false;
            textBox2.Enabled = false;
            textBox3.Enabled = false;
            textBox4.Enabled = false;
            radioButton1.Enabled = false;
            radioButton2.Enabled = false;

            if (ResetItemSet())
            {
                SaveAndApply();
            }
        }

        private void LoadItems() { 
            string poesessid = textBox3.Text;
            string json = test(poesessid);
            if (json.Length != 0)
            {
                loaded = 1;
                Items.y_W1.Clear();
                Items.y_W2.Clear();
                Items.y_helmet.Clear();
                Items.y_chest.Clear();
                Items.y_boots.Clear();
                Items.y_gloves.Clear();
                Items.y_amulet.Clear();
                Items.y_ring.Clear();
                Items.y_belt.Clear();
                Items.n_W1.Clear();
                Items.n_W2.Clear();
                Items.n_helmet.Clear();
                Items.n_chest.Clear();
                Items.n_boots.Clear();
                Items.n_gloves.Clear();
                Items.n_amulet.Clear();
                Items.n_ring.Clear();
                Items.n_belt.Clear();
                //arr = new bool[576];
                label6.Text = "Load Success";
                JObject JS = JObject.Parse(json);
                Console.WriteLine("=================== start ================");
                foreach (JObject item in JS["items"])
                {
                    if(item["ilvl"].ToString() != "0")
                    {
                        String needToParseText = item["icon"].ToString();
                        String[] parsedText = parse(needToParseText);
                        /*Console.WriteLine("---item---");
                        foreach (String type in parsedText)
                        {
                            Console.WriteLine(type);
                        }*/
                        if (parsedText[0] == "Rings")
                        {
                            int itemlev = Convert.ToInt32(item["ilvl"].ToString());
                            int _x = Convert.ToInt32(item["x"].ToString());
                            int _y = Convert.ToInt32(item["y"].ToString());
                            int _w = Convert.ToInt32(item["w"].ToString());
                            int _h = Convert.ToInt32(item["h"].ToString());
                            if (itemlev >= 60 && itemlev < 75)
                                Items.y_ring.Add(new Item(_x, _y, _w, _h));
                            else if (itemlev >= 75)
                                Items.n_ring.Add(new Item(_x, _y, _w, _h));
                        }
                        else if (parsedText[0] == "Belts")
                        {
                            int itemlev = Convert.ToInt32(item["ilvl"].ToString());
                            int _x = Convert.ToInt32(item["x"].ToString());
                            int _y = Convert.ToInt32(item["y"].ToString());
                            int _w = Convert.ToInt32(item["w"].ToString());
                            int _h = Convert.ToInt32(item["h"].ToString());
                            if (itemlev >= 60 && itemlev < 75)
                                Items.y_belt.Add(new Item(_x, _y, _w, _h));
                            else if (itemlev >= 75)
                                Items.n_belt.Add(new Item(_x, _y, _w, _h));
                        }
                        else if (parsedText[0] == "Amulets")
                        { 
                            int itemlev = Convert.ToInt32(item["ilvl"].ToString());
                            int _x = Convert.ToInt32(item["x"].ToString());
                            int _y = Convert.ToInt32(item["y"].ToString());
                            int _w = Convert.ToInt32(item["w"].ToString());
                            int _h = Convert.ToInt32(item["h"].ToString());
                            if (itemlev>=60 && itemlev < 75)
                                Items.y_amulet.Add(new Item(_x, _y, _w, _h));
                            else if (itemlev >= 75)
                                Items.n_amulet.Add(new Item(_x, _y, _w, _h));
                        }
                        else if (parsedText[0] == "Weapons")
                        {
                            
                            if (parsedText[1] == "OneHandWeapons")
                            {
                                int itemlev = Convert.ToInt32(item["ilvl"].ToString());
                                int _x = Convert.ToInt32(item["x"].ToString());
                                int _y = Convert.ToInt32(item["y"].ToString());
                                int _w = Convert.ToInt32(item["w"].ToString());
                                int _h = Convert.ToInt32(item["h"].ToString());
                                if (itemlev >= 60 && itemlev < 75)
                                    Items.y_W1.Add(new Item(_x, _y, _w, _h));
                                else if (itemlev >= 75)
                                    Items.n_W1.Add(new Item(_x, _y, _w, _h));
                            }
                            else if (parsedText[1] == "TwoHandWeapons")
                            {

                                int itemlev = Convert.ToInt32(item["ilvl"].ToString());
                                int _x = Convert.ToInt32(item["x"].ToString());
                                int _y = Convert.ToInt32(item["y"].ToString());
                                int _w = Convert.ToInt32(item["w"].ToString());
                                int _h = Convert.ToInt32(item["h"].ToString());
                                if (itemlev >= 60 && itemlev < 75)
                                    Items.y_W2.Add(new Item(_x, _y, _w, _h));
                                else if (itemlev >= 75)
                                    Items.n_W2.Add(new Item(_x, _y, _w, _h));
                            }
                        }
                        else if (parsedText[0] == "Armours")
                        {
                            if (parsedText[1] == "Shields")
                            {
                                int itemlev = Convert.ToInt32(item["ilvl"].ToString());
                                int _x = Convert.ToInt32(item["x"].ToString());
                                int _y = Convert.ToInt32(item["y"].ToString());
                                int _w = Convert.ToInt32(item["w"].ToString());
                                int _h = Convert.ToInt32(item["h"].ToString());
                                if (itemlev >= 60 && itemlev < 75)
                                    Items.y_W1.Add(new Item(_x, _y, _w, _h));
                                else if (itemlev >= 75)
                                    Items.n_W1.Add(new Item(_x, _y, _w, _h));
                            }
                            else if (parsedText[1] == "Helmets")
                            {
                                int itemlev = Convert.ToInt32(item["ilvl"].ToString());
                                int _x = Convert.ToInt32(item["x"].ToString());
                                int _y = Convert.ToInt32(item["y"].ToString());
                                int _w = Convert.ToInt32(item["w"].ToString());
                                int _h = Convert.ToInt32(item["h"].ToString());
                                if (itemlev >= 60 && itemlev < 75)
                                    Items.y_helmet.Add(new Item(_x, _y, _w, _h));
                                else if (itemlev >= 75)
                                    Items.n_helmet.Add(new Item(_x, _y, _w, _h));
                            }
                            else if (parsedText[1] == "Gloves")
                            {
                                int itemlev = Convert.ToInt32(item["ilvl"].ToString());
                                int _x = Convert.ToInt32(item["x"].ToString());
                                int _y = Convert.ToInt32(item["y"].ToString());
                                int _w = Convert.ToInt32(item["w"].ToString());
                                int _h = Convert.ToInt32(item["h"].ToString());
                                if (itemlev >= 60 && itemlev < 75)
                                    Items.y_gloves.Add(new Item(_x, _y, _w, _h));
                                else if (itemlev >= 75)
                                    Items.n_gloves.Add(new Item(_x, _y, _w, _h));
                            }
                            else if (parsedText[1] == "BodyArmours")
                            {
                                int itemlev = Convert.ToInt32(item["ilvl"].ToString());
                                int _x = Convert.ToInt32(item["x"].ToString());
                                int _y = Convert.ToInt32(item["y"].ToString());
                                int _w = Convert.ToInt32(item["w"].ToString());
                                int _h = Convert.ToInt32(item["h"].ToString());
                                if (itemlev >= 60 && itemlev < 75)
                                    Items.y_chest.Add(new Item(_x, _y, _w, _h));
                                else if (itemlev >= 75)
                                    Items.n_chest.Add(new Item(_x, _y, _w, _h));
                            }
                            else if (parsedText[1] == "Boots")
                            {
                                int itemlev = Convert.ToInt32(item["ilvl"].ToString());
                                int _x = Convert.ToInt32(item["x"].ToString());
                                int _y = Convert.ToInt32(item["y"].ToString());
                                int _w = Convert.ToInt32(item["w"].ToString());
                                int _h = Convert.ToInt32(item["h"].ToString());
                                if (itemlev >= 60 && itemlev < 75)
                                    Items.y_boots.Add(new Item(_x, _y, _w, _h));
                                else if (itemlev >= 75)
                                    Items.n_boots.Add(new Item(_x, _y, _w, _h));
                            }
                        }
                    }
                }
                f3.ChangeValues(
                    Items.y_W1.Count * 2 +
                    Items.y_W2.Count,
                    Items.y_helmet.Count,
                    Items.y_chest.Count,
                    Items.y_boots.Count,
                    Items.y_gloves.Count,
                    Items.y_amulet.Count,
                    Items.y_ring.Count,
                    Items.y_belt.Count,
                    Items.n_W1.Count * 2 +
                    Items.n_W2.Count,
                    Items.n_helmet.Count,
                    Items.n_chest.Count,
                    Items.n_boots.Count,
                    Items.n_gloves.Count,
                    Items.n_amulet.Count,
                    Items.n_ring.Count,
                    Items.n_belt.Count
                );

                Trace.WriteLine("LoadItems Done");
                //timer1.Enabled = true;
                //timer1.Start();
            }
        }

        private void Button2_Click(object sender, EventArgs e)      // Init
        {
            loaded = 0;
            //textBox1.Enabled = true;
            textBox2.Enabled = true;
            textBox3.Enabled = true;
            textBox4.Enabled = true;
            radioButton1.Enabled = true;
            radioButton2.Enabled = true;
            textBox1.Text = "";
            textBox2.Text = "0";
            textBox3.Text = "0";
            textBox4.Text = "0";

            left = 15;
            top = 160;
            right = 650;
            bottom = 795;

            f2.Hide();

            stash = 2;
            

            comboBox2.SelectedIndex = 2;
            comboBox3.SelectedIndex = 2;
            comboBox4.SelectedIndex = 2;
            comboBox5.SelectedIndex = 2;
            comboBox6.SelectedIndex = 2;
            comboBox7.SelectedIndex = 2;
            comboBox1.SelectedIndex = 2;

            comboBox8.SelectedIndex = 8;
            comboBox9.SelectedIndex = 9;
            comboBox10.SelectedIndex = 10;
            comboBox11.SelectedIndex = 11;
            comboBox12.SelectedIndex = 6;
            comboBox13.SelectedIndex = 7;
            comboBox14.SelectedIndex = 5;

            label6.Text = "Need to Load";
            radioButton1.Checked = true;
            radioButton2.Checked = false;
            //timer1.Enabled = false;
            //timer1.Stop();
            Items.y_W1.Clear();
            Items.y_W2.Clear();
            Items.y_helmet.Clear();
            Items.y_chest.Clear();
            Items.y_boots.Clear();
            Items.y_gloves.Clear();
            Items.y_amulet.Clear();
            Items.y_ring.Clear();
            Items.y_belt.Clear();
            Items.n_W1.Clear();
            Items.n_W2.Clear();
            Items.n_helmet.Clear();
            Items.n_chest.Clear();
            Items.n_boots.Clear();
            Items.n_gloves.Clear();
            Items.n_amulet.Clear();
            Items.n_ring.Clear();
            Items.n_belt.Clear();
            /*f3.ChangeValues(
            Items.y_W1.Count * 2 +
            Items.y_W2.Count,
            Items.y_helmet.Count,
            Items.y_chest.Count,
            Items.y_boots.Count,
            Items.y_gloves.Count,
            Items.y_amulet.Count,
            Items.y_ring.Count,
            Items.y_belt.Count,
            Items.n_W1.Count * 2 +
            Items.n_W2.Count,
            Items.n_helmet.Count,
            Items.n_chest.Count,
            Items.n_boots.Count,
            Items.n_gloves.Count,
            Items.n_amulet.Count,
            Items.n_ring.Count,
            Items.n_belt.Count
            );*/


            if (File.Exists(@".\myConfig.ini"))
            {
                string[] lines = File.ReadAllLines(@".\myConfig.ini");
                //Console.WriteLine(lines.Length);
                if (lines.Length < 23)
                {
                    MessageBox.Show("Config file is not exist or Damaged. Load Default Settings.\n myConfing.ini 파일이 없거나 손상되었습니다. 기본값을 불러옵니다.");
                    return;
                }
                textBox1.Text = lines[0];
                textBox2.Text = lines[1];
                textBox3.Text = lines[2];
                textBox4.Text = lines[3];

                comboBox2.SelectedIndex = Convert.ToInt32(lines[4]);
                comboBox3.SelectedIndex = Convert.ToInt32(lines[5]);
                comboBox4.SelectedIndex = Convert.ToInt32(lines[6]);
                comboBox5.SelectedIndex = Convert.ToInt32(lines[7]);
                comboBox6.SelectedIndex = Convert.ToInt32(lines[8]);
                comboBox7.SelectedIndex = Convert.ToInt32(lines[9]);
                comboBox8.SelectedIndex = Convert.ToInt32(lines[10]);
                comboBox9.SelectedIndex = Convert.ToInt32(lines[11]);
                comboBox10.SelectedIndex = Convert.ToInt32(lines[12]);
                comboBox11.SelectedIndex = Convert.ToInt32(lines[13]);
                comboBox12.SelectedIndex = Convert.ToInt32(lines[14]);
                comboBox13.SelectedIndex = Convert.ToInt32(lines[15]);

                left = Convert.ToInt32(lines[16]);
                top = Convert.ToInt32(lines[17]);
                right = Convert.ToInt32(lines[18]);
                bottom = Convert.ToInt32(lines[19]);
                stash = Convert.ToInt32(lines[20]);
                if (stash == 1)
                {
                    radioButton1.Checked = false;
                    radioButton2.Checked = true;
                }
                else if (stash == 2)
                {
                    radioButton1.Checked = true;
                    radioButton2.Checked = false;
                }
                trackBar1.Value = Convert.ToInt32(lines[21]);
                checkBox1.Checked = Convert.ToBoolean(lines[22]);
                label1.Text = "(" + left + ", " + top + ")";
                label2.Text = "(" + right + ", " + bottom + ")";

                if (lines.Length > 23)
                    comboBox1.SelectedIndex = Convert.ToInt32(lines[23]);
                if (lines.Length > 24)
                    comboBox14.SelectedIndex = Convert.ToInt32(lines[24]);
            }
        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://adc000.blog.me/221576299228");
        }

        private void Label2_Click(object sender, EventArgs e)
        {

        }

        //private void Timer1_Tick(object sender, EventArgs e)
        //{
        //    Button1_Click(null, null);
        //}

        private void Button3_Click(object sender, EventArgs e)      // Save and Apply button event
        {
            SaveAndApply();
        }

        private void SaveAndApply()      // Save and Apply
        {
            UnregisterHotKey((int)this.Handle, 0); //이 폼에 ID가 0인 핫키 해제
            UnregisterHotKey((int)this.Handle, 1);
            UnregisterHotKey((int)this.Handle, 2);
            UnregisterHotKey((int)this.Handle, 3);
            UnregisterHotKey((int)this.Handle, 4);
            UnregisterHotKey((int)this.Handle, 5);
            UnregisterHotKey((int)this.Handle, 6);

            RegisterHotKey((int)this.Handle, 0, comboBox2.SelectedIndex, comboBox8.SelectedIndex + (comboBox8.SelectedIndex<12 ? 112 : (comboBox8.SelectedIndex>21?43:36)));
            RegisterHotKey((int)this.Handle, 1, comboBox3.SelectedIndex, comboBox9.SelectedIndex + (comboBox9.SelectedIndex < 12 ? 112 : (comboBox9.SelectedIndex > 21 ? 43 : 36)));
            RegisterHotKey((int)this.Handle, 2, comboBox4.SelectedIndex, comboBox10.SelectedIndex + (comboBox10.SelectedIndex < 12 ? 112 : (comboBox10.SelectedIndex > 21 ? 43 : 36)));
            RegisterHotKey((int)this.Handle, 3, comboBox5.SelectedIndex, comboBox11.SelectedIndex + (comboBox11.SelectedIndex < 12 ? 112 : (comboBox11.SelectedIndex > 21 ? 43 : 36)));
            RegisterHotKey((int)this.Handle, 4, comboBox6.SelectedIndex, comboBox12.SelectedIndex + (comboBox12.SelectedIndex < 12 ? 112 : (comboBox12.SelectedIndex > 21 ? 43 : 36)));
            RegisterHotKey((int)this.Handle, 5, comboBox7.SelectedIndex, comboBox13.SelectedIndex + (comboBox13.SelectedIndex < 12 ? 112 : (comboBox13.SelectedIndex > 21 ? 43 : 36)));
            RegisterHotKey((int)this.Handle, 6, comboBox1.SelectedIndex, comboBox14.SelectedIndex + (comboBox14.SelectedIndex < 12 ? 112 : (comboBox14.SelectedIndex > 21 ? 43 : 36)));

            using (StreamWriter ini = new StreamWriter(@".\myConfig.ini"))
            {
                ini.WriteLine(textBox1.Text.Replace("\\p{Z}", ""));
                ini.WriteLine(textBox2.Text.Replace("\\p{Z}", ""));
                ini.WriteLine(textBox3.Text.Replace("\\p{Z}", ""));
                ini.WriteLine(textBox4.Text.Replace("\\p{Z}", ""));
                ini.WriteLine(comboBox2.SelectedIndex.ToString());
                ini.WriteLine(comboBox3.SelectedIndex.ToString());
                ini.WriteLine(comboBox4.SelectedIndex.ToString());
                ini.WriteLine(comboBox5.SelectedIndex.ToString());
                ini.WriteLine(comboBox6.SelectedIndex.ToString());
                ini.WriteLine(comboBox7.SelectedIndex.ToString());
                ini.WriteLine(comboBox8.SelectedIndex.ToString());
                ini.WriteLine(comboBox9.SelectedIndex.ToString());
                ini.WriteLine(comboBox10.SelectedIndex.ToString());
                ini.WriteLine(comboBox11.SelectedIndex.ToString());
                ini.WriteLine(comboBox12.SelectedIndex.ToString());
                ini.WriteLine(comboBox13.SelectedIndex.ToString());
                ini.WriteLine(left.ToString());
                ini.WriteLine(top.ToString());
                ini.WriteLine(right.ToString());
                ini.WriteLine(bottom.ToString());
                ini.WriteLine(stash.ToString());
                ini.WriteLine(trackBar1.Value.ToString());
                ini.WriteLine(checkBox1.Checked.ToString());
                ini.WriteLine(comboBox1.SelectedIndex.ToString());
                ini.WriteLine(comboBox14.SelectedIndex.ToString());
            }

            ShowSaveMessage();
        }

        private async void ShowSaveMessage()
        {
            int alpha = 255;
            label22.BackColor = Color.FromArgb(alpha, Color.Black);

            label22.Visible = true;

            await System.Threading.Tasks.Task.Delay(2000);

            while (alpha > 0)
            {
                await System.Threading.Tasks.Task.Delay(1);
                alpha = Math.Max(alpha - 10, 0);
                label22.BackColor = Color.FromArgb(alpha, Color.Black);
            }

            label22.Visible = false;
        }

        private void TrackBar1_Scroll(object sender, EventArgs e)
        {
            int value = trackBar1.Value;
            SetLayeredWindowAttributes(f3.Handle, 0, (Byte)value, LWA_ALPHA);
        }
    }



    public class WebClientEx : WebClient
    {
        private CookieContainer _cookieContainer = new CookieContainer();

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            if (request is HttpWebRequest)
            {
                (request as HttpWebRequest).CookieContainer = _cookieContainer;
                (request as HttpWebRequest).AllowAutoRedirect = true;
                (request as HttpWebRequest).Timeout = 10000;
            }
            return request;
        }
    }

    public class Item
    {
        public int x;
        public int y;
        public int w;
        public int h;
        public Item(int a, int b, int c, int d)
        {
            x = a;
            y = b;
            w = c;
            h = d;
        }
        public int get_x() { return x; }
        public int get_y() { return y; }
        public int get_w() { return w; }
        public int get_h() { return h; }
    }
    public struct Items
    {
        public static List<Item> y_W1;
        public static List<Item> y_W2;
        public static List<Item> y_helmet;
        public static List<Item> y_chest;
        public static List<Item> y_boots;
        public static List<Item> y_gloves;
        public static List<Item> y_amulet;
        public static List<Item> y_ring;
        public static List<Item> y_belt;
        public static List<Item> n_W1;
        public static List<Item> n_W2;
        public static List<Item> n_helmet;
        public static List<Item> n_chest;
        public static List<Item> n_boots;
        public static List<Item> n_gloves;
        public static List<Item> n_amulet;
        public static List<Item> n_ring;
        public static List<Item> n_belt;

        public static List<List<Item>> itemSetList = new List<List<Item>>();
    }
}

