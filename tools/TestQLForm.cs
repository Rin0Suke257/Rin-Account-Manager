using System;
using System.Windows.Forms;
using RinAccountManager;
public class TestQLForm {
  [STAThread]
  public static void Main() {
    try {
      System.Net.ServicePointManager.SecurityProtocol =
        System.Net.SecurityProtocolType.Tls12 |
        System.Net.SecurityProtocolType.Tls11 |
        System.Net.SecurityProtocolType.Tls;
    } catch { }
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);
    QuickLoginForm f = new QuickLoginForm();
    Application.Run(f);
  }
}
