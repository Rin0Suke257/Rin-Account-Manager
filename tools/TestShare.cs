using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using RinAccountManager;
public class TestShare {
  public static void Main() {
    try {
      ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
    } catch { }
    List<Account> accs = AccountStore.Load();
    string ck = accs[0].Cookie;
    string[] urls = new string[] {
      "https://www.roblox.com/share-links?code=af50b5106393c14ab0dcfddf12b9b33e&type=Server",
      "https://www.roblox.com/share?code=af50b5106393c14ab0dcfddf12b9b33e&type=Server"
    };
    for (int i = 0; i < urls.Length; i++) {
      try {
        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(urls[i]);
        req.Method = "GET";
        req.Accept = "text/html";
        req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)";
        req.Headers["Cookie"] = ".ROBLOSECURITY=" + ck;
        req.Timeout = 20000;
        using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse()) {
          using (StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8)) {
            string html = sr.ReadToEnd();
            Console.WriteLine("URL" + i + " len=" + html.Length);
            File.WriteAllText("C:\\Users\\PC\\AppData\\Local\\Temp\\opencode\\share_auth" + i + ".html", html);
            System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(html, "joinPrivateGame\\(([^)]+)\\)");
            Console.WriteLine("  joinPrivateGame: " + (m.Success ? m.Groups[1].Value : "none"));
            System.Text.RegularExpressions.Match p = System.Text.RegularExpressions.Regex.Match(html, "placeId[\"']?\\s*[:=]\\s*(\\d+)");
            Console.WriteLine("  placeId: " + (p.Success ? p.Groups[1].Value : "none"));
            System.Text.RegularExpressions.Match lc = System.Text.RegularExpressions.Regex.Match(html, "privateServerLinkCode[\"']?\\s*[:=]\\s*[\"']([^\"']+)");
            Console.WriteLine("  linkCode: " + (lc.Success ? lc.Groups[1].Value : "none"));
            System.Text.RegularExpressions.Match ac = System.Text.RegularExpressions.Regex.Match(html, "accessCode[\"']?\\s*[:=]\\s*[\"']([^\"']+)");
            Console.WriteLine("  accessCode: " + (ac.Success ? ac.Groups[1].Value : "none"));
          }
        }
      } catch (WebException wex) {
        HttpWebResponse r = wex.Response as HttpWebResponse;
        Console.WriteLine("URL" + i + " HTTP " + (r == null ? ("TRANSPORT " + wex.Message) : ((int)r.StatusCode).ToString()));
        if (r != null) { try { r.Close(); } catch { } }
      }
    }
  }
}
