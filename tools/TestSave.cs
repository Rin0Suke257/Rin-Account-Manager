using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using RinAccountManager;
public class TestSave {
  public static void Main() {
    List<Account> accs = AccountStore.Load();
    Console.WriteLine("before0=" + accs[0].Alias + "/" + accs[0].Cookie.Length);
    accs[0].Alias = "ZZZTEST";
    accs[0].Cookie = new string('Q', accs[0].Cookie.Length + 50);
    string err;
    Console.WriteLine("save=" + AccountStore.Save(accs, out err));
    List<Account> r = AccountStore.Load();
    Console.WriteLine("after0=" + r[0].Alias + "/" + r[0].Cookie.Length);
  }
}
