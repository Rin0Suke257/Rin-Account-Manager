using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RinAccountManager
{
    public class GamesForm : Form
    {
        private GameData data;
        private FlowLayoutPanel flowSaved;
        private FlowLayoutPanel flowRecent;
        private Label lblMsg;

        public long SelectedPlaceId { get; private set; }
        public string SelectedJob { get; private set; }
        public bool HasSelection { get; private set; }

        private string curPlaceText;
        private string curJobText;

        public GamesForm(string placeText, string jobText)
        {
            curPlaceText = placeText;
            curJobText = jobText;
            data = GamesStore.Load();

            this.Text = "Game - Rin";
            this.Size = new Size(520, 560);
            this.MinimumSize = new Size(520, 560);
            this.MaximumSize = new Size(520, 560);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = RinTheme.Bg;

            UiHelper.SetAppIcon(this);

            Image mascotImg = null;
            try { mascotImg = UiHelper.LoadMascot(64); } catch { }

            TitleBar bar = new TitleBar(this, mascotImg, "Game da luu", 520, false, false);
            this.Controls.Add(bar);

            RoundedButton btnSave = new RoundedButton();
            btnSave.Text = "+ Luu game dang nhap";
            btnSave.Location = new Point(12, 40);
            btnSave.Size = new Size(200, 32);
            btnSave.CornerRadius = 11;
            btnSave.FillColor = RinTheme.Primary;
            btnSave.ForeColor = Color.White;
            btnSave.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnSave.Click += new EventHandler(BtnSave_Click);
            this.Controls.Add(btnSave);

            lblMsg = new Label();
            lblMsg.Location = new Point(218, 40);
            lblMsg.Size = new Size(290, 32);
            lblMsg.Font = new Font("Segoe UI", 8.5f);
            lblMsg.ForeColor = RinTheme.Muted;
            lblMsg.Text = "Bam game de dien vao o Play.";
            this.Controls.Add(lblMsg);

            Label l1 = HeadLabel("Da luu", 78);
            this.Controls.Add(l1);
            flowSaved = FlowBox(100, 150);
            this.Controls.Add(flowSaved);

            Label l2 = HeadLabel("Gan day", 256);
            this.Controls.Add(l2);
            flowRecent = FlowBox(278, 150);
            this.Controls.Add(flowRecent);

            RoundedButton btnClear = new RoundedButton();
            btnClear.Text = "Xoa gan day";
            btnClear.Location = new Point(12, 434);
            btnClear.Size = new Size(130, 30);
            btnClear.CornerRadius = 10;
            btnClear.FillColor = RinTheme.PrimarySoft;
            btnClear.HoverColor = RinTheme.Border;
            btnClear.ForeColor = RinTheme.PrimaryDark;
            btnClear.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            btnClear.Click += new EventHandler(BtnClear_Click);
            this.Controls.Add(btnClear);

            RoundedButton btnClose = new RoundedButton();
            btnClose.Text = "Dong";
            btnClose.Location = new Point(378, 434);
            btnClose.Size = new Size(130, 30);
            btnClose.CornerRadius = 10;
            btnClose.FillColor = Color.White;
            btnClose.HoverColor = RinTheme.PrimarySoft;
            btnClose.ForeColor = RinTheme.PrimaryDark;
            btnClose.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            btnClose.Click += delegate(object s, EventArgs e) { this.Close(); };
            this.Controls.Add(btnClose);

            Label credit = new Label();
            credit.Text = "by Rin0Suke257";
            credit.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
            credit.ForeColor = RinTheme.Primary;
            credit.BackColor = Color.Transparent;
            credit.Location = new Point(340, 470);
            credit.Size = new Size(168, 16);
            credit.TextAlign = ContentAlignment.MiddleRight;
            this.Controls.Add(credit);

            this.Load += new EventHandler(GamesForm_Load);
        }

        private Label HeadLabel(string text, int y)
        {
            Label l = new Label();
            l.Text = text;
            l.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            l.ForeColor = RinTheme.Text;
            l.Location = new Point(14, y);
            l.AutoSize = true;
            return l;
        }

        private FlowLayoutPanel FlowBox(int y, int h)
        {
            FlowLayoutPanel f = new FlowLayoutPanel();
            f.Location = new Point(12, y);
            f.Size = new Size(496, h);
            f.BackColor = Color.White;
            f.AutoScroll = true;
            f.WrapContents = true;
            return f;
        }

        private void GamesForm_Load(object sender, EventArgs e)
        {
            UiHelper.ApplyRound(this, 16);
            RefreshLists();
        }

        private void RefreshLists()
        {
            flowSaved.Controls.Clear();
            for (int i = 0; i < data.Saved.Count; i++)
            {
                SavedGame g = data.Saved[i];
                flowSaved.Controls.Add(MakeCard(g.Name, g.PlaceId, g.IsVip, true, i));
            }
            if (data.Saved.Count == 0)
            {
                flowSaved.Controls.Add(EmptyLabel("Chua luu game nao."));
            }
            flowRecent.Controls.Clear();
            for (int i = 0; i < data.Recent.Count; i++)
            {
                RecentGame g = data.Recent[i];
                flowRecent.Controls.Add(MakeCard(g.Name, g.PlaceId, false, false, i));
            }
            if (data.Recent.Count == 0)
            {
                flowRecent.Controls.Add(EmptyLabel("Chua play game nao."));
            }
            Task.Run(new Action(LoadThumbs));
        }

        private Label EmptyLabel(string text)
        {
            Label l = new Label();
            l.Text = text;
            l.Font = new Font("Segoe UI", 9f, FontStyle.Italic);
            l.ForeColor = RinTheme.Muted;
            l.AutoSize = true;
            l.Padding = new Padding(8);
            return l;
        }

        private Panel MakeCard(string name, long placeId, bool isVip, bool canDelete, int index)
        {
            Panel p = new Panel();
            p.Size = new Size(150, 104);
            p.BackColor = RinTheme.PrimarySoft;
            p.Cursor = Cursors.Hand;
            p.Margin = new Padding(5);

            PictureBox pic = new PictureBox();
            pic.Location = new Point(5, 5);
            pic.Size = new Size(52, 52);
            pic.SizeMode = PictureBoxSizeMode.Zoom;
            pic.BackColor = Color.White;
            pic.Tag = placeId;
            p.Controls.Add(pic);

            Label n = new Label();
            n.Text = name;
            n.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
            n.ForeColor = RinTheme.Text;
            n.Location = new Point(62, 5);
            n.Size = new Size(83, 52);
            p.Controls.Add(n);

            Label id = new Label();
            id.Text = (isVip ? "[VIP] " : "") + placeId.ToString();
            id.Font = new Font("Segoe UI", 7.5f);
            id.ForeColor = isVip ? RinTheme.PrimaryDark : RinTheme.Muted;
            id.Location = new Point(5, 60);
            id.Size = new Size(100, 16);
            p.Controls.Add(id);

            RoundedButton pick = new RoundedButton();
            pick.Text = "Chon";
            pick.Location = new Point(5, 78);
            pick.Size = new Size(70, 22);
            pick.CornerRadius = 8;
            pick.FillColor = RinTheme.Primary;
            pick.ForeColor = Color.White;
            pick.Font = new Font("Segoe UI", 7.5f, FontStyle.Bold);
            pick.Tag = placeId;
            pick.Click += new EventHandler(Pick_Click);
            p.Controls.Add(pick);

            if (canDelete)
            {
                RoundedButton del = new RoundedButton();
                del.Text = "Xoa";
                del.Location = new Point(80, 78);
                del.Size = new Size(60, 22);
                del.CornerRadius = 8;
                del.FillColor = Color.White;
                del.HoverColor = RinTheme.PrimarySoft;
                del.ForeColor = RinTheme.PrimaryDark;
                del.Font = new Font("Segoe UI", 7.5f, FontStyle.Bold);
                del.Tag = index;
                del.Click += new EventHandler(Del_Click);
                p.Controls.Add(del);
            }
            return p;
        }

        private void LoadThumbs()
        {
            try
            {
                this.Invoke(new Action(delegate()
                {
                    FillThumbs(flowSaved);
                    FillThumbs(flowRecent);
                }));
            }
            catch { }
        }

        private void FillThumbs(FlowLayoutPanel flow)
        {
            foreach (Control c in flow.Controls)
            {
                Panel p = c as Panel;
                if (p == null)
                {
                    continue;
                }
                foreach (Control inner in p.Controls)
                {
                    PictureBox pic = inner as PictureBox;
                    if (pic != null && pic.Tag is long)
                    {
                        long pid = (long)pic.Tag;
                        Image img = GamesStore.LoadThumb(pid);
                        if (img != null)
                        {
                            try
                            {
                                if (pic.Image != null)
                                {
                                    try { pic.Image.Dispose(); } catch { }
                                }
                                pic.Image = img;
                            }
                            catch { }
                        }
                    }
                }
            }
        }

        private void Pick_Click(object sender, EventArgs e)
        {
            RoundedButton b = sender as RoundedButton;
            if (b == null || !(b.Tag is long))
            {
                return;
            }
            long pid = (long)b.Tag;
            SelectedPlaceId = pid;
            SelectedJob = "";
            for (int i = 0; i < data.Saved.Count; i++)
            {
                if (data.Saved[i].PlaceId == pid && data.Saved[i].IsVip && !string.IsNullOrEmpty(data.Saved[i].VipLink))
                {
                    SelectedJob = data.Saved[i].VipLink;
                }
            }
            HasSelection = true;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void Del_Click(object sender, EventArgs e)
        {
            RoundedButton b = sender as RoundedButton;
            if (b == null || !(b.Tag is int))
            {
                return;
            }
            int idx = (int)b.Tag;
            if (idx < 0 || idx >= data.Saved.Count)
            {
                return;
            }
            data.Saved.RemoveAt(idx);
            GamesStore.Save(data);
            RefreshLists();
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            data.Recent.Clear();
            GamesStore.Save(data);
            RefreshLists();
        }

        private static bool TryFirstNumber(string t, out long num)
        {
            num = 0;
            if (string.IsNullOrEmpty(t))
            {
                return false;
            }
            string cur = "";
            for (int i = 0; i < t.Length; i++)
            {
                char c = t[i];
                if (c >= '0' && c <= '9')
                {
                    cur += c;
                }
                else if (cur.Length >= 5)
                {
                    break;
                }
                else
                {
                    cur = "";
                }
            }
            if (cur.Length >= 5 && long.TryParse(cur, out num) && num > 0)
            {
                return true;
            }
            return false;
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            long pid;
            if (!TryFirstNumber(curPlaceText, out pid) && !TryFirstNumber(curJobText, out pid))
            {
                MessageBox.Show("O PlaceId dang trong. Nhap ID/link game truoc.", "Rin",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string vip = RobloxApi.ExtractVipCode(curJobText);
            if (vip == null)
            {
                vip = RobloxApi.ExtractVipCode(curPlaceText);
            }
            lblMsg.Text = "Dang tai thong tin game...";
            long p = pid;
            string v = vip;
            string name = null;
            string thumb = null;
            string err = null;
            bool ok = await Task.Run(new Func<bool>(delegate()
            {
                string n;
                string th;
                string e2;
                if (GamesStore.FetchInfo(p, out n, out th, out e2))
                {
                    name = n;
                    thumb = th;
                    if (!string.IsNullOrEmpty(th))
                    {
                        GamesStore.DownloadThumb(th, p);
                    }
                    return true;
                }
                err = e2;
                return false;
            }));
            if (!ok)
            {
                lblMsg.Text = err;
                MessageBox.Show(err, "Rin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            for (int i = data.Saved.Count - 1; i >= 0; i--)
            {
                if (data.Saved[i].PlaceId == p)
                {
                    data.Saved.RemoveAt(i);
                }
            }
            SavedGame g = new SavedGame();
            g.PlaceId = p;
            g.Name = name;
            g.ThumbUrl = thumb;
            if (v != null)
            {
                g.IsVip = true;
                string src = curJobText;
                if (RobloxApi.ExtractVipCode(src) == null)
                {
                    src = curPlaceText;
                }
                g.VipLink = src.Trim();
            }
            data.Saved.Insert(0, g);
            GamesStore.Save(data);
            curPlaceText = p.ToString();
            lblMsg.Text = "Da luu: " + name;
            RefreshLists();
        }
    }
}
