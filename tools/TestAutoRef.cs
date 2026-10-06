using System;
using System.Collections.Generic;
using RinAccountManager;
public class TestAutoRef {
  public static void Main() {
    try {
      System.Net.ServicePointManager.SecurityProtocol =
        System.Net.SecurityProtocolType.Tls12 |
        System.Net.SecurityProtocolType.Tls11 |
        System.Net.SecurityProtocolType.Tls;
    } catch { }
    List<Account> accs = AccountStore.Load();
    Console.WriteLine("count=" + accs.Count);
    for (int i = 0; i < accs.Count; i++) {
      Account a = accs[i];
      RobloxUser u = RobloxApi.GetAuthenticatedUser(a.Cookie);
      Console.WriteLine(a.Alias + " live=" + u.Ok + (u.Ok ? "" : " err=" + u.Error));
      if (!u.Ok) continue;
      string nc, er;
      if (RobloxApi.TryRefreshCookie(a.Cookie, out nc, out er)) {
        Console.WriteLine("  refreshed " + a.Cookie.Length + "->" + nc.Length);
        a.Cookie = RobloxApi.ExtractCookie(nc);
        a.Live = "live";
      } else {
        Console.WriteLine("  refresh FAIL " + er);
      }
    }
    string serr;
    Console.WriteLine("save=" + AccountStore.Save(accs, out serr) + " err=" + serr);
  }
}
