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

namespace MonkeyTester
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            // http://dobon.net/vb/dotnet/system/cursorposition.html
            //        System.Windows.Forms.Cursor.Position = this.PointToScreen(new System.Drawing.Point(0, 0));
        }
        // http://homepage2.nifty.com/nonnon/SoftSample/CS.NET/SampleSendInput.html
        // マウスイベント(mouse_eventの引数と同様のデータ)
        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public int mouseData;
            public int dwFlags;
            public int time;
            public int dwExtraInfo;
        };

        // キーボードイベント(keybd_eventの引数と同様のデータ)
        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public short wVk;
            public short wScan;
            public int dwFlags;
            public int time;
            public int dwExtraInfo;
        };

        // ハードウェアイベント
        [StructLayout(LayoutKind.Sequential)]
        private struct HARDWAREINPUT
        {
            public int uMsg;
            public short wParamL;
            public short wParamH;
        };

        // 各種イベント(SendInputの引数データ)
        [StructLayout(LayoutKind.Explicit)]
        private struct INPUT
        {
            [FieldOffset(0)]
            public int type;
            [FieldOffset(4)]
            public MOUSEINPUT mi;
            [FieldOffset(4)]
            public KEYBDINPUT ki;
            [FieldOffset(4)]
            public HARDWAREINPUT hi;
        };

        // キー操作、マウス操作をシミュレート(擬似的に操作する)
        [DllImport("user32.dll")]
        private extern static void SendInput(
            int nInputs, ref INPUT pInputs, int cbsize);

        // 仮想キーコードをスキャンコードに変換
        [DllImport("user32.dll", EntryPoint = "MapVirtualKeyA")]
        private extern static int MapVirtualKey(
            int wCode, int wMapType);

        private const int INPUT_MOUSE = 0;                  // マウスイベント
        private const int INPUT_KEYBOARD = 1;               // キーボードイベント
        private const int INPUT_HARDWARE = 2;               // ハードウェアイベント

        private const int MOUSEEVENTF_MOVE = 0x1;           // マウスを移動する
        private const int MOUSEEVENTF_ABSOLUTE = 0x8000;    // 絶対座標指定
        private const int MOUSEEVENTF_LEFTDOWN = 0x2;       // 左　ボタンを押す
        private const int MOUSEEVENTF_LEFTUP = 0x4;         // 左　ボタンを離す
        private const int MOUSEEVENTF_RIGHTDOWN = 0x8;      // 右　ボタンを押す
        private const int MOUSEEVENTF_RIGHTUP = 0x10;       // 右　ボタンを離す
        private const int MOUSEEVENTF_MIDDLEDOWN = 0x20;    // 中央ボタンを押す
        private const int MOUSEEVENTF_MIDDLEUP = 0x40;      // 中央ボタンを離す
        private const int MOUSEEVENTF_WHEEL = 0x800;        // ホイールを回転する
        private const int WHEEL_DELTA = 120;                // ホイール回転値

        private const int KEYEVENTF_KEYDOWN = 0x0;          // キーを押す
        private const int KEYEVENTF_KEYUP = 0x2;            // キーを離す
        private const int KEYEVENTF_EXTENDEDKEY = 0x1;      // 拡張コード
        private const int VK_SHIFT = 0x10;                  // SHIFTキー
        private int _count = 0;
        private Timer _timer = new Timer();
        private void button1_Click(object sender, EventArgs e)
        {
            _count = 0;
            label1.Text = "" + _count + " / " + numericUpDown1.Value;

            _timer.Interval = Decimal.ToInt16(numericUpDown2_interval.Value);
            _timer.Tick += tick;
            _timer.Start();
        }

        private void tick(object sender, EventArgs e)
        {
            if (_count >= numericUpDown1.Value)
            {
                _timer.Stop();
                System.Media.SystemSounds.Beep.Play();
                return;
            }

            Random random = new Random();
            int ran = random.Next(20);
            if (ran < 3)
            {
                mouseWheel();
            }
            else
            {
                mouseRandom();
            }

            _count++;

            label1.Text = "" + _count + " / " + numericUpDown1.Value;
        }

        private void mouseRandom()
        {
            // マウス操作実行用のデータ
            const int num = 3;
            INPUT[] inp = new INPUT[num];
            inp = randomMove(inp);
            // マウス操作実行
            SendInput(num, ref inp[0], Marshal.SizeOf(inp[0]));
        }

        private INPUT[] randomMove(INPUT[] inp)
        {

            // (1)マウスカーソルを移動する(スクリーン座標でX座標=800ピクセル,Y=400ピクセルの位置)
            inp[0].type = INPUT_MOUSE;
            inp[0].mi.dwFlags = MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE;
            Point p0 = new Point();
            Random random = new Random();
            p0.X = webBrowser1.Location.X + random.Next(webBrowser1.Size.Width);
            p0.Y = webBrowser1.Location.Y + random.Next(webBrowser1.Size.Height);
            Point p1 = this.PointToScreen(p0);

            inp[0].mi.dx = p1.X * (65535 / Screen.PrimaryScreen.Bounds.Width);
            inp[0].mi.dy = p1.Y * (65535 / Screen.PrimaryScreen.Bounds.Height);
            inp[0].mi.mouseData = 0;
            inp[0].mi.dwExtraInfo = 0;
            inp[0].mi.time = 0;

            // (2)マウスの右ボタンを押す
            inp[1].type = INPUT_MOUSE;
            inp[1].mi.dwFlags = MOUSEEVENTF_LEFTDOWN;
            inp[1].mi.dx = 0;
            inp[1].mi.dy = 0;
            inp[1].mi.mouseData = 0;
            inp[1].mi.dwExtraInfo = 0;
            inp[1].mi.time = 0;

            // (3)マウスの右ボタンを離す
            inp[2].type = INPUT_MOUSE;
            inp[2].mi.dwFlags = MOUSEEVENTF_LEFTUP;
            inp[2].mi.dx = 0;
            inp[2].mi.dy = 0;
            inp[2].mi.mouseData = 0;
            inp[2].mi.dwExtraInfo = 0;
            inp[2].mi.time = 0;
            return inp;
        }

        // マウスホイールを回転する
        private void mouseWheel()
        {
            // マウス操作実行用のデータ
            const int num = 1;
            INPUT[] inp = new INPUT[num];

            // (2)マウスホイールを前方(近づく方向)へ回転する
            inp[0].type = INPUT_MOUSE;
            inp[0].mi.dwFlags = MOUSEEVENTF_WHEEL;
            inp[0].mi.dx = 0;
            inp[0].mi.dy = 0;
            inp[0].mi.mouseData = (new Random().Next(10) - 6) * WHEEL_DELTA;
            inp[0].mi.dwExtraInfo = 0;
            inp[0].mi.time = 0;

            // マウス操作実行
            SendInput(num, ref inp[0], Marshal.SizeOf(inp[0]));
            System.Threading.Thread.Sleep(10);
        }


        private void comboBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    string url = comboBox1.Text;
                    if (!comboBox1.Text.StartsWith("http://")) url = "http://" + url;
                    webBrowser1.Navigate(new Uri(url));
                }
                catch
                {
                }
            }
        }

        private String currentURL;
        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            // コンボボックスのテキスト変更 
            this.Text = webBrowser1.Document.Title.ToString();
            this.comboBox1.Text = webBrowser1.Url.ToString();

            if (currentURL != comboBox1.Text)
            {
                comboBox1.Items.Add(comboBox1.Text);
                currentURL = comboBox1.Text;
            }

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                webBrowser1.Navigate(comboBox1.Text);
            }
            catch
            {
            }
        }




        private void Form1_Resize(object sender, EventArgs e)
        {
            Point p = panel1.Location;
            p.X = this.Width - 120;
            panel1.Location = p;

            webBrowser1.Width = this.Width - 120;
            webBrowser1.Height = this.Height - webBrowser1.Location.Y - 38;
            comboBox1.Width = webBrowser1.Width - comboBox1.Location.Y;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Form1_Resize(null, null);
        }


    }
}
