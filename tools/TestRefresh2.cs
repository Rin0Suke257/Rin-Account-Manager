using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using RinAccountManager;
public class TestRefresh2 {
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
    Console.WriteLine("live=" + a.Alias + " oldlen=" + a.Cookie.Length);
    string csrf, e1;
    if (!RobloxApi.TryGetCsrfToken(a.Cookie, out csrf, out e1)) {
      Console.WriteLine("csrf fail " + e1);
      return;
    }
    try {
      HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://auth.roblox.com/v2/session/refresh");
      req.Method = "POST";
      req.ContentType = "application/json";
      req.UserAgent = "Mozilla/5.0";
      req.Headers["X-CSRF-TOKEN"] = csrf;
      req.Headers["Cookie"] = ".ROBLOSECURITY=" + a.Cookie;
      req.ContentLength = 0;
      using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse()) {
        Console.WriteLine("status=" + (int)resp.StatusCode);
        string[] sc = null;
        try { sc = resp.Headers.GetValues("Set-Cookie"); } catch { }
        string nc = null;
        if (sc != null) {
          for (int i = 0; i < sc.Length; i++) {
            if (sc[i] != null && sc[i].StartsWith(".ROBLOSECURITY=")) {
              string v = sc[i].Substring(17);
              int s = v.IndexOf(';');
              if (s >= 0) v = v.Substring(0, s);
              nc = v.Trim();
            }
          }
        }
        if (nc == null) { Console.WriteLine("NO_NEW_COOKIE"); return; }
        Console.WriteLine("newlen=" + nc.Length + " changed=" + (nc != a.Cookie));
        RobloxUser u2 = RobloxApi.GetAuthenticatedUser(RobloxApi.ExtractCookie(nc));
        Console.WriteLine("newcookie_live=" + u2.Ok + " user=" + u2.Name);
        if (u2.Ok) {
          a.Cookie = RobloxApi.ExtractCookie(nc);
          string err;
          AccountStore.Save(accs, out err);
          Console.WriteLine("SAVED err=" + err);
        }
      }
    } catch (WebException wex) {
      HttpWebResponse r = wex.Response as HttpWebResponse;
      Console.WriteLine("ERR " + (r == null ? wex.Message : ((int)r.StatusCode).ToString()));
      if (r != null) { try { r.Close(); } catch { } }
    }
  }
}
