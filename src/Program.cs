using System;
using System.Windows.Forms;

namespace RinAccountManager
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            try
            {
                System.Net.ServicePointManager.SecurityProtocol =
                    System.Net.SecurityProtocolType.Tls12 |
                    System.Net.SecurityProtocolType.Tls11 |
                    System.Net.SecurityProtocolType.Tls;
            }
            catch { }
            // Profile WebView2 cu (dang nhap ton tai) khong dung nua -> xoa de khoi ket session.
            try
            {
                string oldWv2 = System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
                    "RinAccountManager", "wv2");
                if (System.IO.Directory.Exists(oldWv2))
                {
                    System.IO.Directory.Delete(oldWv2, true);
                }
            }
            catch { }
            // Don profile tam WebView2 cua cac lan truoc (neu dong app dot ngot).
            try
            {
                string tmp = System.IO.Path.GetTempPath();
                string[] dirs = System.IO.Directory.GetDirectories(tmp, "RinWv2_*");
                for (int i = 0; i < dirs.Length; i++)
                {
                    try { System.IO.Directory.Delete(dirs[i], true); } catch { }
                }
            }
            catch { }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Bat multi ngay tu dau (giu mutex), giong MultiBloxy.
            MultiRoblox.Enable();

            try
            {
                Application.Run(new MainForm());
            }
            finally
            {
                MultiRoblox.Disable();
            }
        }
    }
}
