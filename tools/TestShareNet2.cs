using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using RinAccountManager;
public class TestShareNet2 {
  static List<string> hits = new List<string>();
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
        File.WriteAllLines("C:\\Users\\PC\\AppData\\Local\\Temp\\opencode\\share_posts.txt", hits.ToArray());
      } catch { }
      Environment.Exit(0);
    });
    t.Start();
    Application.Run(f);
  }
  static async void Init(WebView2 web, string ck) {
    try {
      string dir = Path.Combine(Path.GetTempPath(), "RinShareNet2_" + Guid.NewGuid().ToString("N"));
      CoreWebView2Environment env = await CoreWebView2Environment.CreateAsync(null, dir);
      await web.EnsureCoreWebView2Async(env);
      if (ck != "") {
        try {
          CoreWebView2Cookie c = web.CoreWebView2.CookieManager.CreateCookie(".ROBLOSECURITY", RobloxApi.ExtractCookie(ck), ".roblox.com", "/");
          web.CoreWebView2.CookieManager.AddOrUpdateCookie(c);
        } catch { }
      }
      await web.CoreWebView2.CallDevToolsProtocolMethodAsync("Network.enable", "{}");
      CoreWebView2DevToolsProtocolEventReceiver recv = web.CoreWebView2.GetDevToolsProtocolEventReceiver("Network.requestWillBeSent");
      recv.DevToolsProtocolEventReceived += new EventHandler<CoreWebView2DevToolsProtocolEventReceivedEventArgs>(delegate(object s, CoreWebView2DevToolsProtocolEventReceivedEventArgs e) {
        try {
          string json = e.ParameterObjectAsJson;
          if (json.Contains("sharelinks") || json.Contains("resolve")) {
            lock (hits) {
              if (hits.Count < 20) { hits.Add(json); }
            }
          }
        } catch { }
      });
      web.CoreWebView2.Navigate("https://www.roblox.com/share-links?code=af50b5106393c14ab0dcfddf12b9b33e&type=Server");
    } catch { }
  }
}
