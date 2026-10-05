using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;

namespace RinAccountManager
{
    public static class Launcher
    {
        public static string FindPlayerExe()
        {
            try
            {
                string baseDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Roblox", "Versions");
                if (!Directory.Exists(baseDir))
                {
                    return null;
                }
                string[] dirs = Directory.GetDirectories(baseDir, "version-*");
                // chon ban co RobloxPlayerBeta.exe moi nhat
                string best = null;
                DateTime bestTime = DateTime.MinValue;
                for (int i = 0; i < dirs.Length; i++)
                {
                    string exe = Path.Combine(dirs[i], "RobloxPlayerBeta.exe");
                    if (File.Exists(exe))
                    {
                        DateTime t = File.GetLastWriteTime(exe);
                        if (t > bestTime)
                        {
                            bestTime = t;
                            best = exe;
                        }
                    }
                }
                return best;
            }
            catch
            {
                return null;
            }
        }

        public static string BuildJoinUrl(string ticket, long placeId, string jobId, string browserTrackerId, bool isTeleport)
        {
            long launchTime = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
            string placeLauncher;
            if (string.IsNullOrEmpty(jobId))
            {
                placeLauncher = "https://assetgame.roblox.com/game/PlaceLauncher.ashx?request=RequestGame&browserTrackerId=" + browserTrackerId
                    + "&placeId=" + placeId.ToString()
                    + "&isPlayTogetherGame=false" + (isTeleport ? "&isTeleport=true" : "");
            }
            else
            {
                placeLauncher = "https://assetgame.roblox.com/game/PlaceLauncher.ashx?request=RequestGameJob&browserTrackerId=" + browserTrackerId
                    + "&placeId=" + placeId.ToString()
                    + "&gameId=" + jobId
                    + "&isPlayTogetherGame=false" + (isTeleport ? "&isTeleport=true" : "");
            }
            string url = "roblox-player:1+launchmode:play+gameinfo:" + ticket
                + "+launchtime:" + launchTime.ToString()
                + "+placelauncherurl:" + HttpUtility.UrlEncode(placeLauncher)
                + "+browsertrackerid:" + browserTrackerId
                + "+robloxLocale:en_us+gameLocale:en_us+channel:+LaunchExp:InApp";
            return url;
        }

        public static string LaunchAccount(Account acc, long placeId, string jobId)
        {
            string csrf;
            string csrfErr;
            if (!RobloxApi.TryGetCsrfToken(acc.Cookie, out csrf, out csrfErr))
            {
                return "CSRF fail (" + acc.Username + "): " + csrfErr;
            }
            string ticket;
            string ticketErr;
            if (!RobloxApi.TryGetAuthTicket(acc.Cookie, csrf, out ticket, out ticketErr))
            {
                return "Ticket fail (" + acc.Username + "): " + ticketErr;
            }
            string url = BuildJoinUrl(ticket, placeId, jobId, acc.BrowserTrackerId, false);
            return StartUrl(url, acc.Username);
        }

        public static string LaunchVip(Account acc, long placeId, string linkCode)
        {
            return LaunchVipCode(acc, placeId, linkCode);
        }

        public static string LaunchFollow(Account acc, long targetUserId)
        {
            string csrf;
            string csrfErr;
            if (!RobloxApi.TryGetCsrfToken(acc.Cookie, out csrf, out csrfErr))
            {
                return "CSRF fail (" + acc.Username + "): " + csrfErr;
            }
            string ticket;
            string ticketErr;
            if (!RobloxApi.TryGetAuthTicket(acc.Cookie, csrf, out ticket, out ticketErr))
            {
                return "Ticket fail (" + acc.Username + "): " + ticketErr;
            }
            string placeLauncher = "https://assetgame.roblox.com/game/PlaceLauncher.ashx?request=RequestFollowUser&userId=" + targetUserId.ToString();
            return LaunchUrl(acc, ticket, placeLauncher);
        }

        // Link share VIP dang moi (/share?code=..&type=Server): resolve tung acc roi join.
        public static string LaunchVipShare(Account acc, string shareCode)
        {
            RobloxApi.ShareResolved res;
            string resErr;
            if (!RobloxApi.TryResolveShareLink(acc.Cookie, shareCode, out res, out resErr))
            {
                return "Share fail (" + acc.Username + "): " + resErr;
            }
            return LaunchVipCode(acc, res.PlaceId, res.LinkCode);
        }

        private static string LaunchVipCode(Account acc, long placeId, string linkCode)
        {
            string csrf;
            string csrfErr;
            if (!RobloxApi.TryGetCsrfToken(acc.Cookie, out csrf, out csrfErr))
            {
                return "CSRF fail (" + acc.Username + "): " + csrfErr;
            }
            string access;
            string accErr;
            if (!RobloxApi.TryGetPrivateAccessCode(acc.Cookie, placeId, linkCode, out access, out accErr))
            {
                return "VIP fail (" + acc.Username + "): " + accErr;
            }
            string ticket;
            string ticketErr;
            if (!RobloxApi.TryGetAuthTicket(acc.Cookie, csrf, out ticket, out ticketErr))
            {
                return "Ticket fail (" + acc.Username + "): " + ticketErr;
            }
            string placeLauncher = "https://assetgame.roblox.com/game/PlaceLauncher.ashx?request=RequestPrivateGame&placeId=" + placeId.ToString()
                + "&accessCode=" + access + "&linkCode=" + linkCode;
            return LaunchUrl(acc, ticket, placeLauncher);
        }

        private static string LaunchUrl(Account acc, string ticket, string placeLauncherOrUrl)
        {
            string url = placeLauncherOrUrl;
            if (!url.StartsWith("roblox-player:"))
            {
                long launchTime = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
                url = "roblox-player:1+launchmode:play+gameinfo:" + ticket
                    + "+launchtime:" + launchTime.ToString()
                    + "+placelauncherurl:" + HttpUtility.UrlEncode(placeLauncherOrUrl)
                    + "+browsertrackerid:" + acc.BrowserTrackerId
                    + "+robloxLocale:en_us+gameLocale:en_us+channel:+LaunchExp:InApp";
            }
            return StartUrl(url, acc.Username);
        }

        private static string StartUrl(string url, string username)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = url;
                psi.UseShellExecute = true;
                Process p = Process.Start(psi);
                if (p != null)
                {
                    try { p.Dispose(); } catch { }
                }
                return null; // ok
            }
            catch (Exception ex)
            {
                return "Launch fail (" + username + "): " + ex.Message;
            }
        }

        public static void KillAll()
        {
            Process[] ps = Process.GetProcessesByName("RobloxPlayerBeta");
            for (int i = 0; i < ps.Length; i++)
            {
                try { ps[i].Kill(); } catch { }
                try { ps[i].Dispose(); } catch { }
            }
        }

        public static int CountRunning()
        {
            try
            {
                Process[] ps = Process.GetProcessesByName("RobloxPlayerBeta");
                int n = ps.Length;
                for (int i = 0; i < ps.Length; i++)
                {
                    try { ps[i].Dispose(); } catch { }
                }
                return n;
            }
            catch
            {
                return 0;
            }
        }
    }
}
