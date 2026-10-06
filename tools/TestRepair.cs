using System;
using System.Collections.Generic;
using RinAccountManager;
public class TestRepair {
  public static void Main() {
    List<Account> accs = AccountStore.Load();
    for (int i = 0; i < accs.Count; i++) {
      if (accs[i].Username == "TheDummyBot" || accs[i].Alias == "ZZZTEST") {
        accs[i].Alias = "TheDummyBot";
        Console.WriteLine("renamed back, user=" + accs[i].Username);
      }
    }
    string err;
    Console.WriteLine("save=" + AccountStore.Save(accs, out err));
  }
}
