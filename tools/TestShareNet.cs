using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using RinAccountManager;
public class TestShareNet {
  static List<string> seen = new List<string>();
  [STAThread]
  public static void Main() {
    try {
      System.Net.ServicePointManager.SecurityProtocol =
        System.Net.SecurityProtocolType.Tls12 |
        System.Net.SecurityProtocolType.Tls11 |
        System.Net.SecurityProtocolType.Tls;
    } catch { }
    Application.EnableVisualStyles();
    WebView2 web = new WebView2();
    Form f = new Form();
    f.Size = new System.Drawing.Size(900, 700);
    web.Dock = DockStyle.Fill;
    f.Controls.Add(web);
    List<Account> accs = AccountStore.Load();
    string ck = accs.Count > 0 ? accs[0].Cookie : "";
    f.Shown += new EventHandler(delegate(object s, EventArgs e) {
      Init(web, ck);
    });
    Timer t = new Timer();
    t.Interval = 25000;
    t.Tick += new EventHandler(delegate(object s, EventArgs e) {
      t.Stop();
      try {
        File.WriteAllLines("C:\\Users\\PC\\AppData\\Local\\Temp\\opencode\\share_urls.txt", seen.ToArray());
      } catch { }
      Environment.Exit(0);
    });
    t.Start();
    Application.Run(f);
  }
  static async void Init(WebView2 web, string ck) {
    try {
      string dir = Path.Combine(Path.GetTempPath(), "RinShareNet_" + Guid.NewGuid().ToString("N"));
      CoreWebView2Environment env = await CoreWebView2Environment.CreateAsync(null, dir);
      await web.EnsureCoreWebView2Async(env);
      if (ck != "") {
        try {
          CoreWebView2Cookie c = web.CoreWebView2.CookieManager.CreateCookie(".ROBLOSECURITY", RobloxApi.ExtractCookie(ck), ".roblox.com", "/");
          web.CoreWebView2.CookieManager.AddOrUpdateCookie(c);
        } catch { }
      }
      web.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
      web.CoreWebView2.WebResourceRequested += new EventHandler<CoreWebView2WebResourceRequestedEventArgs>(delegate(object s, CoreWebView2WebResourceRequestedEventArgs e) {
        try {
          string u = e.Request.Uri;
          lock (seen) {
            if (!seen.Contains(u) && (u.Contains("roblox.com") && (u.Contains("share") || u.Contains("link") || u.Contains("private") || u.Contains("server") || u.Contains("code") || u.Contains("api")))) {
              seen.Add(u);
            }
          }
        } catch { }
      });
      web.CoreWebView2.Navigate("https://www.roblox.com/share-links?code=af50b5106393c14ab0dcfddf12b9b33e&type=Server");
    } catch { }
  }
}
