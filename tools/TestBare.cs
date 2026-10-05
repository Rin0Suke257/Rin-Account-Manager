using System;
using System.Collections.Generic;
using RinAccountManager;
public class TestBare {
  public static void Main() {
    try {
      System.Net.ServicePointManager.SecurityProtocol =
        System.Net.SecurityProtocolType.Tls12 |
        System.Net.SecurityProtocolType.Tls11 |
        System.Net.SecurityProtocolType.Tls;
    } catch { }
    Console.WriteLine("bare-numeric=" + RobloxApi.IsBareVipCode("63021856984433728387263348667938"));
    Console.WriteLine("bare-hex32=" + RobloxApi.IsBareVipCode("af50b5106393c14ab0dcfddf12b9b33e"));
    Console.WriteLine("guid-job=" + RobloxApi.IsBareVipCode("3c1a2b3c-4d5e-6f70-8a9b-0c1d2e3f4a5b"));
    Console.WriteLine("normal=" + RobloxApi.IsBareVipCode("abc"));
    List<Account> accs = AccountStore.Load();
    string csrf, e1;
    if (!RobloxApi.TryGetCsrfToken(accs[0].Cookie, out csrf, out e1)) {
      Console.WriteLine("csrf fail " + e1);
      return;
    }
    string access, e2;
    if (RobloxApi.TryGetPrivateAccessCode(accs[0].Cookie, 15532962292L, "63021856984433728387263348667938", out access, out e2)) {
      Console.WriteLine("ACCESS_OK len=" + access.Length);
    } else {
      Console.WriteLine("ACCESS_FAIL " + e2);
    }
  }
}
