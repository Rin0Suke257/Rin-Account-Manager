using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace RinAccountManager
{
    public class RobloxUser
    {
        public long Id;
        public string Name;
        public bool Ok;
        public string Error;
    }

    public static class RobloxApi
    {
        static RobloxApi()
        {
            try
            {
                System.Net.ServicePointManager.SecurityProtocol =
                    System.Net.SecurityProtocolType.Tls12 |
                    System.Net.SecurityProtocolType.Tls11 |
                    System.Net.SecurityProtocolType.Tls;
            }
            catch { }
        }

        private const string RefererGame = "https://www.roblox.com/games/4924922222/Brookhaven-RP";

        private static HttpWebRequest Create(string url, string cookie)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) RinAccountManager/1.0";
            req.Timeout = 15000;
            req.ReadWriteTimeout = 15000;
            if (!string.IsNullOrEmpty(cookie))
            {
                // Cookie co the bat dau bang "_|WARNING:-DO-NOT-SHARE-THIS..."
                // Chi lay phan sau dau "|" cuoi cung.
                string c = ExtractCookie(cookie);
                req.Headers["Cookie"] = ".ROBLOSECURITY=" + c;
            }
            return req;
        }

        public static string ExtractCookie(string raw)
        {
            if (string.IsNullOrEmpty(raw))
            {
                return raw;
            }
            string t = raw.Trim().Trim('"');
            // dinh dang chuan: ...|_COOKIE_THAT
            // lay sau cum "|_" cuoi cung de bo warning + dau _ thua
            int sep = t.LastIndexOf("|_");
            if (sep >= 0 && sep + 2 < t.Length)
            {
                string tail = t.Substring(sep + 2);
                if (tail.Length >= 100)
                {
                    return tail;
                }
            }
            // fallback: lay sau dau | cuoi
            int lastPipe = t.LastIndexOf('|');
            if (lastPipe >= 0 && t.Length - lastPipe > 200)
            {
                return t.Substring(lastPipe + 1);
            }
            // neu nguoi dung paste full "cookie:xxx" thi cat
            if (t.StartsWith("_|"))
            {
                int p = t.LastIndexOf('|');
                if (p >= 0 && p + 1 < t.Length)
                {
                    return t.Substring(p + 1);
                }
            }
            return t;
        }

        public static string ExtractVipCode(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }
            int i = text.IndexOf("privateServerLinkCode=");
            if (i < 0)
            {
                return null;
            }
            string code = text.Substring(i + "privateServerLinkCode=".Length);
            int amp = code.IndexOf('&');
            if (amp >= 0)
            {
                code = code.Substring(0, amp);
            }
            code = code.Trim();
            return code == "" ? null : code;
        }

        // Link share moi: /share?code=XXX&type=Server (hoac /share-links?...).
        // Chi nhan type=Server (moi join VIP kieu nay).
        public static string ExtractShareCode(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }
            int t = text.IndexOf("type=Server");
            if (t < 0)
            {
                return null;
            }
            System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(text, "[?&]code=([0-9a-fA-F]{16,64})");
            if (m.Success && m.Groups.Count >= 2)
            {
                return m.Groups[1].Value;
            }
            return null;
        }

        // Ma VIP tran (khong kem link): JobId la 1 chuoi ma + PlaceId co game.
        // JobId thuong la GUID co gach ngang -> khong nham.
        public static bool IsBareVipCode(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }
            string t = text.Trim();
            if (t.Contains(" ") || t.Contains("=") || t.Contains("/") || t.Contains("?"))
            {
                return false;
            }
            if (System.Text.RegularExpressions.Regex.IsMatch(t, "^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$"))
            {
                return false;
            }
            if (t.Length >= 16 && System.Text.RegularExpressions.Regex.IsMatch(t, "^[0-9A-Za-z\\-]+$"))
            {
                return true;
            }
            return false;
        }

        public class ShareResolved
        {
            public long PlaceId;
            public string LinkCode;
            public long UniverseId;
        }

        public static bool TryResolveShareLink(string cookie, string shareCode, out ShareResolved res, out string error)
        {
            res = null;
            error = null;
            string json = "{\"linkId\":\"" + shareCode + "\",\"linkType\":\"Server\"}";
            string[] csrfHolder = new string[1];
            for (int attempt = 0; attempt < 2; attempt++)
            {
                try
                {
                    HttpWebRequest req = Create("https://apis.roblox.com/sharelinks/v1/resolve-link", cookie);
                    req.Method = "POST";
                    req.ContentType = "application/json";
                    req.Accept = "application/json, text/plain, */*";
                    req.Referer = "https://www.roblox.com/";
                    if (csrfHolder[0] != null)
                    {
                        req.Headers["X-CSRF-TOKEN"] = csrfHolder[0];
                    }
                    byte[] bytes = Encoding.UTF8.GetBytes(json);
                    req.ContentLength = bytes.Length;
                    using (Stream ws = req.GetRequestStream())
                    {
                        ws.Write(bytes, 0, bytes.Length);
                    }
                    using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                    {
                        using (StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                        {
                            string body = sr.ReadToEnd();
                            JavaScriptSerializer ser = new JavaScriptSerializer();
                            Dictionary<string, object> d = ser.Deserialize<Dictionary<string, object>>(body);
                            object ps;
                            if (d == null || !d.TryGetValue("privateServerInviteData", out ps) || !(ps is Dictionary<string, object>))
                            {
                                error = "Link khong phai VIP server.";
                                return false;
                            }
                            Dictionary<string, object> p = (Dictionary<string, object>)ps;
                            object st;
                            if (!p.TryGetValue("status", out st) || Convert.ToString(st) != "Valid")
                            {
                                error = "Link VIP het han hoac khong hop le.";
                                return false;
                            }
                            ShareResolved r = new ShareResolved();
                            object v;
                            if (p.TryGetValue("placeId", out v)) { r.PlaceId = Convert.ToInt64(v); }
                            if (p.TryGetValue("linkCode", out v)) { r.LinkCode = Convert.ToString(v); }
                            if (p.TryGetValue("universeId", out v)) { r.UniverseId = Convert.ToInt64(v); }
                            if (r.PlaceId <= 0 || string.IsNullOrEmpty(r.LinkCode))
                            {
                                error = "Link VIP thieu thong tin.";
                                return false;
                            }
                            res = r;
                            return true;
                        }
                    }
                }
                catch (WebException wex)
                {
                    HttpWebResponse r = wex.Response as HttpWebResponse;
                    if (r != null)
                    {
                        string tok = r.Headers["x-csrf-token"];
                        try { r.Close(); } catch { }
                        if (tok != null && attempt == 0)
                        {
                            csrfHolder[0] = tok;
                            continue;
                        }
                    }
                    error = "Mang loi: " + wex.Message;
                    return false;
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                    return false;
                }
            }
            error = "Khong resolve duoc link.";
            return false;
        }

        // Lay accessCode cho VIP server tu trang game (can cookie cua acc duoc moi).
        public static bool TryGetPrivateAccessCode(string cookie, long placeId, string linkCode, out string accessCode, out string error)
        {
            accessCode = null;
            error = null;
            try
            {
                string url = "https://www.roblox.com/games/" + placeId.ToString() + "?privateServerLinkCode=" + linkCode;
                HttpWebRequest req = Create(url, cookie);
                req.Method = "GET";
                req.Accept = "text/html";
                req.Referer = "https://www.roblox.com/games/" + placeId.ToString();
                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                {
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                    {
                        string html = sr.ReadToEnd();
                        System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(
                            html, "Roblox\\.GameLauncher\\.joinPrivateGame\\(\\d+\\,\\s*'([\\w\\-]+)'");
                        if (m.Success && m.Groups.Count >= 2)
                        {
                            accessCode = m.Groups[1].Value;
                            return true;
                        }
                        error = "Link VIP sai/het han hoac acc khong duoc moi.";
                        return false;
                    }
                }
            }
            catch (WebException wex)
            {
                HttpWebResponse r = wex.Response as HttpWebResponse;
                if (r != null)
                {
                    error = "VIP HTTP " + ((int)r.StatusCode).ToString();
                    try { r.Close(); } catch { }
                }
                else
                {
                    error = "Mang loi (co the bi chan roblox.com): " + wex.Message;
                }
                return false;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        // Mang VN chan www.roblox.com: kiem tra truoc de bao bat VPN.
        public static bool IsWwwReachable()
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://www.roblox.com/");
                req.Method = "HEAD";
                req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) RinAccountManager/1.0";
                req.Timeout = 7000;
                req.ReadWriteTimeout = 7000;
                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                {
                    resp.Close();
                    return true;
                }
            }
            catch (WebException wex)
            {
                // Co HTTP response (ke ca 4xx/5xx) nghia la toi duoc, chi mang rot moi null.
                if ((wex.Response as HttpWebResponse) != null)
                {
                    try { wex.Response.Close(); } catch { }
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        // Username -> UserId (cho join theo user).
        public static bool ResolveUserId(string username, out long userId, out string error)
        {
            userId = 0;
            error = null;
            string name = username.Trim().TrimStart('@');
            if (name == "")
            {
                error = "Chua nhap username.";
                return false;
            }
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://users.roblox.com/v1/usernames/users");
                req.Method = "POST";
                req.ContentType = "application/json";
                req.Accept = "application/json";
                req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) RinAccountManager/1.0";
                req.Timeout = 15000;
                req.ReadWriteTimeout = 15000;
                string json = "{\"usernames\":[\"" + name.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"],\"excludeBannedUsers\":true}";
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                req.ContentLength = bytes.Length;
                using (Stream ws = req.GetRequestStream())
                {
                    ws.Write(bytes, 0, bytes.Length);
                }
                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                {
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                    {
                        string body = sr.ReadToEnd();
                        JavaScriptSerializer ser = new JavaScriptSerializer();
                        Dictionary<string, object> d = ser.Deserialize<Dictionary<string, object>>(body);
                        System.Collections.ArrayList data = d["data"] as System.Collections.ArrayList;
                        if (data == null || data.Count == 0)
                        {
                            error = "Khong tim thay user @" + name + ".";
                            return false;
                        }
                        Dictionary<string, object> u0 = data[0] as Dictionary<string, object>;
                        userId = Convert.ToInt64(u0["id"]);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                error = "Mang loi: " + ex.Message;
                return false;
            }
        }

        public static bool TryRefreshCookie(string cookie, out string newCookie, out string error)
        {
            newCookie = null;
            error = null;
            string csrf;
            string csrfErr;
            if (!TryGetCsrfToken(cookie, out csrf, out csrfErr))
            {
                error = csrfErr;
                return false;
            }
            try
            {
                HttpWebRequest req = Create("https://auth.roblox.com/v2/session/refresh", cookie);
                req.Method = "POST";
                req.ContentType = "application/json";
                req.ContentLength = 0;
                req.Headers["X-CSRF-TOKEN"] = csrf;
                req.Referer = RefererGame;
                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                {
                    string[] sc = null;
                    try { sc = resp.Headers.GetValues("Set-Cookie"); } catch { }
                    if (sc != null)
                    {
                        for (int i = 0; i < sc.Length; i++)
                        {
                            if (sc[i] != null && sc[i].StartsWith(".ROBLOSECURITY="))
                            {
                                string v = sc[i].Substring(".ROBLOSECURITY=".Length);
                                int semi = v.IndexOf(';');
                                if (semi >= 0)
                                {
                                    v = v.Substring(0, semi);
                                }
                                newCookie = v.Trim();
                                return true;
                            }
                        }
                    }
                    error = "Khong thay cookie moi.";
                    return false;
                }
            }
            catch (WebException wex)
            {
                HttpWebResponse r = wex.Response as HttpWebResponse;
                if (r != null)
                {
                    error = "HTTP " + ((int)r.StatusCode).ToString();
                    try { r.Close(); } catch { }
                }
                else
                {
                    error = "Mang loi: " + wex.Message;
                }
                return false;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public static RobloxUser GetAuthenticatedUser(string cookie)
        {
            RobloxUser u = new RobloxUser();
            u.Ok = false;
            try
            {
                HttpWebRequest req = Create("https://users.roblox.com/v1/users/authenticated", cookie);
                req.Method = "GET";
                req.Accept = "application/json";
                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                {
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                    {
                        string body = sr.ReadToEnd();
                        JavaScriptSerializer ser = new JavaScriptSerializer();
                        try
                        {
                            var dict = ser.Deserialize<System.Collections.Generic.Dictionary<string, object>>(body);
                            object idObj;
                            object nameObj;
                            if (dict.TryGetValue("id", out idObj) && dict.TryGetValue("name", out nameObj))
                            {
                                u.Id = Convert.ToInt64(idObj);
                                u.Name = Convert.ToString(nameObj);
                                u.Ok = true;
                                return u;
                            }
                            u.Error = "Cookie het han hoac khong hop le.";
                            return u;
                        }
                        catch (Exception ex)
                        {
                            u.Error = "Parse user: " + ex.Message;
                            return u;
                        }
                    }
                }
            }
            catch (WebException wex)
            {
                HttpWebResponse resp = wex.Response as HttpWebResponse;
                int code = 0;
                if (resp != null)
                {
                    try { code = (int)resp.StatusCode; } catch { }
                    try { resp.Close(); } catch { }
                }
                if (code == 401 || code == 403)
                {
                    u.Error = "cookie die (HTTP " + code.ToString() + ")";
                }
                else if (code > 0)
                {
                    u.Error = "loi mang (HTTP " + code.ToString() + ")";
                }
                else
                {
                    u.Error = "loi mang: " + wex.Message;
                }
                return u;
            }
            catch (Exception ex)
            {
                u.Error = ex.Message;
                return u;
            }
        }

        public static bool TryGetCsrfToken(string cookie, out string token, out string error)
        {
            token = null;
            error = null;
            try
            {
                HttpWebRequest req = Create("https://auth.roblox.com/v1/authentication-ticket/", cookie);
                req.Method = "POST";
                req.ContentType = "application/json";
                req.ContentLength = 0;
                req.Referer = RefererGame;
                try
                {
                    using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                    {
                        resp.Close();
                    }
                    error = "Roblox khong tra 403 nhu ky vong.";
                    return false;
                }
                catch (WebException wex)
                {
                    HttpWebResponse resp = wex.Response as HttpWebResponse;
                    if (resp != null)
                    {
                        try
                        {
                            if (resp.StatusCode == HttpStatusCode.Forbidden)
                            {
                                string t = resp.Headers["x-csrf-token"];
                                if (!string.IsNullOrEmpty(t))
                                {
                                    token = t;
                                    return true;
                                }
                                error = "Khong tim thay x-csrf-token.";
                                return false;
                            }
                            error = "CSRF status: " + ((int)resp.StatusCode).ToString() + " " + resp.StatusCode.ToString();
                            return false;
                        }
                        finally
                        {
                            try { resp.Close(); } catch { }
                        }
                    }
                    error = wex.Message;
                    return false;
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public static bool TryGetAuthTicket(string cookie, string csrfToken, out string ticket, out string error)
        {
            ticket = null;
            error = null;
            try
            {
                HttpWebRequest req = Create("https://auth.roblox.com/v1/authentication-ticket/", cookie);
                req.Method = "POST";
                req.ContentType = "application/json";
                req.ContentLength = 0;
                req.Headers["X-CSRF-TOKEN"] = csrfToken;
                req.Referer = RefererGame;
                try
                {
                    using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                    {
                        ticket = resp.Headers["rbx-authentication-ticket"];
                        resp.Close();
                    }
                    if (!string.IsNullOrEmpty(ticket))
                    {
                        return true;
                    }
                    error = "Khong co rbx-authentication-ticket trong response.";
                    return false;
                }
                catch (WebException wex)
                {
                    HttpWebResponse resp = wex.Response as HttpWebResponse;
                    string body = "";
                    if (resp != null)
                    {
                        try
                        {
                            using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                            {
                                body = sr.ReadToEnd();
                            }
                        }
                        catch { }
                        try { resp.Close(); } catch { }
                    }
                    error = "Ticket failed: " + wex.Message + " " + body;
                    return false;
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}
