using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using RinAccountManager;
public class TestCsrf {
  public static void Main() {
    List<Account> accs = AccountStore.Load();
    Account a = accs[0];
    HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://auth.roblox.com/v1/authentication-ticket/");
    req.Method = "POST";
    req.ContentType = "application/json";
    req.UserAgent = "Mozilla/5.0";
    req.Referer = "https://www.roblox.com/games/4924922222/Brookhaven-RP";
    req.Headers["Cookie"] = ".ROBLOSECURITY=" + a.Cookie;
    req.ContentLength = 0;
    req.Timeout = 15000;
    try {
      using (HttpWebResponse r = (HttpWebResponse)req.GetResponse()) {
        Console.WriteLine("UNEXPECTED " + (int)r.StatusCode);
      }
    } catch (WebException wex) {
      HttpWebResponse r = wex.Response as HttpWebResponse;
      if (r == null) { Console.WriteLine("TRANSPORT " + wex.Message); return; }
      Console.WriteLine("status=" + (int)r.StatusCode);
      Console.WriteLine("csrf=" + r.Headers["x-csrf-token"]);
      try {
        using (StreamReader sr = new StreamReader(r.GetResponseStream())) {
          string b = sr.ReadToEnd();
          Console.WriteLine("body=" + (b.Length > 300 ? b.Substring(0,300) : b));
        }
      } catch (Exception ex) { Console.WriteLine("nobody " + ex.Message); }
      r.Close();
    }
  }
}
