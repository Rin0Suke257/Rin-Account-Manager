using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RinAccountManager
{
    public class SettingsForm : Form
    {
        private TextBox txtWebhook;
        private CheckBox chkEvJoin;
        private CheckBox chkEvFail;
        private CheckBox chkEvRelogin;
        private CheckBox chkEvDie;
        private CheckBox chkShot;
        private TextBox txtShotDelay;
        private TextBox txtReloginMax;
        private TextBox txtReloginGap;
        private CheckBox chkAlias;
        private CheckBox chkSavePw;
        private CheckBox chkRelogin;
        private CheckBox chkAfk;
        private TextBox txtAfkMin;
        private Label lblMsg;

        public SettingsForm()
        {
            this.Text = "Cai dat - Rin";
            this.Size = new Size(480, 700);
            this.MinimumSize = new Size(480, 700);
            this.MaximumSize = new Size(480, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = RinTheme.Bg;

            UiHelper.SetAppIcon(this);

            Image mascotImg = null;
            try { mascotImg = UiHelper.LoadMascot(64); } catch { }

            TitleBar bar = new TitleBar(this, mascotImg, "Cai dat", 480, false, false);
            this.Controls.Add(bar);

            AppSettings s = SettingsStore.Load();

            int y = 42;
            AddHead("Webhook Discord", y);
            y += 22;

            txtWebhook = new TextBox();
            txtWebhook.Location = new Point(14, y);
            txtWebhook.Size = new Size(452, 24);
            txtWebhook.BorderStyle = BorderStyle.FixedSingle;
            txtWebhook.BackColor = Color.White;
            txtWebhook.ForeColor = RinTheme.Text;
            txtWebhook.Font = new Font("Segoe UI", 8.5f);
            txtWebhook.Text = s.WebhookUrl == null ? "" : s.WebhookUrl;
            this.Controls.Add(txtWebhook);
            y += 30;

            RoundedButton btnTest = SmallBtn("Gui thu", 14, y);
            btnTest.Click += new EventHandler(BtnTest_Click);
            this.Controls.Add(btnTest);

            Label hint = new Label();
            hint.Text = "Tao o Discord: Server Settings > Integrations > Webhooks > New Webhook > Copy URL.";
            hint.Font = new Font("Segoe UI", 8f, FontStyle.Italic);
            hint.ForeColor = RinTheme.Muted;
            hint.Location = new Point(130, y);
            hint.Size = new Size(336, 30);
            this.Controls.Add(hint);
            y += 38;

            AddHead("Gui khi", y);
            y += 22;
            chkEvJoin = AddCheck("Vao game OK (+ anh)", 14, y, s.EvJoin);
            chkEvFail = AddCheck("Launch loi", 250, y, s.EvFail);
            y += 26;
            chkEvRelogin = AddCheck("Tu vao lai", 14, y, s.EvRelogin);
            chkEvDie = AddCheck("Cookie die", 250, y, s.EvDie);
            y += 26;
            chkAlias = AddCheck("Hien Alias thay username", 14, y, s.UseAlias);
            y += 26;
            chkSavePw = AddCheck("Luu mat khau (tat mac dinh)", 14, y, s.SavePasswords);
            y += 22;
            Label pwWarn = new Label();
            pwWarn.Text = "Bat len thi tu chiu: ai mo duoc may nay thi mo duoc pass.";
            pwWarn.Font = new Font("Segoe UI", 8f, FontStyle.Italic);
            pwWarn.ForeColor = RinTheme.Muted;
            pwWarn.Location = new Point(14, y);
            pwWarn.Size = new Size(452, 16);
            this.Controls.Add(pwWarn);
            y += 22;

            AddHead("Anh chup khi vao game", y);
            y += 22;
            chkShot = AddCheck("Dinh kem anh client", 14, y, s.ShotEnabled);
            y += 26;
            AddMiniLabel("Chup sau (giay, 10-300):", 14, y);
            txtShotDelay = MiniNum(200, y, s.ShotDelaySec.ToString());
            y += 30;

            AddHead("Tu vao lai", y);
            y += 22;
            AddMiniLabel("So lan toi da (1-20):", 14, y);
            txtReloginMax = MiniNum(200, y, s.ReloginMax.ToString());
            AddMiniLabel("Gian cach (giay, >=5):", 270, y);
            txtReloginGap = MiniNum(410, y, s.ReloginGapSec.ToString());
            y += 34;

            AddHead("Treo acc (mac dinh tat)", y);
            y += 22;
            chkRelogin = AddCheck("Tu vao lai khi kick", 14, y, s.ReloginEnabled);
            y += 26;
            chkAfk = AddCheck("Nhay chong-kick moi", 14, y, s.AfkEnabled);
            chkAfk.CheckedChanged += new EventHandler(ChkAfkWarn_Click);
            AddMiniLabel("phut (1-19):", 250, y);
            txtAfkMin = MiniNum(340, y, s.AfkMinutes.ToString());
            y += 30;
            Label afkNote = new Label();
            afkNote.Text = "Chong-kick se giat focus ~1 giay moi lan. Game gat co the ban acc farm.";
            afkNote.Font = new Font("Segoe UI", 8f, FontStyle.Italic);
            afkNote.ForeColor = RinTheme.Muted;
            afkNote.Location = new Point(14, y);
            afkNote.Size = new Size(452, 16);
            this.Controls.Add(afkNote);
            y += 22;

            lblMsg = new Label();
            lblMsg.Location = new Point(14, y);
            lblMsg.Size = new Size(452, 20);
            lblMsg.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            lblMsg.ForeColor = RinTheme.PrimaryDark;
            lblMsg.Text = "";
            this.Controls.Add(lblMsg);
            y += 24;

            RoundedButton btnSave = new RoundedButton();
            btnSave.Text = "Luu";
            btnSave.Location = new Point(236, y);
            btnSave.Size = new Size(110, 36);
            btnSave.CornerRadius = 12;
            btnSave.FillColor = RinTheme.Primary;
            btnSave.ForeColor = Color.White;
            btnSave.Click += new EventHandler(BtnSave_Click);
            this.Controls.Add(btnSave);

            RoundedButton btnCancel = new RoundedButton();
            btnCancel.Text = "Dong";
            btnCancel.Location = new Point(356, y);
            btnCancel.Size = new Size(110, 36);
            btnCancel.CornerRadius = 12;
            btnCancel.FillColor = Color.White;
            btnCancel.HoverColor = RinTheme.PrimarySoft;
            btnCancel.ForeColor = RinTheme.PrimaryDark;
            btnCancel.Click += delegate(object snd, EventArgs ev) { this.Close(); };
            this.Controls.Add(btnCancel);

            Label credit = new Label();
            credit.Text = "by Rin0Suke257";
            credit.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
            credit.ForeColor = RinTheme.Primary;
            credit.BackColor = Color.Transparent;
            credit.Location = new Point(14, y + 4);
            credit.Size = new Size(160, 16);
            this.Controls.Add(credit);

            this.Load += delegate(object snd, EventArgs ev) { UiHelper.ApplyRound(this, 16); };
        }

        private void AddHead(string text, int y)
        {
            Label l = new Label();
            l.Text = text;
            l.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            l.ForeColor = RinTheme.Text;
            l.Location = new Point(14, y);
            l.AutoSize = true;
            this.Controls.Add(l);
        }

        private CheckBox AddCheck(string text, int x, int y, bool val)
        {
            CheckBox c = new CheckBox();
            c.Text = text;
            c.Font = new Font("Segoe UI", 9f);
            c.ForeColor = RinTheme.Text;
            c.BackColor = Color.Transparent;
            c.Location = new Point(x, y);
            c.Size = new Size(220, 22);
            c.Checked = val;
            this.Controls.Add(c);
            return c;
        }

        private void AddMiniLabel(string text, int x, int y)
        {
            Label l = new Label();
            l.Text = text;
            l.Font = new Font("Segoe UI", 9f);
            l.ForeColor = RinTheme.Text;
            l.Location = new Point(x, y + 2);
            l.AutoSize = true;
            this.Controls.Add(l);
        }

        private TextBox MiniNum(int x, int y, string val)
        {
            TextBox t = new TextBox();
            t.Location = new Point(x, y);
            t.Size = new Size(56, 22);
            t.BorderStyle = BorderStyle.FixedSingle;
            t.BackColor = Color.White;
            t.ForeColor = RinTheme.Text;
            t.Font = new Font("Segoe UI", 9f);
            t.Text = val;
            this.Controls.Add(t);
            return t;
        }

        private RoundedButton SmallBtn(string text, int x, int y)
        {
            RoundedButton b = new RoundedButton();
            b.Text = text;
            b.Location = new Point(x, y);
            b.Size = new Size(110, 30);
            b.CornerRadius = 10;
            b.FillColor = RinTheme.PrimarySoft;
            b.HoverColor = RinTheme.Border;
            b.ForeColor = RinTheme.PrimaryDark;
            b.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            return b;
        }

        private int Num(TextBox t, int def, int min, int max)
        {
            try
            {
                int v = int.Parse(t.Text.Trim());
                if (v < min)
                {
                    v = min;
                }
                if (v > max)
                {
                    v = max;
                }
                return v;
            }
            catch
            {
                return def;
            }
        }

        private AppSettings Collect()
        {
            AppSettings s = new AppSettings();
            s.WebhookUrl = txtWebhook.Text.Trim();
            s.EvJoin = chkEvJoin.Checked;
            s.EvFail = chkEvFail.Checked;
            s.EvRelogin = chkEvRelogin.Checked;
            s.EvDie = chkEvDie.Checked;
            s.ShotEnabled = chkShot.Checked;
            s.ShotDelaySec = Num(txtShotDelay, 45, 10, 300);
            s.ReloginMax = Num(txtReloginMax, 5, 1, 20);
            s.ReloginGapSec = Num(txtReloginGap, 15, 5, 300);
            s.UseAlias = chkAlias.Checked;
            s.SavePasswords = chkSavePw.Checked;
            s.ReloginEnabled = chkRelogin.Checked;
            s.AfkEnabled = chkAfk.Checked;
            s.AfkMinutes = Num(txtAfkMin, 15, 1, 19);
            return s;
        }

        private void ChkAfkWarn_Click(object sender, EventArgs e)
        {
            if (chkAfk.Checked)
            {
                MessageBox.Show("Chong-kick se giat focus: moi vai phut dua 1 cua so game len truoc mat ~1 giay roi tra lai.\n" +
                    "Dang xem phim full-screen hay go do co the bi gian doan. Game gat co the ban acc farm.",
                    "Rin - Nhay chong-kick", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            SettingsStore.Save(Collect());
            lblMsg.Text = "Da luu.";
        }

        private async void BtnTest_Click(object sender, EventArgs e)
        {
            string url = txtWebhook.Text.Trim();
            if (url == "")
            {
                MessageBox.Show("Nhap webhook URL truoc.", "Rin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            lblMsg.Text = "Dang gui thu...";
            bool ok = await Task.Run(new Func<bool>(delegate()
            {
                return Webhook.Send(url, "Rin test webhook - " + DateTime.Now.ToString("HH:mm:ss"), "Test thanh cong", "Neu thay tin nay la webhook chay.", null);
            }));
            lblMsg.Text = ok ? "Da gui, kiem tra Discord." : "Gui that bai, kiem tra URL.";
        }
    }
}
