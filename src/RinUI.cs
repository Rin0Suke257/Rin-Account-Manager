using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RinAccountManager
{
    public static class RinTheme
    {
        public static readonly Color Bg = Color.FromArgb(255, 242, 246);
        public static readonly Color Card = Color.White;
        public static readonly Color Primary = Color.FromArgb(240, 98, 146);
        public static readonly Color PrimaryDark = Color.FromArgb(216, 27, 96);
        public static readonly Color PrimarySoft = Color.FromArgb(252, 228, 236);
        public static readonly Color Border = Color.FromArgb(248, 187, 208);
        public static readonly Color Text = Color.FromArgb(93, 58, 74);
        public static readonly Color Muted = Color.FromArgb(166, 124, 142);
        public static readonly Color Teal = Color.FromArgb(38, 166, 154);
        public static readonly Color LogBg = Color.FromArgb(255, 249, 251);

        public static Font TitleFont()
        {
            return new Font("Segoe UI", 18f, FontStyle.Bold);
        }

        public static Font SubFont()
        {
            return new Font("Segoe UI", 9f, FontStyle.Regular);
        }

        public static Font BtnFont()
        {
            return new Font("Segoe UI", 9.5f, FontStyle.Bold);
        }
    }

    public class RoundedButton : Button
    {
        public int CornerRadius = 14;
        public Color FillColor = RinTheme.Primary;
        public Color HoverColor = RinTheme.PrimaryDark;
        public Color PressedColor = Color.FromArgb(173, 20, 87);

        private bool hovering = false;
        private bool pressing = false;

        public RoundedButton()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            this.UpdateStyles();
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.FlatAppearance.MouseOverBackColor = Color.Transparent;
            this.FlatAppearance.MouseDownBackColor = Color.Transparent;
            this.BackColor = RinTheme.Bg;
            this.ForeColor = Color.White;
            this.Font = RinTheme.BtnFont();
            this.Cursor = Cursors.Hand;
            this.MouseEnter += delegate(object s, EventArgs e) { hovering = true; this.Invalidate(); };
            this.MouseLeave += delegate(object s, EventArgs e) { hovering = false; pressing = false; this.Invalidate(); };
            this.MouseDown += delegate(object s, MouseEventArgs e) { pressing = true; this.Invalidate(); };
            this.MouseUp += delegate(object s, MouseEventArgs e) { pressing = false; this.Invalidate(); };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(RinTheme.Bg);
            Color fill = FillColor;
            if (pressing)
            {
                fill = PressedColor;
            }
            else if (hovering)
            {
                fill = HoverColor;
            }
            using (GraphicsPath path = UiHelper.RoundedRect(new Rectangle(0, 0, this.Width - 1, this.Height - 1), CornerRadius))
            {
                using (SolidBrush b = new SolidBrush(fill))
                {
                    e.Graphics.FillPath(b, path);
                }
            }
            TextRenderer.DrawText(e.Graphics, this.Text, this.Font, new Rectangle(4, 0, this.Width - 8, this.Height),
                this.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.Invalidate();
        }
    }

    public class TitleBar : Panel
    {
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        private static extern int DragMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        public Label StatusLabel;
        public Label GearLabel;

        public TitleBar(Form owner, Image icon, string text, int width, bool showStatus, bool showMin, bool showGear = false)
        {
            this.Location = new Point(0, 0);
            this.Size = new Size(width, 30);
            this.BackColor = RinTheme.PrimaryDark;

            PictureBox pic = new PictureBox();
            pic.Location = new Point(6, 4);
            pic.Size = new Size(22, 22);
            pic.SizeMode = PictureBoxSizeMode.Zoom;
            pic.BackColor = Color.Transparent;
            if (icon != null)
            {
                pic.Image = icon;
            }
            this.Controls.Add(pic);

            Label name = new Label();
            name.Text = text;
            name.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            name.ForeColor = Color.White;
            name.BackColor = Color.Transparent;
            name.Location = new Point(32, 0);
            name.Size = new Size(250, 30);
            name.TextAlign = ContentAlignment.MiddleLeft;
            this.Controls.Add(name);

            StatusLabel = new Label();
            StatusLabel.Text = "";
            StatusLabel.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            StatusLabel.ForeColor = Color.White;
            StatusLabel.BackColor = Color.Transparent;
            StatusLabel.TextAlign = ContentAlignment.MiddleRight;
            this.Controls.Add(StatusLabel);

            int bx = width;
            Label btnClose = new Label();
            btnClose.Text = "X";
            btnClose.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            btnClose.ForeColor = Color.White;
            btnClose.BackColor = Color.Transparent;
            btnClose.TextAlign = ContentAlignment.MiddleCenter;
            btnClose.Size = new Size(44, 30);
            btnClose.Location = new Point(bx - 44, 0);
            btnClose.Cursor = Cursors.Hand;
            bx -= 44;
            this.Controls.Add(btnClose);

            Label btnMin = null;
            if (showMin)
            {
                btnMin = new Label();
                btnMin.Text = "_";
                btnMin.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
                btnMin.ForeColor = Color.White;
                btnMin.BackColor = Color.Transparent;
                btnMin.TextAlign = ContentAlignment.MiddleCenter;
                btnMin.Size = new Size(40, 30);
                btnMin.Location = new Point(bx - 40, 0);
                btnMin.Cursor = Cursors.Hand;
                bx -= 40;
                this.Controls.Add(btnMin);
            }

            StatusLabel.Location = new Point(286, 0);
            StatusLabel.Size = new Size(Math.Max(50, bx - 286 - 6), 30);
            if (!showStatus)
            {
                StatusLabel.Visible = false;
            }

            GearLabel = null;
            if (showGear)
            {
                GearLabel = new Label();
                GearLabel.Text = "⚙";
                GearLabel.Font = new Font("Segoe UI Symbol", 13f, FontStyle.Regular);
                GearLabel.ForeColor = Color.White;
                GearLabel.BackColor = Color.Transparent;
                GearLabel.TextAlign = ContentAlignment.MiddleCenter;
                GearLabel.Size = new Size(36, 30);
                GearLabel.Location = new Point(bx - 36, 0);
                GearLabel.Cursor = Cursors.Hand;
                bx -= 36;
                // dat lai status cho chua gear
                StatusLabel.Size = new Size(Math.Max(50, bx - 286 - 6), 30);
                this.Controls.Add(GearLabel);
                GearLabel.MouseEnter += delegate(object s, EventArgs e) { GearLabel.BackColor = RinTheme.Primary; };
                GearLabel.MouseLeave += delegate(object s, EventArgs e) { GearLabel.BackColor = Color.Transparent; };
            }

            Form f = owner;
            btnClose.Click += delegate(object s, EventArgs e) { f.Close(); };
            btnClose.MouseEnter += delegate(object s, EventArgs e) { btnClose.BackColor = Color.FromArgb(183, 28, 28); };
            btnClose.MouseLeave += delegate(object s, EventArgs e) { btnClose.BackColor = Color.Transparent; };
            if (btnMin != null)
            {
                btnMin.Click += delegate(object s, EventArgs e) { f.WindowState = FormWindowState.Minimized; };
                btnMin.MouseEnter += delegate(object s, EventArgs e) { btnMin.BackColor = RinTheme.Primary; };
                btnMin.MouseLeave += delegate(object s, EventArgs e) { btnMin.BackColor = Color.Transparent; };
            }

            MouseEventHandler drag = delegate(object s, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    try
                    {
                        ReleaseCapture();
                        DragMessage(f.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
                    }
                    catch { }
                }
            };
            this.MouseDown += drag;
            pic.MouseDown += drag;
            name.MouseDown += drag;
            StatusLabel.MouseDown += drag;
        }
    }

    public static class WinApi
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsIconic(IntPtr hWnd);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT r);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int w, int h, bool repaint);

        public const int SW_RESTORE = 9;
        public const int SW_MINIMIZE = 6;
        public const int SW_RESTORE_NOACTIVATE = 4;
    }

    public static class UiHelper
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

        private const int EM_SETCUEBANNER = 0x1501;

        // Chu mo hien san trong o nhap (placeholder).
        public static void SetCue(TextBox t, string cue)
        {
            try
            {
                if (t.IsHandleCreated)
                {
                    SendMessage(t.Handle, EM_SETCUEBANNER, (IntPtr)1, cue);
                }
                else
                {
                    t.HandleCreated += delegate(object s, EventArgs e)
                    {
                        try { SendMessage(t.Handle, EM_SETCUEBANNER, (IntPtr)1, cue); } catch { }
                    };
                }
            }
            catch { }
        }

        public static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            int d = Math.Max(1, radius * 2);
            GraphicsPath path = new GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static void ApplyRound(Control c, int radius)
        {
            try
            {
                if (c.Width <= 0 || c.Height <= 0)
                {
                    return;
                }
                Rectangle r = new Rectangle(0, 0, c.Width, c.Height);
                using (GraphicsPath path = RoundedRect(r, radius))
                {
                    if (c.Region != null)
                    {
                        try { c.Region.Dispose(); } catch { }
                    }
                    c.Region = new Region(path);
                }
            }
            catch { }
        }

        public static Panel CardPanel(int x, int y, int w, int h)
        {
            Panel p = new Panel();
            p.Location = new Point(x, y);
            p.Size = new Size(w, h);
            p.BackColor = RinTheme.Card;
            p.BorderStyle = BorderStyle.None;
            return p;
        }

        // Logo uu tien doc tu resource nhuúng trong exe (build /resource),
        // fallback ve file roi neu chay ban dev.
        public static Image LoadMascot(int size)
        {
            try
            {
                System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
                string[] names = asm.GetManifestResourceNames();
                for (int i = 0; i < names.Length; i++)
                {
                    if (names[i].EndsWith("Rin.png", StringComparison.OrdinalIgnoreCase))
                    {
                        using (Stream s = asm.GetManifestResourceStream(names[i]))
                        {
                            if (s != null)
                            {
                                return FinishMascot(s);
                            }
                        }
                    }
                }
            }
            catch { }
            return LoadMascotFromFile();
        }

        private static Image FinishMascot(Stream s)
        {
            using (Image src = Image.FromStream(s))
            {
                Bitmap bmp = new Bitmap(src.Width, src.Height);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.DrawImage(src, 0, 0, src.Width, src.Height);
                }
                try { bmp.MakeTransparent(Color.FromArgb(0, 0, 0)); } catch { }
                return bmp;
            }
        }

        private static Image LoadMascotFromFile()
        {
            string[] candidates = new string[0];
            try
            {
                string appDir = Path.GetDirectoryName(Application.ExecutablePath);
                candidates = new string[]
                {
                    Path.Combine(appDir, "Rin.png"),
                    Path.Combine(Directory.GetCurrentDirectory(), "Rin.png"),
                    Path.Combine(appDir, "..", "Rin.png"),
                    "D:\\Rin Account Manager\\Rin.png",
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RinAccountManager", "Rin.png")
                };
            }
            catch { }
            for (int i = 0; i < candidates.Length; i++)
            {
                try
                {
                    string f = candidates[i];
                    if (string.IsNullOrEmpty(f))
                    {
                        continue;
                    }
                    f = Path.GetFullPath(f);
                    if (!File.Exists(f))
                    {
                        continue;
                    }
                    using (Image src = Image.FromFile(f))
                    {
                        Bitmap bmp = new Bitmap(src.Width, src.Height);
                        using (Graphics g = Graphics.FromImage(bmp))
                        {
                            g.DrawImage(src, 0, 0, src.Width, src.Height);
                        }
                        try { bmp.MakeTransparent(Color.FromArgb(0, 0, 0)); } catch { }
                        return bmp;
                    }
                }
                catch { }
            }
            return null;
        }

        public static void SetAppIcon(Form f)
        {
            try
            {
                Image img = LoadMascot(64);
                if (img == null)
                {
                    return;
                }
                using (Bitmap bmp = new Bitmap(img))
                {
                    IntPtr h = bmp.GetHicon();
                    try
                    {
                        f.Icon = Icon.FromHandle(h);
                    }
                    catch { }
                }
            }
            catch { }
        }
    }
}
