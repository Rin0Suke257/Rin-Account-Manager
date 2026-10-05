using System;
using System.Collections.Generic;
using RinAccountManager;
public class TestAcct {
  public static void Main() {
    List<Account> accs = AccountStore.Load();
    Console.WriteLine("count=" + accs.Count);
    for (int i = 0; i < accs.Count; i++) {
      Account a = accs[i];
      string c = a.Cookie == null ? "" : a.Cookie;
      string shape = "empty";
      if (c.Length > 0) {
        if (c.IndexOf("|") >= 0) shape = "has-pipe len=" + c.Length;
        else shape = "no-pipe len=" + c.Length;
      }
      Console.WriteLine("[" + i + "] alias=" + a.Alias + " user=" + a.Username + " id=" + a.UserId + " cookie:" + shape + " tracker=" + (a.BrowserTrackerId == null ? "null" : a.BrowserTrackerId.Length + "ch"));
    }
  }
}
