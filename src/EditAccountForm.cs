using System;
using System.Drawing;
using System.Windows.Forms;

namespace RinAccountManager
{
    // Sua ten goi nho + ghi chu. Double-click acc trong list de mo.
    public class EditAccountForm : Form
    {
        private TextBox txtAlias;
        private TextBox txtNote;

        public string AliasText { get; private set; }
        public string NoteText { get; private set; }

        public EditAccountForm(string alias, string note, string username)
        {
            this.Text = "Sua acc - Rin";
            this.Size = new Size(400, 300);
            this.MinimumSize = new Size(400, 300);
            this.MaximumSize = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = RinTheme.Bg;

            UiHelper.SetAppIcon(this);

            Image mascotImg = null;
            try { mascotImg = UiHelper.LoadMascot(64); } catch { }

            TitleBar bar = new TitleBar(this, mascotImg, "Sua acc" + (string.IsNullOrEmpty(username) ? "" : " (" + username + ")"), 400, false, false);
            this.Controls.Add(bar);

            Label l1 = new Label();
            l1.Text = "Ten goi nho:";
            l1.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            l1.ForeColor = RinTheme.Text;
            l1.Location = new Point(14, 42);
            l1.AutoSize = true;
            this.Controls.Add(l1);

            txtAlias = new TextBox();
            txtAlias.Location = new Point(14, 62);
            txtAlias.Size = new Size(372, 24);
            txtAlias.BorderStyle = BorderStyle.FixedSingle;
            txtAlias.BackColor = Color.White;
            txtAlias.ForeColor = RinTheme.Text;
            txtAlias.Font = new Font("Segoe UI", 10f);
            txtAlias.Text = alias;
            this.Controls.Add(txtAlias);

            Label l2 = new Label();
            l2.Text = "Ghi chu (vd: farm map X, het han...):";
            l2.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            l2.ForeColor = RinTheme.Text;
            l2.Location = new Point(14, 94);
            l2.AutoSize = true;
            this.Controls.Add(l2);

            txtNote = new TextBox();
            txtNote.Location = new Point(14, 114);
            txtNote.Size = new Size(372, 70);
            txtNote.Multiline = true;
            txtNote.ScrollBars = ScrollBars.Vertical;
            txtNote.BorderStyle = BorderStyle.FixedSingle;
            txtNote.BackColor = Color.White;
            txtNote.ForeColor = RinTheme.Text;
            txtNote.Font = new Font("Segoe UI", 9f);
            txtNote.Text = note;
            this.Controls.Add(txtNote);

            RoundedButton btnOk = new RoundedButton();
            btnOk.Text = "Luu";
            btnOk.Location = new Point(178, 196);
            btnOk.Size = new Size(100, 36);
            btnOk.CornerRadius = 12;
            btnOk.FillColor = RinTheme.Primary;
            btnOk.ForeColor = Color.White;
            btnOk.Click += new EventHandler(BtnOk_Click);
            this.Controls.Add(btnOk);

            RoundedButton btnCancel = new RoundedButton();
            btnCancel.Text = "Huy";
            btnCancel.Location = new Point(286, 196);
            btnCancel.Size = new Size(100, 36);
            btnCancel.CornerRadius = 12;
            btnCancel.FillColor = RinTheme.PrimarySoft;
            btnCancel.HoverColor = RinTheme.Border;
            btnCancel.ForeColor = RinTheme.PrimaryDark;
            btnCancel.Click += delegate(object s, EventArgs e) { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.Controls.Add(btnCancel);

            Label credit = new Label();
            credit.Text = "by Rin0Suke257";
            credit.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
            credit.ForeColor = RinTheme.Primary;
            credit.BackColor = Color.Transparent;
            credit.Location = new Point(228, 244);
            credit.Size = new Size(158, 16);
            credit.TextAlign = ContentAlignment.MiddleRight;
            this.Controls.Add(credit);

            this.Load += delegate(object s, EventArgs e) { UiHelper.ApplyRound(this, 16); };
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (txtAlias.Text.Trim() == "")
            {
                MessageBox.Show("Nhap ten goi nho.", "Rin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            AliasText = txtAlias.Text.Trim();
            NoteText = txtNote.Text.Trim();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
