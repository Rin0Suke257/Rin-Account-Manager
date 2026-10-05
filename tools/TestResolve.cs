using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using RinAccountManager;
public class TestResolve {
  static string Tok(HttpWebRequest r) { return null; }
  public static void Main() {
    try {
      ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
    } catch { }
    List<Account> accs = AccountStore.Load();
    string ck = accs[0].Cookie;
    string code = "af50b5106393c14ab0dcfddf12b9b33e";
    string[] bodies = new string[] {
      "{\"linkId\":\"" + code + "\",\"linkType\":\"Server\"}"
    };
    // lay csrf co cookie
    string csrf = null;
    try {
      HttpWebRequest q = (HttpWebRequest)WebRequest.Create("https://apis.roblox.com/sharelinks/v1/resolve-link");
      q.Method = "POST"; q.ContentType = "application/json";
      q.Headers["Cookie"] = ".ROBLOSECURITY=" + ck;
      q.ContentLength = 2;
      using (Stream s = q.GetRequestStream()) { byte[] b = Encoding.UTF8.GetBytes("{}"); s.Write(b, 0, b.Length); }
      using (HttpWebResponse r = (HttpWebResponse)q.GetResponse()) {}
    } catch (WebException wex) {
      HttpWebResponse r = wex.Response as HttpWebResponse;
      if (r != null) { csrf = r.Headers["x-csrf-token"]; try { r.Close(); } catch { } }
    }
    Console.WriteLine("csrf=" + (csrf == null ? "null" : "ok"));
    // them Referer nhu browser that
    for (int i = 0; i < bodies.Length; i++) {
      try {
        HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://apis.roblox.com/sharelinks/v1/resolve-link");
        req.Method = "POST"; req.ContentType = "application/json";
        req.Accept = "application/json, text/plain, */*";
        req.Referer = "https://www.roblox.com/";
        req.Headers["Cookie"] = ".ROBLOSECURITY=" + ck;
        if (csrf != null) req.Headers["X-CSRF-TOKEN"] = csrf;
        byte[] bb = Encoding.UTF8.GetBytes(bodies[i]);
        req.ContentLength = bb.Length;
        using (Stream s = req.GetRequestStream()) { s.Write(bb, 0, bb.Length); }
        using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse()) {
          using (StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8)) {
            Console.WriteLine("[" + i + "] " + sr.ReadToEnd());
          }
        }
      } catch (WebException wex) {
        HttpWebResponse r = wex.Response as HttpWebResponse;
        string body = "";
        if (r != null) {
          try { using (StreamReader sr = new StreamReader(r.GetResponseStream())) { body = sr.ReadToEnd(); } } catch { }
          try { r.Close(); } catch { }
        }
        Console.WriteLine("[" + i + "] ERR " + body);
      }
    }
  }
}
