using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace RinAccountManager
{
    public class BrowserLoginForm : Form
    {
        private WebView2 web;
        private Label lblInfo;
        private Timer watchTimer;
        private bool checking = false;
        private bool done = false;
        private string userDataDir;
        private string prefillUser;
        private string prefillPass;
        private int prefillTries = 0;

        public Account Result { get; private set; }
        public bool OtherMethod { get; private set; }

        public BrowserLoginForm() : this(null, null)
        {
        }

        public BrowserLoginForm(string username, string password)
        {
            prefillUser = username == null ? "" : username;
            prefillPass = password == null ? "" : password;
            this.Text = "Dang nhap Roblox - Rin";
            this.Size = new Size(480, 680);
            this.MinimumSize = new Size(480, 680);
            this.MaximumSize = new Size(480, 680);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = RinTheme.Bg;

            UiHelper.SetAppIcon(this);

            Image mascotImg = null;
            try { mascotImg = UiHelper.LoadMascot(64); } catch { }

            TitleBar bar = new TitleBar(this, mascotImg, "Dang nhap Roblox", 480, false, false);
            this.Controls.Add(bar);

            lblInfo = new Label();
            lblInfo.Location = new Point(12, 36);
            lblInfo.Size = new Size(456, 32);
            lblInfo.Font = new Font("Segoe UI", 8.5f);
            lblInfo.ForeColor = RinTheme.Text;
            lblInfo.Text = "Dang nhap nhu web binh thuong (captcha/2FA cu lam theo Roblox). Xong app tu them acc.";
            this.Controls.Add(lblInfo);

            web = new WebView2();
            web.Location = new Point(12, 70);
            web.Size = new Size(456, 560);
            this.Controls.Add(web);

            Label credit = new Label();
            credit.Text = "by Rin0Suke257";
            credit.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
            credit.ForeColor = RinTheme.Primary;
            credit.BackColor = Color.Transparent;
            credit.Location = new Point(308, 636);
            credit.Size = new Size(160, 16);
            credit.TextAlign = ContentAlignment.MiddleRight;
            this.Controls.Add(credit);

            LinkLabel lnkOther = new LinkLabel();
            lnkOther.Text = "Dung Quick Login / Cookie";
            lnkOther.Location = new Point(12, 636);
            lnkOther.Size = new Size(220, 16);
            lnkOther.LinkColor = RinTheme.PrimaryDark;
            lnkOther.Click += new EventHandler(LnkOther_Click);
            this.Controls.Add(lnkOther);

            this.Load += new EventHandler(BrowserLoginForm_Load);
            this.Shown += new EventHandler(BrowserLoginForm_Shown);

            watchTimer = new Timer();
            watchTimer.Interval = 3000;
            watchTimer.Tick += new EventHandler(WatchTimer_Tick);
        }

        private void BrowserLoginForm_Load(object sender, EventArgs e)
        {
            UiHelper.ApplyRound(this, 16);
        }

        private async void BrowserLoginForm_Shown(object sender, EventArgs e)
        {
            // Moi lan mo la 1 profile trang: khong ke thua session cu -> khong tu add trung acc.
            string dataDir = Path.Combine(Path.GetTempPath(), "RinWv2_" + Guid.NewGuid().ToString("N"));
            userDataDir = dataDir;
            try
            {
                if (!Directory.Exists(dataDir))
                {
                    Directory.CreateDirectory(dataDir);
                }
            }
            catch { }
            try
            {
                CoreWebView2Environment env = await CoreWebView2Environment.CreateAsync(null, dataDir);
                await web.EnsureCoreWebView2Async(env);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Khong mo duoc trinh duyet nhung (thieu WebView2 Runtime).\n" +
                    "Hay dung Quick Login hoac cookie thay the.\n\n" + ex.Message,
                    "Rin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return;
            }
            try
            {
                web.CoreWebView2.NewWindowRequested += new EventHandler<CoreWebView2NewWindowRequestedEventArgs>(Web_NewWindow);
                web.CoreWebView2.NavigationCompleted += new EventHandler<CoreWebView2NavigationCompletedEventArgs>(Web_Navigated);
                web.CoreWebView2.Navigate("https://www.roblox.com/login");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Loi mo trang login: " + ex.Message, "Rin",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return;
            }
            watchTimer.Start();
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

        private async void Web_Navigated(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                SetInfo("Khong tai duoc trang login (mang nay co the chan roblox.com). Hay dung Quick Login hoac cookie.");
                return;
            }
            TryPrefill();
            await CheckLogin();
        }

        // Dien san user/pass (dang nhap lai): tim o theo loai input, tuong thich React.
        private async void TryPrefill()
        {
            try
            {
                if (web == null || web.CoreWebView2 == null)
                {
                    return;
                }
                if (prefillUser == "" && prefillPass == "")
                {
                    return;
                }
                if (prefillTries >= 6)
                {
                    return;
                }
                prefillTries++;
                string js = "(function(){"
                    + "function setEl(el,v){try{var d=Object.getOwnPropertyDescriptor(window.HTMLInputElement.prototype,'value');d.set.call(el,v);el.dispatchEvent(new Event('input',{bubbles:true}));el.dispatchEvent(new Event('change',{bubbles:true}));return true;}catch(e){try{el.value=v;return true;}catch(e2){return false;}}}"
                    + "var u=document.querySelector('#login-username')||document.querySelector('input[name=\"username\"]')||document.querySelector('input[autocomplete=\"username\"]');"
                    + "var p=document.querySelector('#login-password')||document.querySelector('input[type=\"password\"]');"
                    + "var r='';"
                    + "if(u&&'" + prefillUser.Replace("\\", "\\\\").Replace("'", "\\'") + "'!==''){if(setEl(u,'" + prefillUser.Replace("\\", "\\\\").Replace("'", "\\'") + "'))r+='u';}"
                    + "if(p&&'" + prefillPass.Replace("\\", "\\\\").Replace("'", "\\'") + "'!==''){if(setEl(p,'" + prefillPass.Replace("\\", "\\\\").Replace("'", "\\'") + "'))r+='p';}"
                    + "return r;})()";
                string res = await web.CoreWebView2.ExecuteScriptAsync(js);
                if ((res == null || res.IndexOf("u") < 0) && prefillTries < 6)
                {
                    // React chua render xong -> thu lai sau 2s
                    Timer retry = new Timer();
                    retry.Interval = 2000;
                    retry.Tick += delegate(object s, EventArgs ev)
                    {
                        try { retry.Stop(); retry.Dispose(); } catch { }
                        TryPrefill();
                    };
                    retry.Start();
                }
            }
            catch { }
        }

        private void SetInfo(string s)
        {
            try
            {
                if (lblInfo.InvokeRequired)
                {
                    lblInfo.Invoke(new Action<string>(SetInfo), new object[] { s });
                    return;
                }
                lblInfo.Text = s;
            }
            catch { }
        }

        private async void WatchTimer_Tick(object sender, EventArgs e)
        {
            await CheckLogin();
        }

        private async Task CheckLogin()
        {
            if (checking || done)
            {
                return;
            }
            if (web == null || web.CoreWebView2 == null)
            {
                return;
            }
            checking = true;
            try
            {
                List<CoreWebView2Cookie> list = await web.CoreWebView2.CookieManager.GetCookiesAsync("https://www.roblox.com");
                for (int i = 0; i < list.Count; i++)
                {
                    CoreWebView2Cookie c = list[i];
                    if (c.Name == ".ROBLOSECURITY" && c.Value != null && c.Value.Length > 100)
                    {
                        RobloxUser u = RobloxApi.GetAuthenticatedUser(RobloxApi.ExtractCookie(c.Value));
                        if (u.Ok)
                        {
                            done = true;
                            try { watchTimer.Stop(); } catch { }
                            Account a = new Account();
                            a.Alias = u.Name;
                            a.Cookie = RobloxApi.ExtractCookie(c.Value);
                            a.Username = u.Name;
                            a.UserId = u.Id;
                            Result = a;
                            try
                            {
                                this.DialogResult = DialogResult.OK;
                                this.Close();
                            }
                            catch { }
                            return;
                        }
                    }
                }
            }
            catch { }
            finally
            {
                checking = false;
            }
        }

        private void LnkOther_Click(object sender, EventArgs e)
        {
            OtherMethod = true;
            try
            {
                this.DialogResult = DialogResult.Abort;
                this.Close();
            }
            catch { }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try { watchTimer.Stop(); } catch { }
            try { watchTimer.Dispose(); } catch { }
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
