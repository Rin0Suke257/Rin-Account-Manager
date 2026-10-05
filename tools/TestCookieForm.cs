using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using RinAccountManager;
public class TestCookieForm {
  static string Req(string url, string cookieVal) {
    try {
      HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
      req.Method = "GET";
      if (url.Contains("authentication-ticket")) req.Method = "POST";
      req.UserAgent = "Mozilla/5.0";
      req.Headers["Cookie"] = ".ROBLOSECURITY=" + cookieVal;
      req.ContentLength = 0;
      req.Timeout = 15000;
      using (HttpWebResponse r = (HttpWebResponse)req.GetResponse()) {
        using (StreamReader sr = new StreamReader(r.GetResponseStream())) { sr.ReadToEnd(); }
        return "HTTP " + (int)r.StatusCode;
      }
    } catch (WebException wex) {
      HttpWebResponse r = wex.Response as HttpWebResponse;
      if (r == null) return "TRANSPORT " + wex.Message;
      string csrf = r.Headers["x-csrf-token"];
      string s = "HTTP " + (int)r.StatusCode + " csrf=" + (csrf == null ? "no" : "YES");
      try { r.Close(); } catch { }
      return s;
    }
  }
  public static void Main() {
    try {
      System.Net.ServicePointManager.SecurityProtocol =
        System.Net.SecurityProtocolType.Tls12 |
        System.Net.SecurityProtocolType.Tls11 |
        System.Net.SecurityProtocolType.Tls;
    } catch { }
    List<Account> accs = AccountStore.Load();
    string token = accs[0].Cookie;
    string full = "_|WARNING:-DO-NOT-SHARE-THIS.--Sharing-this-will-allow-someone-to-log-in-as-you-and-to-steal-your-ROBUX-and-items.|_" + token;
    Console.WriteLine("users+stripped: " + Req("https://users.roblox.com/v1/users/authenticated", token));
    Console.WriteLine("users+full: " + Req("https://users.roblox.com/v1/users/authenticated", full));
    Console.WriteLine("auth+stripped: " + Req("https://auth.roblox.com/v1/authentication-ticket/", token));
    Console.WriteLine("auth+full: " + Req("https://auth.roblox.com/v1/authentication-ticket/", full));
  }
}
