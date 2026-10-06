using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using RinAccountManager;
public class TestRefresh {
  public static void Main() {
    try {
      ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
    } catch { }
    List<Account> accs = AccountStore.Load();
    Account a = null;
    for (int i = 0; i < accs.Count; i++) {
      RobloxUser u = RobloxApi.GetAuthenticatedUser(accs[i].Cookie);
      if (u.Ok) { a = accs[i]; break; }
    }
    if (a == null) { Console.WriteLine("NO_LIVE_ACC"); return; }
    Console.WriteLine("live=" + a.Alias);
    string csrf, e1;
    if (!RobloxApi.TryGetCsrfToken(a.Cookie, out csrf, out e1)) {
      Console.WriteLine("csrf fail " + e1);
      return;
    }
    try {
      HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://www.roblox.com/authentication/signoutfromallsessionsandreauthenticate");
      req.Method = "POST";
      req.ContentType = "application/x-www-form-urlencoded";
      req.UserAgent = "Mozilla/5.0";
      req.Referer = "https://www.roblox.com/";
      req.Headers["X-CSRF-TOKEN"] = csrf;
      req.Headers["Cookie"] = ".ROBLOSECURITY=" + a.Cookie;
      req.ContentLength = 0;
      using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse()) {
        Console.WriteLine("status=" + (int)resp.StatusCode);
        string[] sc = null;
        try { sc = resp.Headers.GetValues("Set-Cookie"); } catch { }
        bool found = false;
        if (sc != null) {
          for (int i = 0; i < sc.Length; i++) {
            if (sc[i] != null && sc[i].StartsWith(".ROBLOSECURITY=")) {
              string v = sc[i].Substring(17);
              int s = v.IndexOf(';');
              if (s >= 0) v = v.Substring(0, s);
              Console.WriteLine("NEWCOOKIE len=" + v.Length + " changed=" + (v != a.Cookie));
              found = true;
            }
          }
        }
        if (!found) Console.WriteLine("NO_NEW_COOKIE");
      }
    } catch (WebException wex) {
      HttpWebResponse r = wex.Response as HttpWebResponse;
      Console.WriteLine("ERR " + (r == null ? wex.Message : ((int)r.StatusCode).ToString()));
      if (r != null) { try { r.Close(); } catch { } }
    }
  }
}
