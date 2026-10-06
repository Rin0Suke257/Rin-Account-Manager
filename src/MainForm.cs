using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RinAccountManager
{
    // Layout mini 600x300, vien hong tu ve (khong dung title-bar Windows).
    public class MainForm : Form
    {
        private List<Account> accounts;

        private ListView lv;
        private TextBox txtPlaceId;
        private TextBox txtJobId;
        private TextBox txtDelay;
        private TextBox txtFollow;
        private CheckBox chkMulti;
        private RoundedButton btnAdd;
        private RoundedButton btnDel;
        private RoundedButton btnCheckAll;
        private RoundedButton btnLaunchSel;
        private RoundedButton btnLaunchAll;
        private RoundedButton btnKill;
        private RoundedButton btnFix;
        private Label lblLast;
        private Label lblBarStatus;
        private Timer timerRefresh;
        private Timer timerAfk;
        private bool suppressChk = false;

        private class Session
        {
            public long PlaceId;
            public string JobId;
            public string VipCode;
            public string ShareCode;
            public long FollowId;
            public int Pid;
            public int Retries;
            public DateTime LastAfk;
            public DateTime BornAt;
        }

        private Dictionary<Account, Session> sessions = new Dictionary<Account, Session>();
        private DateTime lastRelogin = DateTime.MinValue;
        private DateTime lastAfkSweep = DateTime.MinValue;
        private int wmiCounter = 0;
        private bool afkBusy = false;
        private bool reloginBusy = false;
        private AppSettings settings = SettingsStore.Load();

        public MainForm()
        {
            this.Text = "Rin Account Manager - Multi Roblox";
            this.Size = new Size(600, 300);
            this.MinimumSize = new Size(600, 300);
            this.MaximumSize = new Size(600, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = RinTheme.Bg;
            this.FormBorderStyle = FormBorderStyle.None;

            accounts = AccountStore.Load();

            UiHelper.SetAppIcon(this);

            Image mascotImg = null;
            try { mascotImg = UiHelper.LoadMascot(64); } catch { }

            TitleBar bar = new TitleBar(this, mascotImg, "Rin Account Manager", 600, true, true, true);
            this.Controls.Add(bar);
            lblBarStatus = bar.StatusLabel;
            lblBarStatus.Text = "Multi: ...";
            if (bar.GearLabel != null)
            {
                bar.GearLabel.Click += new EventHandler(BtnGear_Click);
            }

            Panel accCard = UiHelper.CardPanel(4, 36, 312, 236);
            this.Controls.Add(accCard);

            lv = new ListView();
            lv.Location = new Point(5, 5);
            lv.Size = new Size(302, 226);
            lv.View = View.Details;
            lv.CheckBoxes = true;
            lv.FullRowSelect = true;
            lv.GridLines = false;
            lv.BorderStyle = BorderStyle.None;
            lv.BackColor = Color.White;
            lv.ForeColor = RinTheme.Text;
            lv.Font = new Font("Segoe UI", 8f);
            lv.Columns.Add("Alias", 80);
            lv.Columns.Add("Username", 85);
            lv.Columns.Add("UserId", 50);
            lv.Columns.Add("TT", 32);
            lv.Columns.Add("Note", 48);
            lv.ShowItemToolTips = true;
            lv.MouseDoubleClick += new MouseEventHandler(Lv_DoubleClick);
            accCard.Controls.Add(lv);

            int left = 328;
            int right = 464;
            int bw = 128;
            int by = 36;
            int bh = 24;
            int gap = 5;

            btnAdd = MakeBtn("Them acc", left, by, bw, bh, RinTheme.Primary, Color.White);
            btnAdd.Click += new EventHandler(BtnAdd_Click);
            btnDel = MakeBtn("Xoa", right, by, bw, bh, RinTheme.PrimarySoft, RinTheme.PrimaryDark);
            btnDel.Click += new EventHandler(BtnDel_Click);
            by += bh + gap;
            btnCheckAll = MakeBtn("Check live", left, by, bw, bh, RinTheme.PrimarySoft, RinTheme.PrimaryDark);
            btnCheckAll.Click += new EventHandler(BtnCheckAll_Click);
            btnFix = MakeBtn("Fix 773", right, by, bw, bh, RinTheme.PrimarySoft, RinTheme.PrimaryDark);
            btnFix.Click += new EventHandler(BtnFix_Click);
            by += bh + gap;
            btnLaunchSel = MakeBtn("Play da tick", left, by, bw, bh, RinTheme.Primary, Color.White);
            btnLaunchSel.Click += new EventHandler(BtnLaunchSel_Click);
            btnLaunchAll = MakeBtn("Play TAT CA", right, by, bw, bh, RinTheme.PrimaryDark, Color.White);
            btnLaunchAll.Click += new EventHandler(BtnLaunchAll_Click);
            by += bh + gap;
            btnKill = MakeBtn("Kill all", left, by, bw, 22, Color.White, RinTheme.PrimaryDark);
            btnKill.Click += new EventHandler(BtnKill_Click);
            RoundedButton btnWeb = MakeBtn("Web acc", right, by, bw, 22, RinTheme.PrimarySoft, RinTheme.PrimaryDark);
            btnWeb.Click += new EventHandler(BtnWeb_Click);
            by += 22 + gap;

            txtPlaceId = MiniBox(left, by, 122, 22, "PlaceId / link game");
            RoundedButton btnGame = MakeBtn("v", left + 126, by, 24, 22, RinTheme.PrimarySoft, RinTheme.PrimaryDark);
            btnGame.Click += new EventHandler(BtnGame_Click);
            txtJobId = MiniBox(left + 154, by, 102, 22, "JobId");
            by += 22 + gap;

            txtDelay = MiniBox(left, by, 44, 22, "3s");
            txtDelay.Text = "3";
            chkMulti = new CheckBox();
            chkMulti.Text = "Multi";
            chkMulti.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            chkMulti.ForeColor = RinTheme.PrimaryDark;
            chkMulti.BackColor = Color.Transparent;
            chkMulti.Location = new Point(left + 50, by);
            chkMulti.Size = new Size(64, 22);
            chkMulti.Checked = true;
            chkMulti.CheckedChanged += new EventHandler(ChkMulti_CheckedChanged);
            this.Controls.Add(chkMulti);

            Label lblFollow = new Label();
            lblFollow.Text = "Theo @";
            lblFollow.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            lblFollow.ForeColor = RinTheme.PrimaryDark;
            lblFollow.BackColor = Color.Transparent;
            lblFollow.Location = new Point(left + 118, by + 2);
            lblFollow.Size = new Size(52, 20);
            this.Controls.Add(lblFollow);

            txtFollow = MiniBox(left + 170, by, 86, 22, "username");

            // Cum quan ly cua so game (2x2)
            int wy = 204;
            RoundedButton btnTile = MakeBtn("Xep cua so", left, wy, 128, 26, RinTheme.PrimarySoft, RinTheme.PrimaryDark);
            btnTile.Click += new EventHandler(BtnTile_Click);
            RoundedButton btnMinAll = MakeBtn("Thu gon", left + 132, wy, 128, 26, RinTheme.PrimarySoft, RinTheme.PrimaryDark);
            btnMinAll.Click += new EventHandler(BtnMinAll_Click);
            wy += 30;
            RoundedButton btnShowAll = MakeBtn("Hien het", left, wy, 128, 26, RinTheme.PrimarySoft, RinTheme.PrimaryDark);
            btnShowAll.Click += new EventHandler(BtnShowAll_Click);
            RoundedButton btnKillFroze = MakeBtn("Dong client do", left + 132, wy, 128, 26, RinTheme.PrimarySoft, RinTheme.PrimaryDark);
            btnKillFroze.Click += new EventHandler(BtnKillFrozen_Click);

            lblLast = new Label();
            lblLast.Location = new Point(8, 280);
            lblLast.Size = new Size(415, 14);
            lblLast.Font = new Font("Segoe UI", 7.5f);
            lblLast.ForeColor = RinTheme.Muted;
            lblLast.Text = "San sang.";
            this.Controls.Add(lblLast);

            Label credit = new Label();
            credit.Text = "by Rin0Suke257";
            credit.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
            credit.ForeColor = RinTheme.Primary;
            credit.BackColor = Color.Transparent;
            credit.Location = new Point(428, 278);
            credit.Size = new Size(164, 16);
            credit.TextAlign = ContentAlignment.MiddleRight;
            this.Controls.Add(credit);

            this.Shown += new EventHandler(MainForm_Shown);
            this.Load += new EventHandler(MainForm_Load);

            timerRefresh = new Timer();
            timerRefresh.Interval = 1500;
            timerRefresh.Tick += new EventHandler(TimerRefresh_Tick);
            timerRefresh.Start();

            timerAfk = new Timer();
            timerAfk.Interval = 30000;
            timerAfk.Tick += new EventHandler(TimerAfk_Tick);
            timerAfk.Start();

            RefreshList();
            CheckExeSilent();
            Log("Da load " + accounts.Count.ToString() + " acc. Khong share cookie cho ai.");
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            UiHelper.ApplyRound(this, 14);
            foreach (Control c in this.Controls)
            {
                Panel p = c as Panel;
                if (p != null && !(p is TitleBar))
                {
                    UiHelper.ApplyRound(p, 12);
                }
            }
        }

        private TextBox MiniBox(int x, int y, int w, int h, string cue)
        {
            TextBox t = new TextBox();
            t.Location = new Point(x, y);
            t.Size = new Size(w, h);
            t.BorderStyle = BorderStyle.FixedSingle;
            t.BackColor = Color.White;
            t.ForeColor = RinTheme.Text;
            t.Font = new Font("Segoe UI", 8.5f);
            this.Controls.Add(t);
            UiHelper.SetCue(t, cue);
            return t;
        }

        private RoundedButton MakeBtn(string text, int x, int y, int w, int h, Color fill, Color fg)
        {
            RoundedButton b = new RoundedButton();
            b.Text = text;
            b.Location = new Point(x, y);
            b.Size = new Size(w, h);
            b.CornerRadius = 10;
            b.FillColor = fill;
            b.HoverColor = (fill == RinTheme.PrimarySoft || fill == Color.White) ? RinTheme.Border : RinTheme.PrimaryDark;
            b.PressedColor = RinTheme.PrimaryDark;
            b.ForeColor = fg;
            b.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            if (fill == Color.White)
            {
                b.HoverColor = RinTheme.PrimarySoft;
            }
            this.Controls.Add(b);
            return b;
        }

        private void Log(string s)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action<string>(Log), new object[] { s });
                    return;
                }
                string line = "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + s;
                if (line.Length > 90)
                {
                    line = line.Substring(0, 90) + "...";
                }
                lblLast.Text = line;
                lblLast.Refresh();
            }
            catch { }
        }

        private void RefreshList()
        {
            lv.Items.Clear();
            for (int i = 0; i < accounts.Count; i++)
            {
                Account a = accounts[i];
                ListViewItem it = new ListViewItem(a.Alias);
                it.SubItems.Add(a.Username);
                it.SubItems.Add(a.UserId > 0 ? a.UserId.ToString() : "-");
                string st = "?";
                if (a.Live == "live")
                {
                    st = "ok";
                }
                else if (a.Live == "die")
                {
                    st = "die";
                }
                it.SubItems.Add(st);
                it.SubItems.Add(a.Note == null ? "" : a.Note);
                it.Tag = a;
                it.Checked = true;
                if (!string.IsNullOrEmpty(a.Note))
                {
                    it.ToolTipText = a.Note;
                }
                lv.Items.Add(it);
            }
        }

        private void Lv_DoubleClick(object sender, MouseEventArgs e)
        {
            ListViewItem hit = null;
            try
            {
                hit = lv.GetItemAt(e.X, e.Y);
            }
            catch { }
            if (hit == null)
            {
                return;
            }
            Account a = hit.Tag as Account;
            if (a == null)
            {
                return;
            }
            // Acc DIE: double-click = dang nhap lai (dien san user/pass).
            if (a.Live == "die")
            {
                ReloginAccount(a);
                return;
            }
            bool allowPw = settings.SavePasswords;
            EditAccountForm f = new EditAccountForm(a.Alias, a.Note == null ? "" : a.Note, a.Username, allowPw, a.Password == null ? "" : a.Password);
            if (f.ShowDialog(this) == DialogResult.OK)
            {
                a.Alias = f.AliasText;
                a.Note = f.NoteText;
                if (allowPw)
                {
                    a.Password = f.PasswordText;
                }
                SaveAccounts();
                RefreshList();
                Log("Da sua: " + a.Alias);
            }
            try { f.Dispose(); } catch { }
        }

        private void ReloginAccount(Account a)
        {
            string pw = "";
            if (settings.SavePasswords && !string.IsNullOrEmpty(a.Password))
            {
                pw = a.Password;
            }
            BrowserLoginForm b = new BrowserLoginForm(a.Username, pw);
            DialogResult dr = DialogResult.Cancel;
            try
            {
                dr = b.ShowDialog(this);
            }
            catch { }
            Account acc = b.Result;
            try { b.Dispose(); } catch { }
            if (dr == DialogResult.OK && acc != null)
            {
                AddAccountIfNew(acc);
            }
        }

        private void SaveAccounts()
        {
            string err;
            if (!AccountStore.Save(accounts, out err))
            {
                Log("Luu acc that bai: " + err);
            }
        }

        // Them moi, hoac cap nhat cookie neu acc da co (dang nhap lai khoi xoa).
        private bool AddAccountIfNew(Account acc)
        {
            if (acc != null && acc.UserId > 0)
            {
                for (int i = 0; i < accounts.Count; i++)
                {
                    if (accounts[i].UserId == acc.UserId)
                    {
                        accounts[i].Cookie = acc.Cookie;
                        accounts[i].Username = acc.Username;
                        accounts[i].Live = "live";
                        SaveAccounts();
                        RefreshList();
                        Log("Da cap nhat cookie: " + accounts[i].Alias);
                        MessageBox.Show("Acc " + acc.Username + " da co, da cap nhat cookie moi (giu nguyen ten + ghi chu).",
                            "Rin", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;
                    }
                }
            }
            accounts.Add(acc);
            SaveAccounts();
            RefreshList();
            Log("Da them: " + acc.Alias + "/" + acc.Username);
            return true;
        }

        private void CheckExeSilent()
        {
            string exe = Launcher.FindPlayerExe();
            if (exe == null)
            {
                Log("KHONG TIM THAY Roblox Player!");
            }
        }

        private void TimerRefresh_Tick(object sender, EventArgs e)
        {
            int n = Launcher.CountRunning();
            string multi = MultiRoblox.IsEnabled ? "ON" : "OFF";
            lblBarStatus.Text = "Multi " + multi + " - chay: " + n.ToString();
            wmiCounter++;
            if (wmiCounter >= 7)
            {
                wmiCounter = 0;
                Task.Run(new Action(AttachPids));
            }
            if (settings.ReloginEnabled)
            {
                CheckRelogin();
            }
        }

        private static bool PidAlive(int pid)
        {
            if (pid <= 0)
            {
                return false;
            }
            try
            {
                System.Diagnostics.Process p = System.Diagnostics.Process.GetProcessById(pid);
                bool alive = !p.HasExited && p.ProcessName == "RobloxPlayerBeta";
                try { p.Dispose(); } catch { }
                return alive;
            }
            catch
            {
                return false;
            }
        }

        private static System.Collections.Generic.List<int> LivePids()
        {
            System.Collections.Generic.List<int> list = new System.Collections.Generic.List<int>();
            System.Diagnostics.Process[] ps = System.Diagnostics.Process.GetProcessesByName("RobloxPlayerBeta");
            for (int i = 0; i < ps.Length; i++)
            {
                try
                {
                    if (!ps[i].HasExited)
                    {
                        list.Add(ps[i].Id);
                    }
                }
                catch { }
                try { ps[i].Dispose(); } catch { }
            }
            return list;
        }

        // Gan pid cho session: 1) khop BrowserTrackerId (neu doc duoc), 2) nhan client chua ai nhan.
        private void AttachPids()
        {
            try
            {
                // 1) WMI tracker (giong RAM, khi doc duoc)
                List<Account> need = new List<Account>();
                lock (sessions)
                {
                    foreach (KeyValuePair<Account, Session> kv in sessions)
                    {
                        if (!PidAlive(kv.Value.Pid))
                        {
                            need.Add(kv.Key);
                        }
                    }
                }
                if (need.Count > 0)
                {
                    try
                    {
                        System.Management.ManagementObjectSearcher searcher = new System.Management.ManagementObjectSearcher(
                            "SELECT ProcessId, CommandLine FROM Win32_Process WHERE Name='RobloxPlayerBeta.exe'");
                        foreach (System.Management.ManagementObject mo in searcher.Get())
                        {
                            try
                            {
                                int pid = Convert.ToInt32(mo["ProcessId"]);
                                string cmd = Convert.ToString(mo["CommandLine"]);
                                if (cmd == null)
                                {
                                    continue;
                                }
                                System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(cmd, "\\-b\\s*(\\d+)");
                                if (!m.Success)
                                {
                                    continue;
                                }
                                string tracker = m.Groups[1].Value;
                                lock (sessions)
                                {
                                    foreach (Account a in need)
                                    {
                                        Session ss;
                                        if (sessions.TryGetValue(a, out ss) && !PidAlive(ss.Pid) && a.BrowserTrackerId == tracker)
                                        {
                                            ss.Pid = pid;
                                        }
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                    catch { }
                }
                // 2) Client song ma chua ai nhan -> gan cho session doi lau nhat (chong mo trung).
                AdoptOrphans();
            }
            catch { }
        }

        // Gan pid ngay sau khi mo (snapshot truoc/sau) - chinh xac hon doan theo thu tu.
        private async Task AttachSpawned(Account a, System.Collections.Generic.List<int> before)
        {
            await Task.Delay(3000);
            try
            {
                System.Collections.Generic.List<int> now = LivePids();
                System.Collections.Generic.List<int> fresh = new System.Collections.Generic.List<int>();
                for (int i = 0; i < now.Count; i++)
                {
                    if (!before.Contains(now[i]))
                    {
                        bool taken = false;
                        lock (sessions)
                        {
                            foreach (KeyValuePair<Account, Session> kv in sessions)
                            {
                                if (kv.Value.Pid == now[i])
                                {
                                    taken = true;
                                    break;
                                }
                            }
                        }
                        if (!taken)
                        {
                            fresh.Add(now[i]);
                        }
                    }
                }
                if (fresh.Count == 0)
                {
                    return;
                }
                int best = fresh[0];
                DateTime bestT = DateTime.MaxValue;
                for (int i = 0; i < fresh.Count; i++)
                {
                    try
                    {
                        System.Diagnostics.Process p = System.Diagnostics.Process.GetProcessById(fresh[i]);
                        DateTime st = DateTime.MaxValue;
                        try { st = p.StartTime; } catch { }
                        try { p.Dispose(); } catch { }
                        if (st < bestT)
                        {
                            bestT = st;
                            best = fresh[i];
                        }
                    }
                    catch { }
                }
                lock (sessions)
                {
                    Session ss;
                    if (sessions.TryGetValue(a, out ss) && !PidAlive(ss.Pid))
                    {
                        ss.Pid = best;
                    }
                }
            }
            catch { }
        }

        private void AdoptOrphans()
        {
            try
            {
                System.Collections.Generic.List<int> alive = LivePids();
                lock (sessions)
                {
                    foreach (KeyValuePair<Account, Session> kv in sessions)
                    {
                        if (PidAlive(kv.Value.Pid))
                        {
                            alive.Remove(kv.Value.Pid);
                        }
                    }
                    if (alive.Count == 0)
                    {
                        return;
                    }
                    // chi session chua tung co pid (vua launch) moi duoc nhan client lac:
                    // session tung co pid roi chet thi chi duoc relaunch, khong nhan bua.
                    List<Account> waiting = new List<Account>();
                    foreach (KeyValuePair<Account, Session> kv in sessions)
                    {
                        if (kv.Value.Pid == 0)
                        {
                            waiting.Add(kv.Key);
                        }
                    }
                    waiting.Sort(delegate(Account x, Account y)
                    {
                        return sessions[x].BornAt.CompareTo(sessions[y].BornAt);
                    });
                    int n = alive.Count < waiting.Count ? alive.Count : waiting.Count;
                    for (int i = 0; i < n; i++)
                    {
                        sessions[waiting[i]].Pid = alive[i];
                        Log("Nhan client " + alive[i].ToString() + " cho " + waiting[i].Alias + ".");
                    }
                }
            }
            catch { }
        }

        private async void CheckRelogin()
        {
            if (reloginBusy)
            {
                return;
            }
            List<Account> dead = new List<Account>();
            int maxTry = settings.ReloginMax;
            if (maxTry < 1)
            {
                maxTry = 5;
            }
            lock (sessions)
            {
                foreach (KeyValuePair<Account, Session> kv in sessions)
                {
                    if (kv.Value.Retries < maxTry && !PidAlive(kv.Value.Pid))
                    {
                        dead.Add(kv.Key);
                    }
                }
            }
            // Acc chua tung gan duoc pid (client mo cham) thi cho 180s, khong tinh la rot.
            // Tran: client dang load van song nhung chua nhan pid.
            List<Account> ready = new List<Account>();
            for (int i = 0; i < dead.Count; i++)
            {
                Session ss;
                lock (sessions)
                {
                    if (!sessions.TryGetValue(dead[i], out ss))
                    {
                        continue;
                    }
                }
                if ((DateTime.Now - ss.BornAt).TotalSeconds > 180)
                {
                    ready.Add(dead[i]);
                }
            }
            if (ready.Count == 0)
            {
                return;
            }
            // Tran cung: so client dang chay khong duoc vuot so acc theo doi.
            // Neu thua client lac (chua nhan) thi nhan not mo moi.
            AdoptOrphans();
            bool stillReady = false;
            lock (sessions)
            {
                foreach (Account a in ready)
                {
                    Session ss;
                    if (sessions.TryGetValue(a, out ss) && !PidAlive(ss.Pid))
                    {
                        stillReady = true;
                        break;
                    }
                }
            }
            if (!stillReady)
            {
                return;
            }
            if (LivePids().Count >= sessions.Count)
            {
                Log("Tam dung tu vao lai: client dang chay du so acc.");
                return;
            }
            int gap = settings.ReloginGapSec;
            if (gap < 5)
            {
                gap = 15;
            }
            if ((DateTime.Now - lastRelogin).TotalSeconds < gap)
            {
                return;
            }
            reloginBusy = true;
            try
            {
                Account a = ready[0];
                Session ss;
                lock (sessions)
                {
                    if (!sessions.TryGetValue(a, out ss))
                    {
                        return;
                    }
                }
                ss.Retries++;
                ss.Pid = 0;
                ss.BornAt = DateTime.Now;
                lastRelogin = DateTime.Now;
                Log("Tu vao lai (" + ss.Retries.ToString() + "/" + maxTry.ToString() + "): " + a.Alias);
                Account na = a;
                Task.Run(new Action(delegate()
                {
                    try
                    {
                        AppSettings st = SettingsStore.Load();
                        if (st.WebhookUrl == null || st.WebhookUrl.Trim() == "" || !st.EvRelogin)
                        {
                            return;
                        }
                        Webhook.Send(st.WebhookUrl.Trim(), "",
                            "Rin - Tu vao lai",
                            Webhook.AccName(st, na) + " rot/kick, dang vao lai luc " + DateTime.Now.ToString("HH:mm"), null);
                    }
                    catch { }
                }));
                string err = await Task.Run(new Func<string>(delegate()
                {
                    if (ss.FollowId > 0)
                    {
                        return Launcher.LaunchFollow(a, ss.FollowId);
                    }
                    if (ss.ShareCode != null)
                    {
                        return Launcher.LaunchVipShare(a, ss.ShareCode);
                    }
                    if (ss.VipCode != null)
                    {
                        return Launcher.LaunchVip(a, ss.PlaceId, ss.VipCode);
                    }
                    return Launcher.LaunchAccount(a, ss.PlaceId, ss.JobId);
                }));
                if (err != null)
                {
                    Log("Vao lai loi: " + err);
                }
            }
            finally
            {
                reloginBusy = false;
            }
        }

        private int AfkMinutes()
        {
            int m = settings.AfkMinutes;
            if (m < 1)
            {
                m = 1;
            }
            if (m > 19)
            {
                m = 19;
            }
            return m;
        }

        private async void TimerAfk_Tick(object sender, EventArgs e)
        {
            if (!settings.AfkEnabled || afkBusy)
            {
                return;
            }
            if ((DateTime.Now - lastAfkSweep).TotalMinutes < AfkMinutes())
            {
                return;
            }
            afkBusy = true;
            try
            {
                // Quet toan bo cua so game, khong phu thuoc gán acc (tranh lech mapping).
                List<int> wins = LivePids();
                lastAfkSweep = DateTime.Now;
                for (int i = 0; i < wins.Count; i++)
                {
                    await Task.Run(new Action(delegate() { JumpPid(wins[i]); }));
                    if (i < wins.Count - 1)
                    {
                        await Task.Delay(5000);
                    }
                }
            }
            finally
            {
                afkBusy = false;
            }
        }

        private void JumpPid(int pid)
        {
            try
            {
                System.Diagnostics.Process p = System.Diagnostics.Process.GetProcessById(pid);
                IntPtr hwnd = IntPtr.Zero;
                try { hwnd = p.MainWindowHandle; } catch { }
                try { p.Dispose(); } catch { }
                if (hwnd == IntPtr.Zero)
                {
                    return;
                }
                JumpHwnd(hwnd);
            }
            catch { }
        }

        private void JumpHwnd(IntPtr hwnd)
        {
            try
            {
                IntPtr prev = WinApi.GetForegroundWindow();
                try { WinApi.ShowWindow(hwnd, WinApi.SW_RESTORE); } catch { }
                try
                {
                    uint cur = WinApi.GetCurrentThreadId();
                    uint tFore;
                    uint tTarget;
                    WinApi.GetWindowThreadProcessId(prev, out tFore);
                    WinApi.GetWindowThreadProcessId(hwnd, out tTarget);
                    WinApi.AttachThreadInput(cur, tFore, true);
                    WinApi.AttachThreadInput(cur, tTarget, true);
                    WinApi.BringWindowToTop(hwnd);
                    WinApi.SetForegroundWindow(hwnd);
                    WinApi.AttachThreadInput(cur, tFore, false);
                    WinApi.AttachThreadInput(cur, tTarget, false);
                }
                catch { }
                bool focused = false;
                for (int i = 0; i < 6; i++)
                {
                    System.Threading.Thread.Sleep(500);
                    try
                    {
                        if (WinApi.GetForegroundWindow() == hwnd)
                        {
                            focused = true;
                            break;
                        }
                        WinApi.SetForegroundWindow(hwnd);
                    }
                    catch { }
                }
                if (!focused)
                {
                    return;
                }
                bool wasIconic = false;
                try { wasIconic = WinApi.IsIconic(hwnd); } catch { }
                byte scan = 0x39;
                try { scan = (byte)WinApi.MapVirtualKey(WinApi.VK_SPACE, WinApi.MAPVK_VK_TO_VSC); } catch { }
                try { WinApi.keybd_event((byte)WinApi.VK_SPACE, scan, WinApi.KEYEVENTF_SCANCODE, UIntPtr.Zero); } catch { }
                System.Threading.Thread.Sleep(120);
                try { WinApi.keybd_event((byte)WinApi.VK_SPACE, scan, WinApi.KEYEVENTF_SCANCODE | WinApi.KEYEVENTF_KEYUP, UIntPtr.Zero); } catch { }
                System.Threading.Thread.Sleep(800);
                try
                {
                    if (wasIconic)
                    {
                        WinApi.ShowWindow(hwnd, WinApi.SW_MINIMIZE);
                    }
                    else if (prev != IntPtr.Zero)
                    {
                        try
                        {
                            uint cur2 = WinApi.GetCurrentThreadId();
                            uint tPrev;
                            WinApi.GetWindowThreadProcessId(prev, out tPrev);
                            WinApi.AttachThreadInput(cur2, tPrev, true);
                            WinApi.SetForegroundWindow(prev);
                            WinApi.AttachThreadInput(cur2, tPrev, false);
                        }
                        catch
                        {
                            try { WinApi.SetForegroundWindow(prev); } catch { }
                        }
                    }
                }
                catch { }
            }
            catch { }
        }

        private void ChkMulti_CheckedChanged(object sender, EventArgs e)
        {
            if (suppressChk)
            {
                return;
            }
            if (chkMulti.Checked)
            {
                string err;
                if (MultiRoblox.TryEnable(out err))
                {
                    Log("Multi-Roblox: BAT.");
                }
                else
                {
                    Log("BAT that bai: " + err);
                    if (AskAndFixMulti())
                    {
                        Log("Multi-Roblox: BAT sau fix.");
                    }
                    else
                    {
                        suppressChk = true;
                        try { chkMulti.Checked = false; } catch { }
                        suppressChk = false;
                    }
                }
            }
            else
            {
                MultiRoblox.Disable();
                Log("Multi-Roblox: TAT.");
            }
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            CheckExeSilent();
            if (MultiRoblox.IsEnabled)
            {
                return;
            }
            string err;
            if (MultiRoblox.TryEnable(out err))
            {
                Log("Multi-Roblox: BAT san.");
                return;
            }
            Log("Chua bat duoc Multi: " + err);
            AskAndFixMulti();
            if (!MultiRoblox.IsEnabled)
            {
                suppressChk = true;
                try { chkMulti.Checked = false; } catch { }
                suppressChk = false;
            }
        }

        private bool AskAndFixMulti()
        {
            DialogResult dr = MessageBox.Show(
                "Roblox (tien trinh tray) dang giu singleton Event nen chua bat duoc Multi.\n\n" +
                "YES: tu FIX (khong tat game).\n" +
                "NO: TAT HET Roblox roi bat Multi.\n" +
                "CANCEL: dung che do 1 client.",
                "Rin - Bat Multi-Roblox",
                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
            {
                try
                {
                    int n = HandleCloser.CloseSingletonHandles();
                    Log("Fix: dong " + n.ToString() + " handle.");
                }
                catch (Exception ex)
                {
                    Log("Fix loi: " + ex.Message);
                }
                string err2;
                if (MultiRoblox.TryEnable(out err2))
                {
                    return true;
                }
                Log("Van chua bat duoc: " + err2);
                return false;
            }
            else if (dr == DialogResult.No)
            {
                try
                {
                    Launcher.KillAll();
                    System.Threading.Thread.Sleep(800);
                }
                catch { }
                string err2;
                if (MultiRoblox.TryEnable(out err2))
                {
                    return true;
                }
                Log("Van chua bat duoc: " + err2);
                return false;
            }
            return false;
        }

        private async void BtnAdd_Click(object sender, EventArgs e)
        {
            btnAdd.Enabled = false;
            try
            {
                bool www = await Task.Run(new Func<bool>(delegate() { return RobloxApi.IsWwwReachable(); }));
                if (!www)
                {
                    DialogResult dr = MessageBox.Show(
                        "Khong mo duoc www.roblox.com (mang chan?).\nBat VPN ghim 1 server roi thu lai.\n\nVan mo form dang nhap chu?",
                        "Rin - Mang bi chan", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (dr != DialogResult.Yes)
                    {
                        OpenQuickLogin();
                        return;
                    }
                }
            }
            catch { }
            finally
            {
                btnAdd.Enabled = true;
            }
            OpenBrowserLogin();
        }

        private void OpenBrowserLogin()
        {
            BrowserLoginForm b = new BrowserLoginForm();
            DialogResult dr = DialogResult.Cancel;
            try
            {
                dr = b.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Khong mo duoc form dang nhap: " + ex.Message, "Rin",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Account acc = b.Result;
            bool other = b.OtherMethod;
            try { b.Dispose(); } catch { }
            if (dr == DialogResult.OK && acc != null)
            {
                AddAccountIfNew(acc);
            }
            else if (dr == DialogResult.Abort && other)
            {
                OpenQuickLogin();
            }
        }

        private void OpenQuickLogin()
        {
            // Duong de dang: Quick Login (quet ma/nhap ma). Duong cu: paste cookie.
            QuickLoginForm q = new QuickLoginForm();
            DialogResult qdr = q.ShowDialog(this);
            Account qacc = q.Result;
            bool manual = q.ManualCookie;
            try { q.Dispose(); } catch { }
            if (qdr == DialogResult.OK && qacc != null)
            {
                AddAccountIfNew(qacc);
                return;
            }
            if (qdr != DialogResult.Abort && !manual)
            {
                return;
            }
            OpenCookieForm();
        }

        private void OpenCookieForm()
        {
            AddAccountForm f = new AddAccountForm();
            if (f.ShowDialog(this) == DialogResult.OK && f.Result != null)
            {
                AddAccountIfNew(f.Result);
            }
            try { f.Dispose(); } catch { }
        }

        private void BtnGame_Click(object sender, EventArgs e)
        {
            string ck = null;
            for (int i = 0; i < accounts.Count; i++)
            {
                if (accounts[i].Live == "live" && !string.IsNullOrEmpty(accounts[i].Cookie))
                {
                    ck = accounts[i].Cookie;
                    break;
                }
            }
            if (ck == null && accounts.Count > 0)
            {
                ck = accounts[0].Cookie;
            }
            GamesForm g = new GamesForm(txtPlaceId.Text, txtJobId.Text, ck);
            if (g.ShowDialog(this) == DialogResult.OK && g.HasSelection && g.SelectedPlaceId > 0)
            {
                txtPlaceId.Text = g.SelectedPlaceId.ToString();
                txtJobId.Text = g.SelectedJob == null ? "" : g.SelectedJob;
                Log("Da chon game " + g.SelectedPlaceId.ToString() + (g.SelectedJob != "" ? " (VIP)" : ""));
            }
            try { g.Dispose(); } catch { }
        }

        private void BtnGear_Click(object sender, EventArgs e)
        {
            SettingsForm f = new SettingsForm();
            try { f.ShowDialog(this); } catch { }
            try { f.Dispose(); } catch { }
            settings = SettingsStore.Load();
            Log("Da tai lai cai dat.");
        }

        private int GetSessionPid(Account a)
        {
            lock (sessions)
            {
                Session ss;
                if (sessions.TryGetValue(a, out ss))
                {
                    return ss.Pid;
                }
            }
            return 0;
        }

        private byte[] CaptureGame(int pid)
        {
            try
            {
                System.Diagnostics.Process p = System.Diagnostics.Process.GetProcessById(pid);
                IntPtr hwnd = IntPtr.Zero;
                try { hwnd = p.MainWindowHandle; } catch { }
                try { p.Dispose(); } catch { }
                if (hwnd == IntPtr.Zero)
                {
                    return null;
                }
                WinApi.RECT r;
                if (!WinApi.GetWindowRect(hwnd, out r))
                {
                    return null;
                }
                int w = r.Right - r.Left;
                int h = r.Bottom - r.Top;
                if (w < 100 || h < 100 || w > 5000 || h > 5000)
                {
                    return null;
                }
                IntPtr prev = WinApi.GetForegroundWindow();
                try { WinApi.ShowWindow(hwnd, WinApi.SW_RESTORE); } catch { }
                try { WinApi.SetForegroundWindow(hwnd); } catch { }
                System.Threading.Thread.Sleep(1200);
                Bitmap bmp = new Bitmap(w, h);
                try
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.CopyFromScreen(r.Left, r.Top, 0, 0, new Size(w, h));
                    }
                }
                catch
                {
                    try { bmp.Dispose(); } catch { }
                    return null;
                }
                finally
                {
                    try
                    {
                        if (prev != IntPtr.Zero)
                        {
                            WinApi.SetForegroundWindow(prev);
                        }
                    }
                    catch { }
                }
                using (MemoryStream ms = new MemoryStream())
                {
                    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    try { bmp.Dispose(); } catch { }
                    return ms.ToArray();
                }
            }
            catch
            {
                return null;
            }
        }

        private void NotifyJoin(Account a, long placeId)
        {
            Account acc = a;
            long pid = placeId;
            Task.Run(new Action(delegate()
            {
                try
                {
                    AppSettings st = SettingsStore.Load();
                    if (st.WebhookUrl == null || st.WebhookUrl.Trim() == "")
                    {
                        return;
                    }
                    string gameName = "Game " + pid.ToString();
                    try
                    {
                        string nm;
                        string th;
                        string er;
                        if (GamesStore.FetchInfo(pid, out nm, out th, out er) && !string.IsNullOrEmpty(nm))
                        {
                            gameName = nm;
                        }
                    }
                    catch { }
                    byte[] shot = null;
                    if (st.ShotEnabled)
                    {
                        int waited = 0;
                        int maxWait = st.ShotDelaySec * 1000;
                        int proc = 0;
                        while (waited < maxWait)
                        {
                            proc = GetSessionPid(acc);
                            if (proc > 0 && PidAlive(proc))
                            {
                                break;
                            }
                            System.Threading.Thread.Sleep(2000);
                            waited += 2000;
                        }
                        proc = GetSessionPid(acc);
                        if (proc > 0 && PidAlive(proc))
                        {
                            shot = CaptureGame(proc);
                        }
                    }
                    if (!st.EvJoin)
                    {
                        return;
                    }
                    Webhook.Send(st.WebhookUrl.Trim(), "",
                        "Rin - Vao game",
                        Webhook.AccName(st, acc) + " da vao " + gameName + " (" + pid.ToString() + ") luc " + DateTime.Now.ToString("HH:mm"),
                        shot);
                }
                catch { }
            }));
        }

        private void NotifyFollow(Account a, string followName)
        {
            Account acc = a;
            string fn = followName;
            Task.Run(new Action(delegate()
            {
                try
                {
                    AppSettings st = SettingsStore.Load();
                    if (st.WebhookUrl == null || st.WebhookUrl.Trim() == "" || !st.EvJoin)
                    {
                        return;
                    }
                    Webhook.Send(st.WebhookUrl.Trim(), "",
                        "Rin - Theo user",
                        Webhook.AccName(st, acc) + " dang theo @" + fn + " luc " + DateTime.Now.ToString("HH:mm"), null);
                }
                catch { }
            }));
        }

        private void NotifyFail(Account a, string reason)
        {
            Account acc = a;
            string rs = reason;
            if (rs != null && rs.Length > 200)
            {
                rs = rs.Substring(0, 200) + "...";
            }
            Task.Run(new Action(delegate()
            {
                try
                {
                    AppSettings st = SettingsStore.Load();
                    if (st.WebhookUrl == null || st.WebhookUrl.Trim() == "" || !st.EvFail)
                    {
                        return;
                    }
                    Webhook.Send(st.WebhookUrl.Trim(), "",
                        "Rin - Loi",
                        Webhook.AccName(st, acc) + " loi: " + rs, null);
                }
                catch { }
            }));
        }

        private void BtnDel_Click(object sender, EventArgs e)
        {
            List<Account> toRemove = new List<Account>();
            foreach (ListViewItem it in lv.Items)
            {
                if (it.Selected)
                {
                    Account a = it.Tag as Account;
                    if (a != null)
                    {
                        toRemove.Add(a);
                    }
                }
            }
            if (toRemove.Count == 0)
            {
                MessageBox.Show("Chon acc can xoa (click vao dong).", "Rin", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            for (int i = 0; i < toRemove.Count; i++)
            {
                accounts.Remove(toRemove[i]);
            }
            SaveAccounts();
            RefreshList();
            Log("Da xoa " + toRemove.Count.ToString() + " acc.");
        }

        private async void BtnCheckAll_Click(object sender, EventArgs e)
        {
            btnCheckAll.Enabled = false;
            Log("Dang check " + accounts.Count.ToString() + " acc...");
            await Task.Run(new Action(DoCheckAll));
            SaveAccounts();
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(RefreshList));
                }
                else
                {
                    RefreshList();
                }
            }
            catch { }
            Log("Check xong.");
            btnCheckAll.Enabled = true;
            int nLive = 0;
            int nDie = 0;
            for (int i = 0; i < accounts.Count; i++)
            {
                if (accounts[i].Live == "live")
                {
                    nLive++;
                }
                else if (accounts[i].Live == "die")
                {
                    nDie++;
                }
            }
            MessageBox.Show("Live: " + nLive.ToString() + " - Die: " + nDie.ToString() +
                (nDie > 0 ? "\nAcc die can them lai (Quick Login nhanh nhat)." : ""),
                "Rin - Check acc", MessageBoxButtons.OK,
                nDie > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            if (nDie > 0)
            {
                List<string> deadNames = new List<string>();
                for (int i = 0; i < accounts.Count; i++)
                {
                    if (accounts[i].Live == "die")
                    {
                        deadNames.Add(Webhook.AccName(settings, accounts[i]));
                    }
                }
                Task.Run(new Action(delegate()
                {
                    try
                    {
                        AppSettings st = SettingsStore.Load();
                        if (st.WebhookUrl == null || st.WebhookUrl.Trim() == "" || !st.EvDie)
                        {
                            return;
                        }
                        Webhook.Send(st.WebhookUrl.Trim(), "",
                            "Rin - Cookie die",
                            "Can them lai: " + string.Join(", ", deadNames.ToArray()), null);
                    }
                    catch { }
                }));
            }
        }

        private void DoCheckAll()
        {
            for (int i = 0; i < accounts.Count; i++)
            {
                Account a = accounts[i];
                RobloxUser u = RobloxApi.GetAuthenticatedUser(a.Cookie);
                if (u.Ok)
                {
                    a.Username = u.Name;
                    a.UserId = u.Id;
                    a.Live = "live";
                    Log("LIVE: " + a.Alias + "=" + u.Name);
                }
                else
                {
                    a.Live = "die";
                    Log("DIE: " + a.Alias + " " + u.Error);
                }
            }
        }

        private List<Account> GetCheckedAccounts()
        {
            List<Account> list = new List<Account>();
            foreach (ListViewItem it in lv.Items)
            {
                if (it.Checked)
                {
                    Account a = it.Tag as Account;
                    if (a != null)
                    {
                        list.Add(a);
                    }
                }
            }
            return list;
        }

        private bool TryParsePlaceId(out long placeId)
        {
            placeId = 0;
            string t = txtPlaceId.Text.Trim();
            if (long.TryParse(t, out placeId) && placeId > 0)
            {
                return true;
            }
            string digits = "";
            string cur = "";
            for (int i = 0; i < t.Length; i++)
            {
                char c = t[i];
                if (c >= '0' && c <= '9')
                {
                    cur += c;
                }
                else
                {
                    if (cur.Length >= 5)
                    {
                        digits = cur;
                        break;
                    }
                    cur = "";
                }
            }
            if (digits == "" && cur.Length >= 5)
            {
                digits = cur;
            }
            if (digits != "")
            {
                if (long.TryParse(digits, out placeId) && placeId > 0)
                {
                    return true;
                }
            }
            return false;
        }

        private async void BtnLaunchSel_Click(object sender, EventArgs e)
        {
            List<Account> list = GetCheckedAccounts();
            if (list.Count == 0)
            {
                MessageBox.Show("Tick chon it nhat 1 acc.", "Rin", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            await LaunchList(list);
        }

        private async void BtnLaunchAll_Click(object sender, EventArgs e)
        {
            if (accounts.Count == 0)
            {
                return;
            }
            await LaunchList(new List<Account>(accounts));
        }

        private async Task LaunchList(List<Account> list)
        {
            // Theo user uu tien nhat (o "Theo @").
            string followName = txtFollow.Text.Trim().TrimStart('@');
            long followId = 0;
            bool isFollow = followName != "";
            if (isFollow)
            {
                Log("Tim user @" + followName + "...");
                long fid = 0;
                string ferr = null;
                bool fok = await Task.Run(new Func<bool>(delegate()
                {
                    long uid;
                    string er;
                    if (RobloxApi.ResolveUserId(followName, out uid, out er))
                    {
                        fid = uid;
                        return true;
                    }
                    ferr = er;
                    return false;
                }));
                if (!fok)
                {
                    MessageBox.Show(ferr, "Rin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                followId = fid;
                Log("Theo @" + followName + " (id " + followId.ToString() + ").");
            }
            long placeId = 0;
            if (!isFollow && !TryParsePlaceId(out placeId))
            {
                MessageBox.Show("Nhap PlaceId (hoac paste link game).", "Rin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string jobId = txtJobId.Text.Trim();
            int delaySec = 3;
            try { delaySec = Math.Max(0, int.Parse(txtDelay.Text.Trim())); } catch { delaySec = 3; }

            if (!MultiRoblox.IsEnabled && list.Count > 1)
            {
                DialogResult dr = MessageBox.Show(
                    "Multi dang TAT. Mo " + list.Count.ToString() + " acc se bi chan.\nBat Multi len?",
                    "Rin", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.Yes)
                {
                    suppressChk = true;
                    chkMulti.Checked = true;
                    suppressChk = false;
                    string errx;
                    MultiRoblox.TryEnable(out errx);
                }
            }

            string exe = Launcher.FindPlayerExe();
            if (exe == null)
            {
                MessageBox.Show("Khong tim thay RobloxPlayerBeta.exe.", "Rin", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // VIP dang moi: link /share?code=..&type=Server (uu tien truoc dang cu)
            string shareCode = null;
            bool isShare = false;
            // VIP dang cu: link chua privateServerLinkCode=
            string vipCode = null;
            bool isVip = false;
            if (!isFollow)
            {
                shareCode = RobloxApi.ExtractShareCode(txtJobId.Text);
                if (shareCode == null)
                {
                    shareCode = RobloxApi.ExtractShareCode(txtPlaceId.Text);
                }
                isShare = shareCode != null;
                if (isShare)
                {
                    Log("Che do VIP (share link).");
                }
                else
                {
                    vipCode = RobloxApi.ExtractVipCode(txtJobId.Text);
                    if (vipCode == null)
                    {
                        vipCode = RobloxApi.ExtractVipCode(txtPlaceId.Text);
                    }
                    isVip = vipCode != null;
                    if (isVip)
                    {
                        Log("Che do VIP server.");
                    }
                    else if (placeId > 0 && RobloxApi.IsBareVipCode(txtJobId.Text))
                    {
                        // Ma VIP tran o o JobId + game o o PlaceId
                        string bare = txtJobId.Text.Trim();
                        if (System.Text.RegularExpressions.Regex.IsMatch(bare, "^[0-9a-fA-F]{32}$"))
                        {
                            shareCode = bare;
                            isShare = true;
                            Log("Che do VIP (ma share).");
                        }
                        else
                        {
                            vipCode = bare;
                            isVip = true;
                            Log("Che do VIP (ma roi).");
                        }
                    }
                }
            }

            btnLaunchSel.Enabled = false;
            btnLaunchAll.Enabled = false;

            // Loc acc DIE truoc de bao ro, khoi launch vo ich.
            Log("Kiem tra acc truoc khi launch...");
            List<string> results = new List<string>();
            List<Account> liveList = new List<Account>();
            for (int i = 0; i < list.Count; i++)
            {
                Account a = list[i];
                RobloxUser vu = await Task.Run(new Func<RobloxUser>(delegate() { return RobloxApi.GetAuthenticatedUser(a.Cookie); }));
                if (vu.Ok)
                {
                    a.Username = vu.Name;
                    a.UserId = vu.Id;
                    a.Live = "live";
                    liveList.Add(a);
                }
                else
                {
                    a.Live = "die";
                    results.Add("BO QUA " + a.Alias + ": cookie die, them lai acc bang Quick Login");
                    Log("DIE (bo qua): " + a.Alias);
                }
            }
            SaveAccounts();
            RefreshList();
            if (liveList.Count == 0)
            {
                MessageBox.Show("Khong acc nao LIVE de play.\n" + string.Join("\n", results.ToArray()),
                    "Rin - Play", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                btnLaunchSel.Enabled = true;
                btnLaunchAll.Enabled = true;
                return;
            }

            Log("Launch " + liveList.Count.ToString() + " acc -> " + placeId.ToString());

            // Luu session de tu vao lai / chong-kick theo doi.
            for (int i = 0; i < liveList.Count; i++)
            {
                Account sa = liveList[i];
                Session ss;
                if (!sessions.TryGetValue(sa, out ss))
                {
                    ss = new Session();
                    sessions[sa] = ss;
                }
                ss.PlaceId = placeId;
                ss.JobId = jobId;
                ss.VipCode = vipCode;
                ss.ShareCode = shareCode;
                ss.FollowId = isFollow ? followId : 0;
                ss.Retries = 0;
                ss.LastAfk = DateTime.Now;
                ss.BornAt = DateTime.Now;
                ss.Pid = 0;
            }

            for (int i = 0; i < liveList.Count; i++)
            {
                Account a = liveList[i];
                int idx = i;
                System.Collections.Generic.List<int> beforePids = null;
                if (settings.ReloginEnabled)
                {
                    beforePids = LivePids();
                }
                string tag = isVip || isShare ? "VIP " : (isFollow ? "THEO @" + followName + " " : "");
                string err;
                if (isFollow)
                {
                    long tf = followId;
                    err = await Task.Run(new Func<string>(delegate() { return Launcher.LaunchFollow(a, tf); }));
                }
                else if (isShare)
                {
                    string sc = shareCode;
                    err = await Task.Run(new Func<string>(delegate() { return Launcher.LaunchVipShare(a, sc); }));
                }
                else if (isVip)
                {
                    string vc = vipCode;
                    err = await Task.Run(new Func<string>(delegate() { return Launcher.LaunchVip(a, placeId, vc); }));
                }
                else
                {
                    err = await Task.Run(new Func<string>(delegate() { return Launcher.LaunchAccount(a, placeId, jobId); }));
                }
                if (err == null)
                {
                    results.Add("OK " + tag + a.Alias + " (" + a.Username + ")");
                    Log("[" + (idx + 1).ToString() + "/" + liveList.Count.ToString() + "] OK " + a.Alias);
                    if (isFollow)
                    {
                        NotifyFollow(a, followName);
                    }
                    else
                    {
                        NotifyJoin(a, placeId);
                    }
                    if (settings.ReloginEnabled && beforePids != null)
                    {
                        await AttachSpawned(a, beforePids);
                    }
                }
                else
                {
                    results.Add("LOI " + tag + a.Alias + ": " + err);
                    Log("[" + (idx + 1).ToString() + "/" + liveList.Count.ToString() + "] LOI " + err);
                    NotifyFail(a, err);
                }
                if (idx < liveList.Count - 1 && delaySec > 0)
                {
                    Log("Cho " + delaySec.ToString() + "s...");
                    await Task.Delay(delaySec * 1000);
                }
            }
            Log("Launch xong.");
            btnLaunchSel.Enabled = true;
            btnLaunchAll.Enabled = true;
            // Luu game vao "gan day" (nen, khong chan UI). Follow thi khong co placeId.
            if (!isFollow)
            {
            long rp = placeId;
            Task recentTask = Task.Run(new Action(delegate()
            {
                try
                {
                    string nm;
                    string th;
                    string er;
                    if (GamesStore.FetchInfo(rp, out nm, out th, out er))
                    {
                        if (!string.IsNullOrEmpty(th))
                        {
                            GamesStore.DownloadThumb(th, rp);
                        }
                        GameData gd = GamesStore.Load();
                        GamesStore.AddRecent(gd, rp, nm);
                    }
                }
                catch { }
            }));
            }
            string summary = string.Join("\n", results.ToArray());
            if (results.Count > 25)
            {
                List<string> head = results.GetRange(0, 25);
                head.Add("... (con " + (results.Count - 25).ToString() + " dong, xem dong trang thai)");
                summary = string.Join("\n", head.ToArray());
            }
            MessageBox.Show(summary, "Rin - Ket qua Play", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnWeb_Click(object sender, EventArgs e)
        {
            Account sel = null;
            foreach (ListViewItem it in lv.Items)
            {
                if (it.Selected)
                {
                    sel = it.Tag as Account;
                    break;
                }
            }
            if (sel == null)
            {
                MessageBox.Show("Chon 1 acc trong list truoc (click vao dong).", "Rin",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            WebAccForm f = new WebAccForm(sel);
            try { f.ShowDialog(this); } catch { }
            try { f.Dispose(); } catch { }
        }

        private void BtnKill_Click(object sender, EventArgs e)
        {
            Launcher.KillAll();
            sessions.Clear();
            Log("Da kill all Roblox.");
        }

        private static System.Collections.Generic.List<IntPtr> GameWindows()
        {
            System.Collections.Generic.List<IntPtr> list = new System.Collections.Generic.List<IntPtr>();
            System.Diagnostics.Process[] ps = System.Diagnostics.Process.GetProcessesByName("RobloxPlayerBeta");
            for (int i = 0; i < ps.Length; i++)
            {
                try
                {
                    IntPtr h = IntPtr.Zero;
                    try { h = ps[i].MainWindowHandle; } catch { }
                    if (h != IntPtr.Zero)
                    {
                        list.Add(h);
                    }
                }
                catch { }
                try { ps[i].Dispose(); } catch { }
            }
            return list;
        }

        private void BtnTile_Click(object sender, EventArgs e)
        {
            System.Collections.Generic.List<IntPtr> wins = GameWindows();
            if (wins.Count == 0)
            {
                MessageBox.Show("Khong co cua so game nao.", "Rin", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            Rectangle area = Screen.PrimaryScreen.WorkingArea;
            int cols = (int)System.Math.Ceiling(System.Math.Sqrt(wins.Count));
            int rows = (int)System.Math.Ceiling(wins.Count / (double)cols);
            int w = area.Width / cols;
            int h = area.Height / rows;
            for (int i = 0; i < wins.Count; i++)
            {
                try
                {
                    int cx = area.Left + (i % cols) * w;
                    int cy = area.Top + (i / cols) * h;
                    WinApi.ShowWindow(wins[i], WinApi.SW_RESTORE_NOACTIVATE);
                    WinApi.MoveWindow(wins[i], cx, cy, w, h, true);
                }
                catch { }
            }
            Log("Da xep " + wins.Count.ToString() + " cua so (" + cols.ToString() + "x" + rows.ToString() + ").");
        }

        private void BtnMinAll_Click(object sender, EventArgs e)
        {
            System.Collections.Generic.List<IntPtr> wins = GameWindows();
            for (int i = 0; i < wins.Count; i++)
            {
                try { WinApi.ShowWindow(wins[i], WinApi.SW_MINIMIZE); } catch { }
            }
            Log("Da thu gon " + wins.Count.ToString() + " cua so.");
        }

        private void BtnShowAll_Click(object sender, EventArgs e)
        {
            System.Collections.Generic.List<IntPtr> wins = GameWindows();
            for (int i = 0; i < wins.Count; i++)
            {
                try { WinApi.ShowWindow(wins[i], WinApi.SW_RESTORE_NOACTIVATE); } catch { }
            }
            Log("Da hien " + wins.Count.ToString() + " cua so.");
        }

        private void BtnKillFrozen_Click(object sender, EventArgs e)
        {
            int n = 0;
            System.Diagnostics.Process[] ps = System.Diagnostics.Process.GetProcessesByName("RobloxPlayerBeta");
            for (int i = 0; i < ps.Length; i++)
            {
                try
                {
                    bool responding = true;
                    try { responding = ps[i].Responding; } catch { responding = true; }
                    if (!responding)
                    {
                        try { ps[i].Kill(); n++; } catch { }
                    }
                }
                catch { }
                try { ps[i].Dispose(); } catch { }
            }
            Log(n > 0 ? ("Da dong " + n.ToString() + " client do.") : "Khong co client do.");
            if (n > 0)
            {
                MessageBox.Show("Da dong " + n.ToString() + " client do.", "Rin",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void BtnFix_Click(object sender, EventArgs e)
        {
            btnFix.Enabled = false;
            Log("Dang fix teleport...");
            int n = await Task.Run(new Func<int>(delegate() { return HandleCloser.CloseSingletonHandles(); }));
            Log("Fix xong: " + n.ToString() + " handle.");
            btnFix.Enabled = true;
        }
    }
}
