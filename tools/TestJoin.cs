using System;
using RinAccountManager;
public class TestJoin {
  public static void Main() {
    string url = Launcher.BuildJoinUrl("TICKET123", 4924922222L, "", "111111222222", false);
    Console.WriteLine(url.Contains("roblox-player:1+launchmode:play") ? "URL_PREFIX_OK" : "URL_PREFIX_FAIL");
    Console.WriteLine(url.Contains("TICKET123") ? "TICKET_OK" : "TICKET_FAIL");
    Console.WriteLine(url.ToLower().Contains("placeid") ? "PLACE_OK" : "PLACE_FAIL");
    string raw = "_|WARNING:-DO-NOT-SHARE-THIS.--Sharing-this-will-allow-someone-to-log-in-as-you-and-to-steal-your-ROBUX-and-items.|_" + new string('A', 200);
    string c = RobloxApi.ExtractCookie(raw);
    Console.WriteLine(c == new string('A', 200) ? "COOKIE_OK" : "COOKIE_FAIL:" + c);
    string exe = Launcher.FindPlayerExe();
    Console.WriteLine(exe != null ? "EXE_OK:" + exe : "EXE_FAIL");
  }
}
