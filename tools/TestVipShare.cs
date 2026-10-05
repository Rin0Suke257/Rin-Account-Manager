using System;
using System.Collections.Generic;
using RinAccountManager;
public class TestVipShare {
  public static void Main() {
    try {
      System.Net.ServicePointManager.SecurityProtocol =
        System.Net.SecurityProtocolType.Tls12 |
        System.Net.SecurityProtocolType.Tls11 |
        System.Net.SecurityProtocolType.Tls;
    } catch { }
    List<Account> accs = AccountStore.Load();
    Account a = accs[0];
    Console.WriteLine("acc=" + a.Alias);
    string err = Launcher.LaunchVipShare(a, "af50b5106393c14ab0dcfddf12b9b33e");
    Console.WriteLine(err == null ? "LAUNCH_SENT" : ("LAUNCH_ERR: " + err));
  }
}
