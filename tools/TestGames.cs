using System;
using RinAccountManager;
public class TestGames {
  public static void Main() {
    long[] ids = new long[] { 4924922222L, 8737899170L, 12345L };
    for (int i = 0; i < ids.Length; i++) {
      string n, th, err;
      bool ok = GamesStore.FetchInfo(ids[i], out n, out th, out err);
      Console.WriteLine(ids[i] + " ok=" + ok + " name=" + n + " thumb=" + (th == null ? "null" : th.Substring(0, 60) + "...") + " err=" + err);
      if (ok && th != null) {
        Console.WriteLine("  dl=" + GamesStore.DownloadThumb(th, ids[i]) + " cached=" + System.IO.File.Exists(GamesStore.ThumbPath(ids[i])));
      }
    }
    // VIP sai -> phai loi dep
    string acc, ae;
    bool vok = RobloxApi.TryGetPrivateAccessCode("dummy", 4924922222L, "nope", out acc, out ae);
    Console.WriteLine("vip_dummy ok=" + vok + " err=" + ae);
  }
}
