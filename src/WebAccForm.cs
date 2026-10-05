using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace RinAccountManager
{
    // Mo web Roblox da dang nhap san acc (nhoi cookie truoc khi tai trang).
    public class WebAccForm : Form
    {
        private Account acc;
        private WebView2 web;
        private TextBox txtUrl;
        private string userDataDir;

        public WebAccForm(Account account)
        {
            acc = account;

            this.Text = "Web acc - " + (account == null ? "Rin" : account.Username);
            this.Size = new Size(620, 700);
            this.MinimumSize = new Size(620, 700);
            this.MaximumSize = new Size(620, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = RinTheme.Bg;

            UiHelper.SetAppIcon(this);

            Image mascotImg = null;
            try { mascotImg = UiHelper.LoadMascot(64); } catch { }

            TitleBar bar = new TitleBar(this, mascotImg, "Web acc (" + (account == null ? "?" : account.Username) + ")", 620, false, false);
            this.Controls.Add(bar);

            RoundedButton btnBack = BarBtn("<", 12, 36);
            btnBack.Click += delegate(object s, EventArgs e) { try { if (web.CoreWebView2 != null && web.CoreWebView2.CanGoBack) { web.CoreWebView2.GoBack(); } } catch { } };
            this.Controls.Add(btnBack);

            RoundedButton btnFwd = BarBtn(">", 52, 36);
            btnFwd.Click += delegate(object s, EventArgs e) { try { if (web.CoreWebView2 != null && web.CoreWebView2.CanGoForward) { web.CoreWebView2.GoForward(); } } catch { } };
            this.Controls.Add(btnFwd);

            RoundedButton btnReload = BarBtn("O", 92, 36);
            btnReload.Click += delegate(object s, EventArgs e) { try { if (web.CoreWebView2 != null) { web.CoreWebView2.Reload(); } } catch { } };
            this.Controls.Add(btnReload);

            txtUrl = new TextBox();
            txtUrl.Location = new Point(132, 36);
            txtUrl.Size = new Size(340, 24);
            txtUrl.BorderStyle = BorderStyle.FixedSingle;
            txtUrl.BackColor = Color.White;
            txtUrl.ForeColor = RinTheme.Text;
            txtUrl.Font = new Font("Segoe UI", 8.5f);
            txtUrl.KeyDown += new KeyEventHandler(TxtUrl_KeyDown);
            this.Controls.Add(txtUrl);

            RoundedButton btnOut = BarBtn("~>", 476, 36);
            btnOut.Click += new EventHandler(BtnOut_Click);
            this.Controls.Add(btnOut);

            web = new WebView2();
            web.Location = new Point(12, 64);
            web.Size = new Size(596, 600);
            this.Controls.Add(web);

            Label credit = new Label();
            credit.Text = "by Rin0Suke257 - thao tac gi cung tinh len acc that";
            credit.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
            credit.ForeColor = RinTheme.Primary;
            credit.BackColor = Color.Transparent;
            credit.Location = new Point(228, 670);
            credit.Size = new Size(380, 16);
            credit.TextAlign = ContentAlignment.MiddleRight;
            this.Controls.Add(credit);

            this.Load += new EventHandler(WebAccForm_Load);
            this.Shown += new EventHandler(WebAccForm_Shown);
        }

        private RoundedButton BarBtn(string text, int x, int y)
        {
            RoundedButton b = new RoundedButton();
            b.Text = text;
            b.Location = new Point(x, y);
            b.Size = new Size(32, 24);
            b.CornerRadius = 8;
            b.FillColor = RinTheme.PrimarySoft;
            b.HoverColor = RinTheme.Border;
            b.ForeColor = RinTheme.PrimaryDark;
            b.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            return b;
        }

        private void TxtUrl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    if (web.CoreWebView2 != null && txtUrl.Text.Trim() != "")
                    {
                        string u = txtUrl.Text.Trim();
                        if (u.IndexOf("://") < 0)
                        {
                            u = "https://" + u;
                        }
                        web.CoreWebView2.Navigate(u);
                    }
                }
                catch { }
                e.SuppressKeyPress = true;
            }
        }

        private void BtnOut_Click(object sender, EventArgs e)
        {
            try
            {
                string u = txtUrl.Text.Trim();
                if (u == "")
                {
                    u = "https://www.roblox.com/home";
                }
                System.Diagnostics.Process.Start(u);
            }
            catch { }
        }

        private void WebAccForm_Load(object sender, EventArgs e)
        {
            UiHelper.ApplyRound(this, 16);
        }

        private async void WebAccForm_Shown(object sender, EventArgs e)
        {
            if (acc == null)
            {
                this.Close();
                return;
            }
            // Cookie chet thi bao ngay, khoi mo web vo ich.
            bool live = await System.Threading.Tasks.Task.Run(new Func<bool>(delegate()
            {
                RobloxUser u = RobloxApi.GetAuthenticatedUser(acc.Cookie);
                return u.Ok;
            }));
            if (!live && !this.IsDisposed)
            {
                MessageBox.Show("Cookie acc " + acc.Username + " da die. Them lai acc truoc.",
                    "Rin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                try { this.Close(); } catch { }
                return;
            }
            userDataDir = Path.Combine(Path.GetTempPath(), "RinWv2_" + Guid.NewGuid().ToString("N"));
            try
            {
                CoreWebView2Environment env = await CoreWebView2Environment.CreateAsync(null, userDataDir);
                await web.EnsureCoreWebView2Async(env);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Khong mo duoc trinh duyet nhung.\n\n" + ex.Message,
                    "Rin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                try { this.Close(); } catch { }
                return;
            }
            try
            {
                CoreWebView2Cookie cookie = web.CoreWebView2.CookieManager.CreateCookie(
                    ".ROBLOSECURITY", RobloxApi.ExtractCookie(acc.Cookie), ".roblox.com", "/");
                cookie.IsHttpOnly = true;
                cookie.IsSecure = true;
                try { cookie.Expires = DateTime.Now.AddYears(1); } catch { }
                web.CoreWebView2.CookieManager.AddOrUpdateCookie(cookie);
                web.CoreWebView2.NewWindowRequested += new EventHandler<CoreWebView2NewWindowRequestedEventArgs>(Web_NewWindow);
                web.CoreWebView2.SourceChanged += new EventHandler<CoreWebView2SourceChangedEventArgs>(Web_Source);
                web.CoreWebView2.Navigate("https://www.roblox.com/home");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Loi nap acc: " + ex.Message, "Rin",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                try { this.Close(); } catch { }
            }
        }

        private void Web_NewWindow(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            try
            {
                e.Handled = true;
                web.CoreWebView2.Navigate(e.Uri);
            }
            catch { }
        }

        private void Web_Source(object sender, CoreWebView2SourceChangedEventArgs e)
        {
            try
            {
                string u = web.Source.ToString();
                if (txtUrl.InvokeRequired)
                {
                    txtUrl.Invoke(new Action(delegate() { txtUrl.Text = u; }));
                }
                else
                {
                    txtUrl.Text = u;
                }
            }
            catch { }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try { web.Dispose(); } catch { }
            try
            {
                if (!string.IsNullOrEmpty(userDataDir) && Directory.Exists(userDataDir))
                {
                    Directory.Delete(userDataDir, true);
                }
            }
            catch { }
            base.OnFormClosed(e);
        }
    }
}
