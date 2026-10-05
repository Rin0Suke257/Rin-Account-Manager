using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RinAccountManager
{
    public class QuickLoginForm : Form
    {
        private QuickLoginTicket ticket;
        private Timer pollTimer;
        private bool polling = false;
        private bool done = false;

        private Label lblCode;
        private PictureBox qrBox;
        private Label lblStatus;
        private TextBox txtAlias;
        private RoundedButton btnCopy;
        private RoundedButton btnCancel;
        private LinkLabel lnkCookie;

        public Account Result { get; private set; }
        public bool ManualCookie { get; private set; }

        public QuickLoginForm()
        {
            this.Text = "Quick Login - Rin";
            this.Size = new Size(440, 560);
            this.MinimumSize = new Size(440, 560);
            this.MaximumSize = new Size(440, 560);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = RinTheme.Bg;

            UiHelper.SetAppIcon(this);

            Image mascotImg = null;
            try { mascotImg = UiHelper.LoadMascot(64); } catch { }

            TitleBar bar = new TitleBar(this, mascotImg, "Quick Login", 440, false, false);
            this.Controls.Add(bar);

            Label guide = new Label();
            guide.Location = new Point(14, 40);
            guide.Size = new Size(412, 66);
            guide.Font = new Font("Segoe UI", 8.5f);
            guide.ForeColor = RinTheme.Text;
            guide.Text = "1. Mo app Roblox (hoac web roblox.com) tren dien thoai/may DA dang nhap acc can them.\n" +
                "2. Vao muc Quick Log In / Dang nhap nhanh, quet QR hoac nhap ma ben duoi.\n" +
                "3. Bam Xac nhan tren thiet bi do, app tu them acc.";
            this.Controls.Add(guide);

            lblCode = new Label();
            lblCode.Location = new Point(14, 110);
            lblCode.Size = new Size(412, 44);
            lblCode.Font = new Font("Consolas", 26f, FontStyle.Bold);
            lblCode.ForeColor = RinTheme.PrimaryDark;
            lblCode.BackColor = RinTheme.PrimarySoft;
            lblCode.TextAlign = ContentAlignment.MiddleCenter;
            lblCode.Text = "......";
            lblCode.Cursor = Cursors.Hand;
            lblCode.Click += new EventHandler(LblCode_Click);
            this.Controls.Add(lblCode);

            qrBox = new PictureBox();
            qrBox.Location = new Point(145, 162);
            qrBox.Size = new Size(150, 150);
            qrBox.SizeMode = PictureBoxSizeMode.Zoom;
            qrBox.BackColor = Color.White;
            qrBox.Visible = false;
            this.Controls.Add(qrBox);

            lblStatus = new Label();
            lblStatus.Location = new Point(14, 320);
            lblStatus.Size = new Size(412, 34);
            lblStatus.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblStatus.ForeColor = RinTheme.PrimaryDark;
            lblStatus.TextAlign = ContentAlignment.MiddleCenter;
            lblStatus.Text = "Dang tao ma...";
            this.Controls.Add(lblStatus);

            Label lAlias = new Label();
            lAlias.Text = "Ten goi nho (de trong = lay ten acc):";
            lAlias.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lAlias.ForeColor = RinTheme.Text;
            lAlias.Location = new Point(14, 360);
            lAlias.AutoSize = true;
            this.Controls.Add(lAlias);

            txtAlias = new TextBox();
            txtAlias.Location = new Point(14, 380);
            txtAlias.Size = new Size(412, 24);
            txtAlias.BorderStyle = BorderStyle.FixedSingle;
            txtAlias.BackColor = Color.White;
            txtAlias.ForeColor = RinTheme.Text;
            txtAlias.Font = new Font("Segoe UI", 10f);
            this.Controls.Add(txtAlias);

            btnCopy = new RoundedButton();
            btnCopy.Text = "Sao chep ma";
            btnCopy.Location = new Point(14, 416);
            btnCopy.Size = new Size(200, 38);
            btnCopy.CornerRadius = 12;
            btnCopy.FillColor = RinTheme.PrimarySoft;
            btnCopy.HoverColor = RinTheme.Border;
            btnCopy.ForeColor = RinTheme.PrimaryDark;
            btnCopy.Click += new EventHandler(LblCode_Click);
            this.Controls.Add(btnCopy);

            btnCancel = new RoundedButton();
            btnCancel.Text = "Huy";
            btnCancel.Location = new Point(226, 416);
            btnCancel.Size = new Size(200, 38);
            btnCancel.CornerRadius = 12;
            btnCancel.FillColor = Color.White;
            btnCancel.HoverColor = RinTheme.PrimarySoft;
            btnCancel.ForeColor = RinTheme.PrimaryDark;
            btnCancel.Click += new EventHandler(BtnCancel_Click);
            this.Controls.Add(btnCancel);

            lnkCookie = new LinkLabel();
            lnkCookie.Text = "Nhap cookie thu cong thay the";
            lnkCookie.Location = new Point(14, 462);
            lnkCookie.Size = new Size(240, 18);
            lnkCookie.LinkColor = RinTheme.PrimaryDark;
            lnkCookie.Click += new EventHandler(LnkCookie_Click);
            this.Controls.Add(lnkCookie);

            Label credit = new Label();
            credit.Text = "by Rin0Suke257";
            credit.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
            credit.ForeColor = RinTheme.Primary;
            credit.BackColor = Color.Transparent;
            credit.Location = new Point(268, 462);
            credit.Size = new Size(158, 18);
            credit.TextAlign = ContentAlignment.MiddleRight;
            this.Controls.Add(credit);

            this.Load += new EventHandler(QuickLoginForm_Load);
            this.FormClosed += new FormClosedEventHandler(QuickLoginForm_Closed);

            pollTimer = new Timer();
            pollTimer.Interval = 4000;
            pollTimer.Tick += new EventHandler(PollTimer_Tick);
        }

        private void QuickLoginForm_Load(object sender, EventArgs e)
        {
            UiHelper.ApplyRound(this, 16);
            UiHelper.ApplyRound(lblCode, 12);
            Task.Run(new Action(SetupTicket));
        }

        private void SetupTicket()
        {
            QuickLoginTicket t;
            string err;
            if (!QuickLogin.TryCreate(out t, out err))
            {
                SetStatus("Loi tao ma: " + err);
                return;
            }
            ticket = t;
            SetCode(t.Code);
            SetStatus("Nhap ma tren thiet bi da dang nhap, app tu doi...");
            // QR nap nen
            if (!string.IsNullOrEmpty(t.QrUrl))
            {
                System.Drawing.Image qr = QuickLogin.LoadQr(t.QrUrl);
                if (qr != null && !this.IsDisposed)
                {
                    try
                    {
                        this.Invoke(new Action(delegate()
                        {
                            try
                            {
                                qrBox.Image = qr;
                                qrBox.Visible = true;
                            }
                            catch { }
                        }));
                    }
                    catch { }
                }
            }
            try
            {
                this.Invoke(new Action(delegate()
                {
                    try { pollTimer.Start(); } catch { }
                }));
            }
            catch { }
        }

        private void SetCode(string code)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action<string>(SetCode), new object[] { code });
                    return;
                }
                lblCode.Text = code;
            }
            catch { }
        }

        private void SetStatus(string s)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action<string>(SetStatus), new object[] { s });
                    return;
                }
                lblStatus.Text = s;
            }
            catch { }
        }

        private async void PollTimer_Tick(object sender, EventArgs e)
        {
            if (polling || done || ticket == null)
            {
                return;
            }
            polling = true;
            try
            {
                if (DateTime.UtcNow > ticket.ExpiresUtc)
                {
                    pollTimer.Stop();
                    SetStatus("Ma het han. Dong cua so va tao ma moi.");
                    return;
                }
                QuickLoginTicket t = ticket;
                string res = await Task.Run(new Func<string>(delegate()
                {
                    string acc;
                    string err;
                    string st = QuickLogin.GetStatus(t, out acc, out err);
                    if (st == null)
                    {
                        return "ERR:" + err;
                    }
                    if (st == "UserLinked" && !string.IsNullOrEmpty(acc))
                    {
                        return "LINKED:" + acc;
                    }
                    return st;
                }));
                if (done)
                {
                    return;
                }
                if (res == null)
                {
                    return;
                }
                if (res.StartsWith("ERR:"))
                {
                    SetStatus("Loi mang, dang thu lai... (" + res.Substring(4) + ")");
                }
                else if (res.StartsWith("LINKED:"))
                {
                    SetStatus("Da nhap ma: " + res.Substring(7) + " - cho xac nhan...");
                }
                else if (res == "Validated")
                {
                    pollTimer.Stop();
                    await FinishLogin();
                }
                else if (res == "Cancelled" || res == "Expired")
                {
                    pollTimer.Stop();
                    SetStatus(res == "Cancelled" ? "Da bi huy tu thiet bi kia." : "Ma het han. Dong cua so va tao ma moi.");
                }
            }
            finally
            {
                polling = false;
            }
        }

        private async Task FinishLogin()
        {
            SetStatus("Da xac nhan! Dang lay acc...");
            QuickLoginTicket t = ticket;
            string exchangeErr = null;
            string cookie = await Task.Run(new Func<string>(delegate()
            {
                string c;
                string err;
                if (QuickLogin.TryExchange(t, out c, out err))
                {
                    return c;
                }
                exchangeErr = err;
                return null;
            }));
            if (cookie == null)
            {
                SetStatus("Loi lay acc: " + exchangeErr);
                return;
            }
            RobloxUser u = RobloxApi.GetAuthenticatedUser(RobloxApi.ExtractCookie(cookie));
            if (!u.Ok)
            {
                SetStatus("Cookie khong hop le: " + u.Error);
                return;
            }
            string alias = txtAlias.Text.Trim();
            if (alias == "")
            {
                alias = u.Name;
            }
            Account a = new Account();
            a.Alias = alias;
            a.Cookie = RobloxApi.ExtractCookie(cookie);
            a.Username = u.Name;
            a.UserId = u.Id;
            done = true;
            Result = a;
            try
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch { }
        }

        private void LblCode_Click(object sender, EventArgs e)
        {
            try
            {
                if (ticket != null && !string.IsNullOrEmpty(ticket.Code))
                {
                    Clipboard.SetText(ticket.Code);
                    SetStatus("Da copy ma. Nhap ma tren thiet bi da dang nhap...");
                }
            }
            catch { }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            try { pollTimer.Stop(); } catch { }
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void LnkCookie_Click(object sender, EventArgs e)
        {
            try { pollTimer.Stop(); } catch { }
            ManualCookie = true;
            this.DialogResult = DialogResult.Abort;
            this.Close();
        }

        private void QuickLoginForm_Closed(object sender, FormClosedEventArgs e)
        {
            try { pollTimer.Stop(); } catch { }
            try { pollTimer.Dispose(); } catch { }
            if (!done && ticket != null)
            {
                QuickLoginTicket t = ticket;
                Task.Run(new Action(delegate() { QuickLogin.Cancel(t); }));
            }
        }
    }
}
