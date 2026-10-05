using System;
using System.Drawing;
using System.Windows.Forms;

namespace RinAccountManager
{
    public class AddAccountForm : Form
    {
        private TextBox txtAlias;
        private TextBox txtCookie;
        private RoundedButton btnOk;
        private RoundedButton btnCancel;
        private Label lblInfo;

        public Account Result { get; private set; }

        public AddAccountForm()
        {
            this.Text = "Them acc - Rin";
            this.Size = new Size(460, 452);
            this.MinimumSize = new Size(460, 452);
            this.MaximumSize = new Size(460, 452);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = RinTheme.Bg;

            UiHelper.SetAppIcon(this);

            Image mascotImg = null;
            try { mascotImg = UiHelper.LoadMascot(64); } catch { }

            TitleBar bar = new TitleBar(this, mascotImg, "Them acc", 460, false, false);
            this.Controls.Add(bar);

            Panel top = UiHelper.CardPanel(12, 42, 436, 84);
            this.Controls.Add(top);

            PictureBox m = new PictureBox();
            m.Location = new Point(12, 10);
            m.Size = new Size(64, 64);
            m.SizeMode = PictureBoxSizeMode.Zoom;
            m.BackColor = Color.Transparent;
            if (mascotImg != null)
            {
                m.Image = mascotImg;
            }
            top.Controls.Add(m);

            Label t = new Label();
            t.Text = "Them acc moi";
            t.Font = new Font("Segoe UI", 14f, FontStyle.Bold);
            t.ForeColor = RinTheme.PrimaryDark;
            t.Location = new Point(86, 10);
            t.Size = new Size(336, 28);
            top.Controls.Add(t);

            Label s = new Label();
            s.Text = "Rin se tu check username giup ban.";
            s.Font = RinTheme.SubFont();
            s.ForeColor = RinTheme.Muted;
            s.Location = new Point(88, 40);
            s.Size = new Size(336, 20);
            top.Controls.Add(s);

            Label l1 = new Label();
            l1.Text = "Ten goi nho (vd: acc farm 1):";
            l1.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            l1.ForeColor = RinTheme.Text;
            l1.Location = new Point(14, 136);
            l1.AutoSize = true;
            this.Controls.Add(l1);

            txtAlias = new TextBox();
            Panel aliasBox = PinkBox(txtAlias, 12, 156, 436, 28, false);
            this.Controls.Add(aliasBox);

            Label l2 = new Label();
            l2.Text = "Cookie .ROBLOSECURITY:";
            l2.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            l2.ForeColor = RinTheme.Text;
            l2.Location = new Point(14, 192);
            l2.AutoSize = true;
            this.Controls.Add(l2);

            txtCookie = new TextBox();
            Panel cookieBox = PinkBox(txtCookie, 12, 212, 436, 100, true);
            this.Controls.Add(cookieBox);

            lblInfo = new Label();
            lblInfo.Location = new Point(14, 320);
            lblInfo.Size = new Size(436, 36);
            lblInfo.Font = new Font("Segoe UI", 8.5f);
            lblInfo.ForeColor = RinTheme.Muted;
            lblInfo.Text = "Paste ca doan (ke ca _|WARNING...) cung duoc.\nCookie luu ma hoa theo may nay, dung share cho ai.";
            this.Controls.Add(lblInfo);

            btnOk = new RoundedButton();
            btnOk.Text = "Luu + Check";
            btnOk.Location = new Point(222, 362);
            btnOk.Size = new Size(110, 38);
            btnOk.CornerRadius = 13;
            btnOk.FillColor = RinTheme.Primary;
            btnOk.ForeColor = Color.White;
            btnOk.Click += new EventHandler(BtnOk_Click);
            this.Controls.Add(btnOk);

            btnCancel = new RoundedButton();
            btnCancel.Text = "Huy";
            btnCancel.Location = new Point(340, 362);
            btnCancel.Size = new Size(108, 38);
            btnCancel.CornerRadius = 13;
            btnCancel.FillColor = RinTheme.PrimarySoft;
            btnCancel.HoverColor = RinTheme.Border;
            btnCancel.ForeColor = RinTheme.PrimaryDark;
            btnCancel.Click += delegate(object snd, EventArgs ev) { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.Controls.Add(btnCancel);

            Label credit = new Label();
            credit.Text = "by Rin0Suke257";
            credit.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
            credit.ForeColor = RinTheme.Primary;
            credit.BackColor = Color.Transparent;
            credit.Location = new Point(288, 412);
            credit.Size = new Size(160, 16);
            credit.TextAlign = ContentAlignment.MiddleRight;
            this.Controls.Add(credit);

            this.Load += delegate(object snd, EventArgs ev)
            {
                UiHelper.ApplyRound(this, 16);
                foreach (Control c in this.Controls)
                {
                    Panel p = c as Panel;
                    if (p != null && !(p is TitleBar))
                    {
                        UiHelper.ApplyRound(p, 12);
                    }
                }
            };
        }

        private Panel PinkBox(TextBox t, int x, int y, int w, int h, bool multi)
        {
            Panel p = new Panel();
            p.Location = new Point(x, y);
            p.Size = new Size(w, h);
            p.BackColor = RinTheme.Border;
            p.Padding = new Padding(2);
            t.BorderStyle = BorderStyle.None;
            t.BackColor = Color.White;
            t.ForeColor = RinTheme.Text;
            t.Font = new Font("Segoe UI", 9f);
            t.Location = new Point(3, 3);
            t.Size = new Size(w - 6, h - 6);
            t.Multiline = multi;
            if (multi)
            {
                t.ScrollBars = ScrollBars.Vertical;
            }
            p.Controls.Add(t);
            return p;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            string alias = txtAlias.Text.Trim();
            string cookie = txtCookie.Text.Trim().Trim('"');
            if (alias == "")
            {
                MessageBox.Show("Nhap ten goi nho.", "Rin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (cookie.Length < 100)
            {
                MessageBox.Show("Cookie qua ngan, kiem tra lai.", "Rin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            this.Cursor = Cursors.WaitCursor;
            try
            {
                RobloxUser u = RobloxApi.GetAuthenticatedUser(cookie);
                if (!u.Ok)
                {
                    MessageBox.Show("Cookie khong hop le: " + u.Error, "Rin", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                Account a = new Account();
                a.Alias = alias;
                a.Cookie = RobloxApi.ExtractCookie(cookie);
                a.Username = u.Name;
                a.UserId = u.Id;
                this.Result = a;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
    }
}
