using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
public class TestDump {
  public static void Main() {
    string p = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RinAccountManager", "accounts.dat");
    byte[] prot = File.ReadAllBytes(p);
    Console.WriteLine("prot_len=" + prot.Length);
    byte[] raw = ProtectedData.Unprotect(prot, null, DataProtectionScope.CurrentUser);
    string json = Encoding.UTF8.GetString(raw);
    Console.WriteLine("json_len=" + json.Length);
    // in do dai tung cookie (khong in secret): tim "Cookie":"..."
    int i = 0;
    while (true) {
      int k = json.IndexOf("\"Cookie\":\"", i);
      if (k < 0) break;
      int s = k + 10;
      int e = json.IndexOf("\"", s);
      Console.WriteLine("cookie_len=" + (e - s));
      i = e;
    }
  }
}
