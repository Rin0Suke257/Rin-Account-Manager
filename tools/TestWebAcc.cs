using System;
using System.Collections.Generic;
using System.Windows.Forms;
using RinAccountManager;
public class TestWebAcc {
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
    List<Account> accs = AccountStore.Load();
    if (accs.Count == 0) return;
    Application.Run(new WebAccForm(accs[0]));
  }
}
