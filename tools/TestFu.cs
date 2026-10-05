using System;
using RinAccountManager;
public class TestFu {
  public static void Main() {
    try {
      System.Net.ServicePointManager.SecurityProtocol =
        System.Net.SecurityProtocolType.Tls12 |
        System.Net.SecurityProtocolType.Tls11 |
        System.Net.SecurityProtocolType.Tls;
    } catch { }
    long id;
    string err;
    if (RobloxApi.ResolveUserId("Roblox", out id, out err)) {
      Console.WriteLine("OK id=" + id);
    } else {
      Console.WriteLine("FAIL " + err);
    }
    if (RobloxApi.ResolveUserId("ten_khong_ton_tai_xyz123", out id, out err)) {
      Console.WriteLine("UNEXPECTED");
    } else {
      Console.WriteLine("NOTFOUND_OK " + err);
    }
  }
}
